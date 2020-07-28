using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using HideriDotNet;
using Newtonsoft.Json;

namespace ImageModule
{
    class AvatarCommand : BotCommand
    {
        public override int GetPriority()
        {
            return 5;
        }
        public override string GetCategory()
        {
            return "Image";
        }
        public override int getArguments()
        {
            return 1;
        }

        public override string GetUsage()
        {
            return "[User]";
        }

        public override string GetHelp()
        {
            return "Retrieve an user's avatar";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                if (message.headless)
                {
                    message.Channel.SendMessageAsync("Can only use this command inside of Discord.");
                    return false;
                }
                if (!typeof(IGuildChannel).IsAssignableFrom(message.message.Channel.GetType()))
                {
                    message.Channel.SendMessageAsync(message.message.Author.GetAvatarUrl(ImageFormat.Auto, 2048));
                    return true;
                }
                doThing(bot, arguments, message);
                return true;
            }
            return false;
        }

        async Task doThing(Program bot, string[] arguments, MessageWrapper message)
        {
            if (arguments.Length == 0)
            {
                message.Channel.SendMessageAsync(message.message.Author.GetAvatarUrl(ImageFormat.Auto, 2048));
                return;
            }
            var mention = await Utils.GetMention(arguments[0], (message.message.Channel as IGuildChannel).Guild);
            if (!mention.success)
            {
                message.Channel.SendMessageAsync("Invalid user.");
                return;
            }
            else
            {
                var user = await message.Channel.channel.GetUserAsync(mention.id);
                if (user != null)
                {
                    message.Channel.SendMessageAsync(user.GetAvatarUrl(ImageFormat.Auto,2048));
                }
                else
                {
                    message.Channel.SendMessageAsync("Invalid user.");
                    return;
                }
            }
        }
    }
}
