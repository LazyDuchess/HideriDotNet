using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;


namespace HideriDotNet
{
    public enum CommandContext { Guild, DM, Console };
    public abstract class BotCommand
    {
        public virtual string GetUsage()
        {
            return "";
        }

        public virtual string GetHelp()
        {
            return "";
        }

        public virtual string[] splitArgs( string arguments )
        {
            var args = arguments.Split(' ');
            return args.Where((val, idx) => idx != 0).ToArray();
        }
        public virtual bool IsOwnerOnly()
        {
            return false;
        }
        public virtual bool CanRunCommand( Program bot, string[] arguments, MessageWrapper message)
        {
            if (IsOwnerOnly())
            {
                if (message.headless)
                    return true;
                if (bot.botSettings.owners.Contains(message.message.Author.Id.ToString()))
                    return true;
                return false;
            }
            return true;
        }

        public virtual bool Run( Program bot, string[] arguments, MessageWrapper message)
        {
            if (!CanRunCommand( bot, arguments, message))
            {
                message.Channel.SendMessageAsync("Can't execute that command in this context.");
                return false;
            }
            return true;
        }

    }
}
