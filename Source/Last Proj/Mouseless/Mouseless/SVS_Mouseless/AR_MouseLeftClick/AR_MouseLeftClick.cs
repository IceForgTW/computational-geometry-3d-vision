using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARInterface;

namespace AR_MouseLeftClick
{
    public class AR_MouseLeftClick : IActionRecognizer
    {
        #region IActionRecognizer Members

        public const int KEY_FINGER = 1;
        public const int MIN_DETECTED_FINGER_NUM = 2;

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
                        return rsl;
                    }
                }
            }

            return rsl;
        }

        public string GetName()
        {
            return "LEFT CLICK ACTION";
        }

        #endregion
    }
}