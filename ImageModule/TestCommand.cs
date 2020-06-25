using Discord;
using HideriDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageModule
{
    class TestCommand : BotCommand
    {
        public override string GetHelp()
        {
            return "IMAGE MODULE TEST!!!!!.";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                if (message.headless)
                    return false;
                runAsync(message);
                return true;
            }
            return false;
        }

        private async Task runAsync( MessageWrapper message)
        {
            var channel = await message.Channel.channel.GetMessagesAsync().FlattenAsync();
            foreach(var element in channel)
            {
                if (element.Attachments.Count > 0)
                {
                    var att = element.Attachments.First();
                    message.Channel.SendMessageAsync(att.Url);
                    break;
                }
            }
        }
    }
}
