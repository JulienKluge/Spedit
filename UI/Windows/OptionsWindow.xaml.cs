using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;

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

        private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.OptionsObject.Editor_FontSize = size;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            for (int i = 0; i < editors.Length; ++i)
            {
                editors[i].UpdateFontSize(size);
            }
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.OptionsObject.Editor_WordWrap = wrapping;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            for (int i = 0; i < editors.Length; ++i)
            {
                editors[i].editor.WordWrap = wrapping;
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
            for (int i = 0; i < editors.Length; ++i)
            {
                editors[i].editor.FontFamily = family;
            }
        }

        private void HighlightDeprecateds_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.SH_HighlightDeprecateds = HighlightDeprecateds.IsChecked.Value;
            ToggleRestartText();
        }

        /*private void SMInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.SPIncludePath = SMInclude.Text;
        }

        private void SMCopy_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.SPCopyPath = SMCopy.Text;
        }

        private void ServerExec_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.ServerPath = ServerExec.Text;
        }

        private void ServerArg_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.ServerArgs = ServerArg.Text;
        }

        private void OptimizeLevel_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.OptimizationLevel = (int)OptimizeLevel.Value;
        }

        private void VerboseLevel_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.VerboseLevel = (int)VerboseLevel.Value;
        }*/

        private void LoadSettings()
        {
            HardwareAcc.IsChecked = Program.OptionsObject.Program_UseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UI_Animations;
            OpenIncludes.IsChecked = Program.OptionsObject.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.Program_OpenIncludesRecursively;
            if (!Program.OptionsObject.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            HighlightDeprecateds.IsChecked = Program.OptionsObject.SH_HighlightDeprecateds;
            /*SMInclude.Text = Program.OptionsObject.SPIncludePath;
            SMCopy.Text = Program.OptionsObject.SPCopyPath;
            ServerExec.Text = Program.OptionsObject.ServerPath;
            ServerArg.Text = Program.OptionsObject.ServerArgs;
            OptimizeLevel.Value = Program.OptionsObject.OptimizationLevel;
            VerboseLevel.Value = Program.OptionsObject.VerboseLevel;*/
            FontSizeD.Value = Program.OptionsObject.Editor_FontSize;
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
