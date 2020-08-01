using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using HideriDotNet;

namespace HideriModules
{
    class PingCommand : BotCommand
    {
        public override string GetHelp()
        {
            return "Display loaded modules.";
        }
        public override bool IsOwnerOnly()
        {
            return true;
        }
        async Task sendModules(MessageWrapper message)
        {
            if (message.headless)
                message.Channel.SendMessageAsync("Loaded modules:");
            else
                await message.Channel.channel.SendMessageAsync("Loaded modules:");
            foreach (var element in Program.modules)
            {
                message.Channel.SendMessageAsync("[" + element.Key + "] " + element.Value.data.name + " - " + element.Value.data.description + " by " + element.Value.data.author);
            }
        }
        public override bool Run(  string[] arguments, MessageWrapper message)
        {
            if (base.Run(  arguments, message))
            {
                sendModules(message);
                return true;
            }
            return false;
        }
    }
}
