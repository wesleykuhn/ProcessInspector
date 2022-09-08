using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Windows.Forms;

namespace ProcessInspector
{
    public static class UIAutomation
    {
        const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// The FindWindow API
        /// </summary>
        /// <param name="lpClassName">the class name for the window to search for</param>
        /// <param name="lpWindowName">the name of the window to search for</param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// The SendMessage API
        /// </summary>
        /// <param name="hWnd">handle to the required window</param>
        /// <param name="msg">the system/Custom message to send</param>
        /// <param name="wParam">first message parameter</param>
        /// <param name="lParam">second message parameter</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        public static void SendMessageToCalculator(AutomationElement root)
        {
            var hwnd = FindWindow(null, "Device Selection");
            root = AutomationElement.FromHandle(hwnd);
            var listTree = root.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "1049"));

            listTree.SetFocus();
            hwnd = new IntPtr(listTree.Current.NativeWindowHandle);
            SendMessage(hwnd, WM_KEYDOWN, (int)Keys.Down, IntPtr.Zero);

            var btn = root.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, "OK"));
            btn.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern);
            (pattern as InvokePattern).Invoke();
        }

        private static void InvokeButtonByName(AutomationElement root, string buttonName)
        {
            var btn = root.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, buttonName));
            var success = btn.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern);

            if (success)
                (pattern as InvokePattern).Invoke();
        }
    }
}
