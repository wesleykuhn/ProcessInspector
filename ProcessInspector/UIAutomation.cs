using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace ProcessInspector
{
    public static class UIAutomation
    {
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

        public static void SendMessageToCalculator()
        {
            Process process = null;
            AutomationElement root = null;
            while (root is null)
            {
                try
                {
                    process = Process.GetProcessesByName("CalculatorApp").FirstOrDefault();

                    if (process is null)
                        throw new ElementNotAvailableException();

                    root = AutomationElement.FromHandle(process.Handle);
                }
                catch (System.Windows.Automation.ElementNotAvailableException)
                {
                    process = Process.GetProcessesByName("CalculatorApp").FirstOrDefault();
                    Thread.Sleep(500);
                }
            }

            //Condition conditions = new AndCondition(
            //    new PropertyCondition(AutomationElement.IsEnabledProperty, true),
            //    new PropertyCondition(AutomationElement.AutomationIdProperty, "num4button"),
            //    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

            var btn = root.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, "Quatro"));
            var success = btn.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern);

            if (success)
                (pattern as InvokePattern).Invoke();
        }
    }
}
