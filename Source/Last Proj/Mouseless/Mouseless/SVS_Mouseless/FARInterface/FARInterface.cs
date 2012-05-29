using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAdapter;

namespace FARInterface
{
    public interface IFingerActionRecognizer
    {
        FARResult Recognize(GroupOfFingers[][] ListHand, int[][][] Prev, int n, int currentIdx, int fingerIndx, int nthGroup);
        string GetName();
    }
}