using HideriDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace HideriModules
{
    public class LoadCommand : BotCommand
    {
        public override int getArguments()
        {
            return 1;
        }
        public override string GetUsage()
        {
            return "[Module name]";
        }
        public override string GetHelp()
        {
            return "Load a module.";
        }
        public override bool IsOwnerOnly()
        {
            return true;
        }
        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run( bot, arguments, message))
            {
                message.Channel.SendMessageAsync("Attempting to load module " + arguments[0]);
                var result = bot.LoadModule(arguments[0]);
                if (result)
                    message.Channel.SendMessageAsync("Module loaded.");
                else
                    message.Channel.SendMessageAsync("Couldn't load module.");
                return true;
            }
            return false;
        }
    }
}
