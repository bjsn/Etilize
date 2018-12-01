using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Etilize.Integration
{

    public static class RTFParser
    {
        public static RichTextBox rtBox = new RichTextBox();
        public static string DetaultRTFHEader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Microsoft Sans Serif;}{\f2\fnil\fcharset2 Symbol;}}";

        public static string ConvertIntoParragraph(IEnumerable<XElement> rootElement, string subElementName)
        {
            rtBox.BulletIndent = 15;
            rtBox.AcceptsTab = true;
            rtBox.SelectionBullet = false;
            rtBox.Rtf = DetaultRTFHEader;
            string rtf = "";
            int num = 0;
            foreach (XElement element in rootElement.Elements<XElement>())
            {
                string str2 = element.Attribute("name").Value;
                if (!str2.Equals(subElementName))
                {
                    rtBox.SelectionBullet = true;
                    rtBox.SelectionIndent = 15;
                    List<string> list1 = new List<string>();
                    IEnumerator<XElement> enumerator = element.Elements().GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            XElement current = enumerator.Current;
                            if (num != 0)
                            {
                                rtBox.AppendText(Environment.NewLine);
                            }
                            if (!current.Value.Contains("<ul>"))
                            {
                                if (!string.IsNullOrEmpty(current.Value))
                                {
                                    string text = Regex.Replace(current.Value, "<.*?>", string.Empty);
                                    rtBox.AppendText(current.Attribute("name").Value + ": " + text);
                                }
                            }
                            else
                            {
                                List<string> values = new List<string>();
                                string[] separator = new string[] { "</li>" };
                                values.AddRange(current.Value.Split(separator, StringSplitOptions.None));
                                string text = string.Empty;
                                if (values.Count <= 0)
                                {
                                    text = current.Attribute("name").Value;
                                }
                                else
                                {
                                    text = Regex.Replace(string.Join(", ", values), "<.*?>", string.Empty).TrimEnd(new char[0]);
                                    text = text.Substring(0, text.Length - 1);
                                    text = string.IsNullOrEmpty(string.Join("", values)) ? current.Attribute("name").Value : (current.Attribute("name").Value + ": " + text);
                                }
                                text = Regex.Replace(text, "<.*?>", string.Empty);
                                rtBox.AppendText(text);
                            }
                            num++;
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }
            rtBox.SelectAll();
            rtBox.SelectionFont = new Font("Segoe UI", 11f);
            rtf = rtBox.Rtf;
            rtBox.Clear();
            return rtf;
        }

        public static string ConvertIntoParragraph(IEnumerable<XElement> rootElement, string subElementName, string atributeName)
        {
            rtBox.BulletIndent = 15;
            rtBox.SelectionBullet = false;
            rtBox.SelectionIndent = 0;
            rtBox.Rtf = DetaultRTFHEader;
            string rtf = "";
            foreach (XElement element in rootElement.Elements<XElement>())
            {
                string str2 = element.Attribute("name").Value;
                if (str2.Equals(subElementName))
                {
                    List<string> list = new List<string>();
                    foreach (XElement element2 in element.Elements())
                    {
                        if (element2.Attribute("name").Value.Equals(atributeName))
                        {
                            string[] separator = new string[] { "</p>" };
                            list.AddRange(element2.Value.Replace("<br />", "</p>").Replace("<p>", "</p>").Replace("</p>", " </p></p>").Split(separator, StringSplitOptions.None));
                        }
                    }
                    foreach (string str4 in list)
                    {
                        if (str4.Contains("<ul>"))
                        {
                            rtBox.SelectionBullet = true;
                            rtBox.SelectionIndent = 15;
                            List<string> list2 = new List<string>();
                            string[] separator = new string[] { "</li>" };
                            list2.AddRange(str4.Split(separator, StringSplitOptions.None));
                            foreach (string str5 in list2)
                            {
                                string str6 = Regex.Replace(str5, "<.*?>", string.Empty);
                                rtBox.AppendText(str6.TrimStart(new char[0]));
                                rtBox.AppendText(Environment.NewLine);
                            }
                            continue;
                        }
                        rtBox.SelectionIndent = 0;
                        rtBox.SelectionBullet = false;
                        string text = str4.TrimStart(new char[0]);
                        text.Replace("<b />", "<b/>").Replace("<b >", "<b>");
                        int length = rtBox.Text.Length;
                        rtBox.AppendText(text);
                        rtBox.AppendText(Environment.NewLine);
                    }
                }
            }
            rtBox.Text = Regex.Replace(rtBox.Text, "<.*?>", string.Empty);
            rtBox.SelectAll();
            rtf = rtBox.Rtf;
            rtBox.Clear();
            return rtf;
        }

        public static string ConvertXMLIntoBullets(IEnumerable<XElement> rootElement, string subElementName)
        {
            rtBox.BulletIndent = 15;
            rtBox.Rtf = DetaultRTFHEader;
            if (rootElement.Elements<XElement>().Count<XElement>() > 0)
            {
                rtBox.SelectionIndent = 15;
                rtBox.SelectionBullet = true;
            }
            string rtf = "";
            int num = 0;
            foreach (XElement element in rootElement.Elements<XElement>())
            {
                if (element.Name.LocalName.ToString().Equals(subElementName))
                {
                    if (num != 0)
                    {
                        rtBox.AppendText(Environment.NewLine);
                    }
                    rtBox.AppendText(element.Value.TrimStart(new char[0]));
                    num++;
                }
            }
            rtBox.SelectAll();
            rtBox.SelectionFont = new Font("Segoe UI", 11f);
            rtf = rtBox.Rtf;
            rtBox.SelectionBullet = false;
            rtBox.SelectionIndent = 0;
            var aaaa = rtBox.Text;
            rtBox.Clear();
            return rtf;
        }

        public static List<int> GetAllIndexes(string source, string matchString)
        {
            List<int> list = new List<int>();
            matchString = Regex.Escape(matchString);
            foreach (Match match in Regex.Matches(source, matchString))
            {
                list.Add(match.Index);
            }
            return list;
        }
    }
}

