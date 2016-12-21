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
		}
	}
}
