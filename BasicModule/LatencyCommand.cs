using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace BasicModule
{
    public class LatencyCommand : BotCommand
    {
        public override string GetHelp()
        {
            return "Display latency of the bot.";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                message.Channel.SendMessageAsync("Latency is ``" + bot._client.Latency.ToString() + "ms``");
                return true;
            }
            return false;
        }
    }
}
