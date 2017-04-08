using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Spedit.Utils
{
    public static class ManagedAES
    {
        private static byte[] _salt;

        public static string Encrypt(string plainText)
        {
            if (plainText.Length < 1)
                return string.Empty;

            try
            {
                var symmetricKey = new RijndaelManaged() {Mode = CipherMode.CBC, Padding = PaddingMode.Zeros};
                var encryptor = symmetricKey.CreateEncryptor(SaltKey(Program.OptionsObject.ProgramCryptoKey),
                    Encoding.ASCII.GetBytes("SPEdit.Utils.AES")); //so cool that this matches :D
                byte[] cipherTextBytes;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        var buffer = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(buffer, 0, buffer.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memoryStream.ToArray();
                    }
                }

                return Convert.ToBase64String(cipherTextBytes);
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }

        public static string Decrypt(string encryptedText)
        {
            if (encryptedText.Length < 1)
                return string.Empty;

            try
            {
                var cipherTextBytes = Convert.FromBase64String(encryptedText);
                var symmetricKey = new RijndaelManaged() {Mode = CipherMode.CBC, Padding = PaddingMode.None};
                var decryptor = symmetricKey.CreateDecryptor(SaltKey(Program.OptionsObject.ProgramCryptoKey),
                    Encoding.ASCII.GetBytes("SPEdit.Utils.AES"));
                string outString;

                using (var memoryStream = new MemoryStream(cipherTextBytes))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        var plainTextBytes = new byte[cipherTextBytes.Length];
                        var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        outString =
                            Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd('\0');
                    }
                }

                return outString;
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }

        private static byte[] SaltKey(byte[] key)
        {
            if (_salt == null)
                CreateSalt();

            if (!Program.OptionsObject.ProgramUseHardwareSalts)
                return key;

            var buffer = new byte[16];

            for (var i = 0; i < 16; ++i)
                if (_salt != null && i < _salt.Length)
                    buffer[i] = (byte) ((uint) key[i] ^ _salt[i]);
                else
                    buffer[i] = key[i];

            return buffer;
        }

        private static void CreateSalt()
        {
            byte[] buffer;
            using (MD5 md5Provider = new MD5CryptoServiceProvider())
            {
                string inString =
                    $"SPEditSalt {CpuId()}{DiskId()}{Environment.ProcessorCount}{(Environment.Is64BitOperatingSystem ? "T" : "F")}";
                var encoder = new UTF8Encoding();
                buffer = md5Provider.ComputeHash(encoder.GetBytes(inString));
            }
            _salt = buffer;
        }

        //thanks to: http://jai-on-asp.blogspot.de/2010/03/finding-hardware-id-of-computer.html
        private static string CpuId()
        {
            var id = string.Empty;

            try
            {
                var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                var mbsList = mbs.Get();

                foreach (var o in mbsList)
                {
                    var mo = (ManagementObject) o;
                    id = mo["ProcessorId"].ToString();
                    break;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return id;
        }

        private static string DiskId()
        {
            var id = string.Empty;

            try
            {
                var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
                dsk.Get();
                id = dsk["VolumeSerialNumber"].ToString();
            }
            catch (Exception)
            {
                // ignored
            }

            return id;
        }
    }
}
