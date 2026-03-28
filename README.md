# Automated 3D Scanner

An automated photogrammetry system that captures multi-angle images of physical objects using three synchronized Raspberry Pi cameras on a motorized turntable, reconstructs them into textured 3D meshes with Meshroom, and serves the results as interactive GLB models in a web viewer.

Built for a Hackathon.

## How It Works

1. Place an object on the turntable inside the lightbox
2. Launch the WinForms control app — camera previews start automatically
3. Adjust zoom levels per camera, type an object name, press **Start Capture**
4. The turntable rotates 24 × 15° while all 3 cameras capture synchronized photos (72 images total)
5. Images are automatically transferred to the host PC and organized into 7 view combinations
6. Select a view combination, press **Start Mesh** — Meshroom reconstructs the 3D model, Blender converts it to GLB
7. Open the web viewer — your interactive 3D model is ready

The full pipeline from physical object to browser-viewable 3D model runs through two button presses.

## System Architecture

```
┌─────────────────────────────────────────────────────┐
│  Windows Host PC (192.168.137.1)                    │
│                                                     │
│  ┌──────────┐  ┌───────────┐  ┌──────────────────┐  │
│  │ WinForms │──│ Mosquitto │  │ Transfer Script  │  │
│  │ App      │  │ Broker    │  │ (SSH/Paramiko)   │  │
│  └──────────┘  └─────┬─────┘  └──────────────────┘  │
│                      │                               │
│  ┌──────────┐  ┌─────┴─────┐  ┌──────────────────┐  │
│  │ Meshroom │  │  Blender  │  │   Web Viewer     │  │
│  │          │  │  CLI      │  │   (Three.js)     │  │
│  └──────────┘  └───────────┘  └──────────────────┘  │
└─────────────────────┬───────────────────────────────┘
                      │ Ethernet Switch
        ┌─────────────┼─────────────┐
        │             │             │
   ┌────┴────┐  ┌─────┴────┐  ┌────┴────┐
   │  Pi_1   │  │   Pi_2   │  │  Pi_3   │
   │ Master  │  │  Slave   │  │  Slave  │
   │ Camera  │  │  Camera  │  │  Camera │
   │ + Motor │  │  only    │  │  only   │
   └─────────┘  └──────────┘  └─────────┘
   192.168.137.50  192.168.137.10  192.168.137.30
   Middle View     Top-Down View   Bottom-Up View
```

## Hardware

| Component | Specification |
|---|---|
| Compute | Raspberry Pi 4 × 3 |
| Cameras | Pi Camera Module 3 (IMX708) × 3 |
| Motor | NEMA 17 (17HS4401S) stepper motor |
| Driver | Microstep driver (1/32 microstepping) |
| Network | Ethernet switch + Cat5e cables |
| Enclosure | White lightbox for diffused lighting |
| Host PC | Windows PC (runs control app + Meshroom) |

## Software Stack

| Layer | Component | Technology |
|---|---|---|
| Presentation | Control app | C# WinForms (.NET) |
| Presentation | Camera preview | WebView2 / MJPEG |
| Presentation | 3D model viewer | HTML / Three.js |
| Communication | MQTT broker | Mosquitto |
| Communication | MQTT clients | MQTTnet (C#) / paho-mqtt (Python) |
| Communication | File transfer | SSH / Paramiko |
| Capture | Camera control | Picamera2 / libcamera |
| Capture | Live streaming | Flask / MJPEG |
| Capture | Motor control | pigpio (hardware-timed waveforms) |
| Processing | 3D reconstruction | Meshroom / AliceVision |
| Processing | Format conversion | Blender 5.0 headless CLI |
| Integration | ROS2 bridge | rclpy / std_msgs (optional) |

## MQTT Topics

### Published by Host (WinForms App)

| Topic | Payload | Purpose |
|---|---|---|
| `host/startpreview` | `"1"` / `"0"` | Start or stop camera streams |
| `host/startcapture` | `"1"` | Begin 24-turn capture sequence |
| `host/objectname` | e.g. `"shoe"` | Set filename prefix for captures |
| `host/cam_zoom` | e.g. `"1_2,2_0,3_2"` | Set zoom per camera |
| `host/status` | `"shutdown"` | Stop all cameras on app close |

### Published by Pi_1 (Master)

| Topic | Payload | Purpose |
|---|---|---|
| `host/capture_now` | `"1"` to `"24"` | Tell all Pis to capture this turn |
| `host/focus_now` | `"1"` | Tell slaves to lock macro autofocus |
| `Pi_1/capture_done` | `"1"` | Confirm Pi_1 finished capturing |
| `Pi_1/Turn` | `"1"` to `"24"` | Report current turn number |
| `Pi_1/CaptureTime` | e.g. `"85.23"` | Total elapsed seconds (triggers transfer) |

### Published by Slaves

| Topic | Payload | Purpose |
|---|---|---|
| `Pi_2/capture_done` | `"1"` | Confirm Pi_2 finished capturing |
| `Pi_3/capture_done` | `"1"` | Confirm Pi_3 finished capturing |

## View Combinations

After transfer, images are organized into 7 folders for flexible reconstruction. The user selects which combination to use in the Mesh Settings panel before running Meshroom.

| Folder | Cameras | Images | Description |
|---|---|---|---|
| `All` | Pi_1 + Pi_2 + Pi_3 | 72 | All 3 angles, best quality |
| `Top_Bottom` | Pi_2 + Pi_3 | 48 | Top-down and bottom-up views |
| `Top_Middle` | Pi_2 + Pi_1 | 48 | Top-down and middle views |
| `Middle_Bottom` | Pi_1 + Pi_3 | 48 | Middle and bottom-up views |
| `Top` | Pi_2 only | 24 | Top-down view only |
| `Middle` | Pi_1 only | 24 | Middle view only |
| `Bottom` | Pi_3 only | 24 | Bottom-up view only |

Different objects reconstruct better with different angle combinations. For example, flat objects may work best with `Top_Bottom`, while tall objects may benefit from `All` or `Middle_Bottom`.

## Image Naming Convention

```
{objectname}_{CameraID}_{turn}.jpg
```

Examples for object "shoe":
- `shoe_1_01.jpg` — Pi_1 (middle view), turn 1
- `shoe_2_15.jpg` — Pi_2 (top-down view), turn 15
- `shoe_3_24.jpg` — Pi_3 (bottom-up view), turn 24

## How the Capture Sync Works

The master (Pi_1) coordinates all 3 cameras to ensure no image is missed:

1. Master rotates the turntable 15°
2. Master publishes `host/capture_now` with the turn number
3. All 3 Pis take a picture simultaneously
4. Each Pi publishes `Pi_X/capture_done` when saved
5. Master waits until all 3 confirmations arrive (30s timeout)
6. Master rotates to the next position

This handshake repeats 24 times for a full 360° rotation, producing 72 images (24 per camera angle).

## Project Structure

```
3D-Scanner/
├── README.md
├── docs/
│   ├── architecture.md
│   ├── pi-scripts.md
│   ├── control-panel.md
│   └── mqtt-topics.md
├── pi/
│   ├── master/
│   │   └── MASTER_P1.py
│   ├── slave-2/
│   │   └── SLAVE_P2.py
│   └── slave-3/
│       └── SLAVE_P3.py
├── host/
│   ├── control-app/
│   │   ├── Form1.cs
│   │   ├── Form2.cs
│   │   ├── Control_App.csproj
│   │   └── config.json
│   ├── transfer/
│   │   └── transfer_images.py
│   └── web-viewer/
│       └── index.html
├── ros2/
│   └── scanner_mqtt_bridge/
│       ├── package.xml
│       ├── setup.py
│       ├── launch/
│       │   └── bridge_launch.py
│       └── scanner_mqtt_bridge/
│           ├── __init__.py
│           └── mqtt_bridge_node.py
├── meshroom/
│   └── Turntable_PipeLine.mg
└── requirements/
    ├── pi-requirements.txt
    └── host-requirements.txt
```

## Setup

### Prerequisites

- 3 × Raspberry Pi 4 with Pi Camera Module 3, running Raspberry Pi OS
- Windows PC with NVIDIA GPU (for Meshroom CUDA acceleration)
- Mosquitto MQTT broker installed on the host PC
- Meshroom 2025.1.0
- Blender 5.0
- Python 3.11+ on host PC
- .NET 8.0 SDK (for the WinForms app)

### Pi Setup (repeat for each Pi)

```bash
# Install dependencies
pip install flask paho-mqtt --break-system-packages

# Start the pigpio daemon (Pi_1 only)
sudo pigpiod

# Run the script
python3 MASTER_P1.py   # on Pi_1
python3 SLAVE_P2.py    # on Pi_2
python3 SLAVE_P3.py    # on Pi_3
```

### Host PC Setup

```bash
# Start Mosquitto
mosquitto -c mosquitto.conf

# Install Python dependencies for the transfer script
pip install paramiko paho-mqtt

# Install Blender CLI (ensure it's at the path in Form1.cs)
# Install Meshroom 2025.1.0

# Build and run the WinForms app from Visual Studio
```

### Configuration

Each Pi script has a configuration section at the top:

```python
CAM_ID = 1              # 1, 2, or 3
MQTT_BROKER = "192.168.137.1"
CAPTURE_DIR = "/home/lwin14/captures"
STREAM_RESOLUTION = (4608, 2592)
STILL_RESOLUTION = (4608, 2592)
FLASK_PORT = 7123
```

The WinForms app loads camera URLs from `config.json`:

```json
{
  "Cam1Url": "http://192.168.137.50:7123",
  "Cam2Url": "http://192.168.137.10:7123",
  "Cam3Url": "http://192.168.137.30:7123"
}
```

## ROS2 Integration (Optional)

The `scanner_mqtt_bridge` package bridges all MQTT topics to ROS2, enabling integration with robotic systems:

```bash
cd ~/ros2_ws/src
cp -r path/to/ros2/scanner_mqtt_bridge .
cd ~/ros2_ws
colcon build --packages-select scanner_mqtt_bridge
source install/setup.bash
ros2 launch scanner_mqtt_bridge bridge_launch.py
```

Control the scanner from ROS2:

```bash
# Start a capture
ros2 topic pub /scanner/cmd/objectname std_msgs/String "data: 'shoe'" --once
ros2 topic pub /scanner/cmd/startcapture std_msgs/String "data: '1'" --once

# Monitor progress
ros2 topic echo /scanner/pi_1/turn
```

## Documentation

- [System Architecture](docs/architecture.md)
- [Pi Scripts — How They Work](docs/pi-scripts.md)
- [Control Panel & Transfer Script](docs/control-panel.md)
- [MQTT Topic Reference](docs/mqtt-topics.md)
