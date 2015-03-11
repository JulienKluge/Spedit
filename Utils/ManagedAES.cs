using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Spedit.Utils
{
    public static class ManagedAES
    {
        public static string Encrypt(string plainText)
        {
            if (plainText.Length < 1)
            {
                return string.Empty;
            }
            try
            {
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
                var encryptor = symmetricKey.CreateEncryptor(Program.OptionsObject.Program_CryptoKey, Encoding.ASCII.GetBytes("SPEdit.Utils.AES")); //so cool that this matches :D
                byte[] cipherTextBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(buffer, 0, buffer.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memoryStream.ToArray();
                        cryptoStream.Close();
                    }
                    memoryStream.Close();
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
                var decryptor = symmetricKey.CreateDecryptor(Program.OptionsObject.Program_CryptoKey, Encoding.ASCII.GetBytes("SPEdit.Utils.AES"));
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd(new char[] { '\0' });
            }
            catch (Exception) { }
            return string.Empty;
        }
    }
}
