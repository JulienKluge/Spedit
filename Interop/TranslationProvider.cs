using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows;

namespace Spedit.Interop
{
	public class TranslationProvider
	{
        public bool IsDefault = true;
        public string[] AvailableLanguageIDs;
		public string[] AvailableLanguages;
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
		public string FileName;
		public string Line;
		public string TypeStr;
		public string NormalSearch;
		public string MatchWholeWords;
		public string AdvancSearch;
		public string RegexSearch;
		public string CurrDoc;
		public string AllDoc;
		public string Find;
		public string Count;
		public string CaseSen;
		public string MultilineRegex;
		public string ErrorFileLoadProc;
		public string NotDissMethod;
		public string DFileChanged;
		public string FileChanged;
		public string FileTryReload;
		public string DSaveError;
		public string SaveError;
		public string SavingFile;
		public string PtAbb;
		public string ColAbb;
		public string LnAbb;
		public string LenAbb;
		public string SPEditCap;
		public string WrittenBy;
		public string License;
		public string PeopleInv;
		public string Preview;
		public string NewFile;
		public string ConfigWrongPars;
		public string NoName;
		public string PosLen;
		public string InheritedFrom;
		public string MethodFrom;
		public string PropertyFrom;
		public string Search;
		public string Delete, Name, ScriptDir, DelimiedWi, CopyDir, ServerExe, serverStartArgs, PreBuildCom, PostBuildCom, OptimizeLvl, VerboseLvl, AutoCopy, DeleteOldSMX;
		public string FTPHost, FTPUser, FTPPw, FTPDir, ComEditorDir, ComScriptDir, ComCopyDir, ComScriptFile, ComScriptName, ComPluginFile, ComPluginName, RConEngine;
		public string RConIP, RconPort, RconPw, RconCom, ComPluginsReload, ComPluginsLoad, ComPluginsUnload, NewConfig, CannotDelConf, YCannotDelConf, SelectExe, CMDLineCom, RConCMDLineCom;
		public string ResetOptions, ResetOptQues, RestartEditor, YRestartEditor, RestartEdiFullEff, RestartEdiEff, Program, HardwareAcc, UIAnim, OpenInc, OpenIncRec, AutoUpdate, ShowToolbar;
		public string DynamicISAC, DarkTheme, ThemeColor, LanguageStr, Editor, FontSize, ScrollSpeed, WordWrap, AggIndentation, ReformatAfterSem, TabsToSpace, AutoCloseBrack, AutoCloseStrChr;
		public string ShowSpaces, ShowTabs, IndentationSize, FontFamily, SyntaxHigh, HighDeprecat;
		public string Compile;
		public string AutoSaveMin;

		public void LoadLanguage(string lang, bool initial = false)
		{
			FillToEnglishDefaults();

			var languageList = new List<string>();
			var languageIdList = new List<string>();

			languageList.Add("English");
			languageIdList.Add("");
			lang = lang.Trim().ToLowerInvariant();
			IsDefault = (string.IsNullOrEmpty(lang) || lang.ToLowerInvariant() == "en") && initial;

			if (File.Exists("lang_0_spedit.xml"))
			{
				try
				{
					var document = new XmlDocument();
					document.Load("lang_0_spedit.xml");

					if (document.ChildNodes.Count < 1)
						throw new Exception("No Root-Node: \"translations\" found");

					XmlNode rootLangNode = null;

					foreach (XmlNode childNode in document.ChildNodes[0].ChildNodes)
					{
						var lId = childNode.Name;
                        var lNm = lId;

						if (childNode.Name.ToLowerInvariant() == lang)
						    rootLangNode = childNode;

					    if (childNode.FirstChild.Name.ToLowerInvariant() == "language")
					        lNm = childNode.FirstChild.InnerText;

					    languageList.Add(lNm);
						languageIdList.Add(lId);
					}

					if (rootLangNode != null)
					{
						foreach (XmlNode node in rootLangNode.ChildNodes)
						{
							if (node.NodeType == XmlNodeType.Comment)
								continue;

							var nn = node.Name.ToLowerInvariant();
                            var nv = node.InnerText;

							//and now: brace yourself and tuckle your seatbells:
							switch (nn)
							{
							    case "language":
							        Language = nv;
							        break;
							    case "serverrunning":
							        ServerRunning = nv;
							        break;
							    case "saving":
							        Saving = nv;
							        break;
							    case "saveufiles":
							        SavingUFiles = nv;
							        break;
							    case "compileall":
							        CompileAll = nv;
							        break;
							    case "compilecurr":
							        CompileCurr = nv;
							        break;
							    case "copy":
							        Copy = nv;
							        break;
							    case "ftpup":
							        FTPUp = nv;
							        break;
							    case "startserver":
							        StartServer = nv;
							        break;
							    case "replace":
							        Replace = nv;
							        break;
							    case "replaceall":
							        ReplaceAll = nv;
							        break;
							    case "opennewfile":
							        OpenNewFile = nv;
							        break;
							    case "nofilopened":
							        NoFileOpened = nv;
							        break;
							    case "nofileopenedcap":
							        NoFileOpenedCap = nv;
							        break;
							    case "savefileas":
							        SaveFileAs = nv;
							        break;
							    case "savefollow":
							        SaveFollow = nv;
							        break;
							    case "decompiling":
							        Decompiling = nv;
							        break;
							    case "chdecomp":
							        ChDecomp = nv;
							        break;
							    case "editconfig":
							        EditConfig = nv;
							        break;
							    case "foundinoff":
							        FoundInOff = nv;
							        break;
							    case "foundnothing":
							        FoundNothing = nv;
							        break;
							    case "replacedoff":
							        ReplacedOff = nv;
							        break;
							    case "replacedocc":
							        ReplacedOcc = nv;
							        break;
							    case "occfound":
							        OccFound = nv;
							        break;
							    case "emptypatt":
							        EmptyPatt = nv;
							        break;
							    case "novalidregex":
							        NoValidRegex = nv;
							        break;
							    case "failedcheck":
							        FailedCheck = nv;
							        break;
							    case "errorupdate":
							        ErrorUpdate = nv;
							        break;
							    case "versuptodate":
							        VersUpToDate = nv;
							        break;
							    case "versionyour":
							        VersionYour = nv;
							        break;
							    case "details":
							        Details = nv;
							        break;
							    case "compiling":
							        Compiling = nv;
							        break;
							    case "error":
							        Error = nv;
							        break;
							    case "spcompnotstarted":
							        SPCompNotStarted = nv;
							        break;
							    case "spcompnotfound":
							        SPCompNotFound = nv;
							        break;
							    case "copied":
							        Copied = nv;
							        break;
							    case "deleted":
							        Deleted = nv;
							        break;
							    case "failcopy":
							        FailCopy = nv;
							        break;
							    case "nofilescopy":
							        NoFilesCopy = nv;
							        break;
							    case "uploaded":
							        Uploaded = nv;
							        break;
							    case "erroruploadfile":
							        ErrorUploadFile = nv;
							        break;
							    case "errorupload":
							        ErrorUpload = nv;
							        break;
							    case "done":
							        Done = nv;
							        break;
							    case "file":
							        FileStr = nv;
							        break;
							    case "new":
							        New = nv;
							        break;
							    case "open":
							        Open = nv;
							        break;
							    case "save":
							        Save = nv;
							        break;
							    case "saveall":
							        SaveAll = nv;
							        break;
							    case "saveas":
							        SaveAs = nv;
							        break;
							    case "close":
							        Close = nv;
							        break;
							    case "closeall":
							        CloseAll = nv;
							        break;
							    case "build":
							        Build = nv;
							        break;
							    case "copyplugin":
							        CopyPlugin = nv;
							        break;
							    case "sendrcon":
							        SendRCon = nv;
							        break;
							    case "config":
							        Config = nv;
							        break;
							    case "edit":
							        Edit = nv;
							        break;
							    case "undo":
							        Undo = nv;
							        break;
							    case "redo":
							        Redo = nv;
							        break;
							    case "cut":
							        Cut = nv;
							        break;
							    case "paste":
							        Paste = nv;
							        break;
							    case "folding":
							        Folding = nv;
							        break;
							    case "expandall":
							        ExpandAll = nv;
							        break;
							    case "collapseall":
							        CollapseAll = nv;
							        break;
							    case "jumpto":
							        JumpTo = nv;
							        break;
							    case "togglcomment":
							        TogglComment = nv;
							        break;
							    case "selectall":
							        SelectAll = nv;
							        break;
							    case "findreplace":
							        FindReplace = nv;
							        break;
							    case "tools":
							        Tools = nv;
							        break;
							    case "options":
							        Options = nv;
							        break;
							    case "parsedincdir":
							        ParsedIncDir = nv;
							        break;
							    case "oldapiweb":
							        OldAPIWeb = nv;
							        break;
							    case "newapiweb":
							        NewAPIWeb = nv;
							        break;
							    case "reformatter":
							        Reformatter = nv;
							        break;
							    case "reformatcurr":
							        ReformatCurr = nv;
							        break;
							    case "reformatall":
							        ReformatAll = nv;
							        break;
							    case "decompile":
							        Decompile = nv;
							        break;
							    case "reportbuggit":
							        ReportBugGit = nv;
							        break;
							    case "checkupdates":
							        CheckUpdates = nv;
							        break;
							    case "about":
							        About = nv;
							        break;
							    case "filename":
							        FileName = nv;
							        break;
							    case "line":
							        Line = nv;
							        break;
							    case "type":
							        TypeStr = nv;
							        break;
							    case "normalsearch":
							        NormalSearch = nv;
							        break;
							    case "matchwhowords":
							        MatchWholeWords = nv;
							        break;
							    case "advancsearch":
							        AdvancSearch = nv;
							        break;
							    case "regexsearch":
							        RegexSearch = nv;
							        break;
							    case "currdoc":
							        CurrDoc = nv;
							        break;
							    case "alldoc":
							        AllDoc = nv;
							        break;
							    case "find":
							        Find = nv;
							        break;
							    case "count":
							        Count = nv;
							        break;
							    case "casesen":
							        CaseSen = nv;
							        break;
							    case "multilineregex":
							        MultilineRegex = nv;
							        break;
							    case "errorfileloadproc":
							        ErrorFileLoadProc = nv;
							        break;
							    case "notdissmethod":
							        NotDissMethod = nv;
							        break;
							    case "dfilechanged":
							        DFileChanged = nv;
							        break;
							    case "filechanged":
							        FileChanged = nv;
							        break;
							    case "filetryreload":
							        FileTryReload = nv;
							        break;
							    case "dsaveerror":
							        DSaveError = nv;
							        break;
							    case "saveerror":
							        SaveError = nv;
							        break;
							    case "savingfile":
							        SavingFile = nv;
							        break;
							    case "colabb":
							        ColAbb = nv;
							        break;
							    case "lnabb":
							        LnAbb = nv;
							        break;
							    case "lenabb":
							        LenAbb = nv;
							        break;
							    case "ptabb":
							        PtAbb = nv;
							        break;
							    case "speditcap":
							        SPEditCap = nv;
							        break;
							    case "writtenby":
							        WrittenBy = nv;
							        break;
							    case "license":
							        License = nv;
							        break;
							    case "peopleinv":
							        PeopleInv = nv;
							        break;
							    case "preview":
							        Preview = nv;
							        break;
							    case "newfile":
							        NewFile = nv;
							        break;
							    case "configwrongpars":
							        ConfigWrongPars = nv;
							        break;
							    case "noname":
							        NoName = nv;
							        break;
							    case "poslen":
							        PosLen = nv;
							        break;
							    case "inheritedfr":
							        InheritedFrom = nv;
							        break;
							    case "methodfrom":
							        MethodFrom = nv;
							        break;
							    case "propertyfrom":
							        PropertyFrom = nv;
							        break;
							    case "search":
							        Search = nv;
							        break;
							    case "delete":
							        Delete = nv;
							        break;
							    case "name":
							        Name = nv;
							        break;
							    case "scriptdir":
							        ScriptDir = nv;
							        break;
							    case "delimitedwi":
							        DelimiedWi = nv;
							        break;
							    case "copydir":
							        CopyDir = nv;
							        break;
							    case "serverexe":
							        ServerExe = nv;
							        break;
							    case "serverstartargs":
							        serverStartArgs = nv;
							        break;
							    case "prebuildcom":
							        PreBuildCom = nv;
							        break;
							    case "postbuildcom":
							        PostBuildCom = nv;
							        break;
							    case "optimizelvl":
							        OptimizeLvl = nv;
							        break;
							    case "verboselvl":
							        VerboseLvl = nv;
							        break;
							    case "autocopy":
							        AutoCopy = nv;
							        break;
							    case "deleteoldsmx":
							        DeleteOldSMX = nv;
							        break;
							    case "ftphost":
							        FTPHost = nv;
							        break;
							    case "ftpuser":
							        FTPUser = nv;
							        break;
							    case "ftppw":
							        FTPPw = nv;
							        break;
							    case "ftpdir":
							        FTPDir = nv;
							        break;
							    case "comeditordir":
							        ComEditorDir = nv;
							        break;
							    case "comscriptdir":
							        ComScriptDir = nv;
							        break;
							    case "comcopydir":
							        ComCopyDir = nv;
							        break;
							    case "comscriptfile":
							        ComScriptFile = nv;
							        break;
							    case "comscriptname":
							        ComScriptName = nv;
							        break;
							    case "compluginfile":
							        ComPluginFile = nv;
							        break;
							    case "compluginname":
							        ComPluginName = nv;
							        break;
							    case "rconengine":
							        RConEngine = nv;
							        break;
							    case "rconip":
							        RConIP = nv;
							        break;
							    case "rconport":
							        RconPort = nv;
							        break;
							    case "rconpw":
							        RconPw = nv;
							        break;
							    case "rconcom":
							        RconCom = nv;
							        break;
							    case "compluginsreload":
							        ComPluginsReload = nv;
							        break;
							    case "compluginsloas":
							        ComPluginsLoad = nv;
							        break;
							    case "compluginsunload":
							        ComPluginsUnload = nv;
							        break;
							    case "newconfig":
							        NewConfig = nv;
							        break;
							    case "cannotdelconf":
							        CannotDelConf = nv;
							        break;
							    case "ycannotdelconf":
							        YCannotDelConf = nv;
							        break;
							    case "selectexe":
							        SelectExe = nv;
							        break;
							    case "cmdlinecom":
							        CMDLineCom = nv;
							        break;
							    case "rconcmdlinecom":
							        RConCMDLineCom = nv;
							        break;
							    case "resetoptions":
							        ResetOptions = nv;
							        break;
							    case "resetoptques":
							        ResetOptQues = nv;
							        break;
							    case "restarteditor":
							        RestartEditor = nv;
							        break;
							    case "yrestarteditor":
							        YRestartEditor = nv;
							        break;
							    case "restartedifulleff":
							        RestartEdiFullEff = nv;
							        break;
							    case "restartedieff":
							        RestartEdiEff = nv;
							        break;
							    case "program":
							        Program = nv;
							        break;
							    case "hardwareacc":
							        HardwareAcc = nv;
							        break;
							    case "uianim":
							        UIAnim = nv;
							        break;
							    case "openinc":
							        OpenInc = nv;
							        break;
							    case "openincrec":
							        OpenIncRec = nv;
							        break;
							    case "autoupdate":
							        AutoUpdate = nv;
							        break;
							    case "showtoolbar":
							        ShowToolbar = nv;
							        break;
							    case "dynamicisac":
							        DynamicISAC = nv;
							        break;
							    case "darktheme":
							        DarkTheme = nv;
							        break;
							    case "themecolor":
							        ThemeColor = nv;
							        break;
							    case "languagestr":
							        LanguageStr = nv;
							        break;
							    case "editor":
							        Editor = nv;
							        break;
							    case "fontsize":
							        FontSize = nv;
							        break;
							    case "scrollspeed":
							        ScrollSpeed = nv;
							        break;
							    case "wordwrap":
							        WordWrap = nv;
							        break;
							    case "aggindentation":
							        AggIndentation = nv;
							        break;
							    case "reformataftersem":
							        ReformatAfterSem = nv;
							        break;
							    case "tabstospace":
							        TabsToSpace = nv;
							        break;
							    case "autoclosebrack":
							        AutoCloseBrack = nv;
							        break;
							    case "autoclosestrchr":
							        AutoCloseStrChr = nv;
							        break;
							    case "showsapaces":
							        ShowSpaces = nv;
							        break;
							    case "showtabs":
							        ShowTabs = nv;
							        break;
							    case "indentationsize":
							        IndentationSize = nv;
							        break;
							    case "fontfamily":
							        FontFamily = nv;
							        break;
							    case "syntaxhigh":
							        SyntaxHigh = nv;
							        break;
							    case "highdeprecat":
							        HighDeprecat = nv;
							        break;
							    case "compile":
							        Compile = nv;
							        break;
							    case "autosavemin":
							        AutoSaveMin = nv;
							        break;
							    default:
							        throw new Exception($"{nn} is not a known language-phrase");
							}
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
			AvailableLanguageIDs = languageIdList.ToArray();
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
			FileName = "File Name";
			Line = "Line";
			TypeStr = "Type";
			NormalSearch = "Normal search";
			MatchWholeWords = "Match whole words";
			AdvancSearch = "Advanced search";
			RegexSearch = "Regex search";
			CurrDoc = "Current document";
			AllDoc = "All open documents";
			Find = "Find";
			Count = "Count";
			CaseSen = "Case sensitive";
			MultilineRegex = "Multiline Regex";
			ErrorFileLoadProc = "Error while loading and processing the file.";
			NotDissMethod = "Could not disassemble method {0}: {1}";
			DFileChanged = "{0} has changed.";
			FileChanged = "File changed";
			FileTryReload = "Try reloading file?";
			DSaveError = "An error occured while saving.";
			SaveError = "Save error";
			SavingFile = "Saving file";
			PtAbb = "pt";
			ColAbb = "Col";
			LnAbb = "Ln";
			LenAbb = "Len";
			SPEditCap = "a lightweight sourcepawn editor";
			WrittenBy = "written by: {0}";
			License = "License";
			PeopleInv = "People involved";
			Preview = "Preview";
			NewFile = "New file";
			ConfigWrongPars = "The config was not able to parse a sourcepawn definiton.";
			NoName = "no name";
			PosLen = "(pos: {0} - len: {1})";
			InheritedFrom = "inherited from";
			MethodFrom = "Method from";
			PropertyFrom = "Property from";
			Search = "search";
			Delete = "Delete";
			Name = "Name";
			ScriptDir = "Scripting directories";
			DelimiedWi = "delimit with";
			CopyDir = "Copy directory";
			ServerExe = "Server executable";
			serverStartArgs = "Server-start arguments";
			PreBuildCom = "Pre-Build commandline";
			PostBuildCom = "Post-Build commandline";
			OptimizeLvl = "Optimization level";
			VerboseLvl = "Verbose level";
			AutoCopy = "Auto copy after compile";
			DeleteOldSMX = "Delete old .smx after copy";
			FTPHost = "FTP host";
			FTPUser = "FTP user";
			FTPPw = "FTP password";
			FTPDir = "FTP directory";
			ComEditorDir = "Directory of the SPEdit binary";
			ComScriptDir = "Directory of the Compiling script";
			ComCopyDir = "Directory where the smx should be copied";
			ComScriptFile = "Full Directory and Name of the script";
			ComScriptName = "File Name of the script";
			ComPluginFile = "Full Directory and Name of the compiled script";
			ComPluginName = "File Name of the compiled script";
			RConEngine = "RCon server engine";
			RConIP = "RCon server IP";
			RconPort = "RCon server port";
			RconPw = "RCon server password";
			RconCom = "RCon Server commands";
			ComPluginsReload = "Reloads all compiled plugins";
			ComPluginsLoad = "Loads all compiled plugins";
			ComPluginsUnload = "Unloads all compiled plugins";
			NewConfig = "New config";
			CannotDelConf = "Cannot delete config";
			YCannotDelConf = "You cannot delete this config.";
			SelectExe = "Select executable";
			CMDLineCom = "Commandline variables";
			RConCMDLineCom = "Rcon commandline variables";
			ResetOptions = "Reset options";
			ResetOptQues = "Are you sure, you want to reset the options?";
			RestartEditor = "Restart Editor";
			YRestartEditor = "You have to restart the editor for the changes to have effect.";
			RestartEdiFullEff = "Restart editor to take full effect...";
			RestartEdiEff = "Restart editor to take effect...";
			Program = "Program";
			HardwareAcc = "Use hardware acceleration (if available)";
			UIAnim = "UI animations";
			OpenInc = "Auto open includes";
			OpenIncRec = "Open Includes Recursivly";
			AutoUpdate = "Search automatically for updates";
			ShowToolbar = "Show toolbar";
			DynamicISAC = "Dynamic Autocompletition/Intellisense";
			DarkTheme = "Dark theme";
			ThemeColor = "Theme Color";
			LanguageStr = "Language";
			Editor = "Editor";
			FontSize = "Font size";
			ScrollSpeed = "Scroll speed";
			WordWrap = "Word wrap";
			AggIndentation = "Agressive Indentation";
			ReformatAfterSem = "Reformatting line after semicolon";
			TabsToSpace = "Replace tabs with spaces";
			AutoCloseBrack = "Auto close brackets";
			AutoCloseStrChr = "Auto close Strings, Chars";
			ShowSpaces = "Show spaces";
			ShowTabs = "Show tabs";
			IndentationSize = "Indentation size";
			FontFamily = "Font";
			SyntaxHigh = "Syntaxhighlighting";
			HighDeprecat = "Highlight deprecated (<1.7) syntax";
			Compile = "Compile";
			AutoSaveMin = "Auto save (min)";
		}
	}
}
