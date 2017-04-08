using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Spedit.Interop
{
    public static class PipeInteropClient
    {
        public static void ConnectToMasterPipeAndSendData(string data)
        {
            var stringData = Encoding.UTF8.GetBytes(data);
            var stringLength = stringData.Length;
            var array = new byte[sizeof(int) + stringLength];

            using (var stream = new MemoryStream(array))
            {
                var stringLengthData = BitConverter.GetBytes(stringLength);
                stream.Write(stringLengthData, 0, stringLengthData.Length);
                stream.Write(stringData, 0, stringData.Length);
            }

			using (var pipeClient = new NamedPipeClientStream(".", "SpeditNamedPipeServer", PipeDirection.Out, PipeOptions.Asynchronous))
			{
				pipeClient.Connect(5000);
				pipeClient.Write(array, 0, array.Length);
				pipeClient.Flush();
			}
        }
    }
}
