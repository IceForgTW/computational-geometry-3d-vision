using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EGInterface;
using System.Runtime.InteropServices;

namespace EG_MouseRightClick
{
    public class EG_MouseRightClick : IEventGenerator
    {
        #region IEventGenerator Members
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

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

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32")]
        public static extern int GetCursorPos(ref POINTAPI point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTAPI
        {
            public int x;
            public int y;
        }

        public void SendEvent(object[] Params)
        {
            mouse_event((int)(MouseEventFlags.RIGHTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.RIGHTUP), 0, 0, 0, 0);
        }

        public string GetName()
        {
            return "RIGHT CLICK EVENT";
        }

        #endregion
    }
}
