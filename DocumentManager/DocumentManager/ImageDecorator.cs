using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using System.IO;

namespace DocumentManager
{
    public class ImageDecorator : Decorator
    {
        public ImageDecorator(WordDocument baseComponent) : base(baseComponent)
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
        /// Add image to the document word and send the paramenter to set it up
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wdWrapTight"></param>
        /// <param name="with"></param>
        /// <param name="height"></param>
        /// <param name="horizontalPossition"></param>
        /// <param name="vericalPossition"></param>
        public void AddImage(Range range, string path, WdWrapType wdWrapTight = WdWrapType.wdWrapInline, WdShapePosition horizontalPossition = WdShapePosition.wdShapeLeft, 
                            WdShapePosition vericalPossition = WdShapePosition.wdShapeTop, int with = 0, int height = 0, Application wordApp = null)
        {
            try
            {
                Object nullobj = System.Reflection.Missing.Value;
                var image = wordApp.ActiveDocument.Shapes.AddPicture(path, false, true, 0, 0, with, height, range);
                image.WrapFormat.Type = wdWrapTight;
                image.Left = (float)horizontalPossition;
                image.Top = (float)vericalPossition;

                //var image = base.m_baseComponent.document.InlineShapes.AddPicture(path, true, false, nullobj);
                //File.AppendAllLines(pathTempFIle, new[] { "base.m_baseComponent.document.InlineShapes.AddPicture(path, true, false, nullobj)" });
                //image.Width = (with == 0) ? image.Width : with;
                //image.Height = (height == 0) ? image.Height : height;
                ////Shape shape = image.ConvertToShape();
                //File.AppendAllLines(pathTempFIle, new[] { "image.ConvertToShape();" });
                ////set the image possition
                //shape.WrapFormat.Type = wdWrapTight;
                //File.AppendAllLines(pathTempFIle, new[] { "wdWrapTight" });
                //shape.Left = (float)horizontalPossition;
                //File.AppendAllLines(pathTempFIle, new[] { "horizontalPossition" });
                //shape.Top = (float)vericalPossition;
                //File.AppendAllLines(pathTempFIle, new[] { "vericalPossition" });
                //shape.Width = (with == 0) ? image.Width : with;
                //File.AppendAllLines(pathTempFIle, new[] { "with" });
                //shape.Height = (height == 0) ? image.Height : height;
                //File.AppendAllLines(pathTempFIle, new[] { "height" });
            }
            catch (Exception e) 
            {
                throw new Exception(e.Message);
            }
        }

    }
}
