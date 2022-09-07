using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector
{
    public static class UIAutomation
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(
                  IntPtr hwnd,      // handle to destination window
                  uint Msg,       // message
                  long wParam,  // first message parameter
                  long lParam   // second message parameter
                  );

        public static void SendMessageToCalculator()
        {
            var process = Process.GetProcessesByName("CalculatorApp").FirstOrDefault();
            var hwnd = process.Handle;

            SendMessage()
        }
    }
}
