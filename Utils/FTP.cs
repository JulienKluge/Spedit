using System.IO;
using System.Net;
using System.Text;

namespace Spedit.Utils
{
    public class FTP
    {
        private string host = null;
        private string user = null;
        private string pass = null;
        private FtpWebRequest ftpRequest = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        public FTP(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

        //thanks to: http://www.codeproject.com/Tips/443588/Simple-Csharp-FTP-Class
        public void upload(string remoteFile, string localFile)
        {
			StringBuilder requestUri = new StringBuilder(host);
			if (host[host.Length - 1] == '/')
			{
				if (remoteFile[0] == '/')
				{ requestUri.Append(remoteFile.Substring(1)); }
				else
				{ requestUri.Append(remoteFile); }
			}
			else
			{
				if (remoteFile[0] == '/')
				{ requestUri.Append(remoteFile); }
				else
				{
					requestUri.Append("/");
					requestUri.Append(remoteFile);
				}
			}
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(requestUri.ToString());
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpStream = ftpRequest.GetRequestStream();
            FileStream localFileStream = new FileStream(localFile, FileMode.Open);
            byte[] byteBuffer = new byte[bufferSize];
            int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
            while (bytesSent != 0)
            {
                ftpStream.Write(byteBuffer, 0, bytesSent);
                bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
            }
            localFileStream.Close();
            ftpStream.Close();
            ftpRequest = null;
        }
    } 
}
