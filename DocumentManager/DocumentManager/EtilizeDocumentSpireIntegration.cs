using Etilize.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace DocumentManager
{
    public class EtilizeDocumentSpireIntegration
    {
        public delegate void UpdateProgressDelegate(int ProgressPercentage);
        public event UpdateProgressDelegate UpdateProgress;
        public delegate void UpdateProgressTextDelegate(string UpdateProgressText);
        public event UpdateProgressTextDelegate UpdateProgressText;
        private EtilizeDocumentConfiguration documentConfiguration;

        /// <summary>
        /// </summary>
        /// <param name="documentConfiguration"></param>
        public EtilizeDocumentSpireIntegration(EtilizeDocumentConfiguration documentConfiguration) 
        {
            this.documentConfiguration = documentConfiguration;
        }

        /// <summary>
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string SaveToTemporaryFile(string html)
        {
            string htmlTempFilePath = Path.Combine(Path.GetTempPath(), string.Format("{0}.html", Path.GetRandomFileName()));
            using (StreamWriter writer = File.CreateText(htmlTempFilePath))
            {
                html = string.Format("<html>{0}</html>", html);

                writer.WriteLine(html);
            }
            return htmlTempFilePath;
        }


        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> SplitHtmlResult(string text)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            string[] htmlTags = { "p", "li" };
            string bkText = "";
            bool fountResult = false;
            text = text.Replace("<br />", "").Replace("</b>", " ").Replace("<b>", "").Replace("</ul>", "").Replace("<ul>", "");
            text = (text.StartsWith(" ") ? text.Substring(1, text.Length) : text);
            while (!String.IsNullOrEmpty(text) && !bkText.Equals(text))
            {
                bkText = text;
                foreach (var tag in htmlTags)
                {
                    string openTag = "<[tag]>";
                    string closetag = "</[tag]>";
                    openTag = openTag.Replace("[tag]", tag);
                    closetag = closetag.Replace("[tag]", tag);
                    if (text.StartsWith(openTag))
                    {
                        int i = text.IndexOf(openTag);
                        if (i > -1)
                        {
                            int j = text.IndexOf(closetag);
                            if (j > -1)
                            {
                                string sub = text.Substring(i, j + (closetag).Length);
                                list.Add(new KeyValuePair<string, string>(tag, sub.Replace(closetag, "").Replace(openTag, "")));
                                text = text.Replace(sub, "");
                                fountResult = true;
                            }
                        }
                    }
                    else if (!text.StartsWith("<") && !text.StartsWith(" "))
                    {
                        int i = text.IndexOf("<");
                        if (i > -1)
                        {
                            string sub = text.Substring(0, i);
                            list.Add(new KeyValuePair<string, string>("p", sub));
                            text = text.Replace(sub, "");
                            fountResult = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = (text.StartsWith(" ") ? text.Substring(1, text.Length - 1) : text);
                    }
                }
            }

            if (!fountResult)
            {
                list.Add(new KeyValuePair<string, string>("p", text));
            }
            return list;

        }

        /// <summary>
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="tempFileSave"></param>
        /// <returns></returns>
        private string TempPartFileFromByteArray(byte[] fileBytes, string tempFileSave)
        {
            try
            {
                FileStream fileSream = new FileStream(tempFileSave, FileMode.Create, FileAccess.ReadWrite);
                BinaryWriter bw = new BinaryWriter(fileSream);
                bw.Write(fileBytes);//filecontent is a byte[]
                fileSream.Close();
                return tempFileSave;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        private void DeleteTempFile(string url)
        {
            if (File.Exists(url))
            {
                File.Delete(url);
            }
        }

    }
}
