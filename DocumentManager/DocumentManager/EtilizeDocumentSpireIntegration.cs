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
        /// <param name="proposalContentByParts"></param>
        /// <param name="savePath"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void StarEtilizeDocAssebly(List<ProposalContentByPart> proposalContentByParts, string savePath, int height = 0, int width = 0)
        {/*
            Document document = new Document();
            document.LoadFromFile(@"C:\CorsPro\PropGen\Data_Byeron\template.doc");

            if (proposalContentByParts.Count > 0)
            {
                int counter = 0;
                UpdateProgress(0);

         
                foreach (var proposalPart in proposalContentByParts)
                {
                    counter++;
                    int total = (int)((counter * 100) / proposalContentByParts.Count);
                    UpdateProgress(total);
                    UpdateProgressText("Assembling content for " + proposalPart.PartNumber);
                    
                    if (proposalPart.Document == null && documentConfiguration.ExcludeIfNoPic && string.IsNullOrEmpty(proposalPart.ProductPicturePath))
                    {
                        continue;
                    }

                    if (proposalPart.Document == null && (string.IsNullOrEmpty(proposalPart.MarketingInfo) && string.IsNullOrEmpty(proposalPart.MarketingInfo)) || (!documentConfiguration.MarketingInfo && !documentConfiguration.Benefits))
                    {
                        continue;
                    }

                    Section section = document.AddSection();
                    if (proposalPart.Document == null && proposalPart.VendorID != 0)
                    {
                        string title = proposalPart.ProductName;
                        if (!string.IsNullOrEmpty(proposalPart.Optional))
                        {
                            string optional = (proposalPart.Optional.ToLower().Equals("y") ? "(Optional)" : "");
                            title += " " + optional;
                        }

                        Paragraph titleParagraph = section.AddParagraph();
                        titleParagraph.ApplyStyle(ParagraphStyle.NameToBuiltIn("Heading 3"));
                        titleParagraph.AppendText(title);

                        Paragraph whiteTitle = section.AddParagraph();
                        if (!String.IsNullOrEmpty(proposalPart.ProductPicturePath) && documentConfiguration.Picture)
                        {
                            DocPicture Pic = whiteTitle.AppendPicture(Image.FromFile(proposalPart.ProductPicturePath));
                            Pic.Width = 180;
                            Pic.Height = 180;
                            Pic.TextWrappingStyle = TextWrappingStyle.Tight;
                            Pic.TextWrappingType = TextWrappingType.Both;
                            Pic.HorizontalAlignment = ShapeHorizontalAlignment.Right;
                        }
                       

                        if (documentConfiguration.Benefits)
                        {
                            string[] bulletItems = proposalPart.FeatureBullets.Replace("</li>", "$").Replace("<li>", "").Replace("</ul>", "").Replace("<ul>", "").Split('$').Where(x => !String.IsNullOrEmpty(x)).ToArray();

                            if (bulletItems.Count() > 0)
                            {
                                for (int j = 0; j < bulletItems.Length; j++)
                                {
                                    Paragraph bullet = section.AddParagraph();
                                    bullet.AppendText(bulletItems[j]);
                                    bullet.ListFormat.ApplyBulletStyle();
                                    bullet.ListFormat.CurrentListLevel.NumberPosition = -10;
                                    if (j == bulletItems.Length - 1)
                                    {
                                        bullet.AppendBreak(BreakType.LineBreak);
                                    }
                                }
                            }
                        }

                        if (documentConfiguration.MarketingInfo)
                        {
                            if (!String.IsNullOrEmpty(proposalPart.MarketingInfo))
                            {
                                bool insertWhiteP = false;
                                List<KeyValuePair<string, string>> results = SplitHtmlResult(proposalPart.MarketingInfo);
                                foreach (var result in results)
                                {
                                    switch (result.Key)
                                    {
                                        case "p":
                                            Paragraph paragraph = section.AddParagraph();
                                            paragraph.ApplyStyle(ParagraphStyle.NameToBuiltIn("Normal"));
                                            if (insertWhiteP)
                                            {
                                                paragraph.AppendBreak(BreakType.LineBreak);
                                            }
                                            paragraph.AppendText(result.Value);
                                            paragraph.AppendBreak(BreakType.LineBreak);
                                            insertWhiteP = false;
                                            break;
                                        case "li":
                                            Paragraph bullet = section.AddParagraph();
                                            bullet.AppendText(result.Value);
                                            bullet.ListFormat.ApplyBulletStyle();
                                            bullet.ListFormat.CurrentListLevel.NumberPosition = -10;
                                            insertWhiteP = true;
                                            break;
                                    }
                                }
                            }
                        }

                        if (documentConfiguration.TechInfo)
                        {
                            Paragraph paragraph = section.AddParagraph();
                            paragraph.AppendBreak(BreakType.LineBreak);
                            paragraph.ApplyStyle(ParagraphStyle.NameToBuiltIn("Normal"));
                            paragraph.AppendText("Features of the " + proposalPart.ProductName + ":");
                            paragraph.AppendBreak(BreakType.LineBreak);

                            proposalPart.TechnicalInfo = proposalPart.TechnicalInfo.Replace("</br>", "").Replace("<br>", "").Replace("</p>", "").Replace("<p>", "");
                            string[] technicalItems = proposalPart.TechnicalInfo.Replace("</li>", "$").Replace("<li>", "").Replace("</ul>", "").Replace("<ul>", "").Split('$').Where(x => !String.IsNullOrEmpty(x)).ToArray();

                            if (technicalItems.Count() > 0)
                            {
                                for (int j = 0; j < technicalItems.Length; j++)
                                {
                                    Paragraph bullet = section.AddParagraph();
                                    bullet.AppendText(technicalItems[j]);
                                    bullet.ListFormat.ApplyBulletStyle();
                                    bullet.ListFormat.CurrentListLevel.NumberPosition = -10;
                                    if (j == technicalItems.Length - 1)
                                    {
                                        bullet.AppendBreak(BreakType.LineBreak);
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        try
                        {
                           
                            if (proposalPart.Document != null)
                            {
                                section.AddParagraph();
                                string fileName = Path.GetFileName(savePath);
                                string tempFilePath = savePath.Replace(fileName, "") + "temp.doc";
                                string filePath = TempPartFileFromByteArray(proposalPart.Document, tempFilePath);
                                Document sourceDoc = new Document();
                                sourceDoc.LoadFromFile(filePath);
                                foreach (Section sec in sourceDoc.Sections)
                                {
                                    foreach (DocumentObject obj in sec.Body.ChildObjects)
                                    {
                                        DocumentObject clonable = obj.Clone();
                                        section.Body.ChildObjects.Add(clonable);
                                    }
                                }
                                DeleteTempFile(filePath);
                            }
                        }
                        catch (Exception e)
                        { 
                        }
                    }
                }
            }
            document.SaveToFile(@"C:\CorsPro\PropGen\Data_Byeron\testDoc.doc", FileFormat.Doc);
            document.Close();

            */
        }


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
