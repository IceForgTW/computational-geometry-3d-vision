using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGInterface;
using System.Runtime.InteropServices;

namespace EG_SurfaceDraw
{
    public class EG_SurfaceDraw : IEventGenerator
    {
        #region IEventGenerator Members
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        public void SendEvent(object[] Params)
        {
            int n = (int)Double.Parse(Params[0].ToString());
            for (int i = 0; i < n; i++)
            {
                IntPtr lParam = new IntPtr((int)Double.Parse(Params[2*i+1].ToString()));
                IntPtr wParam = new IntPtr((int)Double.Parse(Params[2*i+2].ToString()));
                IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "VeHinhDemo");
                SendMessage(hwnd, 41292, wParam, lParam);
            }
        }

        public string GetName()
        {
            return "DRAW SURFACE EVENT";
        }

        #endregion
    }
}
