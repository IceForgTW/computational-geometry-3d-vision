using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Management;
using DataAdapter;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SVS_Mouseless
{
    public partial class SVS_Mouseless : Form
    {
        public const int MAX_FINGERS = 100;
        public const int FRAME_WIDTH = 320;
        public const int FRAME_HEIGHT = 240;

        [DllImport("HandDetector.dll")]
        public static extern void calcPlane(
            double[] p3Dx,
            double[] p3Dy,
            double[] p3Dz,
            int n,
            ref double A,
            ref double B,
            ref double C,
            ref double D
            );

        [DllImport("HandDetector.dll")]
        public static extern void getHandFrom2Cam(
            IntPtr img1,
            IntPtr img1_bin,
            IntPtr img2,
            IntPtr img2_bin,
            int nGOF,
            int[] nFinger,
            double[] p2D1x,
            double[] p2D1y,
            double[] p2D2x,
            double[] p2D2y,
            double[] p3Dx,
            double[] p3Dy,
            double[] p3Dz,
            double thresMax,
            double thresMin
            );
        int nGOF = 1;
        int[] nFinger;
        double[] p2D1x;
        double[] p2D1y;
        double[] p2D2x;
        double[] p2D2y;
        double[] p3Dx;
        double[] p3Dy;
        double[] p3Dz;
        double[] height;
        bool[] status;

        Plane currentPlane;

        bool isPlaneTrained = false;

        Image<Bgr, byte> bin_img1;
        Image<Bgr, byte> bin_img2;

        Timer timer = new Timer();
        Bitmap bmNonCam = new Bitmap(FRAME_WIDTH, FRAME_HEIGHT);

        private Emgu.CV.Capture capture1;
        private Emgu.CV.Capture capture2;

        ActionRecognitionEngine.ActionRecognitionEngine aRecog = new ActionRecognitionEngine.ActionRecognitionEngine();

        EventGenerationEngine.EventGenerationEngine eGene = new EventGenerationEngine.EventGenerationEngine();

        MiddleEngine.MiddleEngine mEngine = new MiddleEngine.MiddleEngine();

        int iFrameCount = 0;

        public SVS_Mouseless()
        {
            InitializeComponent();
            timer.Interval = 100;
            timer.Tick += new EventHandler(timer_Tick);

            initialize();
        }

        public void initializeWebcam()
        {
            capture1 = new Emgu.CV.Capture(0);
            capture1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, FRAME_WIDTH);
            capture1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT);

            capture2 = new Emgu.CV.Capture(1);
            capture2.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, FRAME_WIDTH);
            capture2.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT);
        }

        public void initializeDLL()
        {
            aRecog.Load_FAR_Plugins(@"D:\Thesis\3D Recontruction\ThucNghiem\SVS_Mouseless\SVS_Mouseless\bin\Debug\FARPlugins\");
            aRecog.Load_AR_Plugins(@"D:\Thesis\3D Recontruction\ThucNghiem\SVS_Mouseless\SVS_Mouseless\bin\Debug\ARPlugins\");
            eGene.Load_EG_Plugins(@"D:\Thesis\3D Recontruction\ThucNghiem\SVS_Mouseless\SVS_Mouseless\bin\Debug\EGPlugins\");
            mEngine.initialize(aRecog, eGene);

            string[] actionsName = aRecog.GetActionsName();
            cbAction.Items.AddRange(actionsName);

            string[] eventName = eGene.GetEventsName();
            cbEvent.Items.AddRange(eventName);

            cbAction.SelectedIndex = 0;
            cbEvent.SelectedIndex = 0;
            tbPriority.Text = "10";
        }

        public void initialize()
        {
            initializeDLL();

            using (Graphics grp = Graphics.FromImage(bmNonCam))
            {
                grp.FillRectangle(Brushes.Black, 0, 0, bmNonCam.Width, bmNonCam.Height);
            }
            pbCam1.Image = bmNonCam;
            pbCam2.Image = bmNonCam;

            nFinger = new int[MAX_FINGERS];
            p2D1x = new double[MAX_FINGERS];
            p2D1y = new double[MAX_FINGERS];
            p2D2x = new double[MAX_FINGERS];
            p2D2y = new double[MAX_FINGERS];
            p3Dx = new double[MAX_FINGERS];
            p3Dy = new double[MAX_FINGERS];
            p3Dz = new double[MAX_FINGERS];
            height = new double[MAX_FINGERS];
            status = new bool[MAX_FINGERS];

            bin_img1 = new Image<Bgr, byte>(FRAME_WIDTH, FRAME_HEIGHT);
            bin_img2 = new Image<Bgr, byte>(FRAME_WIDTH, FRAME_HEIGHT);
        }

        void DrawCentralCircle(Graphics g, int CenterX, int CenterY, int Radius, Pen pen)
        {
            int start = CenterX - Radius;
            int end = CenterY - Radius;
            int diam = Radius * 2;
            g.DrawEllipse(pen, start, end, diam, diam);
        }


        void timer_Tick(object sender, EventArgs e)
        {
            Emgu.CV.Image<Bgr, byte> nextFrame1 = capture1.QueryFrame();
            Emgu.CV.Image<Bgr, byte> nextFrame2 = capture2.QueryFrame();
            //Emgu.CV.Image<Bgr, byte> nextFrame1 = new Image<Bgr, Byte>(
                //"D:\\Thesis\\3D Recontruction\\ThucNghiem\\Get_Frames\\Get_Frames\\Set 4\\Cam1_" + iFrameCount.ToString()+".png");
            //Emgu.CV.Image<Bgr, byte> nextFrame2 = new Image<Bgr, Byte>(
                //"D:\\Thesis\\3D Recontruction\\ThucNghiem\\Get_Frames\\Get_Frames\\Set 4\\Cam2_" + iFrameCount.ToString()+".png");

            if (iFrameCount == 395)
            {
                int trap = 0;
            }

            getHandFrom2Cam(nextFrame1.Ptr,
                bin_img1.Ptr,
                nextFrame2.Ptr,
                bin_img2.Ptr,
                nGOF,
                nFinger,
                p2D1x,
                p2D1y,
                p2D2x,
                p2D2y,
                p3Dx,
                p3Dy,
                p3Dz,
                0.1,
                0.1);

            
            Bitmap bmp1 = new Bitmap( bin_img1.ToBitmap());
            Bitmap bmp2 = new Bitmap( bin_img2.ToBitmap());
            
            //draw fingertips
            Graphics g1 = Graphics.FromImage(bmp1);
            Graphics g2 = Graphics.FromImage(bmp2);

            int temp = 0;
            
            POINT3D p3Dtemp;
            if (isPlaneTrained)
            {
                temp = 0;
                for (int i = 0; i < nGOF; i++)
                {
                    for (int j = 0; j < nFinger[i]; j++)
                    {
                        p3Dtemp.X = p3Dx[temp];
                        p3Dtemp.Y = p3Dy[temp];
                        p3Dtemp.Z = p3Dz[temp];

                        height[temp] = currentPlane.DistanceFromPointToPlane(
                            p3Dtemp
                            );
                        status[temp] = currentPlane.isTouchedPlane(height[temp]);
                        temp++;
                    }
                }
            }

            temp = 0;
            for (int i = 0; i < nGOF; i++)
            {
                for (int j = 0; j < nFinger[i]; j++)
                {
                    Pen pen = new Pen(Color.Blue);
                    if (status[temp])
                    {
                        pen = new Pen(Color.Red);
                    }
                    //Pen pen = new Pen(Color.Blue);
                    //if (j == 0)
                    //{
                    //    pen = new Pen(Color.Red);
                    //}
                    //if (j == 1)
                    //{
                    //    pen = new Pen(Color.Green);
                    //}
                    //if (j == 2)
                    //{
                    //    pen = new Pen(Color.Yellow);
                    //}
                    //if (j == 3)
                    //{
                    //    pen = new Pen(Color.Pink);
                    //}

                    DrawCentralCircle(g1,
                        (int)(p2D1x[temp]),
                        (int)(p2D1y[temp]),
                        5, pen);

                    DrawCentralCircle(g2,
                        (int)(p2D2x[temp]),
                        (int)(p2D2y[temp]), 5, pen);

                    temp++;
                }
            }

            // ve ra man hinh
            pbCam1.Image = bmp1;
            pbCam2.Image = bmp2;

            GroupOfFingers[] GOF = new GroupOfFingers[nGOF];
            temp = 0;
            for (int i = 0; i < nGOF; i++)
            {
                Fingertip[] fingers = Fingertip.FromArray(
                    p2D1x,
                    p2D1y,
                    p2D2x,
                    p2D2y,
                    p3Dx,
                    p3Dy,
                    p3Dz,
                    status,
                    height,
                    nFinger[i],
                    temp
                    );
                temp += nFinger[i];
                GOF[i] = new GroupOfFingers(fingers, nFinger[i]);
            }
            mEngine.Process(GOF);

            lvEventLog.Items.Clear();
            lvEventLog.Items.Add(iFrameCount.ToString());
            string[] rsl = aRecog.getState();
            for (int i = 0; i < rsl.Length; i++)
            {
                lvEventLog.Items.Add(rsl[i]);
                //lvEventLog.Items.Add(height[i].ToString());
            }
            iFrameCount++;

            //if (iFrameCount == 45)
            //{
            //    trainPlane();
            //    nGOF = 2;
            //    iFrameCount = 370;
            //}
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            ListViewItem lvi;
            ListViewItem.ListViewSubItem lvsi;
            lvi = new ListViewItem();
            lvi.Text = cbAction.SelectedItem.ToString();
            lvi.Tag = cbAction.SelectedItem.ToString();

            lvsi = new ListViewItem.ListViewSubItem();
            lvsi.Text = cbEvent.SelectedItem.ToString();
            lvi.SubItems.Add(lvsi);

            lvsi = new ListViewItem.ListViewSubItem();
            lvsi.Text = tbPriority.Text;
            lvi.SubItems.Add(lvsi);

            lvsi = new ListViewItem.ListViewSubItem();
            lvsi.Text = "Activated";
            lvi.SubItems.Add(lvsi);

            lvRelation.Items.Add(lvi);

            mEngine.Bind(cbAction.SelectedItem.ToString(),
                cbEvent.SelectedItem.ToString(),
                Int32.Parse(tbPriority.Text)
                );
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            initializeWebcam();
            nGOF = Int32.Parse(tbNOG.Text);
            timer.Start();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            capture1.Dispose();
            capture2.Dispose();
            timer.Stop();
            //pbCam1.Image = bmNonCam;
            //pbCam2.Image = bmNonCam;
        }

        private void trainPlane()
        {
            double[] p3Dx = new double[MAX_FINGERS];
            double[] p3Dy = new double[MAX_FINGERS];
            double[] p3Dz = new double[MAX_FINGERS];
            int n = 0;
            double A = 0, B = 0, C = 0, D = 0;

            aRecog.GetCurrentFingertips3D(p3Dx, p3Dy, p3Dz, ref n);

            calcPlane(p3Dx, p3Dy, p3Dz, n, ref A, ref B, ref C, ref D);

            currentPlane = new Plane(A, B, C, D, 30);
            isPlaneTrained = true;

            aRecog.reset();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            trainPlane();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        
        private void calibrateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"D:\Thesis\3D Recontruction\ThucNghiem\SVS_Mouseless\Debug\StereoCalibration.exe";
            process.StartInfo.Arguments = @"320 240 6 9 24.5 2 30 Camera.xml 0";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.Start();
            process.WaitForExit();
        }
    }
}