using Spedit.UI;
using System;
using System.IO.Pipes;
using System.Text;

namespace Spedit.Interop
{
    public class PipeInteropServer : IDisposable
    {
        NamedPipeServerStream pipeServer;
        MainWindow _window;

        public PipeInteropServer(MainWindow window)
        {
            _window = window;
        }

        public void Start()
        {
            StartInteropServer();
        }

        public void Close()
        {
            pipeServer.Close();
        }

		public void Dispose()
		{
			pipeServer.Close();
		}

        private void StartInteropServer()
        {
            if (pipeServer != null)
            {
                pipeServer.Close();
                pipeServer = null;
            }
            pipeServer = new NamedPipeServerStream("SpeditNamedPipeServer", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeServer.BeginWaitForConnection(new AsyncCallback(PipeConnection_MessageIn), null);
        }

        private void PipeConnection_MessageIn(IAsyncResult iar)
        {
            pipeServer.EndWaitForConnection(iar);
            byte[] byteBuffer = new byte[4];
            pipeServer.Read(byteBuffer, 0, sizeof(Int32));
            int length = BitConverter.ToInt32(byteBuffer, 0);
            byteBuffer = new byte[length];
            pipeServer.Read(byteBuffer, 0, length);
            string data = Encoding.UTF8.GetString(byteBuffer);
            string[] files = data.Split('|');
			bool SelectIt = true;
            for (int i = 0; i < files.Length; ++i)
            {
                _window.Dispatcher.Invoke(() =>
                {
                    if (_window.IsLoaded)
                    {
                        if (_window.TryLoadSourceFile(files[i], SelectIt) && _window.WindowState == System.Windows.WindowState.Minimized)
                        {
                            _window.WindowState = System.Windows.WindowState.Normal;
							SelectIt = false;
						}
                    }
                });
            }
            StartInteropServer();
        }
    }
}
