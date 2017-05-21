using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.Windows;

namespace Spedit.Interop.Updater
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        private UpdateInfo updateInfo;

        public bool Succeeded = false;

        public UpdateWindow()
        {
            InitializeComponent();
        }
        public UpdateWindow(UpdateInfo info)
        {
            updateInfo = info;
            InitializeComponent();
            DescriptionBox.Text = updateInfo.Update_Info;
            if (info.SkipDialog)
            {
                StartUpdate();
            }
        }

        private void ActionYesButton_Click(object sender, RoutedEventArgs e)
        {
            StartUpdate();
        }

        private void ActionNoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StartUpdate()
        {
            if (updateInfo == null)
            {
                Close();
                return;
            }
            ActionYesButton.Visibility = System.Windows.Visibility.Hidden;
            ActionNoButton.Visibility = System.Windows.Visibility.Hidden;
            Progress.IsActive = true;
            MainLine.Text = "Updating to " + updateInfo.Update_StringVersion;
            SubLine.Text = "Downloading Updater";
            Thread t = new Thread(new ThreadStart(UpdateDownloadWorker));
            t.Start();
        }

        private void UpdateDownloadWorker()
        {
            if (File.Exists(updateInfo.Updater_File))
            {
                File.Delete(updateInfo.Updater_File);
            }
            try
            {
                using (WebClient client = new WebClient())
                {
#if DEBUG
                    client.Credentials = new NetworkCredential("sm", "sm_pw"); //heuheu :D 
#endif
                    client.DownloadFile(updateInfo.Updater_DownloadURL, updateInfo.Updater_File);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while downloading the updater." + Environment.NewLine + "Details: " + e.Message + Environment.NewLine + "$$$" + e.StackTrace,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Dispatcher.Invoke(() =>
                {
                    Close();
                });
            }
            Thread.Sleep(100); //safety reasons
            this.Dispatcher.Invoke(() =>
            {
                FinalizeUpdate();
            });
        }

        private void FinalizeUpdate()
        {
            SubLine.Text = "Starting Updater";
            this.UpdateLayout();
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.FileName = updateInfo.Updater_File;
                    p.Start();
                }
                Succeeded = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while trying to start the updater." + Environment.NewLine + "Details: " + e.Message + Environment.NewLine + "$$$" + e.StackTrace,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            Close();
        }
    }
}
