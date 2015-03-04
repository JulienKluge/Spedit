using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spedit.UI;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Spedit.SPCondenser;
using Spedit.Interop;
using System.Reflection;

namespace Spedit
{
    public static class Program
    {
        public static MainWindow MainWindow;
        public static OptionsControl OptionsObject;
        //public static CondensedSourcepawnDefinition spDefinition;
        public static Config[] Configs;
        public static int SelectedConfig = 0;

        [STAThread]
        public static void Main()
        {
            bool mutexReserved;
            using (Mutex appMutex = new Mutex(true, "SpeditGlobalMutex", out mutexReserved))
            {
                if (mutexReserved)
                {
                    SplashScreen splashScreen = new SplashScreen("Res/Icon256x.png");
                    splashScreen.Show(false, true);
                    OptionsObject = OptionsControlIOObject.Load();
                    Configs = ConfigLoader.Load();
                    if (!OptionsObject.Program_UseHardwareAcceleration)
                    {
                        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
                    }
                    MainWindow = new MainWindow(splashScreen);
                    PipeInteropServer pipeServer = new PipeInteropServer(MainWindow);
                    pipeServer.Start();
                    Application app = new Application();
                    app.Run(MainWindow);
                    pipeServer.Close();
                    OptionsControlIOObject.Save();
                }
                else
                {
                    try
                    {
                        string[] args = Environment.GetCommandLineArgs();
                        StringBuilder sBuilder = new StringBuilder();
                        bool addedFiles = false;
                        for (int i = 0; i < args.Length; ++i)
                        {
                            if (!string.IsNullOrWhiteSpace(args[i]))
                            {
                                FileInfo fInfo = new FileInfo(args[i]);
                                if (fInfo.Exists && (fInfo.Extension == ".sp" || fInfo.Extension == ".inc"))
                                {
                                    addedFiles = true;
                                    sBuilder.Append(fInfo.FullName);
                                    if ((i + 1) != args.Length)
                                    { sBuilder.Append("|"); }
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
    }
}
