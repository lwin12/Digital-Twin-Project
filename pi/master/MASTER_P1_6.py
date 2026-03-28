# -*- coding: utf-8 -*-
from picamera2 import Picamera2
from picamera2.encoders import JpegEncoder
from picamera2.outputs import FileOutput
from flask import Flask, Response, render_template_string
from libcamera import controls
import pigpio
import io
import threading
import time
import os
import paho.mqtt.client as mqtt

# =========================================================================
# Configuration
# =========================================================================

CAM_ID = 1
PI_NAME = f"Pi_{CAM_ID}"
VIEW_NAME = "MiddleView"

MQTT_BROKER = "192.168.137.1"
MQTT_PORT = 1883
CAPTURE_DIR = "/home/lwin14/captures"
STREAM_RESOLUTION = (4608, 2592)
STILL_RESOLUTION = (4608, 2592)
FLASK_PORT = 7123
RECONNECT_MIN_DELAY = 1
RECONNECT_MAX_DELAY = 30

# Zoom levels: 0 = no zoom, 1 = 1.5x, 2 = 2x, 3 = 3x, 4 = 4x
ZOOM_LEVELS = {
    0: 1.0,
    1: 1.5,
    2: 2.0,
    3: 3.0,
    4: 4.0,
}

# Stepper motor
DIR_PIN = 17
PUL_PIN = 27
STEPS_PER_REV = 19700  # 6400
TARGET_ANGLE = 15
NUM_MOVES = 24
DIRECTION = 1
PULSES_PER_MOVE = round(STEPS_PER_REV * TARGET_ANGLE / 360)
ACTUAL_ANGLE = (PULSES_PER_MOVE / STEPS_PER_REV) * 360
PULSE_HALF_US = 250
PAUSE_BETWEEN = 0.2
CAPTURE_TIMEOUT = 30
ALL_CAMS = [1, 2, 3]

# =========================================================================
# Setup
# =========================================================================

app = Flask(__name__)
streaming = False
capturing = False
current_zoom = 0
object_name = "unnamed"

# Synchronization
capture_done = {cam: False for cam in ALL_CAMS}
capture_lock = threading.Event()

os.makedirs(CAPTURE_DIR, exist_ok=True)

PAGE = """
<!doctype html>
<html>
<head>
<title>Raspberry Pi Camera Stream</title>
<style>
body {
    margin: 0;
    background: black;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
}
img {
    max-width: 100%;
    max-height: 100%;
}
</style>
</head>
<body>
    <img src="/stream.mjpg" />
</body>
</html>
"""


class StreamingOutput(io.BufferedIOBase):
    def __init__(self):
        self.frame = None
        self.condition = threading.Condition()

    def write(self, buf):
        with self.condition:
            self.frame = buf
            self.condition.notify_all()


output = StreamingOutput()
picam2 = Picamera2()

# =========================================================================
# Stepper Motor
# =========================================================================

pi_gpio = pigpio.pi()
if not pi_gpio.connected:
    print("ERROR: Cannot connect to pigpio daemon. Run: sudo pigpiod")
else:
    pi_gpio.set_mode(DIR_PIN, pigpio.OUTPUT)
    pi_gpio.set_mode(PUL_PIN, pigpio.OUTPUT)
    pi_gpio.write(DIR_PIN, DIRECTION)
    pi_gpio.write(PUL_PIN, 0)
    print("pigpio connected, stepper ready")


def rotate_step(num_pulses):
    pi_gpio.wave_clear()
    pulse = [
        pigpio.pulse(1 << PUL_PIN, 0, PULSE_HALF_US),
        pigpio.pulse(0, 1 << PUL_PIN, PULSE_HALF_US),
    ]
    pi_gpio.wave_add_generic(pulse)
    single_wave = pi_gpio.wave_create()
    if single_wave < 0:
        raise RuntimeError("Failed to create waveform")
    chain = []
    remaining = num_pulses
    while remaining > 0:
        batch = min(remaining, 65535)
        chain += [255, 0]
        chain += [single_wave]
        chain += [255, 1, batch & 0xFF, batch >> 8]
        remaining -= batch
    pi_gpio.wave_chain(chain)
    while pi_gpio.wave_tx_busy():
        time.sleep(0.01)
    pi_gpio.wave_delete(single_wave)


# =========================================================================
# Camera Control
# =========================================================================

def start_camera():
    global streaming
    if streaming:
        return
    config = picam2.create_video_configuration(main={"size": STREAM_RESOLUTION})
    picam2.configure(config)
    picam2.start()
    time.sleep(1)
    picam2.set_controls({
        "AfMode": controls.AfModeEnum.Continuous,
        "AfSpeed": controls.AfSpeedEnum.Fast
    })
    time.sleep(2)
    picam2.start_recording(JpegEncoder(), FileOutput(output))
    streaming = True
    print(f"Camera started (stream: {STREAM_RESOLUTION}, encoder: JpegEncoder)")


def stop_camera():
    global streaming
    if not streaming:
        return
    picam2.stop_recording()
    picam2.stop()
    streaming = False
    print("Camera stopped")


def take_picture(turn=None):
    # Build view-specific subfolder
    subfolder = os.path.join(CAPTURE_DIR, f"{VIEW_NAME}_{object_name}")
    os.makedirs(subfolder, exist_ok=True)

    if turn is not None:
        filename = f"{object_name}_{CAM_ID}_{turn:02d}.jpg"
    else:
        timestamp = time.strftime("%Y%m%d_%H%M%S")
        filename = f"{object_name}_{CAM_ID}_{timestamp}.jpg"

    filepath = os.path.join(subfolder, filename)

    if streaming:
        picam2.capture_file(filepath, format='jpeg')
    else:
        config = picam2.create_still_configuration(main={"size": STILL_RESOLUTION})
        picam2.configure(config)
        picam2.start()
        time.sleep(2)
        picam2.capture_file(filepath, format='jpeg')
        picam2.stop()

    print(f"Picture saved: {filepath}")
    return filepath


def focus_nearest():
    """Focus on nearest object once — call before capture loop."""
    picam2.set_controls({
        "AfMode": controls.AfModeEnum.Auto,
        "AfRange": controls.AfRangeEnum.Macro,
        "AfSpeed": controls.AfSpeedEnum.Fast,
        "AfTrigger": controls.AfTriggerEnum.Start
    })
    time.sleep(1.5)
    print("Focus locked on nearest object")


def apply_zoom(level):
    factor = ZOOM_LEVELS.get(level, 1.0)
    sensor_size = picam2.camera_properties["ScalerCropMaximum"]
    base_x = sensor_size[0]
    base_y = sensor_size[1]
    full_w = sensor_size[2]
    full_h = sensor_size[3]
    crop_w = int(full_w / factor)
    crop_h = int(full_h / factor)
    offset_x = base_x + (full_w - crop_w) // 2
    offset_y = base_y + (full_h - crop_h) // 2
    picam2.set_controls({"ScalerCrop": (offset_x, offset_y, crop_w, crop_h)})
    print(f"Zoom level {level} applied ({factor}x)")


# =========================================================================
# Synchronization
# =========================================================================

def check_all_done():
    if all(capture_done.values()):
        capture_lock.set()


# =========================================================================
# Capture Sequence (Master — Rotate + Sync + Snap x 24)
# =========================================================================

def start_capture_sequence():
    global capturing
    if capturing:
        print("Capture sequence already running")
        return

    capturing = True
    start_time = time.time()
    start_timestamp = time.strftime("%Y%m%d_%H%M%S")

    was_streaming = streaming
    if not streaming:
        start_camera()

    apply_zoom(current_zoom)

    # Focus once on nearest object before starting
    focus_nearest()

    # Tell slaves to focus too
    mqtt_client.publish("host/focus_now", "1")
    time.sleep(2)  # give slaves time to focus

    pi_gpio.write(DIR_PIN, DIRECTION)
    time.sleep(0.01)

    print(f"Capture sequence started at {start_timestamp}")
    print(f"  Object: {object_name}")
    print(f"  Zoom level {current_zoom}: {NUM_MOVES} moves of {ACTUAL_ANGLE:.3f}")

    for i in range(1, NUM_MOVES + 1):
        # Rotate
        rotate_step(PULSES_PER_MOVE)
        time.sleep(PAUSE_BETWEEN)

        # Reset sync flags
        for cam in ALL_CAMS:
            capture_done[cam] = False
        capture_lock.clear()

        # Tell all Pis to capture
        mqtt_client.publish("host/capture_now", str(i))

        # Take own picture
        take_picture(turn=i)
        capture_done[CAM_ID] = True
        mqtt_client.publish(f"{PI_NAME}/capture_done", "1")
        check_all_done()

        # Wait for all Pis to finish
        if not capture_lock.wait(timeout=CAPTURE_TIMEOUT):
            waiting_on = [c for c in ALL_CAMS if not capture_done[c]]
            print(f"WARNING: Turn {i} — timeout waiting for Pi(s): {waiting_on}")

        mqtt_client.publish(f"{PI_NAME}/Turn", str(i))
        print(f"Turn {i}/{NUM_MOVES} complete — all Pis done")

    end_time = time.time()
    end_timestamp = time.strftime("%Y%m%d_%H%M%S")
    elapsed = end_time - start_time

    print(f"Capture sequence complete")
    print(f"  Started : {start_timestamp}")
    print(f"  Ended   : {end_timestamp}")
    print(f"  Duration: {elapsed:.2f}s")

    mqtt_client.publish(f"{PI_NAME}/CaptureTime", f"{elapsed:.2f}")

    # Resume continuous AF
    picam2.set_controls({"AfMode": controls.AfModeEnum.Continuous})

    if not was_streaming:
        stop_camera()

    capturing = False


# =========================================================================
# MQTT Callbacks
# =========================================================================

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print("Connected to MQTT broker")
        client.subscribe("host/startpreview")
        client.subscribe("host/startcapture")
        client.subscribe("host/status")
        client.subscribe("host/cam_zoom")
        client.subscribe("host/objectname")
        for cam in ALL_CAMS:
            client.subscribe(f"Pi_{cam}/capture_done")
    else:
        print(f"MQTT connection failed (rc={rc})")


def on_message(client, userdata, msg):
    global current_zoom, object_name
    topic = msg.topic
    payload = msg.payload.decode().strip()

    if topic == "host/startpreview":
        if payload == "1":
            start_camera()
        elif payload == "0":
            stop_camera()
        elif payload == "3":
            take_picture()

    elif topic == "host/startcapture":
        if payload == "1":
            threading.Thread(target=start_capture_sequence, daemon=True).start()

    elif topic == "host/objectname":
        object_name = payload
        print(f"Object name set to: {object_name}")

    elif topic == "host/status":
        if payload == "shutdown":
            print("Received shutdown from host")
            stop_camera()

    elif topic == "host/cam_zoom":
        try:
            for pair in payload.split(","):
                cam_id, zoom_level = pair.strip().split("_")
                if int(cam_id) == CAM_ID:
                    current_zoom = int(zoom_level)
                    if streaming:
                        apply_zoom(current_zoom)
                    else:
                        print(f"Zoom level {current_zoom} stored, will apply when streaming")
                    break
        except (ValueError, KeyError) as e:
            print(f"Invalid cam_zoom payload '{payload}': {e}")

    elif topic.endswith("/capture_done"):
        try:
            cam = int(topic.split("_")[1].split("/")[0])
            if payload == "1" and cam in ALL_CAMS:
                capture_done[cam] = True
                print(f"Pi_{cam} capture done")
                check_all_done()
        except (ValueError, IndexError):
            pass


def on_disconnect(client, userdata, rc, properties=None, reasonCode=None):
    print(f"Disconnected from MQTT broker (rc={rc})")
    stop_camera()


# =========================================================================
# MQTT Client Setup
# =========================================================================

mqtt_client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2)
mqtt_client.on_connect = on_connect
mqtt_client.on_message = on_message
mqtt_client.on_disconnect = on_disconnect
mqtt_client.reconnect_delay_set(min_delay=RECONNECT_MIN_DELAY, max_delay=RECONNECT_MAX_DELAY)


def initial_connect():
    while True:
        try:
            print(f"Connecting to MQTT broker at {MQTT_BROKER}:{MQTT_PORT}...")
            mqtt_client.connect(MQTT_BROKER, MQTT_PORT)
            mqtt_client.loop_start()
            print("MQTT loop started")
            return
        except Exception as e:
            print(f"Initial connection failed: {e}")
            print(f"Retrying in {RECONNECT_MIN_DELAY}s...")
            time.sleep(RECONNECT_MIN_DELAY)


threading.Thread(target=initial_connect, daemon=True).start()

# =========================================================================
# Flask Routes
# =========================================================================

@app.route("/")
def index():
    return render_template_string(PAGE)


@app.route("/stream.mjpg")
def stream():
    def generate():
        while True:
            with output.condition:
                output.condition.wait()
                frame = output.frame
            yield (b"--frame\r\n"
                   b"Content-Type: image/jpeg\r\n\r\n" + frame + b"\r\n")
    return Response(generate(),
                    mimetype="multipart/x-mixed-replace; boundary=frame")


# =========================================================================
# Entry Point
# =========================================================================

if __name__ == "__main__":
    try:
        app.run(host="0.0.0.0", port=FLASK_PORT, threaded=True)
    finally:
        if pi_gpio.connected:
            pi_gpio.write(PUL_PIN, 0)
            pi_gpio.write(DIR_PIN, 0)
            pi_gpio.stop()
            print("pigpio cleaned up")
