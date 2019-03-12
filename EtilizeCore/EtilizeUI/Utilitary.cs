using Etilize.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtilizeUI
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

        public static EtilizeDocumentConfiguration GetDocumentConfiguration(string[] parameters)
        {
            EtilizeDocumentConfiguration configuration = new EtilizeDocumentConfiguration
            {
                ExcludeIfNoPic = false,
                Picture = true,
                Benefits = true,
                MarketingInfo = true,
                TechInfo = true
            };
            if (parameters.Length > 1)
            {
                string str = "";
                int index = 1;
                while (true)
                {
                    if (index >= parameters.Length)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            parameters = str.Trim().Split(new char[] { ';' });
                            PropertyInfo[] properties = configuration.GetType().GetProperties();
                            int num3 = 0;
                            while (num3 < properties.Length)
                            {
                                PropertyInfo info = properties[num3];
                                string[] strArray = parameters;
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= strArray.Length)
                                    {
                                        num3++;
                                        break;
                                    }
                                    string str2 = strArray[num4];
                                    int length = str2.IndexOf(":");
                                    if (length != -1)
                                    {
                                        string str4 = str2.Substring(length + 1, (str2.Length - length) - 1);
                                        if (str2.Substring(0, length).ToUpper().Equals(info.Name.ToUpper()))
                                        {
                                            info.SetValue(configuration, Convert.ToBoolean(str4.ToLower()), null);
                                        }
                                    }
                                    num4++;
                                }
                            }
                        }
                        break;
                    }
                    str = str + parameters[index].ToString();
                    index++;
                }
            }
            return configuration;
        }

        //
        public static string GetInformationLbl(string[] parameters) 
        {
            string lblInformation = "";
            if (parameters.Length >= 6) 
            {
                lblInformation = parameters[5];
            }
            return lblInformation;
        }
    }
}
