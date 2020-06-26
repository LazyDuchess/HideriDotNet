using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        /*
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
        }*/

        public override int getArguments()
        {
            return 3;
        }

        void SendTag(Tag tag, ChannelWrapper channel, string prefix = "")
        {
            if (channel.headless || tag.attachmentUrl == null || tag.attachmentUrl == "")
            {
                channel.SendMessageAsync(prefix + tag.content);
            }
            else
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(tag.attachmentUrl);
                channel.channel.SendFileAsync(stream, tag.attachmentFilename, prefix + tag.content);
            }
        }

        public override bool Run( Program bot, string[] arguments, MessageWrapper message)
        {
            if (arguments.Length >= 1)
            {
                arguments[0] = arguments[0].ToLower();
            }
            if (arguments.Length >= 2)
            {
                arguments[1] = arguments[1].ToLower();
            }
            switch (arguments[0])
            {
                case "random":
                    var rtag = TagsModule.database.tags.ElementAt(new Random().Next(0,TagsModule.database.tags.Count));
                    SendTag(rtag.Value, message.Channel, "Tag: " + rtag.Key + Environment.NewLine);
                    //message.Channel.SendMessageAsync("Tag: " + rtag.Key+Environment.NewLine+rtag.Value.content);
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
                    {
                        var attachName = "";
                        var attachUrl = "";
                        if (message.message.Attachments.Count > 0)
                        {
                            attachName = message.message.Attachments.First().Filename;
                            attachUrl = message.message.Attachments.First().Url;
                        }
                        TagsModule.database.tags[arguments[1]] = new Tag(message.message.Author.Username, message.message.Author.Discriminator, message.message.Author.Id.ToString(), arguments[2], attachName, attachUrl);
                    }
                    message.Channel.SendMessageAsync("Added tag " + arguments[1]);
                    TagsModule.SaveTags();
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
                            var attachName = "";
                            var attachUrl = "";
                            if (message.message.Attachments.Count > 0)
                            {
                                attachName = message.message.Attachments.First().Filename;
                                attachUrl = message.message.Attachments.First().Url;
                            }
                            TagsModule.database.tags[arguments[1]].content = arguments[2];
                            TagsModule.database.tags[arguments[1]].attachmentFilename = attachName;
                            TagsModule.database.tags[arguments[1]].attachmentUrl = attachUrl;
                            message.Channel.SendMessageAsync("Edited tag " + arguments[1]);
                            TagsModule.SaveTags();
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
                            TagsModule.SaveTags();
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
                SendTag(TagsModule.database.tags[arguments[0]], message.Channel);
                return true;
            }
            message.Channel.SendMessageAsync("That tag doesn't exist.");
            return false;
        }
    }
}
