using Microsoft.Office.Interop.Word;
using Etilize.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DocumentManager
{
    public class EtilizeDocumentIntegration
    {

        public delegate void UpdateProgressDelegate(int ProgressPercentage);
        public event UpdateProgressDelegate UpdateProgress;
        public delegate void UpdateProgressTextDelegate(string UpdateProgressText);
        public event UpdateProgressTextDelegate UpdateProgressText;
        private EtilizeDocumentConfiguration documentConfiguration;
        public static RichTextBox rtBox = new RichTextBox();
        public EtilizeDocumentIntegration(EtilizeDocumentConfiguration documentConfiguration)
        {
            this.documentConfiguration = documentConfiguration;
        }

        private bool InsertExternalDocument(string savePath, ProposalContentByPart proposalPart, Microsoft.Office.Interop.Word.Application app, Document document, string fileExtension)
        {
            string str3 = "";
            try
            {
                object wdStory = WdUnits.wdStory;
                object end = Missing.Value;
                string fileName = Path.GetFileName(savePath);
                string tempFileSave = savePath.Replace(fileName, "") + "temp" + Guid.NewGuid().ToString("n").Substring(0, 8) + fileExtension;
                str3 = this.TempPartFileFromByteArray(proposalPart.Document, tempFileSave);
                object start = app.ActiveDocument.Content.End - 1;
                using (FileStream output = new FileStream(tempFileSave, FileMode.Open, FileAccess.ReadWrite))
                {
                    document.Range(ref start, ref end).InsertFile(tempFileSave);
                }
                this.DeleteTempFile(str3);
                app.ActiveWindow.Selection.EndKey(ref wdStory);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool IsRTFTextInvalid(string RTF)
        {
            try
            {
                rtBox.Rtf = RTF;
                return string.IsNullOrEmpty(rtBox.Text);
            }
            catch (Exception)
            {
                rtBox = new RichTextBox();
                return true;
            }
        }

        public List<KeyValuePair<string, string>> SplitRTFInReadableFormat(string RTF)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            RichTextBox box = new RichTextBox
            {
                Rtf = RTF
            };
            string text = box.Text;
            int num = 0;
            int index = 0;
            int num5 = 0;
            int startIndex = 0;
            string str2 = "\n";
            int length = str2.Length;
            while (num5 < text.Length)
            {
                try
                {
                    startIndex += index;
                    index = text.Substring(startIndex, text.Length - startIndex).IndexOf(str2, (int)(num + length));
                    if (index < 0)
                    {
                        index = text.Length - startIndex;
                    }
                    num5 += index;
                    box.SelectionStart = startIndex;
                    box.SelectionLength = index;
                    if (box.SelectionBullet)
                    {
                        string str3 = box.SelectedText.Replace(str2, "");
                        if (!string.IsNullOrEmpty(str3))
                        {
                            str3 = str3.Replace("&reg", "\x00ae");
                            str3 = str3.Replace("&trade;", "™");
                            list.Add(new KeyValuePair<string, string>("b", str3));
                        }
                    }
                    else
                    {
                        string str4 = box.SelectedText.Replace(str2, "");
                        if (!string.IsNullOrEmpty(str4))
                        {
                            str4 = str4.Replace("&reg", "\x00ae");
                            str4 = str4.Replace("&trade;", "™");
                            list.Add(new KeyValuePair<string, string>("p", str4));
                        }
                    }
                }
                catch (Exception exception1)
                {
                    throw new Exception(exception1.Message);
                }
            }
            return list;
        }

        /// <summary>
        /// </summary>
        /// <param name="proposalContentByParts"></param>
        /// <param name="savePath"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void StarEtilizeDocAssebly(List<ProposalContentByPart> proposalContentByParts, string savePath, int height = 0, int width = 0)
        {
            try
            {
                object range = Missing.Value;
                WordDocument baseComponent = new WordDocument();
                Microsoft.Office.Interop.Word.Application winword = baseComponent.NewApp();
                Document document = baseComponent.New(winword);
                document.Range(0, 0);
                ImageDecorator decorator = new ImageDecorator(baseComponent);
                bool useNormalStyleForList = winword.Options.UseNormalStyleForList;
                if (!winword.Options.UseNormalStyleForList)
                {
                    winword.Options.UseNormalStyleForList = true;
                }
                object wdStory = WdUnits.wdStory;
                if (proposalContentByParts.Count > 0)
                {
                    int num = 0;
                    this.UpdateProgress(0);
                    foreach (ProposalContentByPart part in proposalContentByParts)
                    {
                        num++;
                        int progressPercentage = (num * 100) / proposalContentByParts.Count;
                        this.UpdateProgress(progressPercentage);
                        this.UpdateProgressText("Assembling content for " + part.PartNumber);

                        if (part.Document != null)
                        {
                            if (!this.InsertExternalDocument(savePath, part, winword, document, ".doc"))
                            {
                                this.InsertExternalDocument(savePath, part, winword, document, ".docx");
                            }
                            continue;
                        }

                        if (((!this.documentConfiguration.ExcludeIfNoPic) || !string.IsNullOrEmpty(part.ProductPicturePath)) && ((((part.Document != null) || !this.IsRTFTextInvalid(part.MarketingInfo)) || !this.IsRTFTextInvalid(part.FeatureBullets)) && (this.documentConfiguration.MarketingInfo || this.documentConfiguration.Benefits)))
                        {
                            if (!string.IsNullOrEmpty(part.ProductName))
                            {
                                string productName = part.ProductName;
                                if (!string.IsNullOrEmpty(part.Optional))
                                {
                                    string str3 = part.Optional.ToLower().Equals("y") ? "(Optional)" : "";
                                    productName = productName + " " + str3;
                                }
                                var paragraph = document.Paragraphs.Add(ref range);
                                paragraph.Range.Text = productName;
                                paragraph.Range.set_Style("Heading 3");
                                paragraph.Range.InsertParagraphAfter();
                                if (!string.IsNullOrEmpty(part.ProductPicturePath) && this.documentConfiguration.Picture)
                                {
                                    decorator.AddImage(paragraph.Range, part.ProductPicturePath, WdWrapType.wdWrapTight, WdShapePosition.wdShapeRight, WdShapePosition.wdShapeTop, 180, 180, winword);
                                }
                                paragraph.Range.InsertParagraphAfter();
                                winword.ActiveWindow.Selection.EndKey(ref wdStory);

                                if (this.documentConfiguration.Benefits)
                                {
                                    List<KeyValuePair<string, string>> source = this.SplitRTFInReadableFormat(part.FeatureBullets);
                                    if (source.Count<KeyValuePair<string, string>>() > 0)
                                    {
                                        Paragraph paragraph2 = document.Content.Paragraphs.Add();
                                        paragraph2.Range.set_Style("Normal");
                                        paragraph2.Range.ListFormat.ApplyBulletDefault();
                                        int num3 = 0;
                                        foreach (KeyValuePair<string, string> pair in source)
                                        {
                                            if (!string.IsNullOrEmpty(pair.Value))
                                            {
                                                string text = pair.Value;
                                                if (num3 < (source.Count - 1))
                                                {
                                                    text = text + "\n";
                                                }
                                                paragraph2.Range.InsertBefore(text);
                                            }
                                            num3++;
                                        }
                                        paragraph2.Range.InsertParagraphAfter();
                                        winword.ActiveWindow.Selection.EndKey(ref wdStory);
                                    }
                                    winword.ActiveWindow.Selection.EndKey(ref wdStory);
                                }
                                if (this.documentConfiguration.MarketingInfo)
                                {
                                    bool flag2 = false;
                                    foreach (KeyValuePair<string, string> pair2 in this.SplitRTFInReadableFormat(part.MarketingInfo))
                                    {
                                        string key = pair2.Key;
                                        if (key != null)
                                        {
                                            if (key != "p")
                                            {
                                                if (key != "b")
                                                {
                                                    continue;
                                                }
                                                Paragraph paragraph5 = document.Content.Paragraphs.Add();
                                                paragraph5.Range.ListFormat.ApplyBulletDefault();
                                                paragraph5.Range.InsertBefore(pair2.Value);
                                                flag2 = true;
                                                continue;
                                            }
                                            if (flag2)
                                            {
                                                document.Content.Paragraphs.Add();
                                            }
                                            Paragraph range2 = document.Content.Paragraphs.Add();
                                            range2.Range.Text = pair2.Value + "\n";
                                            range2.Range.InsertParagraphAfter();
                                            flag2 = false;
                                        }
                                    }
                                }
                                winword.ActiveWindow.Selection.EndKey(ref wdStory);
                                if (this.documentConfiguration.TechInfo)
                                {
                                    List<KeyValuePair<string, string>> source = this.SplitRTFInReadableFormat(part.TechnicalInfo);
                                    if (source.Count<KeyValuePair<string, string>>() > 0)
                                    {
                                        Paragraph paragraph6 = document.Content.Paragraphs.Add();
                                        paragraph6.Range.set_Style("Normal");
                                        paragraph6.Range.Text = "Features of the " + part.ProductName + ":\n";
                                        Paragraph paragraph7 = document.Content.Paragraphs.Add();
                                        paragraph7.Range.InsertParagraphBefore();
                                        paragraph7.Range.set_Style("Normal");
                                        paragraph7.Range.ListFormat.ApplyBulletDefault();
                                        int num4 = 0;
                                        foreach (KeyValuePair<string, string> pair3 in source)
                                        {
                                            if (!string.IsNullOrEmpty(pair3.Value))
                                            {
                                                string text = pair3.Value;
                                                if (num4 < (source.Count - 1))
                                                {
                                                    text = text + "\n";
                                                }
                                                paragraph7.Range.InsertBefore(text);
                                            }
                                            num4++;
                                        }
                                        paragraph7.Range.InsertParagraphAfter();
                                        winword.ActiveWindow.Selection.EndKey(ref wdStory);
                                    }
                                    winword.ActiveWindow.Selection.EndKey(ref wdStory);
                                }
                            }
                        }
                    }
                }
                string fileName = Path.GetFileName(savePath);
                winword.Options.UseNormalStyleForList = useNormalStyleForList;
                baseComponent.SaveAndClose(Path.GetFullPath(savePath).Replace(fileName, ""), fileName);
            }
            catch (Exception e) 
            {
                throw new Exception(e.Message);
            }
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
            string str;
            try
            {
                using (FileStream output = new FileStream(tempFileSave, FileMode.Create, FileAccess.ReadWrite))
                {
                    new BinaryWriter(output).Write(fileBytes);
                    str = tempFileSave;
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return str;
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        private void DeleteTempFile(string url)
        {
            if (File.Exists(url))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(url);
            }
        }

    }
}
