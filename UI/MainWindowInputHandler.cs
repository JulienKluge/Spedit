using System.Windows;
using System.Windows.Input;

namespace Spedit.UI
{
    public partial class MainWindow
    {
		//some key bindings are handled in EditorElement.xaml.cs because the editor will fetch some keys before they can be handled here.
        private void MainWindowEvent_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsDown)
                return;

            if (e.SystemKey == Key.F10)
            {
                ServerQuery();
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
                {
                    if (e.Key != Key.S)
                        return;

                    Command_SaveAs();
                    e.Handled = true;
                }
                else if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                {
                    switch (e.Key)
                    {
                        case Key.S: { Command_SaveAll(); e.Handled = true; break; }
                        case Key.W: { Command_CloseAll(); e.Handled = true; break; }
                        case Key.P: { Command_FlushFoldingState(true); e.Handled = true; break; }
                        default:
                            // ignored
                            break;
                    }
                }
                else if (!e.KeyboardDevice.IsKeyDown(Key.RightAlt))
                {
                    switch (e.Key)
                    {
                        case Key.N: { Command_New(); e.Handled = true; break; }
                        case Key.O: { Command_Open(); e.Handled = true; break; }
                        case Key.S: { Command_Save(); e.Handled = true; break; }
                        case Key.F: { ToggleSearchField(); e.Handled = true; break; }
                        case Key.W: { Command_Close(); e.Handled = true; break; }
                        case Key.R: { Command_TidyCode(false); e.Handled = true; break; }
                        case Key.P: { Command_FlushFoldingState(false); e.Handled = true; break; }
						case Key.D7: //i hate key mapping...
						case Key.OemQuestion: { Command_ToggleCommentLine(); break; }
                        default:
                            // ignored
                            break;
                    }
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.F3: { Search(); e.Handled = true; break; }
                    case Key.F5: { Compile_SPScripts(); e.Handled = true; break; }
                    case Key.F6: { Compile_SPScripts(false); e.Handled = true; break; }
                    case Key.F7: { Copy_Plugins(); e.Handled = true; break; } //copy
                    case Key.F8: { FTPUpload_Plugins(); e.Handled = true; break; } //ftp upload
                    case Key.F9: { Server_Start(); e.Handled = true; break; }
                    case Key.Escape:
						{
							if (_inCompiling)
							{
								_inCompiling = false;
								e.Handled = true;
							}
							else if (CompileOutputRow.Height.Value > 8.0)
                            {
                                CompileOutputRow.Height = new GridLength(8.0);
                                e.Handled = true;
							}
							break;
                        }
                    default:
                        // ignored
                        break;
                }
            }
        }
    }
}
