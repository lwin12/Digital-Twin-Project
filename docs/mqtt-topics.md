# MQTT Topic Reference

12 topics are used across the system. The Mosquitto broker runs on the Windows host PC at `192.168.137.1:1883`.

## host/startpreview

- **Published by:** WinForms app
- **Subscribed by:** Pi_1, Pi_2, Pi_3
- **Payload:** `"1"` to start cameras, `"0"` to stop
- **Purpose:** Starts or stops the MJPEG stream and camera on all Pis

## host/startcapture

- **Published by:** WinForms app
- **Subscribed by:** Pi_1 only
- **Payload:** `"1"`
- **Purpose:** Triggers the 24-turn capture sequence on the master. The master then coordinates the slaves via `host/capture_now`.

## host/objectname

- **Published by:** WinForms app
- **Subscribed by:** Pi_1, Pi_2, Pi_3, Transfer script
- **Payload:** Object name string, e.g. `"shoe"`
- **Purpose:** Sets the filename prefix and subfolder name for all captures

## host/cam_zoom

- **Published by:** WinForms app
- **Subscribed by:** Pi_1, Pi_2, Pi_3
- **Payload:** Comma-separated `CamID_ZoomLevel` pairs, e.g. `"1_2,2_0,3_2"`
- **Purpose:** Each Pi parses out its own zoom level and applies digital zoom via `ScalerCrop`

## host/status

- **Published by:** WinForms app
- **Subscribed by:** Pi_1, Pi_2, Pi_3
- **Payload:** `"shutdown"`
- **Purpose:** Tells all Pis to stop cameras when the app closes

## host/capture_now

- **Published by:** Pi_1 (master)
- **Subscribed by:** Pi_2, Pi_3
- **Payload:** Turn number, `"1"` through `"24"`
- **Purpose:** Commands slaves to take a picture at the current turntable position. Pi_1 takes its own picture directly after publishing this.

## host/focus_now

- **Published by:** Pi_1 (master)
- **Subscribed by:** Pi_2, Pi_3
- **Payload:** `"1"`
- **Purpose:** Tells slaves to lock macro autofocus on the nearest object before the capture loop begins

## Pi_1/capture_done

- **Published by:** Pi_1
- **Subscribed by:** Pi_1 (self-monitoring in sync check)
- **Payload:** `"1"`
- **Purpose:** Confirms Pi_1 has saved its image for the current turn

## Pi_2/capture_done

- **Published by:** Pi_2
- **Subscribed by:** Pi_1
- **Payload:** `"1"`
- **Purpose:** Confirms Pi_2 has saved its image. Master waits for this before rotating.

## Pi_3/capture_done

- **Published by:** Pi_3
- **Subscribed by:** Pi_1
- **Payload:** `"1"`
- **Purpose:** Confirms Pi_3 has saved its image. Master waits for this before rotating.

## Pi_1/Turn

- **Published by:** Pi_1
- **Subscribed by:** None currently
- **Payload:** Turn number, `"1"` through `"24"`
- **Purpose:** Reports the current turn number after all 3 Pis confirm capture. Available for monitoring.

## Pi_1/CaptureTime

- **Published by:** Pi_1
- **Subscribed by:** Transfer script
- **Payload:** Elapsed seconds as string, e.g. `"85.23"`
- **Purpose:** Signals that the capture sequence is complete. The transfer script uses this as the trigger to begin downloading images from all Pis.

## Topic Naming Note

`host/capture_now` and `host/focus_now` are published by Pi_1 (master) despite the `host/` prefix. This is a naming convention choice — they use `host/` because all Pis listen to `host/` topics, making it simpler than creating a separate prefix. Worth noting in case of confusion during debugging.
