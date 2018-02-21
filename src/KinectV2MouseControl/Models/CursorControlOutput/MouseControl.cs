using System;
using System.Runtime.InteropServices;
//using System.Windows;

namespace KinectV2MouseControl
{
    /// <summary>
    /// Mouse pressing down/up, clicking, and moving control.
    /// 
    /// Left down, left up, click with mouse_event function.
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646260(v=vs.85).aspx
    /// 
    /// Moving with SetCursorPos function.
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms648394(v=vs.85).aspx
    /// 
    /// Get mouse position with GetCursorPos function.
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms648390(v=vs.85).aspx
    /// </summary>
    public static class MouseControl
    {
        public static void PressDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void PressUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        public static void Click()
        {
            mouse_event(MouseEventFlag.LeftDown | MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
        
        public static bool MoveTo(double x, double y)
        {
            return SetCursorPos((int)x, (int)y);
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }


        // GetCursorPos not used.

        //[StructLayout(LayoutKind.Sequential)]
        //private struct POINT
        //{
        //    public int X;
        //    public int Y;

        //    public static implicit operator Point(POINT point)
        //    {
        //        return new Point(point.X, point.Y);
        //    }
        //}

        //[DllImport("user32.dll")]
        //private static extern bool GetCursorPos(out POINT lpPoint);

        //public static Point GetCursorPosition()
        //{
        //    POINT lpPoint;
        //    GetCursorPos(out lpPoint);

        //    return lpPoint;
        //}

    }
}
