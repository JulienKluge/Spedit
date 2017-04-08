using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Spedit
{
    public static class OptionsControlIoObject
    {
        public static void Save()
        {
            try
            {
                var formatter = new BinaryFormatter();

                using (var fileStream = new FileStream("options_0.dat", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    formatter.Serialize(fileStream, Program.OptionsObject);
                }

            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static OptionsControl Load(out bool programIsNew)
        {
            try
            {
                if (File.Exists("options_0.dat"))
                {
                    object deserializedOptionsObj;
                    var formatter = new BinaryFormatter();
                    using (
                        var fileStream = new FileStream("options_0.dat", FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite))
                    {
                        deserializedOptionsObj = formatter.Deserialize(fileStream);
                    }
                    var oc = (OptionsControl) deserializedOptionsObj;
                    oc.FillNullToDefaults();
                    programIsNew = false;
                    return oc;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var oco = new OptionsControl();
            oco.ReCreateCryptoKey();
#if DEBUG
            programIsNew = true;
#else
			ProgramIsNew = true;
#endif
            return oco;
        }
    }
}