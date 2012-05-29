using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;
using FARInterface;
using System.Runtime.InteropServices;

namespace AR_SurfaceDraw
{
    public class AR_SurfaceDraw : IActionRecognizer
    {
        #region IActionRecognizer Members

        public const int KEY_FINGER = 1;
        public const int MIN_DETECTED_FINGER_NUM = 2;

        public ARResult Recognize(FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int nGOF, int nFARPlugin)
        {
            ARResult rsl = new ARResult();
            rsl.Name = "NULL";

            //if (nGOF != 1)
                //return rsl;

            int cnt = 0;
            for (int i = 0; i < nGOF; i++)
            {
                for (int k = 0; k < (FingersStatus[currStep][i].Length); k++)
                {
                    for (int j = 0; j < nFARPlugin; j++)
                    {
                        if (FingersStatus[currStep][i][k][j].Name == "MOVE FINGER")
                        {
                            cnt++;
                        }
                    }
                }
            }

            if (cnt > 0)
            {
                rsl.Name = GetName();
                rsl.Params = new object[2*cnt +1];
                rsl.Params[0] = cnt;
                cnt = 0;
                for (int i = 0; i < nGOF; i++)
                {
                    for (int k = 0; k < (FingersStatus[currStep][i].Length); k++)
                    {
                        for (int j = 0; j < nFARPlugin; j++)
                        {
                            if (FingersStatus[currStep][i][k][j].Name == "MOVE FINGER")
                            {
                                cnt++;
                                rsl.Params[cnt] = FingersStatus[currStep][i][k][j].Params[0];
                                cnt++;
                                rsl.Params[cnt] = FingersStatus[currStep][i][k][j].Params[1];
                            }
                        }
                    }
                }
                return rsl;
            }

            //for (int i = 0; i < nGOF; i++)
            //{
            //    if (FingersStatus[currStep][i].Length < MIN_DETECTED_FINGER_NUM)
            //        continue;

            //    for (int j = 0; j < nFARPlugin; j++)
            //    {
            //        if (FingersStatus[currStep][i][KEY_FINGER][j].Name == "MOVE FINGER")
            //        {
            //            rsl.Name = GetName();
            //            rsl.Params = new object[2];
            //            rsl.Params[0] = FingersStatus[currStep][i][KEY_FINGER][j].Params[0];
            //            rsl.Params[1] = FingersStatus[currStep][i][KEY_FINGER][j].Params[1];
            //            return rsl;
            //        }
            //    }
            //}

            return rsl;
        }

        public string GetName()
        {
            return "SURFACE DRAW";
        }

        #endregion
    }
}
