using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using Spedit.UI;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow : MetroWindow
    {
		string[] AvailableAccents = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
			"Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        bool RestartTextIsShown = false;
        bool AllowChanging = false;
        public OptionsWindow()
        {
            InitializeComponent();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			LoadSettings();
            AllowChanging = true;
        }

        private async void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Reset options", "Are you sure, you want to reset the options?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                Program.OptionsObject = new OptionsControl();
                Program.MainWindow.OptionMenuEntry.IsEnabled = false;
                await this.ShowMessageAsync("Restart Editor", "You have to restart the editor for the changes to have effect.", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
                this.Close();
            }
        }

        private void HardwareAcc_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_UseHardwareAcceleration = HardwareAcc.IsChecked.Value;
            ToggleRestartText();
        }

        private void UIAnimation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.UI_Animations = UIAnimation.IsChecked.Value;
            ToggleRestartText();
        }

        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }
        private void OpenIncludeRecursivly_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }

        private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_CheckForUpdates = AutoUpdate.IsChecked.Value;
        }

        private void ShowToolbar_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.UI_ShowToolBar = ShowToolBar.IsChecked.Value;
            if (Program.OptionsObject.UI_ShowToolBar)
            {
                Program.MainWindow.Win_ToolBar.Height = double.NaN;
            }
            else
            {
                Program.MainWindow.Win_ToolBar.Height = 0.0;
            }
        }

		private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_DynamicISAC = DynamicISAC.IsChecked.Value;
		}

		private void DarkTheme_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			if (DarkTheme.IsChecked.Value)
			{ Program.OptionsObject.Program_Theme = "BaseDark"; }
			else
			{ Program.OptionsObject.Program_Theme = "BaseLight"; }
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ToggleRestartText(true);
		}

		private void AccentColor_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_AccentColor = (string)AccentColor.SelectedItem;
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
		}

		private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.OptionsObject.Editor_FontSize = size;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].UpdateFontSize(size);
				}
			}
        }

        private void ScrollSpeed_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_ScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.OptionsObject.Editor_WordWrap = wrapping;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.WordWrap = wrapping;
				}
			}
        }

        private void AIndentation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_AgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_ReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool replaceTabs = TabToSpace.IsChecked.Value;
            Program.OptionsObject.Editor_ReplaceTabsToWhitespace = replaceTabs;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            if (editors != null)
            {
                for (int i = 0; i < editors.Length; ++i)
                {
                    editors[i].editor.Options.ConvertTabsToSpaces = replaceTabs;
                }
            }
        }

		private void AutoCloseBrackets_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Editor_AutoCloseBrackets = AutoCloseBrackets.IsChecked.Value;
		}


		private void FontFamily_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            FontFamily family = (FontFamily)FontFamilyCB.SelectedItem;
            string FamilyString = family.Source;
            Program.OptionsObject.Editor_FontFamily = FamilyString;
            FontFamilyTB.Text = "Font (" + FamilyString + "):";
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.FontFamily = family;
				}
			}
        }

        private void HighlightDeprecateds_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.SH_HighlightDeprecateds = HighlightDeprecateds.IsChecked.Value;
            ToggleRestartText();
        }

        private void LoadSettings()
        {
			for (int i = 0; i < AvailableAccents.Length; ++i)
			{
				AccentColor.Items.Add(AvailableAccents[i]);
			}
            HardwareAcc.IsChecked = Program.OptionsObject.Program_UseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UI_Animations;
            OpenIncludes.IsChecked = Program.OptionsObject.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.Program_OpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.OptionsObject.Program_CheckForUpdates;
            if (!Program.OptionsObject.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            ShowToolBar.IsChecked = Program.OptionsObject.UI_ShowToolBar;
			DynamicISAC.IsChecked = Program.OptionsObject.Program_DynamicISAC;
			DarkTheme.IsChecked = (Program.OptionsObject.Program_Theme == "BaseDark");
			for (int i = 0; i < AvailableAccents.Length; ++i)
			{
				if (AvailableAccents[i] == Program.OptionsObject.Program_AccentColor)
				{
					AccentColor.SelectedIndex = i;
					break;
				}
			}
            HighlightDeprecateds.IsChecked = Program.OptionsObject.SH_HighlightDeprecateds;
            FontSizeD.Value = Program.OptionsObject.Editor_FontSize;
            ScrollSpeed.Value = Program.OptionsObject.Editor_ScrollLines;
            WordWrap.IsChecked = Program.OptionsObject.Editor_WordWrap;
            AgressiveIndentation.IsChecked = Program.OptionsObject.Editor_AgressiveIndentation;
            LineReformatting.IsChecked = Program.OptionsObject.Editor_ReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.OptionsObject.Editor_ReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.OptionsObject.Editor_AutoCloseBrackets;
			FontFamilyTB.Text = "Font(" + Program.OptionsObject.Editor_FontFamily + "):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.OptionsObject.Editor_FontFamily);
            LoadSH();
        }

        private void ToggleRestartText(bool FullEffect = false)
        {
            if (AllowChanging)
            {
                if (!RestartTextIsShown)
                {
                    StatusTextBlock.Content = (FullEffect) ? "Restart editor to take full effect..." : "Restart editor to take effect...";
                    RestartTextIsShown = true;
                }
            }
        }
    }
}
