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
        async Task sendModules(MessageWrapper message, Program bot)
        {
            if (message.headless)
                message.Channel.SendMessageAsync("Loaded modules:");
            else
                await message.Channel.channel.SendMessageAsync("Loaded modules:");
            foreach (var element in bot.modules)
            {
                message.Channel.SendMessageAsync("[" + element.Key + "] " + element.Value.data.name + " - " + element.Value.data.description + " by " + element.Value.data.author);
            }
        }
        public override bool Run( Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run( bot, arguments, message))
            {
                sendModules(message, bot);
                return true;
            }
            return false;
        }
    }
}
