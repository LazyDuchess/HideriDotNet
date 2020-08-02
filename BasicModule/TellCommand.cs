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

        async Task RunUserCommand(string[] arguments, MessageWrapper message)
        {
            IGuild guild = null;
            if (!message.headless && typeof(IGuildChannel).IsAssignableFrom(message.message.Channel.GetType()))
                guild = (message.message.Channel as IGuildChannel).Guild;
            var user = await Utils.GetMention(arguments[0], guild);
            var user2 = await message.Channel.channel.GetUserAsync(user.id);
            if (user2 != null)
            {
                user2.SendMessageAsync(arguments[1]);
            }
        }
        public override RCONInputFieldResult GetInputField(string[] arguments, MessageWrapper message)
        {
            return new RCONInputFieldResult()
            {
                keepCommand = true,
                keepArguments = new string[] { arguments[0] }
            };
        }
        public override bool Run( string[] arguments, MessageWrapper message)
        {
            if (base.Run(  arguments, message))
            {
                ulong result;
                if (!ulong.TryParse(arguments[0], out result))
                    result = 0;
                var channel = Program._client.GetChannel(result);
                if (result != 0)
                    (channel as IMessageChannel).SendMessageAsync(arguments[1]);
                else
                {
                    RunUserCommand( arguments, message);
                }
                return true;
            }
            return false;
        }
    }
}
