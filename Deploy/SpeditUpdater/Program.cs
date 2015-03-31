using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace SpeditUpdater
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Process[] p = Process.GetProcessesByName("Spedit.exe");
            if (p.Length > 0)
            {
                for (int i = 0; i < p.Length; ++i)
                {
                    try
                    {
                        p[i].WaitForExit();
                    }
                    catch (Exception) { }
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            UpdateMarquee um = new UpdateMarquee();
            um.Show();
            Application.DoEvents(); //execute Visual
            Thread t = new Thread(new ParameterizedThreadStart(Worker));
            t.Start(um);
            Application.Run();
        }

        private static void Worker(object arg)
        {
            UpdateMarquee um = (UpdateMarquee)arg;
            Thread.Sleep(500); //just for safety reasons and give the user visual feedback

            string zipFile = Path.Combine(Environment.CurrentDirectory, "updateZipFile.zip");

            byte[] zipFileContent = SpeditUpdater.Properties.Resources.spedit1_0_2_0Update;

            File.WriteAllBytes(zipFile, zipFileContent);

            FileInfo zipInfo = new FileInfo(zipFile);

            string extractPath = Environment.CurrentDirectory;
            using (ZipArchive archieve = ZipFile.OpenRead(zipInfo.FullName))
            {
                var entries = archieve.Entries;
                foreach (var entry in entries)
                {
                    string full = Path.Combine(extractPath, entry.FullName);
                    FileInfo fInfo = new FileInfo(full);
                    if (!Directory.Exists(fInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(fInfo.DirectoryName);
                    }
                    entry.ExtractToFile(fInfo.FullName, true);
                }
            }

            zipInfo.Delete();

            Environment.Exit(Environment.ExitCode);
        }
    }
}
