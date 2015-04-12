using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Spedit.Interop.Updater
{
    public static class UpdateCheck
    {
        public static void Check(bool Asynchronous)
        {
            if (Program.UpdateStatus != null)
            {
                if (Program.UpdateStatus.IsAvailable)
                {
                    return;
                }
            }
            if (Asynchronous)
            {
                Thread t = new Thread(new ThreadStart(CheckInternal));
                t.Start();
            }
            else
            {
                CheckInternal();
            }
        }

        private static void CheckInternal()
        {
            UpdateInfo info = new UpdateInfo();
            try
            {
                using (WebClient client = new WebClient())
                {
#if DEBUG
                    client.Credentials = new NetworkCredential("sm", "sm_pw"); //heuheu :D 
                    string versionString = client.DownloadString("ftp://127.0.0.1/version_0.txt");
#else
                    string versionString = client.DownloadString("http://updater.spedit.info/version_0.txt");
#endif
                    string[] versionLines = versionString.Split('\n');
                    string version = (versionLines[0].Trim()).Trim('\r');
                    if (version != Program.ProgramInternalVersion)
                    {
                        string destinationFileName = "updater_" + version + ".exe";
                        string destinationFile = Path.Combine(Environment.CurrentDirectory, destinationFileName);
                        info.IsAvailable = true;
                        info.Updater_File = destinationFile;
                        info.Updater_FileName = destinationFileName;
#if DEBUG
                        info.Updater_DownloadURL = "ftp://127.0.0.1/" + destinationFileName;
#else
                        info.Updater_DownloadURL = "http://updater.spedit.info/" + destinationFileName;
#endif
                        info.Update_Version = version;
                        StringBuilder updateInfoString = new StringBuilder();
                        if (versionLines.Length > 1)
                        {
                            info.Update_StringVersion = versionLines[1];
                            for (int i = 1; i < versionLines.Length; ++i)
                            {
                                updateInfoString.AppendLine((versionLines[i].Trim()).Trim('\r'));
                            }
                        }
                        info.Update_Info = updateInfoString.ToString();
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
                {
                    Program.UpdateStatus = info;
                }
            }
        }
    }
}
