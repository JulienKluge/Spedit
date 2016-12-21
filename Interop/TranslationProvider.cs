using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Windows;

namespace Spedit.Interop
{
	public class TranslationProvider
	{
		public string[] AvailableLanguageIDs;
		public string[] AvailableLanguages;

		public bool IsDefault = true;

		public string Language;
		public string ServerRunning;
		public string Saving;
		public string SavingUFiles;
		public string CompileAll;
		public string CompileCurr;
		public string Copy;
		public string FTPUp;
		public string StartServer;
		public string Replace;
		public string ReplaceAll;
		public string OpenNewFile;
		public string NoFileOpened;
		public string NoFileOpenedCap;
		public string SaveFileAs;
		public string SaveFollow;
		public string ChDecomp;
		public string Decompiling;
		public string EditConfig;
		public string FoundInOff;
		public string FoundNothing;
		public string ReplacedOff;
		public string ReplacedOcc;
		public string OccFound;
		public string EmptyPatt;
		public string NoValidRegex;
		public string FailedCheck;
		public string ErrorUpdate;
		public string VersUpToDate;
		public string VersionYour;
		public string Details;

		public void LoadLanguage(string lang)
		{
			FillToEnglishDefaults();
			List<string> languageList = new List<string>();
			List<string> languageIDList = new List<string>();
			languageList.Add("English");
			languageIDList.Add("");
			lang = lang.Trim().ToLowerInvariant();
			IsDefault = string.IsNullOrEmpty(lang) || lang.ToLowerInvariant() == "en";
			if (File.Exists("lang_0_spedit.xml"))
			{
				try
				{
					XmlDocument document = new XmlDocument();
					document.Load("lang_0_spedit.xml");
					if (document.ChildNodes.Count < 1)
					{
						throw new Exception("No Root-Node: \"translations\" found");
					}
					XmlNode rootLangNode = null;
					foreach (XmlNode childNode in document.ChildNodes[0].ChildNodes)
					{
						string lID = childNode.Name;
						string lNm = lID;
						if (childNode.Name.ToLowerInvariant() == lang)
						{
							rootLangNode = childNode;
						}
						if (childNode.FirstChild.Name.ToLowerInvariant() == "language")
						{
							lNm = childNode.FirstChild.InnerText;
						}
						languageList.Add(lNm);
						languageIDList.Add(lID);
					}
					if (rootLangNode != null)
					{
						foreach (XmlNode node in rootLangNode.ChildNodes)
						{
							string nn = node.Name.ToLowerInvariant();
							string nv = node.InnerText;
							if (nn == "language")
								Language = nv;
							else if (nn == "serverrunning")
								ServerRunning = nv;
							else if (nn == "saving")
								Saving = nv;
							else if (nn == "savingufiles")
								SavingUFiles = nv;
							else if (nn == "compileall")
								CompileAll = nv;
							else if (nn == "compilecurr")
								CompileCurr = nv;
							else if (nn == "copy")
								Copy = nv;
							else if (nn == "ftpup")
								FTPUp = nv;
							else if (nn == "startserver")
								StartServer = nv;
							else if (nn == "replace")
								Replace = nv;
							else if (nn == "replaceall")
								ReplaceAll = nv;
							else if (nn == "opennewfile")
								OpenNewFile = nv;
							else if (nn == "nofileopened")
								NoFileOpened = nv;
							else if (nn == "nofileopenedcap")
								NoFileOpenedCap = nv;
							else if (nn == "savefileas")
								SaveFileAs = nv;
							else if (nn == "savefollow")
								SaveFollow = nv;
							else if (nn == "decompiling")
								Decompiling = nv;
							else if (nn == "chdecomp")
								ChDecomp = nv;
							else if (nn == "editconfig")
								EditConfig = nv;
							else if (nn == "foundinoff")
								FoundInOff = nv;
							else if (nn == "foundnothing")
								FoundNothing = nv;
							else if (nn == "replacedoff")
								ReplacedOff = nv;
							else if (nn == "replacedocc")
								ReplacedOcc = nv;
							else if (nn == "emptypatt")
								EmptyPatt = nv;
							else if (nn == "novalidregex")
								NoValidRegex = nv;
							else if (nn == "failedcheck")
								FailedCheck = nv;
							else if (nn == "errorupdate")
								ErrorUpdate = nv;
							else if (nn == "versuptodate")
								VersUpToDate = nv;
							else if (nn == "versionyour")
								VersionYour = nv;
							else if (nn == "details")
								Details = nv;
						}
					}
				}
				catch (Exception e)
				{
					MessageBox.Show("An error occured while reading the language-file. Without them, the editor wont show translations." + Environment.NewLine + "Details: " + e.Message
						, "Error while reading configs."
						, MessageBoxButton.OK
						, MessageBoxImage.Warning);
				}
			}
			AvailableLanguages = languageList.ToArray();
			AvailableLanguageIDs = languageIDList.ToArray();
		}

		private void FillToEnglishDefaults()
		{
			Language = "English";
			ServerRunning = "Server running";
			Saving = "Saving";
			SavingUFiles = "Save all unsaved files?";
			CompileAll = "Compile all";
			CompileCurr = "Copmile current";
			Copy = "Copy";
			FTPUp = "FTP Upload";
			StartServer = "Start server";
			Replace = "Replace";
			ReplaceAll = "Replace all";
			OpenNewFile = "Open new file";
			NoFileOpened = "No files opened";
			NoFileOpenedCap = "None of the selected files could be opened.";
			SaveFileAs = "Save file as";
			SaveFollow = "Save following files";
			ChDecomp = "Select plugin to decompile";
			Decompiling = "Decompiling";
			EditConfig = "Edit Configurations";
			FoundInOff = "Found in offset {0} with length {1}";
			FoundNothing = "Found nothing";
			ReplacedOff = "Replaced in offset";
			ReplacedOcc = "Replaced {0} occurences in {1} documents";
			OccFound = "occurences found";
			EmptyPatt = "Empty search pattern";
			NoValidRegex = "No valid regex pattern";
			FailedCheck = "Failed to check";
			ErrorUpdate = "Error while checking for updates.";
			VersUpToDate = "Version up to date";
			VersionYour = "Your program version {0} is up to date.";
			Details = "Details";
	}
	}
}
