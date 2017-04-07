using System;
using System.Windows.Forms;

namespace SpeditUpdater
{
    public partial class UpdateMarquee : Form
    {
        public UpdateMarquee()
        {
            InitializeComponent();

            pictureBox1.Image = Properties.Resources.IconPng;
        }

        public void SetToReadyState()
        {
            label1.Text = @"SPEdit got updated!";
            progressBar1.Visible = false;
            button1.Visible = true;
            UseWaitCursor = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
