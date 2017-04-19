using Spedit.UI;
using System;
using System.IO.Pipes;
using System.Text;

namespace Spedit.Interop
{
    public class PipeInteropServer : IDisposable
    {
        private NamedPipeServerStream _pipeServer;
        private readonly MainWindow _window;

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
            _pipeServer.Close();
        }

		public void Dispose()
		{
			_pipeServer.Close();
		}

        private void StartInteropServer()
        {
            if (_pipeServer != null)
            {
                _pipeServer.Close();
                _pipeServer = null;
            }

            _pipeServer = new NamedPipeServerStream("SpeditNamedPipeServer", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _pipeServer.BeginWaitForConnection(PipeConnection_MessageIn, null);
        }

        private void PipeConnection_MessageIn(IAsyncResult iar)
        {
            _pipeServer.EndWaitForConnection(iar);
            var byteBuffer = new byte[4];
            _pipeServer.Read(byteBuffer, 0, sizeof(int));
            var length = BitConverter.ToInt32(byteBuffer, 0);
            byteBuffer = new byte[length];
            _pipeServer.Read(byteBuffer, 0, length);
            var data = Encoding.UTF8.GetString(byteBuffer);
            var files = data.Split('|');
            var selectIt = true;

            foreach (var filePath in files)
            {
                _window.Dispatcher.Invoke(() =>
                {
                    if (!_window.IsLoaded)
                        return;

                    if (!_window.TryLoadSourceFile(filePath, selectIt) ||
                        _window.WindowState != System.Windows.WindowState.Minimized)
                        return;

                    _window.WindowState = System.Windows.WindowState.Normal;
                    selectIt = false;
                });
            }

            StartInteropServer();
        }
    }
}
