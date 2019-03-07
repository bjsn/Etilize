using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Office.Interop.Word;

namespace DocumentManager
{
    /// <summary>
    /// class make to work with microsoft word files, create and update
    /// </summary>
    public class WordDocument : DocumentBuilderComponent
    {
        private object missing;
        public Document document { get; set; }
        public Application winword { get; set; }
        public string savedPath { get; set; }
        public string fileName { get; set; }

        //base constructor
        public WordDocument()  { }

        //override
        public override void Default() { }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Application NewApp()
        {
            //Create an instance for word app
            winword = new Application
            {
                //Set status for word application is to be visible or not.
                Visible = false,
                
            };
            return winword;
        }

        /// <summary>
        /// </summary>
        /// <param name="winword"></param>
        /// <returns></returns>
        public Document New(Application winword)
        {
           object missing = System.Reflection.Missing.Value;

            //Create a new document
            document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            return document;
        }

        /// <summary>
        /// Save document changes
        /// </summary>
        public void SaveChanges()
        {
            document.Save();
        }

        /// <summary>
        /// Save the document in the machine
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void SaveAs(string path, string name)
        {
            fileName = name;
            savedPath = path;

            object fullPath = path + name;
            document.SaveAs(ref @fullPath, Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatXMLDocument);
        }

        /// <summary>
        /// save in local machine and close the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void SaveAndClose(string path, string name)
        {
            SaveAs(path, name);
            Close();
        }

        /// <summary>
        /// Close the document
        /// </summary>
        public void Close()
        {
            document.Close(ref missing, ref missing, ref missing);
            document = null;
            winword.Quit(ref missing, ref missing, ref missing);
            winword = null;
        }
    }
}
