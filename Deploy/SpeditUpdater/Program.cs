using System;
using System.Threading;
using System.Diagnostics;
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
            var process = Process.GetProcessesByName("Spedit.exe");

            if (process.Length > 0)
            {
                foreach (var p in process)
                    try
                    {
                        p.WaitForExit();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            var um = new UpdateMarquee();
            um.Show();

            Application.DoEvents();

            var t = new Thread(Worker);
            t.Start(um);

            Application.Run(um);
        }

        private static void Worker(object arg)
        {
            var um = (UpdateMarquee) arg;
            var zipFile = Path.Combine(Environment.CurrentDirectory, "updateZipFile.zip");
            var zipFileContent = Properties.Resources.spedit1_2_0_1Update;

            File.WriteAllBytes(zipFile, zipFileContent);

            var zipInfo = new FileInfo(zipFile);
            var extractPath = Environment.CurrentDirectory;

            using (var archieve = ZipFile.OpenRead(zipInfo.FullName))
            {
                var entries = archieve.Entries;

                foreach (var entry in entries)
                {
                    var full = Path.Combine(extractPath, entry.FullName);
                    var fInfo = new FileInfo(full);

                    if (!Directory.Exists(fInfo.DirectoryName))
                        if (fInfo.DirectoryName != null)
                            Directory.CreateDirectory(fInfo.DirectoryName);

                    entry.ExtractToFile(fInfo.FullName, true);
                }
            }

            zipInfo.Delete();

            um.Invoke((InvokeDel) (() => { um.SetToReadyState(); }));
        }

        public delegate void InvokeDel();
    }
}
