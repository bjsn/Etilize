using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Corspro.Services
{
    public static class Utilitary
    {
        public static string DecryptCorsProServerMessage(string i_Message, string ivDecrypt)
        {
            string str = string.Empty;
            string s = "kljsdkkdlo4454GG";
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = bytes;
                aes.IV = Convert.FromBase64String(ivDecrypt);
                aes.Padding = PaddingMode.PKCS7;
                byte[] buffer = Convert.FromBase64String(i_Message.Substring(0, i_Message.Length));
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream stream2 = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        stream2.Write(buffer, 0, buffer.Length);
                        stream2.Close();
                        str = Encoding.UTF8.GetString(stream.ToArray());
                    }
                }
            }
            return str;
        }
    }
}
