using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeditUpdater
{
    public partial class UpdateMarquee : Form
    {
        public UpdateMarquee()
        {
            InitializeComponent();
            Bitmap bmp = SpeditUpdater.Properties.Resources.IconPng;
            pictureBox1.Image = (Image)bmp;
        }

        public void SetToReadyState()
        {
            label1.Text = "SPEdit got updated!";
            progressBar1.Visible = false;
            button1.Visible = true;
            this.UseWaitCursor = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
