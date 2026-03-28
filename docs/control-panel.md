# How the Control Panel and Transfer Script Work

## WinForms Control Panel

The control panel is a C# .NET application that serves as the single interface for the entire scanning pipeline. It runs on the Windows host PC and communicates with all 3 Pis through the Mosquitto MQTT broker running on localhost.

### Startup Sequence

On startup, the constructor initializes the UI components, loads camera URLs from a `config.json` file (falling back to hardcoded defaults), sets all 3 WebView2 controls to blank, locks the zoom comboboxes to dropdown-only mode, and sets them all to index 0. It then fires off three things concurrently: the MQTT client connection, the transfer script as a background process, and registers the `Form_Shown` event.

### MQTT Connection

The MQTT client uses MQTTnet v5 with `MqttClientFactory`. It connects to `localhost:1883` with a keep-alive of 30 seconds, retrying up to 10 times with 1 second between attempts. A `DisconnectedAsync` handler automatically reconnects every 2 seconds if the connection drops, unless the form is closing. All MQTT publishing goes through `PublishMessageAsync`, which checks `IsConnected` first and shows a message box if the broker isn't reachable.

### Auto-Start Preview

Once the form is visible, `Form_Shown` waits 2 seconds for MQTT to connect, then simulates a click on the Start Preview button. This publishes `host/startpreview = "1"` which tells all 3 Pis to start their cameras, then publishes `host/cam_zoom` with the current zoom levels from the comboboxes. The WebView2 controls load the Flask MJPEG stream URLs from each Pi. The button text changes to "Restart Preview" — clicking it again stops the streams, waits 1 second, then restarts everything, which is useful if a camera feed freezes.

### Zoom Control

The zoom comboboxes all share the same pattern. When any one changes, the handler reads all 3 current values, updates the zoom labels, builds a payload like `1_2,2_0,3_1`, and publishes it to `host/cam_zoom`. Each Pi parses out its own zoom level and applies it.

### Capture

When the user types an object name and presses Start Capture, the handler publishes `host/objectname` with the text, then `host/startcapture = "1"`. The button is disabled while the textbox is empty using `IsNullOrWhiteSpace` in the `TextChanged` event.

### Mesh Settings

The Start Capture tab includes a Mesh Settings panel with 7 radio buttons corresponding to the view combinations:

- **All** — all 3 camera angles (72 images)
- **Top Bottom** — top-down + bottom-up views (48 images)
- **Top Middle** — top-down + middle views (48 images)
- **Middle Bottom** — middle + bottom-up views (48 images)
- **Top** — top-down view only (24 images)
- **Middle** — middle view only (24 images)
- **Bottom** — bottom-up view only (24 images)

The user selects which combination to use before pressing Start Mesh. Different objects reconstruct better with different angle combinations.

### Meshroom Integration

The Start Mesh button reads the object name and the selected radio button to determine which image folder to use. It builds the input path from `ExtractedImgs/{objectname}/{combination}` and the output path under `MeshroomOutput/{objectname}/{combination}`, checks the input folder exists, then calls `RunMeshroom`.

`RunMeshroom` opens a new Form2 window for log output, then launches `meshroom_batch.exe` on a background thread via `Task.Run`. The process's stdout and stderr are piped to the log form through `AppendLog`, which uses `Invoke` to marshal text onto the UI thread. `WaitForExit` blocks the background thread until Meshroom truly finishes and all output buffers are flushed. Then it searches the output folder recursively for any `.obj` file.

### OBJ to GLB Conversion

If an `.obj` file is found, the app automatically calls `ConvertObjToGlb`, which writes a small Python script that imports the OBJ into Blender and exports as GLB. Blender 5.0 runs headlessly with `--background --python`, and the GLB is saved directly into the web viewer's folder with the name `{objectname}_{combination}.glb`. When Blender's process exits, the handler checks if the GLB file was created and shows a success or failure message.

### Form Closing

When the form closes, `Form1_FormClosing` cancels any pending MQTT operations, publishes `host/status = "shutdown"` so all Pis stop their cameras, disconnects and disposes the MQTT client, kills the transfer script process, then allows the form to close.

---

## Transfer Script

The transfer script is a standalone Python program that runs on the host PC as a background process launched by the WinForms app. Its job is to automatically download images from all 3 Pis after a capture sequence completes, organize them into useful combinations, and clean up the Pis.

### MQTT Listener

It connects to the Mosquitto broker on localhost as a separate MQTT client with the ID `TransferScript`. It subscribes to two topics: `host/objectname` to track what the current scan is called, and `Pi_1/CaptureTime` which is the signal that a capture sequence has finished.

When `host/objectname` arrives, it stores the name in a global variable. When `Pi_1/CaptureTime` arrives with the elapsed seconds, the script waits 2 seconds for all files to flush to disk on the Pis, then spawns the transfer in a daemon thread.

### Download Process

The transfer function SSHs into each Pi using Paramiko with hardcoded credentials. For each Pi, it connects to the IP address, navigates to the capture directory, finds the view-specific subfolder (like `MiddleView_shoe`), and downloads all JPEG files via SFTP to a local temp directory.

### Image Organization

After downloading from all 3 Pis, it organizes the images into 7 combination folders under `ExtractedImgs/{objectname}/`:

| Folder | Source Pis | Description |
|---|---|---|
| **All** | Pi_1 + Pi_2 + Pi_3 | All 3 views combined |
| **Top_Bottom** | Pi_2 + Pi_3 | Top-down and bottom-up views |
| **Top_Middle** | Pi_2 + Pi_1 | Top-down and middle views |
| **Middle_Bottom** | Pi_1 + Pi_3 | Middle and bottom-up views |
| **Top** | Pi_2 | Top-down view only |
| **Middle** | Pi_1 | Middle view only |
| **Bottom** | Pi_3 | Bottom-up view only |

For each combination, it copies the relevant files from the temp directory into the destination folder.

### Cleanup

Once organization is complete, it cleans up the temp directory, then SSHs back into each Pi and runs `rm -rf` on the view subfolder to free up space on the SD cards. The transfer function has a `transfer_in_progress` flag to prevent overlapping transfers if somehow two capture sequences finish in quick succession.

### Background Operation

The script uses `loop_forever` for its MQTT connection, which handles reconnection automatically. It runs silently in the background — the user never interacts with it directly. It just watches, downloads, organizes, and cleans up.
