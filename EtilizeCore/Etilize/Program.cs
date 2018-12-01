using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using EtilizeUI;
using System.Runtime.InteropServices;


namespace Etilize
{

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            string[] args = Environment.GetCommandLineArgs();
            Form1 form = new Form1();
            Application.Run(form);

        }
    }
}
