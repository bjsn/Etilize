using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etilize.Integration
{
    public static class Utilitary
    {
        public static bool CheckForInternetConnection()
        {
            bool flag;
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        flag = true;
                    }
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
        public static string CleanFileName(string filename)
        {
            return Regex.Replace(filename, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static int ConvertToInt(string number)
        {
            int num = 0;
            try
            {
                num = int.Parse(number);
            }
            catch (FormatException)
            {
            }
            return num;
        }

        public static string Decrypt(string cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }
            byte[] bytes = Convert.FromBase64String(cipher);
            char[] array = Encoding.Unicode.GetString(bytes).ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }

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

        public static string GetSetupDLProperty(DataTable setupList, string PropertyToFind)
        {
            string str = "";
            try
            {
                foreach (DataRow row in setupList.Rows)
                {
                    str = (row[PropertyToFind] != DBNull.Value) ? ((string)row[PropertyToFind]) : string.Empty;
                }
            }
            catch (Exception exception)
            {
                return ("Error 500: Exception error: " + exception.Message);
            }
            return str;
        }

        public static string ReadValueFromRegistry(string regKey, string subKey)
        {
            RegistryKey key = null;
            if (Environment.UserInteractive)
            {
                key = Registry.CurrentUser.OpenSubKey(regKey);
            }
            else
            {
                RegistryKey key3 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                regKey = @"SOFTWARE\CorsPro";
                key = key3.OpenSubKey(regKey);
                if (key == null)
                {
                    regKey = @"SOFTWARE\WOW6432Node\CorsPro";
                    key = key3.OpenSubKey(regKey);
                }
            }
            if (key == null)
            {
                return null;
            }
            try
            {
                object obj2 = key.GetValue(subKey);
                return ((obj2 != null) ? ((string)obj2) : null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
