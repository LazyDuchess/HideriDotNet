﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    class StopCommand : BotCommand
    {
        public override bool IsOwnerOnly()
        {
            return true;
        }

        public override string GetHelp()
        {
            return "Shutdown the bot.";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                bot.onStop?.Invoke(message);
                bot.Stop();
                return true;
            }
            return false;
        }
    }
}