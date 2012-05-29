using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;
using FARInterface;

namespace AR_MouseRightClick
{
    public class AR_MouseRightClick : IActionRecognizer
    {
        #region IActionRecognizer Members

        public const int KEY_FINGER = 2;
        public const int MIN_DETECTED_FINGER_NUM = 3;

        public ARResult Recognize(FARInterface.FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int nGOF, int nFARPlugin)
        {
            ARResult rsl = new ARResult();
            rsl.Name = "NULL";


            // xet tung GOF
            // xet second left most
            for (int i = 0; i < nGOF; i++)
            {
                if (FingersStatus[currStep][i].Length < MIN_DETECTED_FINGER_NUM)
                    continue;

                for (int j = 0; j < nFARPlugin; j++)
                {
                    if (FingersStatus[currStep][i][KEY_FINGER][j].Name == "CLICK FINGER")
                    {
                        rsl.Name = GetName();
                        rsl.Params = FingersStatus[currStep][i][KEY_FINGER][j].Params;
                    }
                }
            }

            return rsl;
        }

        public string GetName()
        {
            return "RIGHT CLICK ACTION";
        }

        #endregion
    }
}
