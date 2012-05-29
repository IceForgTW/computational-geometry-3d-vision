using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FARInterface;
using DataAdapter;

namespace FAR_MouseMove
{
    public class FAR_MouseMove : IFingerActionRecognizer
    {
        #region IFingerActionRecognizer Members

        protected int MIN_FRAME = 2;

        public FARResult Recognize(GroupOfFingers[][] arrGOF, 
            int[][][] Prev, 
            int n, 
            int currentIdx, int fingerIndx, int nthGroup)
        {
            FARResult rsl = new FARResult();
            rsl.Name = "NULL";

            if (n < MIN_FRAME)
                return rsl;

            int prevIdx = currentIdx - 1;
            if (prevIdx < 0)
                prevIdx = arrGOF.Length - 1;

            if (Prev[currentIdx][nthGroup][fingerIndx] != -1)
            {
                // 2 frame lien tuc deu cham. mat. ban`
                if (arrGOF[currentIdx][nthGroup].Fingertips[fingerIndx].Status == true
                    && arrGOF[prevIdx][nthGroup].Fingertips[Prev[currentIdx][nthGroup][fingerIndx]].Status == true)
                {
                    rsl.Name = GetName();
                    rsl.Params = new object[2];
                    rsl.Params[0] = arrGOF[currentIdx][nthGroup].Fingertips[fingerIndx].Point2D1.X;
                        //- arrGOF[prevIdx][nthGroup].Fingertips[Prev[currentIdx][nthGroup][fingerIndx]].Point2D1.X;
                    rsl.Params[1] = arrGOF[currentIdx][nthGroup].Fingertips[fingerIndx].Point2D1.Y;
                        //- arrGOF[prevIdx][nthGroup].Fingertips[Prev[currentIdx][nthGroup][fingerIndx]].Point2D1.Y;
                }
            }

            return rsl;
        }

        public string GetName()
        {
            return "MOVE FINGER";
        }

        #endregion
    }
}