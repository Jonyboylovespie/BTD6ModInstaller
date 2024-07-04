using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.Win32;

namespace ModInstaller
{
    public class MainForm : Form
    {
        private ComboBox cboGamePaths;
        private Button btnSearch;
        private Button btnInstallMods;

        public MainForm()
        {
            cboGamePaths = new ComboBox { Left = 10, Top = 10, Width = 570 };
            btnSearch = new Button { Text = "Search for BloonsTD6 Installations", Left = 10, Top = 40, Width = 200};
            btnInstallMods = new Button { Text = "Install Mods", Left = 420, Top = 40 };

            btnSearch.Click += BtnSearch_Click;
            btnInstallMods.Click += BtnInstallMods_Click;

            Controls.Add(cboGamePaths);
            Controls.Add(btnSearch);
            Controls.Add(btnInstallMods);

            Text = "Mod Installer";
            Size = new System.Drawing.Size(600, 120);

            Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckAndInstallDotNet6();
        }

        private void CheckAndInstallDotNet6()
        {
            try
            {
                if (!IsNet6DesktopRuntimeInstalled())
                {
                    InstallDotNet6();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking .NET version: {ex.Message}");
            }
        }
        
        static bool IsNet6DesktopRuntimeInstalled()
        {
            const string net6RegistryKey = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost";
            using (var key = Registry.LocalMachine.OpenSubKey(net6RegistryKey))
            {
                Console.WriteLine(key);
                return key != null;
            }
        }

        private void InstallDotNet6()
        {
            var dotNetInstallerUrl = "https://github.com/Jonyboylovespie/BTD6ModInstaller/raw/master/BTD6Mods/dotnet6-runtime-installer.exe";
            var installerPath = Path.Combine(AppContext.BaseDirectory, "dotnet6-runtime-installer.exe");
            using (var client = new WebClient())
            {
                client.DownloadFile(dotNetInstallerUrl, installerPath);
            }
            MessageBox.Show("After clicking OK, please proceed to install dotnet6, once finished, click close");
            var process = new Process();
            process.StartInfo.FileName = installerPath;
            process.Start();
            process.WaitForExit();
            File.Delete(installerPath);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            cboGamePaths.Items.Clear();
            foreach (var driveInfo in DriveInfo.GetDrives())
            {
                try
                {
                    string steamCommonPath = Path.Combine(driveInfo.Name, "Program Files (x86)", "Steam", "steamapps", "common");
                    if (Directory.Exists(steamCommonPath))
                    {
                        foreach (var bloonsTD6Path in Directory.GetFiles(steamCommonPath, "BloonsTD6.exe", SearchOption.AllDirectories))
                        {
                            if (!string.IsNullOrEmpty(bloonsTD6Path))
                            {
                                cboGamePaths.Items.Add(bloonsTD6Path);
                            }
                        }
                    }
                    string epicCommonPath = Path.Combine(driveInfo.Name, "Program Files", "Epic Games");
                    if (Directory.Exists(epicCommonPath))
                    {
                        foreach (var bloonsTD6Path in Directory.GetFiles(epicCommonPath, "BloonsTD6.exe", SearchOption.AllDirectories))
                        {
                            if (!string.IsNullOrEmpty(bloonsTD6Path))
                            {
                                cboGamePaths.Items.Add(bloonsTD6Path);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching on drive {driveInfo.Name}: {ex.Message}");
                }
            }

            if (cboGamePaths.Items.Count == 0)
            {
                MessageBox.Show("No Bloons TD 6 installations found. Please select the game path manually.", "Installation Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSearch.Enabled = false;
                cboGamePaths.Enabled = false;
            }
            else
            {
                cboGamePaths.SelectedIndex = 0;
                btnSearch.Enabled = false;
            }
        }

        private void BtnInstallMods_Click(object sender, EventArgs e)
        {
            if (cboGamePaths.SelectedItem == null)
            {
                MessageBox.Show("Please select a game installation path first.", "No Installation Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string gamePath = Path.GetDirectoryName(cboGamePaths.SelectedItem.ToString());
            if (Directory.Exists(gamePath))
            {
                foreach (var folder in Directory.GetDirectories(gamePath))
                {
                    if (folder == Path.Combine(gamePath, "MelonLoader"))
                    {
                        MessageBox.Show("Mods already installed, if not, verify game files and try again");
                        return;
                    }
                }
                try
                {
                    string melonloaderDownload = "https://github.com/LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip";
                    string melonZipPath = Path.Combine(gamePath, "melonloader.zip");
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(melonloaderDownload, melonZipPath);
                    }
                    ZipFile.ExtractToDirectory(melonZipPath, gamePath);
                    File.Delete(melonZipPath);
                    string modHelperDownload = "https://github.com/gurrenm3/BTD-Mod-Helper/releases/latest/download/Btd6ModHelper.dll";
                    string modsPath = Path.Combine(gamePath, "Mods");
                    Directory.CreateDirectory(modsPath);
                    string modHelperPath = Path.Combine(modsPath, "Btd6ModHelper.dll");
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(modHelperDownload, modHelperPath);
                    }
                    if (Directory.GetFiles(gamePath).Contains(Path.Combine(gamePath, "EOSBootstrapper.exe")))
                    {
                        string epicCompatDownload = "https://github.com/GrahamKracker/BTD6EpicGamesModCompat/releases/latest/download/BTD6EpicGamesModCompat.dll";
                        string pluginsPath = Path.Combine(gamePath, "Plugins");
                        Directory.CreateDirectory(pluginsPath);
                        string epicCompatPath = Path.Combine(pluginsPath, "BTD6EpicGamesModCompat.dll");
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(epicCompatDownload, epicCompatPath);
                        }
                    }
                    MessageBox.Show("Mods installed successfully!");
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
            }
            else
            {
                MessageBox.Show("Invalid game path!");
            }
        }
    }
}
