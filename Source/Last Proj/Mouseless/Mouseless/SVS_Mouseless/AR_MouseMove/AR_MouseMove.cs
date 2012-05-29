using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;
using FARInterface;
using System.Runtime.InteropServices;

namespace AR_MouseMove
{
    public class AR_MouseMove : IActionRecognizer
    {
        #region IActionRecognizer Members

        [DllImport("HandDetector.dll")]
        public static extern void calcLine(
            double[] p2Dx,
            double[] p2Dy,
            int n,
            ref double A,
            ref double B,
            ref double C
            );

        public const int KEY_FINGER = 0;
        public const int MIN_DETECTED_FINGER_NUM = 1;

        public const int MIN_FRAME = 5;
        public const double MIN_DIST = 7;
        public const double MIN_MOVE_DIST = 100;

        bool checkMovement(FARInterface.FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int i, int j, ref double deltax, ref double deltay)
        {
            int n = MIN_FRAME;
            int lIdx = 0;

            double[] p2D1x = new double[100];
            double[] p2D1y = new double[100];

            int currFing = 0;

            double dist = 0;

            if (FingersStatus[currStep][i][KEY_FINGER][j].Name == "MOVE FINGER")
            {
                p2D1x[0] = Double.Parse(FingersStatus[currStep][i][KEY_FINGER][j].Params[0].ToString());
                p2D1y[0] = Double.Parse(FingersStatus[currStep][i][KEY_FINGER][j].Params[1].ToString());

                bool isOK = true;
                // xet n frame
                currFing = KEY_FINGER;
                for (int k = 1; k < n; k++)
                {
                    lIdx = currStep - k;
                    if (lIdx < 0)           // quay vong index
                        lIdx = FingersStatus.Length - 1;

                    currFing = Prev[lIdx][i][currFing];
                    if (currFing == -1)
                    {
                        isOK = false;
                        break;
                    }
                    if (FingersStatus[lIdx][i][currFing][j].Name != "MOVE FINGER")
                    {
                        isOK = false;
                        break;
                    }
                    p2D1x[k] = Double.Parse(FingersStatus[lIdx][i][currFing][j].Params[0].ToString());
                    p2D1y[k] = Double.Parse(FingersStatus[lIdx][i][currFing][j].Params[1].ToString());
                }

                if (!isOK)
                    return false;

                // neu da lay du n frame move
                // xet xem co di chuyen khong bang viec xem khoang cach giua diem dau va cuoi

                double vxt, vyt;
                vxt = p2D1x[0] - p2D1x[n - 1];
                vyt = p2D1y[0] - p2D1y[n - 1];

                dist = Math.Sqrt(vxt * vxt + vyt * vyt);

                // neu phat hien ko co su di chuyen
                if (dist < MIN_DIST)
                    return false;

                // xac dinh duong thang 
                double A, B, C;
                double vx, vy;
                A = B = C = 0;
                calcLine(p2D1x, p2D1y, n, ref A, ref B, ref C);
                if ((A * B) == 0)
                    return false;
                vx = C / A;
                vy = -C / B;

                if (vx * vxt + vy * vyt > 0)
                {
                    vx = -vx;
                    vy = -vy;
                }
                double temp;
                temp = vx * vx + vy * vy;
                vx = vx / temp;
                vy = vy / temp;

                double scale = dist / MIN_DIST * MIN_MOVE_DIST;

                deltax = (vx * scale);
                deltay = (vy * scale);

                return true;
            }
            return false;
        }

        public ARResult Recognize(FARInterface.FARResult[][][][] FingersStatus,  int currStep, int[][][] Prev, int nGOF, int nFARPlugin)
        {
            // xet ngon tay trai nhat (ngon cai) vi thay thuong detect good ^^
            // thuat toan la xet trong n frame gan nhat
            // neu "do doi" cua frame n-1 va frame 0 du lon thi` xac dinh dang xay ra hanh dong move
            // vector di chuyen dc xac dinh bang cac estimate duong thang fit nhat voi tap point

            ARResult rsl = new ARResult();
            rsl.Name = "NULL";

            
            double deltax = 0;
            double deltay = 0;

            for (int i = 0; i < nGOF; i++)
            {
                if (FingersStatus[currStep][i].Length < MIN_DETECTED_FINGER_NUM)
                    continue;

                for (int j = 0; j < nFARPlugin; j++)
                {
                    // neu key finger di chuyen
                    if (checkMovement(FingersStatus, currStep, Prev, i, j, ref deltax, ref deltay))
                    {
                        rsl.Name = GetName();
                        rsl.Params = new object[2];
                        rsl.Params[0] = (int)deltax;
                        rsl.Params[1] = (int)deltay;
                    }
                }
            }

            return rsl;
        }

        public string GetName()
        {
            return "MOUSE MOVE ACTION";
        }

        #endregion
    }
}