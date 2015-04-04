using Spedit.Interop;
using Spedit.Interop.Updater;
using Spedit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Spedit
{
    public static class Program
    {
        public const string ProgramInternalVersion = "2";

        public static MainWindow MainWindow;
        public static OptionsControl OptionsObject;
        //public static CondensedSourcepawnDefinition spDefinition;
        public static Config[] Configs;
        public static Updater GlobalUpdater;
        public static int SelectedConfig = 0;

        [STAThread]
        public static void Main(string[] args)
        {
            bool InSafe = false;
            bool mutexReserved;
            using (Mutex appMutex = new Mutex(true, "SpeditGlobalMutex", out mutexReserved))
            {
                if (mutexReserved)
                {
#if !DEBUG
                    try
                    {
#endif
                        SplashScreen splashScreen = new SplashScreen("Resources/Icon256x.png");
                        splashScreen.Show(false, true);
                        Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        OptionsObject = OptionsControlIOObject.Load();
                        for (int i = 0; i < args.Length; ++i)
                        {
                            if (args[i].ToLowerInvariant() == "-rcck") //ReCreateCryptoKey
                            {
                                var opt = OptionsObject;
                                OptionsObject.ReCreateCryptoKey();
                                MessageBox.Show("All FTP passwords are now encrypted wrong!" + Environment.NewLine + "You have to replace them!",
                                    "Created new crypto key", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else if (args[i].ToLowerInvariant() == "-safe")
                            {
                                InSafe = true;
                            }
                        }
                        Configs = ConfigLoader.Load();
                        for (int i = 0; i < Configs.Length; ++i)
                        {
                            if (Configs[i].Name == OptionsObject.Program_SelectedConfig)
                            {
                                Program.SelectedConfig = i;
                                break;
                            }
                        }
                        if (!OptionsObject.Program_UseHardwareAcceleration)
                        {
                            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
                        }
                        MainWindow = new MainWindow(splashScreen);
                        PipeInteropServer pipeServer = new PipeInteropServer(MainWindow);
                        pipeServer.Start();
#if !DEBUG
                    }
                    catch (Exception e)
                    {
                        File.WriteAllText("CRASH_" + Environment.TickCount.ToString() + ".txt", BuildExceptionString(e, "SPEDIT LOADING"));
                        MessageBox.Show("An error occured while loading." + Environment.NewLine + "A crash report was written in the editor-directory.",
                            "Error while Loading",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Environment.Exit(Environment.ExitCode);
                    }
#endif
                        if (InSafe)
                        {
                            try
                            {
                                Application app = new Application();
#if !DEBUG
                                GlobalUpdater = new Updater();
                                GlobalUpdater.CheckForUpdatesAsynchronously();
#endif
                                object o = null;
                                o.ToString();
                                app.Run(MainWindow);
                                OptionsControlIOObject.Save();
#if !DEBUG
                                GlobalUpdater.StopThreadAndCheckIfUpdateIsAvailable();
#endif
                            }
                            catch (Exception e)
                            {
                                File.WriteAllText("CRASH_" + Environment.TickCount.ToString() + ".txt", BuildExceptionString(e, "SPEDIT MAIN"));
                                MessageBox.Show("An error occured." + Environment.NewLine + "A crash report was written in the editor-directory.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                Environment.Exit(Environment.ExitCode);
                            }
                        }
                        else
                        {
                            Application app = new Application();
#if !DEBUG
                            GlobalUpdater = new Updater();
                            GlobalUpdater.CheckForUpdatesAsynchronously();
#endif
                            app.Run(MainWindow);
                            OptionsControlIOObject.Save();
#if !DEBUG
                            GlobalUpdater.StopThreadAndCheckIfUpdateIsAvailable();
#endif
                        }
                }
                else
                {
                    try
                    {
                        StringBuilder sBuilder = new StringBuilder();
                        bool addedFiles = false;
                        for (int i = 0; i < args.Length; ++i)
                        {
                            if (!string.IsNullOrWhiteSpace(args[i]))
                            {
                                FileInfo fInfo = new FileInfo(args[i]);
                                if (fInfo.Exists)
                                {
                                    string ext = fInfo.Extension.ToLowerInvariant().Trim(new char[] { '.', ' ' });
                                    if (ext == "sp" || ext == "inc" || ext == "txt" || ext == "smx")
                                    {
                                        addedFiles = true;
                                        sBuilder.Append(fInfo.FullName);
                                        if ((i + 1) != args.Length)
                                        { sBuilder.Append("|"); }
                                    }
                                }
                            }
                        }
                        if (addedFiles)
                        { PipeInteropClient.ConnectToMasterPipeAndSendData(sBuilder.ToString()); }
                    }
                    catch (Exception) { } //dont fuck the user up with irrelevant data
                }
            }
        }

        private static string BuildExceptionString(Exception e, string SectionName)
        {
            StringBuilder outString = new StringBuilder();
            outString.AppendLine("Section: " + SectionName);
            outString.AppendLine(".NET Version: " + Environment.Version);
            outString.AppendLine("OS: " + Environment.OSVersion.VersionString);
            outString.AppendLine("64 bit OS: " + ((Environment.Is64BitOperatingSystem) ? "TRUE" : "FALSE"));
            outString.AppendLine("64 bit mode: " + ((Environment.Is64BitProcess) ? "TRUE" : "FALSE"));
            outString.AppendLine("Dir: " + Environment.CurrentDirectory);
            outString.AppendLine("Working Set: " + (Environment.WorkingSet / 1024).ToString() + " kb");
            outString.AppendLine("Installed UI Culture: " + System.Globalization.CultureInfo.InstalledUICulture ?? "null");
            outString.AppendLine("Current UI Culture: " + System.Globalization.CultureInfo.CurrentUICulture ?? "null");
            outString.AppendLine("Current Culture: " + System.Globalization.CultureInfo.CurrentCulture ?? "null");
            outString.AppendLine();
            Exception current = e;
            int eNumber = 1;
            for (; ; )
            {
                if (e == null)
                {
                    break;
                }
                outString.AppendLine("Exception " + eNumber.ToString());
                outString.AppendLine("Message:");
                outString.AppendLine(e.Message);
                outString.AppendLine("Stacktrace:");
                outString.AppendLine(e.StackTrace);
                outString.AppendLine("Source:");
                outString.AppendLine(e.Source ?? "null");
                outString.AppendLine("HResult Code:");
                outString.AppendLine(e.HResult.ToString());
                outString.AppendLine("Helplink:");
                outString.AppendLine(e.HelpLink ?? "null");
                if (e.TargetSite != null)
                {
                    outString.AppendLine("Targetsite Name:");
                    outString.AppendLine(e.TargetSite.Name ?? "null");
                }
                e = e.InnerException;
                eNumber++;
            }
            return (eNumber - 1).ToString() + Environment.NewLine + outString.ToString();
        }
    }
}
