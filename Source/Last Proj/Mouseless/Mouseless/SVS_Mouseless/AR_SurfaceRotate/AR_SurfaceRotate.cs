using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;
using FARInterface;
using System.Runtime.InteropServices;

namespace AR_SurfaceRotate
{
    public class AR_SurfaceRotate : IActionRecognizer
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

        public const int MIN_FRAME = 5;
        public const double MIN_DIST = 7;
        public const double MIN_MOVE_DIST = 3000;

        bool checkMovement(FARInterface.FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int i, int j, ref double deltax, ref double deltay,
            ref double[] p2D1x, ref double[] p2D1y
            )
        {
            int n = MIN_FRAME;
            int lIdx = 0;

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

        public ARResult Recognize(FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int nGOF, int nFARPlugin)
        {
            ARResult rsl = new ARResult();
            rsl.Name = "NULL";

            if (nGOF != 2)
                return rsl;

            double deltax1 = 0;
            double deltay1 = 0;

            double deltax2 = 0;
            double deltay2 = 0;

            double[] p2D1x = new double[100];
            double[] p2D1y = new double[100];
            double[] p2D2x = new double[100];
            double[] p2D2y = new double[100];

            double dist;

            //FingersStatus[currStep][group][j][loai finger action]
            for (int i = 0; i < nFARPlugin; i++)
            {
                if (FingersStatus[currStep][0].Length == 1
                    && FingersStatus[currStep][1].Length == 1)
                {
                    bool b1 = checkMovement(FingersStatus, currStep, Prev, 0, i, ref deltax1, ref deltay1, ref p2D1x, ref p2D1y);
                    bool b2 = checkMovement(FingersStatus, currStep, Prev, 1, i, ref deltax2, ref deltay2, ref p2D2x, ref p2D2y);
                    if (b1 && !b2)
                    {
                        double vx1, vy1;
                        double vx2, vy2;

                        vx1 = p2D1x[0] - p2D2x[0];
                        vy1 = p2D1y[0] - p2D2y[0];

                        vx2 = p2D1x[MIN_FRAME - 1] - p2D2x[0];
                        vy2 = p2D1y[MIN_FRAME - 1] - p2D2y[0];

                        double g = goc(vx1, vy1, vx2, vy2);
                        rsl.Name = GetName();
                        rsl.Params = new object[1];
                        rsl.Params[0] = g;

                        return rsl;
                    }
                    else if (b2 && !b1)
                    {
                        double vx1, vy1;
                        double vx2, vy2;

                        vx1 = p2D2x[0] - p2D1x[0];
                        vy1 = p2D2y[0] - p2D1y[0];

                        vx2 = p2D2x[MIN_FRAME - 1] - p2D1x[0];
                        vy2 = p2D2y[MIN_FRAME - 1] - p2D1y[0];

                        double g = goc(vx1, vy1, vx2, vy2);
                        rsl.Name = GetName();
                        rsl.Params = new object[1];
                        rsl.Params[0] = g;

                        return rsl;
                    }
                }
            }


            return rsl;
        }

        double goc(double vx1, double vy1, double vx2, double vy2)
        {
            double tichvohuong = vx1 * vx2 + vy1 * vy2;
            double d1 = Math.Sqrt(vx1 * vx1 + vy1 * vy1);
            double d2 = Math.Sqrt(vx2 * vx2 + vy2 * vy2);

            double cos = tichvohuong / (d1 * d2);
            return Math.Acos(cos);
        }

        public string GetName()
        {
            return "SURFACE ROTATE ACTION";
        }

        #endregion
    }
}