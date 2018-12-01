using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;

namespace DocumentManager
{
    public class SecctionDecorator : Decorator
    {
        public SecctionDecorator(WordDocument baseComponent) : base(baseComponent)
        {}

        public override void Default()
        {
            base.Default();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="alignment"></param>
        /// <param name="fontColor"></param>
        public void AddHeader(string text, int fontSize, WdParagraphAlignment alignment, WdColorIndex fontColor)
        {
            foreach (Section section in base.m_baseComponent.document.Sections)
            {
                //Get the header range and add the header details.
                Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
                headerRange.ParagraphFormat.Alignment = alignment;//WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = fontColor; //WdColorIndex.wdBlack;
                headerRange.Font.Size = fontSize; //10;
                headerRange.Text = text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="alignment"></param>
        /// <param name="fontColor"></param>
        public void AddFooter(string text, int fontSize, WdParagraphAlignment alignment, WdColorIndex fontColor)
        {
            //Add the footers into the document
            foreach (Section wordSection in base.m_baseComponent.document.Sections)
            {
                //Get the footer range and add the footer details.
                Range footerRange = wordSection.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                footerRange.Font.ColorIndex = fontColor;
                footerRange.Font.Size = fontSize;
                footerRange.ParagraphFormat.Alignment = alignment;
                footerRange.Text = text;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void SetRange(int start, int end)
        {
            base.m_baseComponent.document.Content.SetRange(start, end);
        }

    }
}
