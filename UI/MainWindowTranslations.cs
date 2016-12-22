using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Spedit.UI
{
	public partial class MainWindow : MetroWindow
	{
		public void Language_Translate(bool Initial = false)
		{
			if (Initial && Program.Translations.IsDefault)
			{
				return;
			}
			if (!Initial)
			{
				compileButtonDict = new ObservableCollection<string>() { Program.Translations.CompileAll, Program.Translations.CompileCurr };
				actionButtonDict = new ObservableCollection<string>() { Program.Translations.Copy, Program.Translations.FTPUp, Program.Translations.StartServer };
				findReplaceButtonDict = new ObservableCollection<string>() { Program.Translations.Replace, Program.Translations.ReplaceAll };
				((MenuItem)ConfigMenu.Items[ConfigMenu.Items.Count - 1]).Header = Program.Translations.EditConfig;
			}
			MenuI_File.Header = Program.Translations.FileStr;
			MenuI_New.Header = Program.Translations.New;
			MenuI_Open.Header = Program.Translations.Open;
			MenuI_Save.Header = Program.Translations.Save;
			MenuI_SaveAll.Header = Program.Translations.SaveAll;
			MenuI_SaveAs.Header = Program.Translations.SaveAs;
			MenuI_Close.Header = Program.Translations.Close;
			MenuI_CloseAll.Header = Program.Translations.CloseAll;

			MenuI_Edit.Header = Program.Translations.Edit;
			MenuI_Undo.Header = Program.Translations.Undo;
			MenuI_Redo.Header = Program.Translations.Redo;
			MenuI_Cut.Header = Program.Translations.Cut;
			MenuI_Copy.Header = Program.Translations.Copy;
			MenuI_Paste.Header = Program.Translations.Paste;
			MenuI_Folding.Header = Program.Translations.Folding;
			MenuI_ExpandAll.Header = Program.Translations.ExpandAll;
			MenuI_CollapseAll.Header = Program.Translations.CollapseAll;
			MenuI_JumpTo.Header = Program.Translations.JumpTo;
			MenuI_ToggleComment.Header = Program.Translations.TogglComment;
			MenuI_SelectAll.Header = Program.Translations.SelectAll;
			MenuI_FindReplace.Header = Program.Translations.FindReplace;

			MenuI_Build.Header = Program.Translations.Build;
			MenuI_CompileAll.Header = Program.Translations.CompileAll;
			MenuI_Compile.Header = Program.Translations.CompileCurr;
			MenuI_CopyPlugin.Header = Program.Translations.CopyPlugin;
			MenuI_FTPUpload.Header = Program.Translations.FTPUp;
			MenuI_StartServer.Header = Program.Translations.StartServer;
			MenuI_SendRCon.Header = Program.Translations.SendRCon;

			ConfigMenu.Header = Program.Translations.Config;
		}
	}
}
