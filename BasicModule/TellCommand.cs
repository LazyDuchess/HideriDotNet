using HideriDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BasicModule
{
    public class TellCommand : BotCommand
    {
        public override int getArguments()
        {
            return 2;
        }
        public override string GetUsage()
        {
            return "[Channel ID] [Text]";
        }
        public override string GetHelp()
        {
            return "Say something in a specific channel.";
        }
        public override bool IsOwnerOnly()
        {
            return true;
        }
        public override string[] splitArgs(string arguments)
        {
            var newArgs = new string[3];
            var arg = arguments.Split(' ');
            var args = arg.Where((val, idx) => idx != 0).ToArray();
            if (args.Length > 0)
                newArgs[0] = args[0];
            if (args.Length > 1)
                newArgs[1] = arguments.Substring(arg[0].Length + arg[1].Length + 2);
            return newArgs;
        }
        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run( bot, arguments, message))
            {
                (bot._client.GetChannel(ulong.Parse(arguments[0])) as IMessageChannel).SendMessageAsync(arguments[1]);
                return true;
            }
            return false;
        }
    }
}
