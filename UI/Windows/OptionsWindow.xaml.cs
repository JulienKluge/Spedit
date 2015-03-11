using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;
using System.Windows;
using System.Windows.Media;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow : MetroWindow
    {
        bool RestartTextIsShown = false;
        bool AllowChanging = false;
        public OptionsWindow()
        {
            InitializeComponent();
            LoadSettings();
            AllowChanging = true;
        }

        private void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            Program.OptionsObject = new OptionsControl();
            Program.MainWindow.ShowMessageAsync("Restart Editor", "You have to restart the editor for the changes to have effect.", MessageDialogStyle.Affirmative, this.MetroDialogOptions);
            this.Close();
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
            Program.OptionsObject.Editor_ScrollSpeed = ScrollSpeed.Value / 1000.0;
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
            HardwareAcc.IsChecked = Program.OptionsObject.Program_UseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UI_Animations;
            OpenIncludes.IsChecked = Program.OptionsObject.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.Program_OpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.OptionsObject.Program_CheckForUpdates;
            if (!Program.OptionsObject.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            HighlightDeprecateds.IsChecked = Program.OptionsObject.SH_HighlightDeprecateds;
            FontSizeD.Value = Program.OptionsObject.Editor_FontSize;
            ScrollSpeed.Value = Program.OptionsObject.Editor_ScrollSpeed * 1000.0;
            WordWrap.IsChecked = Program.OptionsObject.Editor_WordWrap;
            FontFamilyTB.Text = "Font(" + Program.OptionsObject.Editor_FontFamily + "):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.OptionsObject.Editor_FontFamily);
            LoadSH();
        }

        private void ToggleRestartText()
        {
            if (AllowChanging)
            {
                if (!RestartTextIsShown)
                {
                    StatusTextBlock.Content = "Restart editor to take effect...";
                    RestartTextIsShown = true;
                }
            }
        }
    }
}
