using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChuongTrinhVeHinhDemo
{
    public partial class Form1 : Form
    {
        Bitmap bmNonCam = new Bitmap(320,  240);
        Graphics grp;

        public Form1()
        {
            InitializeComponent();
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 41292)
            {
                int x = 320 - m.LParam.ToInt32();
                int y = 240 - m.WParam.ToInt32();

                SolidBrush redBrush = new SolidBrush(Color.Red);
                grp.FillEllipse(redBrush, x - 5, y - 5, 10, 10);
                pbCan.Invalidate();
            }
            base.WndProc(ref m);
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            grp = Graphics.FromImage(bmNonCam);
            grp.FillRectangle(Brushes.White, 0, 0, bmNonCam.Width, bmNonCam.Height);
            
            pbCan.Image = bmNonCam;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            grp.FillRectangle(Brushes.White, 0, 0, bmNonCam.Width, bmNonCam.Height);
            pbCan.Invalidate();
        }
    }
}
