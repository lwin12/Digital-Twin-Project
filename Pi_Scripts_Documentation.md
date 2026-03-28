# How the Master and Slave Pi Scripts Work

Both scripts share the same core structure — Flask web server + MQTT client + camera control. The master adds stepper motor control and capture synchronization on top.

## Startup Sequence (Both Master and Slave)

1. Picamera2 initializes and detects the IMX708 sensor
2. pigpio connects to the daemon (master only — for stepper motor)
3. MQTT client connects to the broker at `192.168.137.1` in a daemon thread, retrying until successful
4. Flask starts on port `7123`, ready to serve the MJPEG stream
5. The script sits idle, waiting for MQTT messages

## Camera Operation

When `host/startpreview = "1"` arrives, `start_camera()` configures Picamera2 for video at the stream resolution, starts the sensor, enables continuous autofocus, then begins recording through the `JpegEncoder` into a `StreamingOutput` buffer. The Flask `/stream.mjpg` route continuously yields frames from that buffer as a multipart JPEG response — this is what the WebView2 control displays in the WinForms app.

When `take_picture()` is called, it captures a full-resolution still from the running camera using `capture_file()`. The filename is built from the object name, camera ID, and turn number — for example `shoe_1_03.jpg`. Files are saved into a view-specific subfolder like `MiddleView_shoe/`.

## Master Capture Sequence

When `host/startcapture = "1"` arrives, Pi\_1 spawns `start_capture_sequence` in a thread. This function takes full control:

First it ensures the camera is running, applies the stored zoom level via `ScalerCrop`, then locks macro autofocus on the nearest object. It publishes `host/focus_now` so the slaves do the same, then waits 2 seconds.

Then it enters the main loop. For each of the 24 turns, it calls `rotate_step()` which builds a pigpio waveform — a precise hardware-timed pulse train of 820 pulses (at the current 19700 steps/rev setting), each pulse being 250μs high and 250μs low. The waveform is chained and executed by the pigpio daemon, bypassing Python's timing limitations entirely. After rotation, it pauses 0.2 seconds for vibrations to settle.

Then it resets the sync flags for all 3 cameras, publishes `host/capture_now` with the turn number, takes its own picture, and marks itself done. It then calls `capture_lock.wait(timeout=30)` which blocks until `check_all_done()` sets the event — this only happens when all 3 entries in `capture_done` are `True`. If 30 seconds pass without all confirmations, it prints a warning but continues anyway.

After all 24 turns, it publishes the total elapsed time to `Pi_1/CaptureTime`, which triggers the transfer script on the host PC.

## Slave Response

Slaves are purely reactive. They subscribe to `host/capture_now` and when they receive a turn number, they spawn `capture_and_report` in a daemon thread. This function calls `take_picture(turn=turn)` to save a JPEG, then publishes `Pi_X/capture_done = "1"`. The master receives this and checks if all 3 are done.

The threading is important — `on_message` runs on paho's network thread, so without spawning a separate thread, the camera capture would block all other MQTT message processing. The `daemon=True` flag ensures these threads don't prevent the script from exiting.

## Digital Zoom

When `host/cam_zoom` arrives with a payload like `1_2,2_0,3_2`, each Pi parses the comma-separated pairs, finds the one matching its `CAM_ID`, and stores the zoom level. If the camera is streaming, it applies immediately via `apply_zoom()`, which calculates a centered crop rectangle from the sensor's `ScalerCropMaximum` property and divides by the zoom factor. A zoom of 2.0 means the crop window is half the sensor width and height, centered — effectively a 2× digital zoom.

## Autofocus

During preview, continuous autofocus runs in the background. Before capture, the master switches to single-shot macro autofocus — `AfMode.Auto` with `AfRange.Macro` prioritizes near objects. It triggers once with `AfTrigger.Start`, waits 1.5 seconds for the lens to lock, then all 24 captures use that fixed focus distance. After the sequence, it switches back to continuous AF for the preview.
