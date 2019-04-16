using Etilize.Integration;
using Etilize.Models;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EtilizeUI
{
    public partial class EtilizeForm : Form
    {
        #region FormBehaviour
        private bool Drag;
        private int MouseX;
        private int MouseY;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }
        private void PanelMove_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            MouseX = Cursor.Position.X - this.Left;
            MouseY = Cursor.Position.Y - this.Top;
        }
        private void PanelMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (Drag)
            {
                this.Top = Cursor.Position.Y - MouseY;
                this.Left = Cursor.Position.X - MouseX;
            }
        }
        private void PanelMove_MouseUp(object sender, MouseEventArgs e) { Drag = false; }
        #endregion

        Integration integration { get; set; }
        string lblInformationString;

        public EtilizeForm(EtilizeDocumentConfiguration documentConfiguration, string lblInformationString)
        {
            InitializeComponent();
            this.lblInformationString = lblInformationString;

            integration = new Integration(documentConfiguration);
            integration.UpdateProgress += UpdateProgress;
            integration.UpdateProgressText += UpdateProgressText;
            integration.UpdateProgressSubTitle += UpdateProgressSubTitle;
            integration.UpdateStep += UpdateStep;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.LblInformation.Text = this.lblInformationString;
                backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                integration.StartProcess();
            }
            catch (Exception error)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
            this.Dispose();
        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= 100)
            {
                // Change the value of the ProgressBar to the BackgroundWorker progress.
                pbStatus.Value = e.ProgressPercentage;
                // Set the text.
                this.Text = e.ProgressPercentage.ToString();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        public void UpdateProgress(int value)
        {
            backgroundWorker1.ReportProgress(value);
        }

        /// <summary>
        /// </summary>
        /// <param name="progressText"></param>
        public void UpdateProgressText(string progressText)
        {
            lblStatus.Invoke(new Action(() =>
            {
                lblStatus.Text = progressText;
            }));
        }

        /// <summary>
        /// </summary>
        /// <param name="progressText"></param>
        public void UpdateProgressSubTitle(string text)
        {
            label2.Invoke(new Action(() =>
            {
                label2.Text = text;
            }));
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        public void UpdateStep(int value)
        {
            Color.FromArgb(0, 106, 170);
            base.Invoke(new Action(() =>
            {
                this.PbSteps.Value = value;
                this.PbSteps.ForeColor = Color.Red;
                this.PbSteps.Style = ProgressBarStyle.Continuous;
            }));
        }
    }
}
