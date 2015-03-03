using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Reflection;

namespace Spedit //leave this here instead of .Interop because of reasons...
{
    [Serializable]
    public class OptionsControl
    {
        public static int SVersion = 0;
        public int Version = 0;

        public bool Program_UseHardwareAcceleration = true;

        public bool Program_OpenCustomIncludes = false;
        public bool Program_OpenIncludesRecursively = false;

        public bool UI_Animations = true;

        public bool Editor_WordWrap = false;
        public double Editor_FontSize = 16.0;
        public string Editor_FontFamily = "consolas";

        public string[] LastOpenFiles = new string[0];

        public string SPIncludePath = "sourcepawn\\";
        public string SPCopyPath = "";
        public string ServerPath = "";
        public string ServerArgs = "";
        public int OptimizationLevel = 2;
        public int VerboseLevel = 1;

        public SerializeableColor SH_Comments = new SerializeableColor(0xFF, 0x00, 0x80, 0x00);
        public SerializeableColor SH_CommentsMarker = new SerializeableColor(0xFF, 0xD0, 0x20, 0x20);
        public SerializeableColor SH_Strings = new SerializeableColor(0xFF, 0xC8, 0x00, 0x00);
        public SerializeableColor SH_PreProcessor = new SerializeableColor(0xFF, 0x00, 0x00, 0xD0);
        public SerializeableColor SH_Types = new SerializeableColor(0xFF, 0x28, 0x90, 0xB0);
        public SerializeableColor SH_TypesValues = new SerializeableColor(0xFF, 0x00, 0x55, 0xC0);
        public SerializeableColor SH_Keywords = new SerializeableColor(0xFF, 0x00, 0x00, 0xFF);
        public SerializeableColor SH_ContextKeywords = new SerializeableColor(0xFF, 0x00, 0x00, 0xFF);
        public SerializeableColor SH_Chars = new SerializeableColor(0xFF, 0xC8, 0x00, 0x40);
        public SerializeableColor SH_UnkownFunctions = new SerializeableColor(0xFF, 0x00, 0x40, 0xE8);
        public SerializeableColor SH_Numbers = new SerializeableColor(0xFF, 0x00, 0x8B, 0x8B);
        public SerializeableColor SH_SpecialCharacters = new SerializeableColor(0xFF, 0x50, 0x50, 0x50);
        public SerializeableColor SH_Deprecated = new SerializeableColor(0xFF, 0xFF, 0x00, 0x00);
        public SerializeableColor SH_Constants = new SerializeableColor(0xFF, 0x80, 0x00, 0xFF);
        public SerializeableColor SH_Functions = new SerializeableColor(0xFF, 0x00, 0x00, 0xFF);
        public SerializeableColor SH_Methods = new SerializeableColor(0xFF, 0x44, 0x6A, 0xCC);

        public void FillNullToDefaults()
        {
            if (OptionsControl.SVersion > this.Version)
            {
                //new Optionsversion - reset new fields to default
                this.Version = OptionsControl.SVersion; //then Update Version afterwars
            }
        }
    }

    [Serializable]
    public class SerializeableColor
    {
        public SerializeableColor(byte _A, byte _R, byte _G, byte _B)
        {
            A = _A; R = _R; G = _G; B = _B;
        }
        public byte A = 0x00;
        public byte R = 0x00;
        public byte G = 0x00;
        public byte B = 0x00;
        public static implicit operator SerializeableColor(Color c)
        { return new SerializeableColor(c.A, c.R, c.G, c.B ); }
        public static implicit operator Color(SerializeableColor c)
        { return Color.FromArgb(c.A, c.R, c.G, c.B); }
    }

    public static class OptionsControlIOObject
    {
        public static void Save()
        {
            try
            {
                var formatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream("options_0.dat", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    formatter.Serialize(fileStream, Program.OptionsObject);
                    var test = Program.OptionsObject;
                }
            }
            catch (Exception) { }
        }

        public static OptionsControl Load()
        {
            try
            {
                if (File.Exists("options_0.dat"))
                {
                    object deserializedOptionsObj;
                    var formatter = new BinaryFormatter();
                    using (FileStream fileStream = new FileStream("options_0.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        deserializedOptionsObj = formatter.Deserialize(fileStream);
                    }
                    OptionsControl oc = (OptionsControl)deserializedOptionsObj;
                    oc.FillNullToDefaults();
                    return oc;
                }
            }
            catch (Exception) { }
            return new OptionsControl();
        }
    }
}
