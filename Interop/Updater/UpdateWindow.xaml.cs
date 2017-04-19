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
    public partial class UpdateWindow
    {
        private readonly UpdateInfo _updateInfo;

        public bool Succeeded;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        public UpdateWindow(UpdateInfo info)
        {
            _updateInfo = info;
            InitializeComponent();
            DescriptionBox.Text = _updateInfo.Info;

            if (info.SkipDialog)
                StartUpdate();
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
            if (_updateInfo == null)
            {
                Close();
                return;
            }

            ActionYesButton.Visibility = Visibility.Hidden;
            ActionNoButton.Visibility = Visibility.Hidden;
            Progress.IsActive = true;
            MainLine.Text = "Updating to " + _updateInfo.UpdateStringVersion;
            SubLine.Text = "Downloading Updater";
            var t = new Thread(UpdateDownloadWorker);
            t.Start();
        }

        private void UpdateDownloadWorker()
        {
            if (File.Exists(_updateInfo.UpdaterFile))
                File.Delete(_updateInfo.UpdaterFile);

            try
            {
                using (var client = new WebClient())
                {
#if DEBUG
                    client.Credentials = new NetworkCredential("sm", "sm_pw"); //heuheu :D 
#endif
                    client.DownloadFile(_updateInfo.UpdaterDownloadUrl, _updateInfo.UpdaterFile);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while downloading the updater." + Environment.NewLine + "Details: " + e.Message + Environment.NewLine + "$$$" + e.StackTrace,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Dispatcher.Invoke(Close);
            }

            Thread.Sleep(100); //safety reasons
            Dispatcher.Invoke(FinalizeUpdate);
        }

        private void FinalizeUpdate()
        {
            SubLine.Text = "Starting Updater";
            UpdateLayout();

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.FileName = _updateInfo.UpdaterFile;
                    process.Start();
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
