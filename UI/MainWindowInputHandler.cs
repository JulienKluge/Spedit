using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        private void MainWindowEvent_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsDown)
            {
                return;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
                {
                    if (e.Key == Key.S)
                    {
                        Command_SaveAs();
                        e.Handled = true;
                    }
                }
                else if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                {
                    switch (e.Key)
                    {
                        case Key.S: { Command_SaveAll(); e.Handled = true; break; }
                        case Key.W: { Command_CloseAll(); e.Handled = true; break; }
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.N: { Command_New(); e.Handled = true; break; }
                        case Key.O: { Command_Open(); e.Handled = true; break; }
                        case Key.S: { Command_Save(); e.Handled = true; break; }
                        case Key.F: { ToggleSearchField(); e.Handled = true; break; }
                        case Key.W: { Command_Close(); e.Handled = true; break; }
                    }
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.F3: { Search(); e.Handled = true; break; }
                    case Key.F5: { Compile_SPScripts(false); e.Handled = true; break; }
                    case Key.F6: { Compile_SPScripts(true); e.Handled = true; break; }
                    case Key.F7: { Server_Start(); e.Handled = true; break; }
                    case Key.Escape: { CompileOutputRow.Height = new GridLength(8.0); e.Handled = true; break; }
                }
            }
        }
    }
}
