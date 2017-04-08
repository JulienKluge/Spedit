namespace Spedit.Interop.Updater
{
    public class UpdateInfo
    {
        public bool WriteAble = true;
        public bool IsAvailable = false;
        public bool SkipDialog = false;
        public bool GotException = false;
        public string ExceptionMessage = string.Empty;
        public string UpdaterDownloadUrl = string.Empty;
        public string UpdaterFileName = string.Empty;
        public string UpdaterFile = string.Empty;
        public string UpdateVersion = string.Empty; //this version is used internally
        public string UpdateStringVersion = string.Empty; //this is, what the user will see
        public string Info = string.Empty;
    }
}
