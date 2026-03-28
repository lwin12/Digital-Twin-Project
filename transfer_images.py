"""
Transfer Script — Runs on Host PC alongside the Windows Control App.

Listens to MQTT for capture completion, then:
1. SSHs into each Pi and downloads images
2. Organizes into 4 combination folders
3. Deletes images and subfolders from the Pis

Requirements (install on Host PC):
    pip install paramiko paho-mqtt
"""

import os
import shutil
import threading
import time
import paramiko
import paho.mqtt.client as mqtt

# =========================================================================
# Configuration
# =========================================================================

MQTT_BROKER = "localhost"
MQTT_PORT = 1883

OUTPUT_BASE = r"C:\Users\lwiny\OneDrive\Desktop\ExtractedImgs"

# Pi credentials and paths
PI_CONFIG = {
    1: {
        "host": "192.168.137.50",
        "username": "lwin14",
        "password": "123",
        "capture_dir": "/home/lwin14/captures",
        "view_name": "MiddleView",
        "label": "Middle",
    },
    2: {
        "host": "192.168.137.30",
        "username": "lwin16",
        "password": "123",
        "capture_dir": "/home/lwin16/captures",
        "view_name": "TopDownView",
        "label": "Top",
    },
    3: {
        "host": "192.168.137.10",
        "username": "lwin15",
        "password": "123",
        "capture_dir": "/home/lwin15/captures",
        "view_name": "BottomUpView",
        "label": "Bottom",
    },
}

# Folder combinations: folder_name -> list of Pi IDs to include
FOLDER_COMBOS = {
    "Top_Bottom":    [2, 3],
    "Top_Middle":    [2, 1],
    "Middle_Bottom": [1, 3],
    "All":           [1, 2, 3],
    "Top":           [2],
    "Middle":        [1],
    "Bottom":        [3],
}

# =========================================================================
# State
# =========================================================================

object_name = "unnamed"
transfer_in_progress = False

# =========================================================================
# SSH / SCP Transfer Functions
# =========================================================================

def ssh_connect(pi_id):
    """Create and return an SSH client connected to the given Pi."""
    config = PI_CONFIG[pi_id]
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(
        hostname=config["host"],
        username=config["username"],
        password=config["password"],
        timeout=10,
    )
    print(f"  SSH connected to Pi_{pi_id} ({config['host']})")
    return ssh


def download_images(pi_id, obj_name, local_temp_dir):
    """
    Download all images from a Pi's view subfolder to a local temp directory.
    Returns the list of downloaded filenames, or empty list on failure.
    """
    config = PI_CONFIG[pi_id]
    remote_folder = f"{config['capture_dir']}/{config['view_name']}_{obj_name}"
    downloaded = []

    try:
        ssh = ssh_connect(pi_id)
        sftp = ssh.open_sftp()

        try:
            file_list = sftp.listdir(remote_folder)
        except FileNotFoundError:
            print(f"  WARNING: Remote folder not found on Pi_{pi_id}: {remote_folder}")
            sftp.close()
            ssh.close()
            return downloaded

        jpg_files = [f for f in file_list if f.lower().endswith(".jpg")]
        if not jpg_files:
            print(f"  WARNING: No .jpg files found on Pi_{pi_id} in {remote_folder}")
            sftp.close()
            ssh.close()
            return downloaded

        pi_temp = os.path.join(local_temp_dir, f"Pi_{pi_id}")
        os.makedirs(pi_temp, exist_ok=True)

        for filename in jpg_files:
            remote_path = f"{remote_folder}/{filename}"
            local_path = os.path.join(pi_temp, filename)
            sftp.get(remote_path, local_path)
            downloaded.append(filename)

        print(f"  Downloaded {len(downloaded)} images from Pi_{pi_id}")

        sftp.close()
        ssh.close()

    except Exception as e:
        print(f"  ERROR downloading from Pi_{pi_id}: {e}")

    return downloaded


def delete_remote_images(pi_id, obj_name):
    """Delete the view subfolder and its contents from the Pi."""
    config = PI_CONFIG[pi_id]
    remote_folder = f"{config['capture_dir']}/{config['view_name']}_{obj_name}"

    try:
        ssh = ssh_connect(pi_id)
        # Remove the entire subfolder
        stdin, stdout, stderr = ssh.exec_command(f"rm -rf {remote_folder}")
        stdout.read()  # wait for command to finish
        err = stderr.read().decode().strip()
        if err:
            print(f"  WARNING: Error deleting on Pi_{pi_id}: {err}")
        else:
            print(f"  Deleted {remote_folder} on Pi_{pi_id}")
        ssh.close()

    except Exception as e:
        print(f"  ERROR deleting from Pi_{pi_id}: {e}")


# =========================================================================
# Main Transfer Logic
# =========================================================================

def run_transfer(obj_name):
    """Full transfer pipeline: download, organize, cleanup."""
    global transfer_in_progress

    if transfer_in_progress:
        print("Transfer already in progress, skipping")
        return

    transfer_in_progress = True
    print(f"\n{'='*60}")
    print(f"TRANSFER STARTED for object: {obj_name}")
    print(f"{'='*60}")

    # Create a temp directory for downloads
    local_temp = os.path.join(OUTPUT_BASE, "_temp_transfer")
    os.makedirs(local_temp, exist_ok=True)

    # Step 1: Download from all Pis
    print("\n--- Step 1: Downloading images from all Pis ---")
    all_downloaded = {}
    for pi_id in PI_CONFIG:
        files = download_images(pi_id, obj_name, local_temp)
        all_downloaded[pi_id] = files

    total = sum(len(f) for f in all_downloaded.values())
    if total == 0:
        print("ERROR: No images downloaded from any Pi. Aborting transfer.")
        shutil.rmtree(local_temp, ignore_errors=True)
        transfer_in_progress = False
        return

    print(f"\nTotal images downloaded: {total}")

    # Step 2: Organize into combination folders
    print("\n--- Step 2: Organizing into folders ---")
    obj_output = os.path.join(OUTPUT_BASE, obj_name)

    for folder_name, pi_ids in FOLDER_COMBOS.items():
        dest_folder = os.path.join(obj_output, folder_name)
        os.makedirs(dest_folder, exist_ok=True)

        count = 0
        for pi_id in pi_ids:
            pi_temp = os.path.join(local_temp, f"Pi_{pi_id}")
            if not os.path.exists(pi_temp):
                continue
            for filename in all_downloaded.get(pi_id, []):
                src = os.path.join(pi_temp, filename)
                dst = os.path.join(dest_folder, filename)
                shutil.copy2(src, dst)
                count += 1

        print(f"  {folder_name}: {count} images")

    # Step 3: Clean up temp directory
    shutil.rmtree(local_temp, ignore_errors=True)

    # Step 4: Delete images from Pis
    print("\n--- Step 3: Deleting images from Pis ---")
    for pi_id in PI_CONFIG:
        if all_downloaded.get(pi_id):
            delete_remote_images(pi_id, obj_name)

    print(f"\n{'='*60}")
    print(f"TRANSFER COMPLETE for object: {obj_name}")
    print(f"  Output: {obj_output}")
    print(f"{'='*60}\n")

    transfer_in_progress = False


# =========================================================================
# MQTT Callbacks
# =========================================================================

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print("Transfer script connected to MQTT broker")
        client.subscribe("host/objectname")
        client.subscribe("Pi_1/CaptureTime")
    else:
        print(f"MQTT connection failed (rc={rc})")


def on_message(client, userdata, msg):
    global object_name
    topic = msg.topic
    payload = msg.payload.decode().strip()

    if topic == "host/objectname":
        object_name = payload
        print(f"Object name updated: {object_name}")

    elif topic == "Pi_1/CaptureTime":
        print(f"Capture completed in {payload}s — starting file transfer...")
        # Small delay to ensure all files are flushed to disk on the Pis
        time.sleep(2)
        # Run transfer in a separate thread to not block MQTT
        threading.Thread(
            target=run_transfer,
            args=(object_name,),
            daemon=True
        ).start()


def on_disconnect(client, userdata, rc, properties=None, reasonCode=None):
    print(f"Transfer script disconnected from MQTT (rc={rc})")


# =========================================================================
# Main
# =========================================================================

def main():
    print("="*60)
    print("Image Transfer Script")
    print(f"  Listening on MQTT broker: {MQTT_BROKER}:{MQTT_PORT}")
    print(f"  Output directory: {OUTPUT_BASE}")
    print("="*60)
    print("Waiting for capture to complete...\n")

    client = mqtt.Client(
        callback_api_version=mqtt.CallbackAPIVersion.VERSION2,
        client_id="TransferScript"
    )
    client.on_connect = on_connect
    client.on_message = on_message
    client.on_disconnect = on_disconnect

    # Keep trying to connect
    while True:
        try:
            client.connect(MQTT_BROKER, MQTT_PORT)
            break
        except Exception as e:
            print(f"Cannot connect to MQTT broker: {e}")
            print("Retrying in 3 seconds...")
            time.sleep(3)

    # loop_forever blocks and handles reconnection automatically
    client.loop_forever()


if __name__ == "__main__":
    main()