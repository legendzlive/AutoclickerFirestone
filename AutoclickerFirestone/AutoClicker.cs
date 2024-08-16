using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace AutoclickerFirestone
{
    public class AutoClicker
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static int offsetX = 0;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02; // left button down
        private const int MOUSEEVENTF_LEFTUP = 0x04; // left button up

        private const int MOUSEEVENTF_RIGHTDOWN = 0x08; // right button down
        private const int MOUSEEVENTF_RIGHTUP = 0x10; // right button up

        private const int MOUSEEVENTF_WHEEL = 0x0800; // mouse wheel movement

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;

        private readonly Timer clickTimer;

        public AutoClicker()
        {
            clickTimer = new Timer
            {
                Interval = 1 // Interval set to 1 millisecond
            };
            clickTimer.Tick += ClickTimer_Tick;
        }
                
        public static void ScrollMouseWheel(Point position, int scrollAmount, int scrollTimes = 1)
        {
            SetCursorPos(position.X + offsetX, position.Y);
            for (int i = 0; i < scrollTimes; i++)
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, scrollAmount, 0);
        }

        public static void LeftClickAtPosition(Point position)
        {
            // Move the cursor to the desired position
            SetCursorPos(position.X + offsetX, position.Y);

            // Simulate mouse down and up to perform a click
            mouse_event(MOUSEEVENTF_LEFTDOWN, position.X + offsetX, position.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, position.X + offsetX, position.Y, 0, 0);
        }

        public static void RightClickAtPosition(Point position)
        {
            // Move the cursor to the desired position
            SetCursorPos(position.X, position.Y);

            // Simulate mouse down and up to perform a click
            mouse_event(MOUSEEVENTF_RIGHTDOWN, position.X + offsetX, position.Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, position.X + offsetX, position.Y, 0, 0);
        }

        public static void DragMouse(Point startPosition, Point endPosition)
        {
            // Move the cursor to the start position
            SetCursorPos(startPosition.X + offsetX, startPosition.Y);

            // Simulate mouse down to start drag
            mouse_event(MOUSEEVENTF_LEFTDOWN, startPosition.X + offsetX, startPosition.Y, 0, 0);

            System.Threading.Thread.Sleep(50);

            // Move the cursor to the end position
            SetCursorPos(endPosition.X + offsetX, endPosition.Y);

            System.Threading.Thread.Sleep(50);

            // Simulate mouse up to release drag
            mouse_event(MOUSEEVENTF_LEFTUP, endPosition.X + offsetX, endPosition.Y, 0, 0);
        }

        public static void SendKey(string windowName, string key, int amount = 1)
        {
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd);
                for (int i = 0; i < amount; i++)
                {
                    SendKeys.SendWait(key);
                }
            }
            else
            {
                MessageBox.Show("Window not found.");
            }
        }

        public void StartClicking(string windowName)
        {
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd);
                clickTimer.Start();
            }
            else
            {
                MessageBox.Show("Window not found.");
            }
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            IntPtr hWnd = FindWindow(null, "Firestone");
            if (hWnd != IntPtr.Zero)
            {
                PostMessage(hWnd, WM_LBUTTONDOWN, 0x0001, 0x000A000A);
                PostMessage(hWnd, WM_LBUTTONUP, 0x0001, 0x000A000A);
            }
            else
            {
                clickTimer.Stop();
                MessageBox.Show("Target window lost.");
            }
        }

        public void StopClicking()
        {
            clickTimer.Stop();
        }
    }

}
