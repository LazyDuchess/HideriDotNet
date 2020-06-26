using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace InternetModule
{
    public class InternetModule : BotModule
    {
        Program bot;
        public override void Initialize(Program botCore)
        {
            bot = botCore;
            botCore.onMessage += MessageLog;
        }

        public override void Unload()
        {
            bot.onMessage -= MessageLog;
        }
        void MessageLog(MessageWrapper message)
        {
            if (message.Content.ToLowerInvariant().Contains("owo"))
                message.Channel.SendMessageAsync("uwu");
        }
    }
}
