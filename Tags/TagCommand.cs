using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using HideriDotNet;

namespace Tags
{
    class TagCommand : BotCommand
    {
        public override string GetUsage()
        {
            return "[add/edit/delete/name of tag]";
        }
        public override string GetHelp()
        {
            return "View tags, create your own, delete, etc.";
        }
        public override string[] splitArgs(string arguments)
        {
            var newArgs = new string[3];
            var arg = arguments.Split(' ');
            var args = arg.Where((val, idx) => idx != 0).ToArray();
            if (args.Length > 0)
                newArgs[0] = args[0].ToLowerInvariant();
            if (args.Length > 1)
                newArgs[1] = args[1].ToLowerInvariant();
            if (args.Length > 2)
                newArgs[2] = arguments.Substring(arg[0].Length + arg[1].Length + arg[2].Length + 3);
            return newArgs;
        }
        public override bool Run( Program bot, string[] arguments, MessageWrapper message)
        {
            switch (arguments[0])
            {
                case "random":
                    var rtag = TagsModule.database.tags.ElementAt(new Random().Next(0,TagsModule.database.tags.Count));
                    message.Channel.SendMessageAsync("Tag: " + rtag.Key+Environment.NewLine+rtag.Value.content);
                    return true;

                case "add":
                    if (TagsModule.database.tags.ContainsKey(arguments[1]))
                    {
                        message.Channel.SendMessageAsync("That tag is already taken.");
                        return false;
                    }
                    if (message.headless)
                        TagsModule.database.tags[arguments[1]] = new Tag("Bot", "Console", "No ID", arguments[2]);
                    else
                        TagsModule.database.tags[arguments[1]] = new Tag(message.message.Author.Username, message.message.Author.Discriminator, message.message.Author.Id.ToString(), arguments[2]);
                    message.Channel.SendMessageAsync("Added tag " + arguments[1]);
                    return true;

                case "edit":
                    if (TagsModule.database.tags.ContainsKey(arguments[1]))
                    {
                        if (message.headless == false && TagsModule.database.tags[arguments[1]].ownerID != message.message.Author.Id.ToString())
                        {
                            message.Channel.SendMessageAsync("That tag is not yours.");
                            return false;
                        }
                        else
                        {
                            TagsModule.database.tags[arguments[1]].content = arguments[2];
                            message.Channel.SendMessageAsync("Edited tag " + arguments[1]);
                            return true;
                        }

                    }
                    message.Channel.SendMessageAsync("That tag doesn't exist.");
                    return false;


                case "delete":
                    if (TagsModule.database.tags.ContainsKey(arguments[1]))
                    {
                        if (message.headless == false && TagsModule.database.tags[arguments[1]].ownerID != message.message.Author.Id.ToString())
                        {
                            message.Channel.SendMessageAsync("That tag is not yours.");
                            return false;
                        }
                        else
                        {
                            TagsModule.database.tags.Remove(arguments[1]);
                            message.Channel.SendMessageAsync("Removed tag " + arguments[1]);
                            return true;
                        }

                    }
                    message.Channel.SendMessageAsync("That tag doesn't exist.");
                    return false;

                case "owner":
                    if (TagsModule.database.tags.ContainsKey(arguments[1]))
                    {
                        var tag = TagsModule.database.tags[arguments[1]];
                        message.Channel.SendMessageAsync("That tag is owned by " + tag.ownerName + "#" + tag.ownerTag + " (" + tag.ownerID + ")");
                        return true;
                    }
                    message.Channel.SendMessageAsync("That tag doesn't exist.");
                    return false;



            }
            if (TagsModule.database.tags.ContainsKey(arguments[0]))
            {
                message.Channel.SendMessageAsync(TagsModule.database.tags[arguments[0]].content);
                return true;
            }
            message.Channel.SendMessageAsync("That tag doesn't exist.");
            return false;
        }
    }
}
