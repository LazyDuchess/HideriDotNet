using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace BasicModule
{
    public class ShowCommand : BotCommand
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public override bool IsOwnerOnly()
        {
            return true;
        }

        public override string GetHelp()
        {
            return "Show bot's console window.";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                var handle = GetConsoleWindow();

                // Show
                ShowWindow(handle, SW_SHOW);
                return true;
            }
            return false;
        }
    }
}
