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
		public string Compiling;
		public string Error;
		public string SPCompNotStarted;
		public string SPCompNotFound;
		public string Copied;
		public string Deleted;
		public string FailCopy;
		public string NoFilesCopy;
		public string Uploaded;
		public string ErrorUploadFile;
		public string ErrorUpload;
		public string Done;
		public string FileStr;
		public string New;
		public string Open;
		public string Save;
		public string SaveAll;
		public string SaveAs;
		public string Close;
		public string CloseAll;
		public string Build;
		public string CopyPlugin;
		public string SendRCon;
		public string Config;
		public string Edit;
		public string Undo;
		public string Redo;
		public string Cut;
		public string Paste;
		public string Folding;
		public string ExpandAll;
		public string CollapseAll;
		public string JumpTo;
		public string TogglComment;
		public string SelectAll;
		public string FindReplace;
		public string Tools;
		public string Options;
		public string ParsedIncDir;
		public string OldAPIWeb;
		public string NewAPIWeb;
		public string Reformatter;
		public string ReformatCurr;
		public string ReformatAll;
		public string Decompile;
		public string ReportBugGit;
		public string CheckUpdates;
		public string About;

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
							//and now: brace yourself and tuckle your seatbells:
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
							else if (nn == "compiling")
								Compiling = nv;
							else if (nn == "error")
								Error = nv;
							else if (nn == "spcompnotstarted")
								SPCompNotStarted = nv;
							else if (nn == "spcompnotfound")
								SPCompNotFound = nv;
							else if (nn == "copied")
								Copied = nv;
							else if (nn == "deleted")
								Deleted = nv;
							else if (nn == "failcopy")
								FailCopy = nv;
							else if (nn == "nofilescopy")
								NoFilesCopy = nv;
							else if (nn == "uploaded")
								Uploaded = nv;
							else if (nn == "erroruploadfile")
								ErrorUploadFile = nv;
							else if (nn == "errorupload")
								ErrorUpload = nv;
							else if (nn == "done")
								Done = nv;
							else if (nn == "file")
								FileStr = nv;
							else if (nn == "new")
								New = nv;
							else if (nn == "open")
								Open = nv;
							else if (nn == "save")
								Save = nv;
							else if (nn == "saveall")
								SaveAll = nv;
							else if (nn == "saveas")
								SaveAs = nv;
							else if (nn == "close")
								Close = nv;
							else if (nn == "closeall")
								CloseAll = nv;
							else if (nn == "build")
								Build = nv;
							else if (nn == "copyplugin")
								CopyPlugin = nv;
							else if (nn == "sendrcon")
								SendRCon = nv;
							else if (nn == "config")
								Config = nv;
							else if (nn == "edit")
								Edit = nv;
							else if (nn == "undo")
								Undo = nv;
							else if (nn == "redo")
								Redo = nv;
							else if (nn == "cut")
								Cut = nv;
							else if (nn == "paste")
								Paste = nv;
							else if (nn == "folding")
								Folding = nv;
							else if (nn == "expandall")
								ExpandAll = nv;
							else if (nn == "collapseall")
								CollapseAll = nv;
							else if (nn == "jumpto")
								JumpTo = nv;
							else if (nn == "togglcomment")
								TogglComment = nv;
							else if (nn == "selectall")
								SelectAll = nv;
							else if (nn == "findreplace")
								FindReplace = nv;
							else if (nn == "tools")
								Tools = nv;
							else if (nn == "options")
								Options = nv;
							else if (nn == "parsedincdir")
								ParsedIncDir = nv;
							else if (nn == "oldapiweb")
								OldAPIWeb = nv;
							else if (nn == "newapiweb")
								NewAPIWeb = nv;
							else if (nn == "reformatter")
								Reformatter = nv;
							else if (nn == "reformatcurr")
								ReformatCurr = nv;
							else if (nn == "reformatall")
								ReformatAll = nv;
							else if (nn == "decompile")
								Decompile = nv;
							else if (nn == "reportbuggit")
								ReportBugGit = nv;
							else if (nn == "checkupdates")
								CheckUpdates = nv;
							else if (nn == "about")
								About = nv;
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
			CompileCurr = "Compile current";
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
			Compiling = "Compiling";
			Error = "Error";
			SPCompNotStarted = "The spcomp.exe compiler did not started correctly.";
			SPCompNotFound = "The spcomp.exe compiler could not be found.";
			Copied = "Copied";
			Deleted = "Deleted";
			FailCopy = "Failed to copy";
			NoFilesCopy = "No files copied";
			Uploaded = "Uploaded";
			ErrorUploadFile = "Error while uploading file: {0} to {1}";
			ErrorUpload = "Error while uploading files";
			Done = "Done";
			FileStr = "File";
			New = "New";
			Open = "Open";
			Save = "Save";
			SaveAll = "Save all";
			SaveAs = "Save as";
			Close = "Close";
			CloseAll = "Close all";
			Build = "Build";
			CopyPlugin = "Copy Plugins";
			SendRCon = "Senc RCon commands";
			Config = "Configuration";
			Edit = "Edit";
			Undo = "Undo";
			Redo = "Redo";
			Cut = "Cut";
			Paste = "Paste";
			Folding = "Foldings";
			ExpandAll = "Expand all";
			CollapseAll = "Collapse all";
			JumpTo = "Jump to";
			TogglComment = "Toggle comment";
			SelectAll = "Select all";
			FindReplace = "Find & Replace";
			Tools = "Tools";
			Options = "Options";
			ParsedIncDir = "Parsed from include directory";
			OldAPIWeb = "Old API webside";
			NewAPIWeb = "New API webside";
			Reformatter = "Syntax reformatter";
			ReformatCurr = "Reformat current";
			ReformatAll = "Reformat all";
			Decompile = "Decompile";
			ReportBugGit = "Report bug on GitHub";
			CheckUpdates = "Check for updates";
			About = "About";
		}
	}
}
