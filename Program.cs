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
using static System.IO.Path;

namespace Spedit
{
    public static class Program
    {
        public const string ProgramInternalVersion = "11";

        public static MainWindow MainWindow;
        public static OptionsControl OptionsObject;
        public static TranslationProvider Translations;
        public static Config[] Configs;
        public static int SelectedConfig;
        public static UpdateInfo UpdateStatus;
        public static bool RCCKMade;

        [STAThread]
        public static void Main(string[] args)
        {
            var inSafe = false;
            bool mutexReserved;

            using (var appMutex = new Mutex(true, "SpeditGlobalMutex", out mutexReserved))
            {
                if (mutexReserved)
                {
                    bool programIsNew;
#if !DEBUG
                    try
                    {
#endif
                    var splashScreen = new SplashScreen("Resources/Icon256x.png");
                    splashScreen.Show(false, true);
                    Environment.CurrentDirectory = GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if !DEBUG
						ProfileOptimization.SetProfileRoot(Environment.CurrentDirectory);
						ProfileOptimization.StartProfile("Startup.Profile");
#endif
                    UpdateStatus = new UpdateInfo();
                    OptionsObject = OptionsControlIoObject.Load(out programIsNew);
                    Translations = new TranslationProvider();
                    Translations.LoadLanguage(OptionsObject.Language, true);

                    foreach (var str in args)
                        switch (str.ToLowerInvariant())
                        {
                            case "-rcck":
                                OptionsObject.ReCreateCryptoKey();
                                MakeRcckAlert();
                                break;
                            case "-safe":
                                inSafe = true;
                                break;
                            default:
                                // ignored
                                break;
                        }

                    Configs = ConfigLoader.Load();

                    for (var i = 0; i < Configs.Length; ++i)
                    {
                        if (Configs[i].Name != OptionsObject.ProgramSelectedConfig)
                            continue;

                        SelectedConfig = i;

                        break;
                    }

                    if (!OptionsObject.ProgramUseHardwareAcceleration)
                        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
#if !DEBUG
						if (ProgramIsNew)
						{
							if (Translations.AvailableLanguageIDs.Length > 0)
							{
								splashScreen.Close(new TimeSpan(0, 0, 1));
								var languageWindow = new UI.Interop.LanguageChooserWindow(Translations.AvailableLanguageIDs, Translations.AvailableLanguages);
								languageWindow.ShowDialog();
								string potentialSelectedLanguageID = languageWindow.SelectedID;
								if (!string.IsNullOrWhiteSpace(potentialSelectedLanguageID))
								{
									OptionsObject.Language = potentialSelectedLanguageID;
									Translations.LoadLanguage(potentialSelectedLanguageID);
								}
								splashScreen.Show(false, true);
							}
						}
#endif
                    MainWindow = new MainWindow(splashScreen);
                    var pipeServer = new PipeInteropServer(MainWindow);
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
                    var app = new Application();

                    if (inSafe)
                    {
                        try
                        {
#if !DEBUG
                            if (OptionsObject.Program_CheckForUpdates)
                            {
                                UpdateCheck.Check(true);
                            }
#endif
                            app.Startup += App_Startup;
                            app.Run(MainWindow);
                            OptionsControlIoObject.Save();
                        }
                        catch (Exception e)
                        {
                            File.WriteAllText("CRASH_" + Environment.TickCount + ".txt",
                                BuildExceptionString(e, "SPEDIT MAIN"));
                            MessageBox.Show(
                                "An error occured." + Environment.NewLine +
                                "A crash report was written in the editor-directory.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Environment.Exit(Environment.ExitCode);
                        }
                    }
                    else
                    {
#if !DEBUG
                        if (OptionsObject.Program_CheckForUpdates)
                        {
                            UpdateCheck.Check(true);
                        }
#endif
                        app.Run(MainWindow);
                        OptionsControlIoObject.Save();
                    }
                }
                else
                {
                    try
                    {
                        var sBuilder = new StringBuilder();
                        var addedFiles = false;

                        for (var i = 0; i < args.Length; ++i)
                        {
                            if (string.IsNullOrWhiteSpace(args[i]))
                                continue;

                            var fInfo = new FileInfo(args[i]);

                            if (!fInfo.Exists)
                                continue;

                            var ext = fInfo.Extension.ToLowerInvariant().Trim('.', ' ');

                            if (ext != "sp" && ext != "inc" && ext != "txt" && ext != "smx")
                                continue;

                            addedFiles = true;
                            sBuilder.Append(fInfo.FullName);

                            if (i + 1 != args.Length)
                                sBuilder.Append("|");
                        }

                        if (addedFiles)
                            PipeInteropClient.ConnectToMasterPipeAndSendData(sBuilder.ToString());
                    }
                    catch (Exception)
                    {
                        // ignored
                        // dont fuck the user up with irrelevant data
                    }
                }
            }
        }

        public static void MakeRcckAlert()
        {
            if (RCCKMade)
                return;

            RCCKMade = true;

            MessageBox.Show(
                "All FTP/RCon passwords are now encrypted wrong!" + Environment.NewLine + "You have to replace them!",
                "Created new crypto key", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ClearUpdateFiles()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.exe", SearchOption.TopDirectoryOnly);

            foreach (var t in files)
            {
                var fInfo = new FileInfo(t);

                if (fInfo.Name.StartsWith("updater_", StringComparison.CurrentCultureIgnoreCase))
                    fInfo.Delete();
            }
        }

        private static void App_Startup(object sender, StartupEventArgs e)
        {
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current,
                MahApps.Metro.ThemeManager.GetAccent("Green"),
                MahApps.Metro.ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1
        }

        private static string BuildExceptionString(Exception e, string sectionName)
        {
            var outString = new StringBuilder();
            var eNumber = 1;

            outString.AppendLine("Section: " + sectionName);
            outString.AppendLine(".NET Version: " + Environment.Version);
            outString.AppendLine("OS: " + Environment.OSVersion.VersionString);
            outString.AppendLine("64 bit OS: " + (Environment.Is64BitOperatingSystem ? "TRUE" : "FALSE"));
            outString.AppendLine("64 bit mode: " + (Environment.Is64BitProcess ? "TRUE" : "FALSE"));
            outString.AppendLine("Dir: " + Environment.CurrentDirectory);
            outString.AppendLine("Working Set: " + (Environment.WorkingSet / 1024) + " kb");
            outString.AppendLine("Installed UI Culture: " + System.Globalization.CultureInfo.InstalledUICulture);
            outString.AppendLine("Current UI Culture: " + System.Globalization.CultureInfo.CurrentUICulture);
            outString.AppendLine("Current Culture: " + System.Globalization.CultureInfo.CurrentCulture);
            outString.AppendLine();

            for (;;)
            {
                if (e == null)
                    break;

                outString.AppendLine("Exception " + eNumber);
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
                    outString.AppendLine(e.TargetSite.Name);
                }

                e = e.InnerException;
                eNumber++;
            }
            return eNumber - 1 + Environment.NewLine + outString;
        }
    }
}
