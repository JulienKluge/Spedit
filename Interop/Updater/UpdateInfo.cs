using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spedit.Interop.Updater
{
    public class UpdateInfo
    {
        public bool WriteAble = true;

        public bool IsAvailable = false;

        public bool SkipDialog = false;

        public bool GotException = false;
        public string ExceptionMessage = string.Empty;

        public string Updater_DownloadURL = string.Empty;
        public string Updater_FileName = string.Empty;
        public string Updater_File = string.Empty;

        public string Update_Version = string.Empty; //this version is used internally

        public string Update_StringVersion = string.Empty; //this is, what the user will see
        public string Update_Info = string.Empty;
    }
}
