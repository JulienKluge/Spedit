using System;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        private bool _restartTextIsShown;
        private readonly bool _allowChanging;
        private readonly string[] _availableAccents = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
            "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };

        public OptionsWindow()
        {
            InitializeComponent();
            Language_Translate();

            if (Program.OptionsObject.ProgramAccentColor != "Red" || Program.OptionsObject.ProgramTheme != "BaseDark")
                ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                    ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));

            LoadSettings();
            _allowChanging = true;
        }

        private async void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync(Program.Translations.ResetOptions, Program.Translations.ResetOptQues, MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
                return;

            Program.OptionsObject = new OptionsControl();
            Program.OptionsObject.ReCreateCryptoKey();
            Program.MainWindow.OptionMenuEntry.IsEnabled = false;
            await this.ShowMessageAsync(Program.Translations.RestartEditor, Program.Translations.YRestartEditor, MessageDialogStyle.Affirmative, MetroDialogOptions);

            Close();
        }

        private void HardwareAcc_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (HardwareAcc.IsChecked != null)
                Program.OptionsObject.ProgramUseHardwareAcceleration = HardwareAcc.IsChecked.Value;

            ToggleRestartText();
        }

        private void UIAnimation_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (UIAnimation.IsChecked != null)
                Program.OptionsObject.UIAnimations = UIAnimation.IsChecked.Value;

            ToggleRestartText();
        }

        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (OpenIncludes.IsChecked == null)
                return;

            Program.OptionsObject.ProgramOpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }

        private void OpenIncludeRecursivly_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (OpenIncludesRecursive.IsChecked != null)
                Program.OptionsObject.ProgramOpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }

        private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (AutoUpdate.IsChecked != null)
                Program.OptionsObject.ProgramCheckForUpdates = AutoUpdate.IsChecked.Value;
        }

        private void ShowToolbar_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (ShowToolBar.IsChecked != null)
                Program.OptionsObject.UIShowToolBar = ShowToolBar.IsChecked.Value;

            Program.MainWindow.Win_ToolBar.Height = Program.OptionsObject.UIShowToolBar ? double.NaN : 0.0;
        }

        private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (DynamicISAC.IsChecked != null)
                Program.OptionsObject.ProgramDynamicIsac = DynamicISAC.IsChecked.Value;
        }

        private void DarkTheme_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (DarkTheme.IsChecked != null && DarkTheme.IsChecked.Value)
                Program.OptionsObject.ProgramTheme = "BaseDark";
            else
                Program.OptionsObject.ProgramTheme = "BaseLight";

            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));
            ThemeManager.ChangeAppStyle(Program.MainWindow,
                ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));

            ToggleRestartText(true);
        }

        private void AccentColor_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.ProgramAccentColor = (string) AccentColor.SelectedItem;
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));
            ThemeManager.ChangeAppStyle(Program.MainWindow,
                ThemeManager.GetAccent(Program.OptionsObject.ProgramAccentColor),
                ThemeManager.GetAppTheme(Program.OptionsObject.ProgramTheme));
        }

        private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var size = FontSizeD.Value;
            Program.OptionsObject.EditorFontSize = size;
            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.UpdateFontSize(size);
        }

        private void ScrollSpeed_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.EditorScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var wrapping = WordWrap.IsChecked != null && WordWrap.IsChecked.Value;
            Program.OptionsObject.EditorWordWrap = wrapping;

            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.WordWrap = wrapping;
        }

        private void AIndentation_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (AgressiveIndentation.IsChecked != null)
                Program.OptionsObject.EditorAgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (LineReformatting.IsChecked != null)
                Program.OptionsObject.EditorReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var replaceTabs = TabToSpace.IsChecked != null && TabToSpace.IsChecked.Value;
            Program.OptionsObject.EditorReplaceTabsToWhitespace = replaceTabs;

            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.Options.ConvertTabsToSpaces = replaceTabs;
        }

        private void AutoCloseBrackets_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (AutoCloseBrackets.IsChecked != null)
                Program.OptionsObject.EditorAutoCloseBrackets = AutoCloseBrackets.IsChecked.Value;
        }

        private void AutoCloseStringChars_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (AutoCloseStringChars.IsChecked != null)
                Program.OptionsObject.EditorAutoCloseStringChars = AutoCloseStringChars.IsChecked.Value;
        }

        private void ShowSpaces_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var showSpacesValue = ShowSpaces.IsChecked != null && (Program.OptionsObject.EditorShowSpaces = ShowSpaces.IsChecked.Value);
            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.Options.ShowSpaces = showSpacesValue;
        }

        private void ShowTabs_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var showTabsValue = ShowTabs.IsChecked != null && (Program.OptionsObject.EditorShowTabs = ShowTabs.IsChecked.Value);
            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.Options.ShowTabs = showTabsValue;
        }

        private void FontFamily_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var family = (FontFamily) FontFamilyCB.SelectedItem;
            var familyString = family.Source;

            Program.OptionsObject.EditorFontFamily = familyString;
            FontFamilyTB.Text = "Font (" + familyString + "):";

            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.FontFamily = family;
        }

        private void IndentationSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var indentationSizeValue =
                Program.OptionsObject.EditorIndentationSize = (int) Math.Round(IndentationSize.Value);
            var editors = Program.MainWindow.GetAllEditorElements();

            if (editors == null)
                return;

            foreach (var element in editors)
                element.editor.Options.IndentationSize = indentationSizeValue;
        }

        private void HighlightDeprecateds_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (HighlightDeprecateds.IsChecked != null)
                Program.OptionsObject.SHHighlightDeprecateds = HighlightDeprecateds.IsChecked.Value;

            ToggleRestartText();
        }

        private void LanguageBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var selectedString = (string) LanguageBox.SelectedItem;

            for (var i = 0; i < Program.Translations.AvailableLanguages.Length; ++i)
                if (Program.Translations.AvailableLanguages[i] == selectedString)
                {
                    Program.Translations.LoadLanguage(Program.Translations.AvailableLanguageIDs[i]);
                    Program.OptionsObject.Language = Program.Translations.AvailableLanguageIDs[i];
                    Program.MainWindow.Language_Translate();
                    break;
                }

            Language_Translate();
            ToggleRestartText(true);
        }

        private void HardwareSalts_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            if (HardwareSalts.IsChecked != null)
                Program.OptionsObject.ProgramUseHardwareSalts = HardwareSalts.IsChecked.Value;

            Program.RCCKMade = false;
            Program.OptionsObject.ReCreateCryptoKey();
            Program.MakeRcckAlert();
        }

        private void AutoSave_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            var newIndex = AutoSave.SelectedIndex;
            var editors = Program.MainWindow.GetAllEditorElements();

            if (newIndex == 0)
            {
                Program.OptionsObject.EditorAutoSave = false;

                if (editors == null)
                    return;

                foreach (var element in editors)
                    if (element.AutoSaveTimer.Enabled)
                        element.AutoSaveTimer.Stop();
            }
            else
            {
                Program.OptionsObject.EditorAutoSave = true;
                switch (newIndex)
                {
                    case 1:
                        Program.OptionsObject.EditorAutoSaveInterval = 30;
                        break;
                    case 7:
                        Program.OptionsObject.EditorAutoSaveInterval = 600;
                        break;
                    case 8:
                        Program.OptionsObject.EditorAutoSaveInterval = 900;
                        break;
                    default:
                        Program.OptionsObject.EditorAutoSaveInterval = (newIndex - 1) * 60;
                        break;
                }

                if (editors == null)
                    return;

                foreach (var element in editors)
                    element.StartAutoSaveTimer();
            }
        }

		private void LoadSettings()
        {
			foreach (var accent in _availableAccents)
			    AccentColor.Items.Add(accent);

            HardwareAcc.IsChecked = Program.OptionsObject.ProgramUseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UIAnimations;
            OpenIncludes.IsChecked = Program.OptionsObject.ProgramOpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.ProgramOpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.OptionsObject.ProgramCheckForUpdates;

            if (!Program.OptionsObject.ProgramOpenCustomIncludes)
                OpenIncludesRecursive.IsEnabled = false;

            ShowToolBar.IsChecked = Program.OptionsObject.UIShowToolBar;
			DynamicISAC.IsChecked = Program.OptionsObject.ProgramDynamicIsac;
			DarkTheme.IsChecked = (Program.OptionsObject.ProgramTheme == "BaseDark");

			for (var i = 0; i < _availableAccents.Length; ++i)
			{
			    if (_availableAccents[i] != Program.OptionsObject.ProgramAccentColor)
                    continue;

			    AccentColor.SelectedIndex = i;
			    break;
			}

			for (var i = 0; i < Program.Translations.AvailableLanguages.Length; ++i)
			{
				LanguageBox.Items.Add(Program.Translations.AvailableLanguages[i]);

				if (Program.OptionsObject.Language == Program.Translations.AvailableLanguageIDs[i])
					LanguageBox.SelectedIndex = i;
			}

			if (Program.OptionsObject.EditorAutoSave)
			{
				var seconds = Program.OptionsObject.EditorAutoSaveInterval;

				if (seconds < 60)
					AutoSave.SelectedIndex = 1;
				else if (seconds <= 300)
					AutoSave.SelectedIndex = Math.Max(1, Math.Min(seconds / 60, 5)) + 1;
				else if (seconds == 600)
					AutoSave.SelectedIndex = 7;
				else
					AutoSave.SelectedIndex = 8;
			}
			else
				AutoSave.SelectedIndex = 0;

            HighlightDeprecateds.IsChecked = Program.OptionsObject.SHHighlightDeprecateds;
            FontSizeD.Value = Program.OptionsObject.EditorFontSize;
            ScrollSpeed.Value = Program.OptionsObject.EditorScrollLines;
            WordWrap.IsChecked = Program.OptionsObject.EditorWordWrap;
            AgressiveIndentation.IsChecked = Program.OptionsObject.EditorAgressiveIndentation;
            LineReformatting.IsChecked = Program.OptionsObject.EditorReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.OptionsObject.EditorReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.OptionsObject.EditorAutoCloseBrackets;
			AutoCloseStringChars.IsChecked = Program.OptionsObject.EditorAutoCloseStringChars;
			ShowSpaces.IsChecked = Program.OptionsObject.EditorShowSpaces;
			ShowTabs.IsChecked = Program.OptionsObject.EditorShowTabs;
			FontFamilyTB.Text = $"{Program.Translations.FontFamily} ({Program.OptionsObject.EditorFontFamily}):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.OptionsObject.EditorFontFamily);
			IndentationSize.Value = Program.OptionsObject.EditorIndentationSize;
			HardwareSalts.IsChecked = Program.OptionsObject.ProgramUseHardwareSalts;
            LoadSH();
        }

        private void ToggleRestartText(bool fullEffect = false)
        {
            if (!_allowChanging)
                return;

            if (_restartTextIsShown)
                return;

            StatusTextBlock.Content = fullEffect
                ? Program.Translations.RestartEdiFullEff
                : Program.Translations.RestartEdiEff;

            _restartTextIsShown = true;
        }

        private void Language_Translate()
        {
            if (Program.Translations.IsDefault)
                return;

            ResetButton.Content = Program.Translations.ResetOptions;
            ProgramHeader.Header = $" {Program.Translations.Program} ";
            HardwareAcc.Content = Program.Translations.HardwareAcc;
            UIAnimation.Content = Program.Translations.UIAnim;
            OpenIncludes.Content = Program.Translations.OpenInc;
            OpenIncludesRecursive.Content = Program.Translations.OpenIncRec;
            AutoUpdate.Content = Program.Translations.AutoUpdate;
            ShowToolBar.Content = Program.Translations.ShowToolbar;
            DynamicISAC.Content = Program.Translations.DynamicISAC;
            DarkTheme.Content = Program.Translations.DarkTheme;
            ThemeColorLabel.Content = Program.Translations.ThemeColor;
            LanguageLabel.Content = Program.Translations.LanguageStr;
            EditorHeader.Header = $" {Program.Translations.Editor} ";
            FontSizeBlock.Text = Program.Translations.FontSize;
            ScrollSpeedBlock.Text = Program.Translations.ScrollSpeed;
            WordWrap.Content = Program.Translations.WordWrap;
            AgressiveIndentation.Content = Program.Translations.AggIndentation;
            LineReformatting.Content = Program.Translations.ReformatAfterSem;
            TabToSpace.Content = Program.Translations.TabsToSpace;
            AutoCloseBrackets.Content = $"{Program.Translations.AutoCloseBrack} (), [], {{}}";
            AutoCloseStringChars.Content = $"{Program.Translations.AutoCloseStrChr} \"\", ''";
            ShowSpaces.Content = Program.Translations.ShowSpaces;
            ShowTabs.Content = Program.Translations.ShowTabs;
            IndentationSizeBlock.Text = Program.Translations.IndentationSize;
            SyntaxHighBlock.Text = Program.Translations.SyntaxHigh;
            HighlightDeprecateds.Content = Program.Translations.HighDeprecat;
            AutoSaveBlock.Text = Program.Translations.AutoSaveMin;
        }
    }
}
