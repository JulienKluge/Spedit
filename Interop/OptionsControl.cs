using System;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Spedit //leave this here instead of .Interop because of reasons...
{
	[Serializable]
    public class OptionsControl
    {
        public static int SVersion = 10;

        public int Version = 10;
        public byte[] ProgramCryptoKey;
		public bool ProgramUseHardwareSalts = true;    
        public bool ProgramUseHardwareAcceleration = true;
        public bool ProgramCheckForUpdates = true;
        public string ProgramSelectedConfig = string.Empty;
        public bool ProgramOpenCustomIncludes = false;
        public bool ProgramOpenIncludesRecursively = false;
		public bool ProgramDynamicIsac = true;
		public string ProgramAccentColor = "Red";
		public string ProgramTheme = "BaseDark";
		public string ProgramObjectBrowserDirectory = string.Empty;
		public double ProgramObjectbrowserWidth = 300.0;
        public bool UIAnimations = true;
        public bool UIShowToolBar;
        public bool EditorWordWrap = false;
        public double EditorFontSize = 16.0;
        public string EditorFontFamily = "Consolas";
        public double EditorScrollLines = 4.0;
        public bool EditorAgressiveIndentation = true;
        public bool EditorReformatLineAfterSemicolon = true;
        public bool EditorReplaceTabsToWhitespace;
		public bool EditorAutoCloseBrackets = true;
		public bool EditorAutoCloseStringChars = true;
		public bool EditorShowSpaces;
		public bool EditorShowTabs;
		public int EditorIndentationSize = 4;
		public bool EditorAutoSave;
		public int EditorAutoSaveInterval = 5 * 60;
		public string[] LastOpenFiles = new string[0];
        public bool SHHighlightDeprecateds = true;
		public string Language = string.Empty;
        public SerializeableColor SHComments = new SerializeableColor(0xFF, 0x57, 0xA6, 0x49);
        public SerializeableColor SHCommentsMarker = new SerializeableColor(0xFF, 0xFF, 0x20, 0x20);
        public SerializeableColor SHStrings = new SerializeableColor(0xFF, 0xF4, 0x6B, 0x6C);
        public SerializeableColor SHPreProcessor = new SerializeableColor(0xFF, 0x7E, 0x7E, 0x7E);
        public SerializeableColor SHTypes = new SerializeableColor(0xFF, 0x28, 0x90, 0xB0); //56 9C D5
        public SerializeableColor SHTypesValues = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SHKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SHContextKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SHChars = new SerializeableColor(0xFF, 0xD6, 0x9C, 0x85);
        public SerializeableColor SHUnkownFunctions = new SerializeableColor(0xFF, 0x45, 0x85, 0xC5);
        public SerializeableColor SHNumbers = new SerializeableColor(0xFF, 0x97, 0x97, 0x97);
        public SerializeableColor SHSpecialCharacters = new SerializeableColor(0xFF, 0x8F, 0x8F, 0x8F);
        public SerializeableColor SHDeprecated = new SerializeableColor(0xFF, 0xFF, 0x00, 0x00);
        public SerializeableColor SHConstants = new SerializeableColor(0xFF, 0xBC, 0x62, 0xC5);
        public SerializeableColor SHFunctions = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
        public SerializeableColor SHMethods = new SerializeableColor(0xFF, 0x3B, 0xC6, 0x7E);

        public void FillNullToDefaults()
        {
            if (ProgramCryptoKey == null)
                ReCreateCryptoKey();

            if (SVersion <= Version)
                return;

            Program.ClearUpdateFiles();

            if (Version < 2)
                UIShowToolBar = false;

            if (Version < 3)
            {
                EditorReformatLineAfterSemicolon = true;
                EditorScrollLines = 4.0;
                ProgramCheckForUpdates = true;
            }

            if (Version < 4)
                EditorReplaceTabsToWhitespace = false;

            if (Version < 5)
                ProgramDynamicIsac = true;

            if (Version < 7)
            {
                ProgramAccentColor = "Red";
                ProgramTheme = "BaseDark";
                NormalizeSHColors();
            }

            if (Version < 8)
                EditorAutoCloseBrackets = true;

            if (Version < 9)
            {
                EditorAutoCloseStringChars = true;
                EditorShowSpaces = false;
                EditorShowTabs = false;
                EditorIndentationSize = 4;
                Language = "";
                ProgramObjectBrowserDirectory = string.Empty;
                ProgramObjectbrowserWidth = 300.0;
                EditorAutoSave = false;
                EditorAutoSaveInterval = 5 * 60;
                ReCreateCryptoKey();
                Program.MakeRcckAlert();
            }

            if (Version < 10)
                ProgramUseHardwareSalts = true;

            //new Optionsversion - reset new fields to default
            Version = SVersion; //then Update Version afterwars
        }

        public void ReCreateCryptoKey()
        {
            var buffer = new byte[16];

            using (var cryptoRandomProvider = new RNGCryptoServiceProvider()) //generate global unique cryptokey
            {
                cryptoRandomProvider.GetBytes(buffer);
            }

            ProgramCryptoKey = buffer;
        }

		private void NormalizeSHColors()
		{
			SHComments = new SerializeableColor(0xFF, 0x57, 0xA6, 0x49);
			SHCommentsMarker = new SerializeableColor(0xFF, 0xFF, 0x20, 0x20);
			SHStrings = new SerializeableColor(0xFF, 0xF4, 0x6B, 0x6C);
			SHPreProcessor = new SerializeableColor(0xFF, 0x7E, 0x7E, 0x7E);
			SHTypes = new SerializeableColor(0xFF, 0x28, 0x90, 0xB0); //56 9C D5
			SHTypesValues = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SHKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SHContextKeywords = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SHChars = new SerializeableColor(0xFF, 0xD6, 0x9C, 0x85);
			SHUnkownFunctions = new SerializeableColor(0xFF, 0x45, 0x85, 0xC5);
			SHNumbers = new SerializeableColor(0xFF, 0x97, 0x97, 0x97);
			SHSpecialCharacters = new SerializeableColor(0xFF, 0x8F, 0x8F, 0x8F);
			SHDeprecated = new SerializeableColor(0xFF, 0xFF, 0x00, 0x00);
			SHConstants = new SerializeableColor(0xFF, 0xBC, 0x62, 0xC5);
			SHFunctions = new SerializeableColor(0xFF, 0x56, 0x9C, 0xD5);
			SHMethods = new SerializeableColor(0xFF, 0x3B, 0xC6, 0x7E);
		}
    }

    [Serializable]
    public class SerializeableColor
    {
        public SerializeableColor(byte _A, byte _R, byte _G, byte _B)
        {
            A = _A; R = _R; G = _G; B = _B;
        }

        public byte A;
        public byte R;
        public byte G;
        public byte B;
        public static implicit operator SerializeableColor(Color c)
        { return new SerializeableColor(c.A, c.R, c.G, c.B ); }
        public static implicit operator Color(SerializeableColor c)
        { return Color.FromArgb(c.A, c.R, c.G, c.B); }
    }
}
