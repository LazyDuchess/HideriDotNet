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

        public override bool Run( string[] arguments, MessageWrapper message)
        {
            if (base.Run( arguments, message))
            {
                message.Channel.SendMessageAsync("Latency is ``" + Program._client.Latency.ToString() + "ms``");
                return true;
            }
            return false;
        }
    }
}
