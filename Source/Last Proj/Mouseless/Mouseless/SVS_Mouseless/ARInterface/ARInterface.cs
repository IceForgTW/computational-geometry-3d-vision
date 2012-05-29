using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FARInterface;

namespace ARInterface
{
    public interface IActionRecognizer
    {
        ARResult Recognize(FARResult[][][][] FingersStatus, int currStep, int[][][] Prev, int nGOF, int nFARPlugin);
        string GetName();
    }
}