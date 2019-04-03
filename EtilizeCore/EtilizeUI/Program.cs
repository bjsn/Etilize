using Etilize.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EtilizeUI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            #if DEBUG  
            //commandLineArgs = new string[7];
            //commandLineArgs[0] = "";
            //commandLineArgs[1] = "ExcludeIfNoPic:False;";
            //commandLineArgs[2] = "Picture:True;" ;
            //commandLineArgs[3] = "Benefits:True;";
            //commandLineArgs[4] = "MarketingInfo:True;";
            //commandLineArgs[5] = "TechInfo:False;";
            //commandLineArgs[6] = "[test]";
            #endif


            if (!Utilitary.CheckForInternetConnection())
            {
                MessageBox.Show("The proposal output process will continue using only already-downloaded or user-added content", "Internet access is not available", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            try
            {
                using (EtilizeForm form = new EtilizeForm(Utilitary.GetDocumentConfiguration(commandLineArgs), Utilitary.GetInformationLbl(commandLineArgs)))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception exception1)
            {
                MessageBox.Show(exception1.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Console.WriteLine("Error 500: " + e.Exception.Message);
            Application.Exit();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Error 500: Undefined error");
            Application.Exit();
        }

        private static void HandleError(Exception e)
        {
            Application.Exit();
        }

    }
}
