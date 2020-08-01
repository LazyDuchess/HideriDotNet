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
    public class ReloadCommand : BotCommand
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
            return "Reload a module.";
        }
        public override bool IsOwnerOnly()
        {
            return true;
        }
        public override bool Run( string[] arguments, MessageWrapper message)
        {
            if (base.Run( arguments, message))
            {
                message.Channel.SendMessageAsync("Attempting to reload module " + arguments[0]);
                var result = Program.UnloadModule(arguments[0]);
                if (result)
                {
                    result = Program.LoadModule(arguments[0]);
                    if (result)
                        message.Channel.SendMessageAsync("Module reloaded.");
                    else
                    {
                        message.Channel.SendMessageAsync("Couldn't reload module.");
                    }
                }
                else
                    message.Channel.SendMessageAsync("Couldn't reload module.");
                return true;
            }
            return false;
        }
    }
}
