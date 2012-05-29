using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGInterface;
using System.Runtime.InteropServices;

namespace EG_MouseMove
{
    public class EG_MouseMove : IEventGenerator
    {
        #region IEventGenerator Members
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        public void SendEvent(object[] Params)
        {
            mouse_event((int)MouseEventFlags.MOVE,
                Int32.Parse(Params[0].ToString()),
                Int32.Parse(Params[1].ToString()),
                0, 0);

            //SetCursorPos(Int32.Parse(Params[0].ToString()),
            //    Int32.Parse(Params[1].ToString()));
        }

        public string GetName()
        {
            return "MOUSE MOVE EVENT";
        }

        #endregion
    }
}
