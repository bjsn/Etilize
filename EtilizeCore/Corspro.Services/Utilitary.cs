using Microsoft.Win32;
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

        private static string ReadValueFromRegistry(string regKey, string subKey)
        {
            // Opening the registry key
            var baseRegistryKey = Registry.CurrentUser;

            // Open a subKey as read-only
            RegistryKey sk1 = baseRegistryKey.OpenSubKey(regKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
            {
                return null;
            }
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                var skey = sk1.GetValue(subKey);

                if (skey == null)
                {
                    return null;
                }

                return (string)skey;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
