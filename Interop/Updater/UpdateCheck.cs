using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Spedit.Interop.Updater
{
    public static class UpdateCheck
    {
        public static void Check(bool asynchronous)
        {
            if (Program.UpdateStatus != null)
                if (Program.UpdateStatus.IsAvailable)
                    return;

            if (asynchronous)
            {
                var thread = new Thread(CheckInternal);
                thread.Start();
            }
            else
            {
                CheckInternal();
            }
        }

        private static void CheckInternal()
        {
            var info = new UpdateInfo();

            try
            {
                using (var client = new WebClient())
                {
#if DEBUG
                    client.Credentials = new NetworkCredential("sm", "sm_pw"); //heuheu :D 
                    var versionString = client.DownloadString("ftp://127.0.0.1/version_0.txt");
#else
                    string versionString = client.DownloadString("https://updater.spedit.info/version_0.txt");
#endif
                    var versionLines = versionString.Split('\n');
                    var version = versionLines[0].Trim().Trim('\r');

                    if (version != Program.ProgramInternalVersion)
                    {
                        var destinationFileName = "updater_" + version + ".exe";
                        var destinationFile = Path.Combine(Environment.CurrentDirectory, destinationFileName);

                        info.IsAvailable = true;
                        info.UpdaterFile = destinationFile;
                        info.UpdaterFileName = destinationFileName;
#if DEBUG
                        info.UpdaterDownloadUrl = "ftp://127.0.0.1/" + destinationFileName;
#else
                        info.Updater_DownloadURL = "http://updater.spedit.info/" + destinationFileName;
#endif
                        info.UpdateVersion = version;
                        var updateInfoString = new StringBuilder();

                        if (versionLines.Length > 1)
                        {
                            info.UpdateStringVersion = versionLines[1];

                            for (var i = 1; i < versionLines.Length; ++i)
                                updateInfoString.AppendLine(versionLines[i].Trim().Trim('\r'));
                        }

                        info.Info = updateInfoString.ToString();
                    }
                    else
                    {
                        info.IsAvailable = false;
                    }
                }
            }
            catch (Exception e)
            {
                info.IsAvailable = false;
                info.GotException = true;
                info.ExceptionMessage = e.Message;
            }

            lock (Program.UpdateStatus) //since multiple checks can occur, we'll wont override another ones...
            {
                if (Program.UpdateStatus.WriteAble)
                    Program.UpdateStatus = info;
            }
        }
    }
}
