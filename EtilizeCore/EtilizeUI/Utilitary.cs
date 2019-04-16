using Etilize.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
            string ITCUrl = ConfigurationManager.AppSettings["ICTUrl"].ToString(CultureInfo.InvariantCulture);
            bool flag;
            try
            {
                using (WebClient client = new WebClient())
                {
                    WebProxy proxy = (WebProxy)WebProxy.GetDefaultProxy();
                    if (proxy.Address != null)
                    {
                        proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                        client.Credentials = CredentialCache.DefaultCredentials;
                    }
                    using (client.OpenRead(ITCUrl))
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

        /// <summary>
        /// get the text in the middle of the brakets
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetInformationLbl(string[] parameters) 
        {
            try 
            {
                string fullString = String.Join(" ", parameters);
                string output = fullString.Split('[', ']')[1];
                return SplitString(output, 53);
            }
            catch(Exception e){}
            return "";

            //string lblInformation = "";
            //if (parameters.Length >= 7) 
            //{
            //    lblInformation = parameters[6];
            //}
            //parameters = ClearParameters(parameters);
            //if (parameters.Length >= 1) 
            //{
            //    lblInformation = parameters[0].ToString();
            //}
            //return SplitString(lblInformation, 53);
        }

        /// <summary>
        /// split the string in small parts saving the words
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lettersByLine"></param>
        /// <returns></returns>
        private static string  SplitString(string text, int lettersByLine) 
        {
            string finalString = "";
            try
            {
                if (!string.IsNullOrEmpty(text)) 
                {
                    if (text.Length <= lettersByLine)
                    {
                        return text;
                    }
                    else
                    {
                        string[] splitedString = text.Split(' ', '\t');
                        int size = 0;
                        int lastCutIndex = 0;
                        foreach (var word in splitedString)
                        {
                            if (!string.IsNullOrEmpty(word))
                            {
                                if (size >= lettersByLine)
                                {
                                    finalString += Environment.NewLine;
                                    lastCutIndex = finalString.Length;
                                }
                                size = finalString.Length - lastCutIndex;
                                finalString += word + " ";
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {}
            return finalString;
        }


        public static string[] ClearParameters(string[] parameters)
        {
            List<string> cleanParameterList = new List<string>();
            if (parameters.Length > 1)
            {
                string parameter = "";
                bool buildPath = false;
                for (int i = 6; i < parameters.Length; i++)
                {
                    if (!String.IsNullOrEmpty(parameters[i]))
                    {
                        if (parameters[i].StartsWith("["))
                        {
                            buildPath = true;
                        }

                        if (buildPath)
                        {
                            parameter += parameters[i].ToString() + ((parameters[i].EndsWith("]")) ? "" : " ");
                        }

                        if (parameters[i].EndsWith("]"))
                        {
                            buildPath = false;
                            parameter = parameter.Replace("[", "");
                            parameter = parameter.Replace("]", "");
                            cleanParameterList.Add(parameter);
                            parameter = "";
                        }
                        else if (!buildPath)
                        {
                            cleanParameterList.Add(parameters[i]);
                        }
                    }
                }
            }
            return cleanParameterList.ToArray();
        }

    }
}
