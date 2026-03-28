# System Architecture

## Overview

The system consists of 4 devices connected via Ethernet through a network switch:

- **Windows Host PC** (192.168.137.1) — runs the control app, MQTT broker, transfer script, Meshroom, Blender, and web viewer
- **Pi_1** (192.168.137.50) — master node with camera + stepper motor, username `lwin14`
- **Pi_2** (192.168.137.10) — slave node with camera only, username `lwin16`
- **Pi_3** (192.168.137.30) — slave node with camera only, username `lwin15`

## Communication

All coordination happens through MQTT (Mosquitto broker on the host PC). There are 12 topics in use — 5 published by the host app, 5 published by Pi_1, and 1 each published by Pi_2 and Pi_3. See [mqtt-topics.md](mqtt-topics.md) for the full reference.

File transfer uses SSH/SFTP via Paramiko. The transfer script SSHs into each Pi after capture completes to download and organize images.

Camera preview streams use Flask MJPEG over HTTP on port 7123 from each Pi, displayed in WebView2 controls in the WinForms app.

## Data Flow

### Capture Phase

1. WinForms app publishes object name and capture command via MQTT
2. Pi_1 (master) receives the command and runs the 24-turn sequence
3. For each turn: Pi_1 rotates the motor, tells all Pis to capture, waits for confirmation
4. Each Pi saves images locally in view-specific subfolders

### Transfer Phase

1. Pi_1 publishes `Pi_1/CaptureTime` when the sequence finishes
2. Transfer script (on host PC) receives this and SSHs into all 3 Pis
3. Images are downloaded and organized into 7 combination folders:
   - **All** — Pi_1 + Pi_2 + Pi_3 (72 images)
   - **Top_Bottom** — Pi_2 + Pi_3 (48 images)
   - **Top_Middle** — Pi_2 + Pi_1 (48 images)
   - **Middle_Bottom** — Pi_1 + Pi_3 (48 images)
   - **Top** — Pi_2 only (24 images)
   - **Middle** — Pi_1 only (24 images)
   - **Bottom** — Pi_3 only (24 images)
4. Remote images are deleted from the Pis

### Reconstruction Phase

1. User selects a view combination from the Mesh Settings radio buttons and clicks Start Mesh
2. Meshroom processes the images from the selected folder through 14 pipeline nodes
3. The resulting OBJ is automatically converted to GLB via Blender CLI
4. The GLB is saved to the web viewer directory as `{objectname}_{combination}.glb`

### Viewing Phase

1. Web viewer (HTML/Three.js) auto-discovers GLB files every 5 seconds
2. User selects a model from the searchable dropdown
3. Interactive 3D model loads with orbit controls

## Software Stack

| Layer | Component | Technology |
|---|---|---|
| Presentation | Control app | C# WinForms (.NET 8.0) |
| Presentation | Camera preview | WebView2 / MJPEG |
| Presentation | 3D model viewer | HTML / Three.js |
| Communication | MQTT broker | Mosquitto |
| Communication | MQTT clients | MQTTnet v5 (C#) / paho-mqtt (Python) |
| Communication | File transfer | SSH / Paramiko |
| Capture | Camera control | Picamera2 / libcamera |
| Capture | Live streaming | Flask / JpegEncoder |
| Capture | Motor control | pigpio (hardware-timed waveforms) |
| Processing | 3D reconstruction | Meshroom 2025.1.0 / AliceVision |
| Processing | Format conversion | Blender 5.0 headless CLI |
| Integration | ROS2 bridge (optional) | rclpy / std_msgs |

## Hardware

| Component | Specification | Quantity |
|---|---|---|
| Raspberry Pi 4 | 4GB RAM | 3 |
| Pi Camera Module 3 | IMX708 sensor, 12MP, autofocus | 3 |
| Stepper Motor | NEMA 17 (17HS4401S), 1.8°/step | 1 |
| Microstep Driver | 1/32 microstepping (6400+ pulses/rev) | 1 |
| Network Switch | Ethernet, 5+ ports | 1 |
| Lightbox | White, diffused lighting | 1 |
| Host PC | Windows, NVIDIA GPU (for Meshroom CUDA) | 1 |

## Network Configuration

All devices are on the `192.168.137.x` subnet. The host PC acts as the network gateway at `.1`. Pis connect via Ethernet through the switch — no WiFi is used to ensure reliable, low-latency communication for the MQTT sync handshake during capture.

## Camera Views

Each Pi captures from a different angle:

- **Pi_1 (Middle View)** — eye-level, captures the side profile of the object
- **Pi_2 (Top-Down View)** — mounted above, captures the top surface
- **Pi_3 (Bottom-Up View)** — mounted below, captures the underside

The 7 view combinations give flexibility during reconstruction. Different objects produce better results with different angle combinations — flat objects may work best with `Top_Bottom`, while tall objects may benefit from `All` or `Middle_Bottom`.

## Capture Synchronization

The master-slave synchronization uses a handshake pattern:

1. Pi_1 publishes `host/capture_now` with the turn number
2. All 3 Pis take a picture (Pi_1 directly, Pi_2/Pi_3 via the MQTT message)
3. Each Pi publishes `Pi_X/capture_done = "1"` when the file is saved
4. Pi_1's `check_all_done()` sets a `threading.Event` when all 3 flags are `True`
5. `capture_lock.wait(timeout=30)` unblocks and the master rotates to the next position

This ensures no camera misses a shot because the turntable moved before all images were saved.
