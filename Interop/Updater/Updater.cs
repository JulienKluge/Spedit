using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;

namespace Spedit.Interop.Updater
{
    public class Updater
    {
        private bool InChecking = false;
        private bool UpdateAvailable = false;
        private string UpdateExecutable = string.Empty;
        private string UpdateInfo = string.Empty;

        public bool OverrideOptions = false;

        public bool UIFeedback = false;

        private Thread t;

        public void CheckForUpdatesAsynchronously()
        {
            if (Program.OptionsObject.Program_CheckForUpdates || OverrideOptions)
            {
                t = new Thread(new ThreadStart(Check));
                t.Start();
                InChecking = true;
            }
        }

        public void StopThreadAndCheckIfUpdateIsAvailable()
        {
            bool UpdateCheckPermission = Program.OptionsObject.Program_CheckForUpdates;
            UpdateCheckPermission |= OverrideOptions;
            if (InChecking) //time was not enough...
            {
                t.Abort();
            }
            else if (UpdateAvailable && UpdateCheckPermission && File.Exists(UpdateExecutable))
            {
                var result = MessageBox.Show("An update is available." + Environment.NewLine + "Do you want to update?" + Environment.NewLine + Environment.NewLine + UpdateInfo,
                        "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.Verb = "runas";
                            p.StartInfo.FileName = UpdateExecutable;
                            p.Start();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error while trying to update." + Environment.NewLine + "Details: " + e.Message + Environment.NewLine + "$$$" + e.StackTrace,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        public void Stop()
        {
            if (InChecking)
            {
                t.Abort();
            }
        }

        private void Check()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string versionString = client.DownloadString("http://updater.spedit.info/version_0.txt");
                    string[] versionLines = versionString.Split('\n');
                    string version = (versionLines[0].Trim()).Trim('\r');
                    if (version != Program.ProgramInternalVersion)
                    {
                        string destinationFileName = "updater_" + version + ".exe";
                        string destinationFile = Path.Combine(Environment.CurrentDirectory, destinationFileName);
                        if (File.Exists(destinationFile))
                        {
                            File.Delete(destinationFile);
                        }
                        client.DownloadFile("http://updater.spedit.info/" + destinationFileName, destinationFile);
                        StringBuilder updateInfoString = new StringBuilder();
                        if (versionLines.Length > 1)
                        {
                            for (int i = 1; i < versionLines.Length; ++i)
                            {
                                updateInfoString.AppendLine((versionLines[i].Trim()).Trim('\r'));
                            }
                        }
                        UpdateInfo = updateInfoString.ToString();
                        UpdateExecutable = destinationFile;
                        UpdateAvailable = true;
                        if (UIFeedback)
                        {
                            string FeedbackText;
                            if (UpdateAvailable)
                            {
                                FeedbackText = "An Updated is available." + Environment.NewLine + "Close Editor to install it!";
                            }
                            else
                            {
                                FeedbackText = "No Update is available yet.";
                            }
                            Program.MainWindow.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(Program.MainWindow, FeedbackText,
                                    "Update Check",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            });

                        }
                    }
                    else //use async-time to cleanup old updaters
                    {
                        string[] executables = Directory.GetFiles(Environment.CurrentDirectory, "*.exe", SearchOption.TopDirectoryOnly);
                        for (int i = 0; i < executables.Length; ++i)
                        {
                            FileInfo fInfo = new FileInfo(executables[i]);
                            if (fInfo.Name.StartsWith("updater"))
                            {
                                fInfo.Delete();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //nobody wants to know that...
            }
            InChecking = false;
        }
    }
}
