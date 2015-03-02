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

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow : MetroWindow
    {
        public SPDefinitionWindow()
        {
            InitializeComponent();
            for (int i = 0; i < Program.spDefinition.Functions.Length; ++i)
            {
                SPFunctionsListBox.Items.Add(Program.spDefinition.Functions[i].Name);
            }
            for (int i = 0; i < Program.spDefinition.Constants.Length; ++i)
            {
                ConstantsList.Items.Add(Program.spDefinition.Constants[i]);
            }
            for (int i = 0; i < Program.spDefinition.Types.Length; ++i)
            {
                TypesList.Items.Add(Program.spDefinition.Types[i]);
            }
            for (int i = 0; i < Program.spDefinition.MethodNames.Length; ++i)
            {
                MethodsList.Items.Add(Program.spDefinition.MethodNames[i]);
            }
            if (SPFunctionsListBox.Items.Count > 0)
            {
                SPFunctionsListBox.SelectedIndex = 0;
            }
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SPFunctionNameBlock.Text = Program.spDefinition.Functions[SPFunctionsListBox.SelectedIndex].Name;
            SPFunctionFullNameBlock.Text = Program.spDefinition.Functions[SPFunctionsListBox.SelectedIndex].FullName;
            SPFunctionCommentBox.Text = Program.spDefinition.Functions[SPFunctionsListBox.SelectedIndex].Comment;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((TextBox)sender).Text;
            bool foundOccurence = false;
            for (int i = 0; i < Program.spDefinition.Functions.Length; ++i)
            {
                if (Program.spDefinition.Functions[i].Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase))
                {
                    SPFunctionsListBox.SelectedIndex = i;
                    SPFunctionsListBox.ScrollIntoView(SPFunctionsListBox.SelectedItem);
                    foundOccurence = true;
                    break;
                }
            }
            if (foundOccurence)
            {
                ((TextBox)sender).Background = null;
            }
            else
            {
                ((TextBox)sender).Background = Brushes.LightYellow;
            }
        }
    }
}
