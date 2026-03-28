using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using MQTTnet;

namespace Control_App
{
    public partial class Form1 : Form
    {
        private bool PreviewStarted = false;
        private bool isClosing = false;

        private IMqttClient mqttClient;
        private CancellationTokenSource mqttCts;
        private readonly SemaphoreSlim mqttLock = new SemaphoreSlim(1, 1);

        private Form2 meshroomLogForm;

        // --- Transfer script process ---
        private Process transferProcess;
        private string TransferScriptPath = @"C:\Users\lwiny\OneDrive\Desktop\ExtractedImgs\transfer_images.py";

        // --- Configuration (loaded from config.json or defaults) ---
        private string Cam1Url = "http://192.168.137.50:7123";
        private string Cam2Url = "http://192.168.137.10:7123";
        private string Cam3Url = "http://192.168.137.30:7123";

        // --- Meshroom paths ---
        private string MeshroomExe = @"C:\Users\lwiny\OneDrive\Desktop\Meshroom\meshroom_batch.exe";
        private string MeshroomPipeline = @"C:\Users\lwiny\OneDrive\Desktop\Meshroom\Pipelines\Turntable_PipeLine.mg";
        private string MeshroomInputBase = @"C:\Users\lwiny\OneDrive\Desktop\ExtractedImgs\captures";
        private string MeshroomOutputBase = @"C:\Users\lwiny\OneDrive\Desktop\MeshroomOutput";
        private string OUTPUT_BASE = @"C:\Users\lwiny\OneDrive\Desktop\ExtractedImgs";


        public Form1()
        {
            InitializeComponent();
            LoadConfig();

            webViewCAM1.Source = new Uri("about:blank");
            webViewCAM2.Source = new Uri("about:blank");
            webViewCAM3.Source = new Uri("about:blank");

            CAM1_ComboBox_ZoomSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            CAM2_ComboBox_ZoomSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            CAM3_ComboBox_ZoomSelect.DropDownStyle = ComboBoxStyle.DropDownList;

            CAM1_ComboBox_ZoomSelect.SelectedIndex = 0;
            CAM2_ComboBox_ZoomSelect.SelectedIndex = 0;
            CAM3_ComboBox_ZoomSelect.SelectedIndex = 0;

            this.Shown += Form1_Shown;

            _ = ConnectMqttClientAsync();

            // Start the transfer script in the background
            StartTransferScript();
        }

        // =====================================================================
        // Transfer Script Management
        // =====================================================================

        private void StartTransferScript()
        {
            try
            {
                if (!File.Exists(TransferScriptPath))
                {
                    Debug.WriteLine($"Transfer script not found at: {TransferScriptPath}");
                    return;
                }

                transferProcess = new Process();
                transferProcess.StartInfo.FileName = @"C:\Users\lwiny\AppData\Local\Programs\Python\Python314\python.exe";
                transferProcess.StartInfo.Arguments = $"\"{TransferScriptPath}\"";
                transferProcess.StartInfo.CreateNoWindow = true;
                transferProcess.StartInfo.UseShellExecute = false;
                transferProcess.Start();

                Debug.WriteLine("Transfer script started successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start transfer script: {ex.Message}");
            }
        }

        private void StopTransferScript()
        {
            try
            {
                if (transferProcess != null && !transferProcess.HasExited)
                {
                    transferProcess.Kill();
                    transferProcess.Dispose();
                    Debug.WriteLine("Transfer script stopped");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping transfer script: {ex.Message}");
            }
            finally
            {
                transferProcess = null;
            }
        }

        // =====================================================================
        // Configuration
        // =====================================================================

        private void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        Cam1Url = config.Cam1Url ?? Cam1Url;
                        Cam2Url = config.Cam2Url ?? Cam2Url;
                        Cam3Url = config.Cam3Url ?? Cam3Url;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Config load failed, using defaults: {ex.Message}");
            }
        }

        // =====================================================================
        // Auto-Start Preview
        // =====================================================================

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await Task.Delay(2000);
            BT_START_STOP_Preview_Click(sender, e);
        }

        // =====================================================================
        // Preview Control
        // =====================================================================

        private async void BT_START_STOP_Preview_Click(object sender, EventArgs e)
        {
            BT_Start_Stop_Preview.Enabled = false;
            try
            {
                if (!PreviewStarted)
                {
                    webViewCAM1.Source = new Uri(Cam1Url);
                    webViewCAM2.Source = new Uri(Cam2Url);
                    webViewCAM3.Source = new Uri(Cam3Url);
                    BT_Start_Stop_Preview.Text = "Restart Preview";
                    PreviewStarted = true;

                    string CAM1zoomLevel = "1_" + CAM1_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string CAM2zoomLevel = "2_" + CAM2_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string CAM3zoomLevel = "3_" + CAM3_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string combinedZoomLevels = CAM1zoomLevel + "," + CAM2zoomLevel + "," + CAM3zoomLevel;

                    await PublishMessageAsync("host/startpreview", "1");
                    await PublishMessageAsync("host/cam_zoom", combinedZoomLevels);
                }
                else
                {
                    webViewCAM1.Source = new Uri("about:blank");
                    webViewCAM2.Source = new Uri("about:blank");
                    webViewCAM3.Source = new Uri("about:blank");
                    await PublishMessageAsync("host/startpreview", "0");

                    await Task.Delay(1000);

                    webViewCAM1.Source = new Uri(Cam1Url);
                    webViewCAM2.Source = new Uri(Cam2Url);
                    webViewCAM3.Source = new Uri(Cam3Url);

                    string CAM1zoomLevel = "1_" + CAM1_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string CAM2zoomLevel = "2_" + CAM2_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string CAM3zoomLevel = "3_" + CAM3_ComboBox_ZoomSelect.SelectedItem.ToString().Last();
                    string combinedZoomLevels = CAM1zoomLevel + "," + CAM2zoomLevel + "," + CAM3zoomLevel;

                    await PublishMessageAsync("host/startpreview", "1");
                    await PublishMessageAsync("host/cam_zoom", combinedZoomLevels);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Preview error:\n" + ex.Message);
            }
            finally
            {
                BT_Start_Stop_Preview.Enabled = true;
            }
        }

        // =====================================================================
        // Capture Control
        // =====================================================================

        private async void BT_Start_Capture_Click(object sender, EventArgs e)
        {
            await PublishMessageAsync("host/objectname", TB_Obj_Name.Text.Trim());
            await PublishMessageAsync("host/startcapture", "1");
        }

        // =====================================================================
        // Meshroom
        // =====================================================================

        private void RunMeshroom(string inputFolder, string outputFolder, string objectName, string orientation)
        {
            try
            {
                if (!File.Exists(MeshroomExe))
                {
                    MessageBox.Show("Meshroom not found at:\n" + MeshroomExe);
                    return;
                }

                Directory.CreateDirectory(outputFolder);

                meshroomLogForm = new Form2();
                meshroomLogForm.Text = "Meshroom Log";
                meshroomLogForm.Show();
                meshroomLogForm.AppendLog($"=== Meshroom started for '{objectName}' ({orientation}) ===");
                meshroomLogForm.AppendLog($"Input:  {inputFolder}");
                meshroomLogForm.AppendLog($"Output: {outputFolder}");
                meshroomLogForm.AppendLog("");

                BT_Start_Mesh.Enabled = false;

                Task.Run(() =>
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = MeshroomExe,
                            Arguments = $"-p \"{MeshroomPipeline}\" -i \"{inputFolder}\" -o \"{outputFolder}\" -v info",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        Process meshroom = new Process();
                        meshroom.StartInfo = psi;

                        meshroom.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data))
                                meshroomLogForm?.AppendLog(ev.Data);
                        };

                        meshroom.ErrorDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data))
                                meshroomLogForm?.AppendLog(ev.Data);
                        };

                        meshroom.Start();
                        meshroom.BeginOutputReadLine();
                        meshroom.BeginErrorReadLine();

                        meshroom.WaitForExit();

                        if (IsHandleCreated && !IsDisposed)
                        {
                            Invoke(new Action(() =>
                            {
                                meshroomLogForm?.AppendLog("");
                                meshroomLogForm?.AppendLog("=== Meshroom finished ===");

                                var objFiles = Directory.GetFiles(outputFolder, "*.obj", SearchOption.AllDirectories);
                                if (objFiles.Length > 0)
                                {
                                    string fileList = string.Join("\n", objFiles);
                                    meshroomLogForm?.AppendLog("Found model(s):\n" + fileList);
                                    meshroomLogForm?.AppendLog("");
                                    meshroomLogForm?.AppendLog("Starting OBJ to GLB conversion...");

                                    // Convert to GLB — saved to ExtractedImgs\{objectName}\{objectName}_{orientation}.glb
                                    ConvertObjToGlb(objFiles[0], objectName, orientation);
                                }
                                else
                                {
                                    BT_Start_Mesh.Enabled = true;
                                    meshroomLogForm?.AppendLog("WARNING: No .obj files found.");
                                    MessageBox.Show("Meshroom finished but no .obj model found.\nCheck: " + outputFolder);
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsHandleCreated && !IsDisposed)
                        {
                            Invoke(new Action(() =>
                            {
                                BT_Start_Mesh.Enabled = true;
                                meshroomLogForm?.AppendLog("ERROR: " + ex.Message);
                                MessageBox.Show("Meshroom error:\n" + ex.Message);
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start Meshroom:\n" + ex.Message);
            }
        }

        private void ConvertObjToGlb(string objPath, string objectName, string orientation)
        {
            try
            {
                // GLB saved directly to the web viewer folder
                string glbDir = @"C:\Users\lwiny\OneDrive\Desktop\Project4Website\myGLBviewer";
                Directory.CreateDirectory(glbDir);
                string glbFilename = $"{objectName}_{orientation}.glb";
                string glbPath = Path.Combine(glbDir, glbFilename);

                string blenderExe = @"C:\Program Files\Blender Foundation\Blender 5.0\blender.exe";

                // Use a temp folder for the Blender script so it doesn't clutter ExtractedImgs
                string scriptDir = Path.Combine(MeshroomOutputBase, objectName, orientation);
                Directory.CreateDirectory(scriptDir);
                string scriptPath = Path.Combine(scriptDir, "convert.py");

                string script = $@"
import bpy
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.wm.obj_import(filepath=r'{objPath.Replace("'", @"\'")}')
bpy.ops.export_scene.gltf(filepath=r'{glbPath.Replace("'", @"\'")}', export_format='GLB')
bpy.ops.wm.quit_blender()
";

                File.WriteAllText(scriptPath, script);

                meshroomLogForm?.AppendLog($"Converting with Blender: {objPath}");
                meshroomLogForm?.AppendLog($"Output GLB: {glbPath}");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = blenderExe,
                    Arguments = $"--background --python \"{scriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Process converter = new Process();
                converter.StartInfo = psi;

                converter.OutputDataReceived += (s, ev) =>
                {
                    if (!string.IsNullOrEmpty(ev.Data))
                        meshroomLogForm?.AppendLog(ev.Data);
                };

                converter.ErrorDataReceived += (s, ev) =>
                {
                    if (!string.IsNullOrEmpty(ev.Data))
                        meshroomLogForm?.AppendLog(ev.Data);
                };

                converter.EnableRaisingEvents = true;
                converter.Exited += (s, ev) =>
                {
                    if (IsHandleCreated && !IsDisposed)
                    {
                        Invoke(new Action(() =>
                        {
                            BT_Start_Mesh.Enabled = true;

                            // Clean up Blender script
                            try { File.Delete(scriptPath); } catch { }

                            if (File.Exists(glbPath))
                            {
                                meshroomLogForm?.AppendLog("");
                                meshroomLogForm?.AppendLog("=== GLB conversion complete ===");
                                meshroomLogForm?.AppendLog($"GLB saved: {glbPath}");
                                MessageBox.Show($"All done!\n\nGLB file:\n{glbPath}");
                            }
                            else
                            {
                                meshroomLogForm?.AppendLog("WARNING: GLB conversion failed.");
                                MessageBox.Show("GLB conversion failed.\nCheck the log for details.");
                            }
                        }));
                    }
                };

                converter.Start();
                converter.BeginOutputReadLine();
                converter.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                BT_Start_Mesh.Enabled = true;
                meshroomLogForm?.AppendLog("ERROR: " + ex.Message);
                MessageBox.Show("Failed to convert to GLB:\n" + ex.Message);
            }
        }

        private void BT_Start_Mesh_Click(object sender, EventArgs e)
        {
            string objectName = TB_Obj_Name.Text.Trim();
            if (string.IsNullOrWhiteSpace(objectName))
            {
                MessageBox.Show("Please enter an object name first.");
                return;
            }

            // Determine input subfolder and orientation label based on selected radio button
            string subFolder;
            if (RB_ALL.Checked)
                subFolder = "All";
            else if (RB_TOPBOTTOM.Checked)
                subFolder = "Top_Bottom";
            else if (RB_MIDDLEBOTTOM.Checked)
                subFolder = "Middle_Bottom";
            else if (RB_TOPMIDDLE.Checked)
                subFolder = "Top_Middle";
            else if (RB_TOP.Checked)
                subFolder = "Top";
            else if (RB_MIDDLE.Checked)
                subFolder = "Middle";
            else if (RB_BOTTOM.Checked)
                subFolder = "Bottom";
            else
            {
                MessageBox.Show("Please select a mesh setting first.");
                return;
            }

            string inputFolder = Path.Combine(OUTPUT_BASE, objectName, subFolder);
            string outputFolder = Path.Combine(MeshroomOutputBase, objectName, subFolder);

            if (!Directory.Exists(inputFolder))
            {
                MessageBox.Show($"Input folder not found:\n{inputFolder}\n\nRun a capture first.");
                return;
            }

            RunMeshroom(inputFolder, outputFolder, objectName, subFolder);
        }

        // =====================================================================
        // MQTT Client
        // =====================================================================

        private async Task ConnectMqttClientAsync()
        {
            mqttCts = new CancellationTokenSource();

            mqttClient = new MqttClientFactory().CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .WithClientId("WinFormsApp")
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();

            mqttClient.DisconnectedAsync += async e =>
            {
                if (isClosing) return;
                await Task.Delay(2000);
                try
                {
                    await mqttClient.ConnectAsync(options);
                }
                catch { }
            };

            int retries = 10;
            while (retries-- > 0)
            {
                mqttCts.Token.ThrowIfCancellationRequested();
                try
                {
                    await mqttClient.ConnectAsync(options, mqttCts.Token);
                    return;
                }
                catch (OperationCanceledException) { throw; }
                catch when (retries > 0)
                {
                    await Task.Delay(1000, mqttCts.Token);
                }
            }
        }

        private async Task PublishMessageAsync(string topic, string message)
        {
            if (mqttClient == null || !mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT client is not connected.\nMake sure Mosquitto is running.");
                return;
            }

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            await mqttClient.PublishAsync(mqttMessage);
        }

        // =====================================================================
        // Form Lifecycle
        // =====================================================================

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isClosing) return;

            e.Cancel = true;
            isClosing = true;

            mqttCts?.Cancel();

            try
            {
                if (mqttClient != null && mqttClient.IsConnected)
                {
                    await PublishMessageAsync("host/status", "shutdown");
                    await Task.Delay(200);
                    await mqttClient.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MQTT disconnect error: {ex.Message}");
            }

            mqttClient?.Dispose();
            mqttClient = null;
            mqttCts?.Dispose();
            mqttCts = null;

            // Stop the transfer script
            StopTransferScript();

            Close();
        }

        // =====================================================================
        // Zoom Controls
        // =====================================================================

        private void CAM1_Zoom_Curr_Click(object sender, EventArgs e)
        {
        }

        private async void CAM1_ComboBox_ZoomSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cam1Zoom = Math.Max(0, CAM1_ComboBox_ZoomSelect.SelectedIndex);
            int cam2Zoom = Math.Max(0, CAM2_ComboBox_ZoomSelect.SelectedIndex);
            int cam3Zoom = Math.Max(0, CAM3_ComboBox_ZoomSelect.SelectedIndex);

            CAM1_Zoom_Curr.Text = "CAM1 Zoom : " + cam1Zoom.ToString();
            CAM2_Zoom_Curr.Text = "CAM2 Zoom : " + cam2Zoom.ToString();
            CAM3_Zoom_Curr.Text = "CAM3 Zoom : " + cam3Zoom.ToString();

            string payload = $"1_{cam1Zoom},2_{cam2Zoom},3_{cam3Zoom}";

            if (PreviewStarted)
            {
                await PublishMessageAsync("host/cam_zoom", payload);
            }
        }

        private async void CAM2_ComboBox_ZoomSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cam1Zoom = Math.Max(0, CAM1_ComboBox_ZoomSelect.SelectedIndex);
            int cam2Zoom = Math.Max(0, CAM2_ComboBox_ZoomSelect.SelectedIndex);
            int cam3Zoom = Math.Max(0, CAM3_ComboBox_ZoomSelect.SelectedIndex);

            CAM1_Zoom_Curr.Text = "CAM1 Zoom : " + cam1Zoom.ToString();
            CAM2_Zoom_Curr.Text = "CAM2 Zoom : " + cam2Zoom.ToString();
            CAM3_Zoom_Curr.Text = "CAM3 Zoom : " + cam3Zoom.ToString();

            string payload = $"1_{cam1Zoom},2_{cam2Zoom},3_{cam3Zoom}";

            if (PreviewStarted)
            {
                await PublishMessageAsync("host/cam_zoom", payload);
            }
        }

        private async void CAM3_ComboBox_ZoomSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cam1Zoom = Math.Max(0, CAM1_ComboBox_ZoomSelect.SelectedIndex);
            int cam2Zoom = Math.Max(0, CAM2_ComboBox_ZoomSelect.SelectedIndex);
            int cam3Zoom = Math.Max(0, CAM3_ComboBox_ZoomSelect.SelectedIndex);

            CAM1_Zoom_Curr.Text = "CAM1 Zoom : " + cam1Zoom.ToString();
            CAM2_Zoom_Curr.Text = "CAM2 Zoom : " + cam2Zoom.ToString();
            CAM3_Zoom_Curr.Text = "CAM3 Zoom : " + cam3Zoom.ToString();

            string payload = $"1_{cam1Zoom},2_{cam2Zoom},3_{cam3Zoom}";

            if (PreviewStarted)
            {
                await PublishMessageAsync("host/cam_zoom", payload);
            }
        }

        // =====================================================================
        // Object Name
        // =====================================================================

        private void TB_Obj_Name_TextChanged(object sender, EventArgs e)
        {
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            BT_Start_Capture.Enabled = !string.IsNullOrWhiteSpace(TB_Obj_Name.Text);
        }

        private void TB_Obj_Name_TextChanged_1(object sender, EventArgs e)
        {
            BT_Start_Capture.Enabled = !string.IsNullOrWhiteSpace(TB_Obj_Name.Text);
        }
    }

    // =========================================================================
    // Config Model
    // =========================================================================

    public class AppConfig
    {
        public string Cam1Url { get; set; }
        public string Cam2Url { get; set; }
        public string Cam3Url { get; set; }
    }
}