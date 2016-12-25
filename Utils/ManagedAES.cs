using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Spedit.Utils
{
    public static class ManagedAES
    {
		private static byte[] Salt = null;
        public static string Encrypt(string plainText)
        {
            if (plainText.Length < 1)
            {
                return string.Empty;
            }
            try
            {
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
                var encryptor = symmetricKey.CreateEncryptor(SaltKey(Program.OptionsObject.Program_CryptoKey), Encoding.ASCII.GetBytes("SPEdit.Utils.AES")); //so cool that this matches :D
                byte[] cipherTextBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(buffer, 0, buffer.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memoryStream.ToArray();
                    }
                }
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch (Exception) { }
            return string.Empty;
        }

        public static string Decrypt(string encryptedText)
        {
            if (encryptedText.Length < 1)
            {
                return string.Empty;
            }
            try
			{
				byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };
                var decryptor = symmetricKey.CreateDecryptor(SaltKey(Program.OptionsObject.Program_CryptoKey), Encoding.ASCII.GetBytes("SPEdit.Utils.AES"));
				string outString = string.Empty;
				using (var memoryStream = new MemoryStream(cipherTextBytes))
				{
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
					{
						byte[] plainTextBytes = new byte[cipherTextBytes.Length];
						int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
						outString = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd(new char[] { '\0' });
					}
				}
				return outString;
			}
            catch (Exception) { }
            return string.Empty;
        }

		private static byte[] SaltKey(byte[] key)
		{
			if (Salt == null)
			{
				CreateSalt();
			}
			byte[] buffer = new byte[16];
			for (int i = 0; i < 16; ++i)
			{
				if (i < Salt.Length)
					buffer[i] = (byte)((uint)key[i] ^ (uint)Salt[i]);
				else
					buffer[i] = key[i];
			}
			return buffer;
		}

		private static void CreateSalt()
		{
			byte[] buffer;
			using (MD5 md5Provider = new MD5CryptoServiceProvider())
			{
				string inString = $"SPEditSalt {cpuId()}{diskId()}{Environment.ProcessorCount.ToString()}{(Environment.Is64BitOperatingSystem ? "T" : "F")}";
				UTF8Encoding encoder = new UTF8Encoding();
				buffer = md5Provider.ComputeHash(encoder.GetBytes(inString));
			}
			Salt = buffer;
		}

		//thanks to: http://jai-on-asp.blogspot.de/2010/03/finding-hardware-id-of-computer.html
		private static string cpuId()
		{
			string id = string.Empty;
			try
			{
				var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
				ManagementObjectCollection mbsList = mbs.Get();
				foreach (ManagementObject mo in mbsList)
				{
					id = mo["ProcessorId"].ToString();
					break;
				}
			}
			catch (Exception) { }
			return id;
		}
		private static string diskId()
		{
			string id = string.Empty;
			try
			{
				ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
				dsk.Get();
				id = dsk["VolumeSerialNumber"].ToString();
			}
			catch (Exception) { }
			return id;
		}
	}
}
