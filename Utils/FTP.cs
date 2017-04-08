using System.IO;
using System.Net;
using System.Text;

namespace Spedit.Utils
{
    public class FTP
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private FtpWebRequest _ftpRequest;
        private Stream _ftpStream;
        private const int BufferSize = 2048;

        public FTP(string hostIP, string userName, string password)
        {
            _host = hostIP;
            _user = userName;
            _pass = password;
        }

        //thanks to: http://www.codeproject.com/Tips/443588/Simple-Csharp-FTP-Class
        public void Upload(string remoteFile, string localFile)
        {
            var requestUri = new StringBuilder(_host);

            if (_host[_host.Length - 1] == '/')
                requestUri.Append(remoteFile[0] == '/' ? remoteFile.Substring(1) : remoteFile);
            else
            {
                if (remoteFile[0] == '/')
                    requestUri.Append(remoteFile);
                else
                {
                    requestUri.Append("/");
                    requestUri.Append(remoteFile);
                }
            }

            _ftpRequest = (FtpWebRequest) WebRequest.Create(requestUri.ToString());
            _ftpRequest.Credentials = new NetworkCredential(_user, _pass);
            _ftpRequest.UseBinary = true;
            _ftpRequest.UsePassive = true;
            _ftpRequest.KeepAlive = true;
            _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            _ftpStream = _ftpRequest.GetRequestStream();
            var localFileStream = new FileStream(localFile, FileMode.Open);
            var byteBuffer = new byte[BufferSize];
            var bytesSent = localFileStream.Read(byteBuffer, 0, BufferSize);
            while (bytesSent != 0)
            {
                _ftpStream.Write(byteBuffer, 0, bytesSent);
                bytesSent = localFileStream.Read(byteBuffer, 0, BufferSize);
            }
            localFileStream.Close();
            _ftpStream.Close();
            _ftpRequest = null;
        }
    }
}
