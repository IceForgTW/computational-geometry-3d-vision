using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;
using FARInterface;
using System.Runtime.InteropServices;

namespace AR_SurfaceZoom
{
    public class AR_SurfaceZoom : IActionRecognizer
    {
        [DllImport("HandDetector.dll")]
        public static extern void calcLine(
            double[] p2Dx,
            double[] p2Dy,
            int n,
            ref double A,
            ref double B,
            ref double C
            );

        #region IActionRecognizer Members
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

        public ARResult Recognize(FARInterface.FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int nGOF, int nFARPlugin)
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

            double dist1, dist2;

            //FingersStatus[currStep][group][j][loai finger action]
            for (int i = 0; i < nFARPlugin; i++)
            {
                if (FingersStatus[currStep][0].Length == 1
                    && FingersStatus[currStep][1].Length == 1)
                {
                    if (checkMovement(FingersStatus, currStep, Prev, 0, i, ref deltax1, ref deltay1, ref p2D1x, ref p2D1y)
                        && checkMovement(FingersStatus, currStep, Prev, 1, i, ref deltax2, ref deltay2, ref p2D2x, ref p2D2y))
                    {
                        if (deltax1 * deltax2 + deltay1 * deltay2 < 0) // have zoom
                        {
                            dist1 = Math.Sqrt((p2D1x[MIN_FRAME - 1] - p2D2x[MIN_FRAME - 1]) * (p2D1x[MIN_FRAME - 1] - p2D2x[MIN_FRAME - 1])
                                + (p2D1y[MIN_FRAME - 1] - p2D2y[MIN_FRAME - 1]) * (p2D1y[MIN_FRAME - 1] - p2D2y[MIN_FRAME - 1]));

                            dist2 = Math.Sqrt((p2D1x[0] - p2D2x[0]) * (p2D1x[0] - p2D2x[0])
                                + (p2D1y[0] - p2D2y[0]) * (p2D1y[0] - p2D2y[0]));

                            rsl.Name = GetName();
                            rsl.Params = new object[1];
                            rsl.Params[0] = dist2 / dist1;
                        }
                    }
                }
            }


            return rsl;
        }

        void swap(ref double a, ref double b)
        {
            double temp = a;
            a = b;
            b = temp;
        }

        public string GetName()
        {
            return "SURFACE ZOOM ACTION";
        }

        #endregion
    }
}