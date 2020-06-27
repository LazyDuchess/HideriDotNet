﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using HideriDotNet;
using Newtonsoft.Json;

namespace NSFWModule
{
    class GelbooruCommand : BotCommand
    {
        public override int GetPriority()
        {
            return 10;
        }
        public override string GetCategory()
        {
            return "NSFW";
        }
        public override int getArguments()
        {
            return 1;
        }

        public override string GetUsage()
        {
            return "[Tags]";
        }

        public override string GetHelp()
        {
            return "Search for images in Gelbooru";
        }

        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                var query = arguments[0].ToLowerInvariant().Trim();
                
                if (!message.headless)
                {
                    var channel = ((ITextChannel)message.message.Channel);
                    if (channel != null && !channel.IsNsfw)
                    {
                        query.Replace("rating:explicit", "");
                        query.Replace("rating:questionable", "");
                        query.Replace("rating:safe", "");
                        query += " rating:safe";
                    }
                }
                query = query.Replace(" ", "%20");
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&pid=0&tags="+query);
                List<GelbooruSearchResult> results;
                using (StreamReader reader = new StreamReader(stream))
                {
                    var data = reader.ReadToEnd();
                    if (data.Length > 0)
                    {
                        results = JsonConvert.DeserializeObject<List<GelbooruSearchResult>>(data);
                        message.Channel.SendMessageAsync(results[new Random().Next(0, results.Count)].file_url);
                    }
                    else
                        message.Channel.SendMessageAsync("No results.");
                }
                //message.Channel.channel.SendFileAsync(stream, att.Filename);
                return true;
            }
            return false;
        }
    }
}