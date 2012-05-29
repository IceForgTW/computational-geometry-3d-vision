using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FARInterface;
using DataAdapter;

namespace FAR_MouseClick
{
    class FAR_MouseClick : IFingerActionRecognizer
    {
        #region IFingerActionRecognizer Members

        protected int MIN_FRAME = 15;
        protected int MIN_FRAME_UNTOUCH = 4;

        public FARResult Recognize(GroupOfFingers[][] arrGOF, int[][][] Prev, int n, int currentIdx, int fingerIndx, int nthGroup)
        {
            // kiem tra hanh dong cho ngon tay fingerIndx
            FARResult rsl = new FARResult();
            rsl.Name = "NULL";

            if (n < MIN_FRAME)
                return rsl;

            int prevIdx = currentIdx - 1;
            if (prevIdx < 0)
                prevIdx = arrGOF.Length - 1;

            if (Prev[currentIdx][nthGroup][fingerIndx] != -1)
            {
                if (arrGOF[currentIdx][nthGroup].Fingertips[fingerIndx].Status == true
                    && arrGOF[prevIdx][nthGroup].Fingertips[Prev[currentIdx][nthGroup][fingerIndx]].Status == false)       // neu vua cham mat ban
                {
                    // kiem tra xem o tren khong du lau chua
                    int i = prevIdx;
                    int curr = currentIdx;
                    int currFingerIndx = fingerIndx;
                    int cnt = 0;
                    bool isOK = false;

                    while (i != currentIdx)
                    {
                        if (arrGOF[i][nthGroup].Fingertips[Prev[curr][nthGroup][currFingerIndx]].Status == true)
                        {
                            isOK = true;
                            break;
                        }
                        
                        currFingerIndx = Prev[curr][nthGroup][currFingerIndx];
                        curr = i;
                        // giam i
                        i--;
                        if (i < 0)
                            i = arrGOF.Length - 1;

                        cnt++;
                        if (cnt > n
                            || Prev[curr][nthGroup][currFingerIndx] < 0)
                        {
                            isOK = false;
                            break;
                        }
                    }

                    if (isOK && cnt > MIN_FRAME_UNTOUCH && cnt <= n)
                    {
                        rsl.Name = GetName();
                        rsl.Params = new object[2];
                        rsl.Params[0] = 100;
                        rsl.Params[1] = 200;
                    }
                }
            }

            return rsl;
        }

        public string GetName()
        {
            return "CLICK FINGER";
        }

        #endregion
    }
}