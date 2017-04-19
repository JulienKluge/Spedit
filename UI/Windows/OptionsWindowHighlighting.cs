using System.Windows;

namespace Spedit.UI.Windows
{
    public partial class OptionsWindow
    {
        private void LoadSH()
        {
            SH_Comments.SetContent("Comments", Program.OptionsObject.SHComments);
            SH_CommentMarkers.SetContent("Comment Markers", Program.OptionsObject.SHCommentsMarker);
            SH_PreProcessor.SetContent("Pre-Processor Directives", Program.OptionsObject.SHPreProcessor);
            SH_Strings.SetContent("Strings / Includes", Program.OptionsObject.SHStrings);
            SH_Types.SetContent("Types", Program.OptionsObject.SHTypes);
            SH_TypesValues.SetContent("Type-Values", Program.OptionsObject.SHTypesValues);
            SH_Keywords.SetContent("Keywords", Program.OptionsObject.SHKeywords);
            SH_ContextKeywords.SetContent("Context-Keywords", Program.OptionsObject.SHContextKeywords);
            SH_Chars.SetContent("Chars", Program.OptionsObject.SHChars);
            SH_Numbers.SetContent("Numbers", Program.OptionsObject.SHNumbers);
            SH_SpecialCharacters.SetContent("Special Characters", Program.OptionsObject.SHSpecialCharacters);
            SH_UnknownFunctions.SetContent("Unknown Functions", Program.OptionsObject.SHUnkownFunctions);
            SH_Deprecated.SetContent("Deprecated Content", Program.OptionsObject.SHDeprecated);
            SH_Constants.SetContent("Parsed Contants", Program.OptionsObject.SHConstants);
            SH_Functions.SetContent("Parsed Functions", Program.OptionsObject.SHFunctions);
            SH_Methods.SetContent("Parsed Methods", Program.OptionsObject.SHMethods);
        }

        private void Comments_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHComments = SH_Comments.GetColor();
            ToggleRestartText();
        }

        private void CommentMarker_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHCommentsMarker = SH_CommentMarkers.GetColor();
            ToggleRestartText();
        }

        private void PreProcessor_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHPreProcessor = SH_PreProcessor.GetColor();
            ToggleRestartText();
        }

        private void String_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHStrings = SH_Strings.GetColor();
            ToggleRestartText();
        }

        private void Types_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHTypes = SH_Types.GetColor();
            ToggleRestartText();
        }

        private void TypeValues_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHTypesValues = SH_TypesValues.GetColor();
            ToggleRestartText();
        }

        private void Keywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHKeywords = SH_Keywords.GetColor();
            ToggleRestartText();
        }

        private void ContextKeywords_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHContextKeywords = SH_ContextKeywords.GetColor();
            ToggleRestartText();
        }

        private void Chars_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHChars = SH_Chars.GetColor();
            ToggleRestartText();
        }

        private void UFunctions_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHUnkownFunctions = SH_UnknownFunctions.GetColor();
            ToggleRestartText();
        }

        private void Numbers_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHNumbers = SH_Numbers.GetColor();
            ToggleRestartText();
        }

        private void SpecialCharacters_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHSpecialCharacters = SH_SpecialCharacters.GetColor();
            ToggleRestartText();
        }

        private void Deprecated_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHDeprecated = SH_Deprecated.GetColor();
            ToggleRestartText();
        }

        private void Constants_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHConstants = SH_Constants.GetColor();
            ToggleRestartText();
        }

        private void Functions_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHFunctions = SH_Functions.GetColor();
            ToggleRestartText();
        }

        private void Methods_Changed(object sender, RoutedEventArgs e)
        {
            if (!_allowChanging)
                return;

            Program.OptionsObject.SHMethods = SH_Methods.GetColor();
            ToggleRestartText();
        }
    }
}
