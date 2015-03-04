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
using Spedit.SPCondenser;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow : MetroWindow
    {
        CondensedSourcepawnDefinition def;

        public SPDefinitionWindow()
        {
            InitializeComponent();
            def = Program.Configs[Program.SelectedConfig].GetSMDef();
            if (def == null)
            {
                MessageBox.Show("The config was not able to parse a sourcepawn definiton.", "Stop", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }
            for (int i = 0; i < def.Functions.Length; ++i)
            {
                SPFunctionsListBox.Items.Add(def.Functions[i].Name);
            }
            for (int i = 0; i < def.Constants.Length; ++i)
            {
                ConstantsList.Items.Add(def.Constants[i]);
            }
            for (int i = 0; i < def.Types.Length; ++i)
            {
                TypesList.Items.Add(def.Types[i]);
            }
            for (int i = 0; i < def.MethodNames.Length; ++i)
            {
                MethodsList.Items.Add(def.MethodNames[i]);
            }
            if (SPFunctionsListBox.Items.Count > 0)
            {
                SPFunctionsListBox.SelectedIndex = 0;
            }
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SPFunctionNameBlock.Text = def.Functions[SPFunctionsListBox.SelectedIndex].Name;
            SPFunctionFullNameBlock.Text = def.Functions[SPFunctionsListBox.SelectedIndex].FullName;
            SPFunctionCommentBox.Text = def.Functions[SPFunctionsListBox.SelectedIndex].Comment;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((TextBox)sender).Text;
            bool foundOccurence = false;
            for (int i = 0; i < def.Functions.Length; ++i)
            {
                if (def.Functions[i].Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase))
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
