using Etilize.Integration;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace EtilizeUI
{
    public partial class EtilizeForm : Form
    {
        Integration integration { get; set; }
        public EtilizeForm(EtilizeDocumentConfiguration documentConfiguration)
        {
            InitializeComponent();
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
