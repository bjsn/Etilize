using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.DocumentManager
{
    public class Utilitary
    {

        public static bool CheckIfDocumentIsValid(string documentPath) 
        {
            try
            {
                Application ap = new Application();
                ap.Visible = false;
                Document document = ap.Documents.Open(@documentPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
