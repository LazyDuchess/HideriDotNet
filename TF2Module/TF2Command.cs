using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using HideriDotNet;
using HideriDotNet.Database;

namespace TF2Module
{
    class TF2Command : BotCommand
    {
        public override int getArguments()
        {
            return 2;
        }
        public override string GetCategory()
        {
            return "Team Fortress 2";
        }
        public override int GetPriority()
        {
            return 5;
        }
        public override string GetHelp()
        {
            return "Return a TF2 item by name or register your steam id";
        }
        public override string GetUsage()
        {
            return "[item/steam/track] [Name/64 bit Steam ID/Track your inventory]";
        }
        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run(bot, arguments, message))
            {
                if (arguments.Length < 1)
                {
                    message.Channel.SendMessageAsync("Provide a valid command");
                    return false;
                }
                switch (arguments[0])
                {
                    case "item":
                        if (arguments.Length > 1)
                        {
                            if (TF2Module.schemaItemsByName.ContainsKey(arguments[1].ToLowerInvariant()))
                            {
                                var item = TF2Module.schemaItemsByName[arguments[1].ToLowerInvariant()];
                                if (message.headless)
                                {
                                    message.Channel.SendMessageAsync("Item name: " + item.item_name);
                                    message.Channel.SendMessageAsync("Item description: " + item.item_description);
                                    message.Channel.SendMessageAsync("Item picture: " + item.image_url_large);
                                }
                                else
                                {
                                    EmbedBuilder itemEmbedBuilder = new EmbedBuilder();
                                    itemEmbedBuilder.Title = item.item_name;
                                    itemEmbedBuilder.ImageUrl = item.image_url_large;
                                    itemEmbedBuilder.Description = item.item_description;
                                    message.Channel.channel.SendMessageAsync("", false, itemEmbedBuilder.Build());
                                }
                                return true;
                            }
                            if (TF2Module.loaded)
                                message.Channel.SendMessageAsync("Couldn't find an item with that name.");
                            else
                                message.Channel.SendMessageAsync("Can't find that item yet, it either doesn't exist or it hasn't been loaded yet.");
                            return false;
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("Type the name of an item.");
                            return false;
                        }

                    case "steam":
                        if (message.headless)
                        {
                            message.Channel.SendMessageAsync("Can't perform this action from console.");
                            return false;
                        }
                        if (arguments.Length > 1)
                        {
                            TF2Module.database.userIDtoSteamID[message.message.Author.Id.ToString()] = arguments[1];
                            TF2Module.Save();
                            message.Channel.SendMessageAsync("Registered " + arguments[1] + " as the 64 bit Steam ID of your account.");
                            return true;
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("Type your desired 64 bit Steam ID");
                            return false;
                        }
                    case "ref":
                        if (message.headless)
                        {
                            message.Channel.SendMessageAsync("Can't perform this action from console.");
                            return false;
                        }
                        if (!TF2Module.database.userIDtoSteamID.ContainsKey(message.message.Author.Id.ToString()))
                        {
                            message.Channel.SendMessageAsync("Register your 64 bit Steam ID with the steam command first.");
                            return false;
                        }
                        var dbp = IDFiles.ReturnObjectForId<TF2PlayerDatabase>(message.message.Author.Id, "player.json", TF2Module.instance);
                        if (!dbp.cached)
                        {
                            message.Channel.SendMessageAsync("I still haven't cached your inventory. Please wait a few minutes.");
                            return false;
                        }
                        var refAmount = 0.0f;
                        foreach(var element in dbp.defindex)
                        {
                            switch(element)
                            {
                                case 5000:
                                    refAmount += 0.11f;
                                    break;
                                case 5001:
                                    refAmount += 0.33f;
                                    break;
                                case 5002:
                                    refAmount += 1.0f;
                                    break;
                            }
                        }
                        var text = "You have **" + refAmount.ToString() + "** ref." + Environment.NewLine;
                        if (dbp.timestamp != null)
                        {
                            text += "*Last updated ";
                            var lastUpdated = DateTime.Now - dbp.timestamp;
                            if (lastUpdated.TotalDays >= 1f)
                            {
                                text += Math.Truncate(lastUpdated.TotalDays).ToString() + " days ago";
                            }
                            else if (lastUpdated.TotalHours >= 1f)
                            {
                                text += Math.Truncate(lastUpdated.TotalHours).ToString() + " hours ago";
                            }
                            else if (lastUpdated.TotalMinutes >= 1f)
                            {
                                text += Math.Truncate(lastUpdated.TotalMinutes).ToString() + " minutes ago";
                            }
                            else if (lastUpdated.TotalSeconds >= 1f)
                            {
                                text += Math.Truncate(lastUpdated.TotalSeconds).ToString() + " seconds ago";
                            }
                            else
                            {
                                text += Math.Truncate(lastUpdated.TotalMilliseconds).ToString() + " milliseconds ago";
                            }
                            text += ".*";
                        }
                        message.Channel.SendMessageAsync(text);
                        return true;
                    case "channel":
                        if (message.headless || !typeof(IGuildChannel).IsAssignableFrom(message.message.Channel.GetType()))
                        {
                            message.Channel.SendMessageAsync("Can only use this command inside a guild.");
                            return false;
                        }
                        if ((message.message.Author as IGuildUser).GetPermissions(message.message.Channel as IGuildChannel).ManageChannel)
                        {
                            var datab = IDFiles.ReturnObjectForId<TF2ServerDatabase>((message.message.Channel as IGuildChannel).GuildId, "server.json", TF2Module.instance);
                            if (datab.trackingChannelID == message.message.Channel.Id.ToString())
                            {
                                datab.trackingChannelID = "";
                                message.Channel.SendMessageAsync("Cleared inventory tracking channel from this server.");
                            }
                            else
                            {
                                datab.trackingChannelID = message.message.Channel.Id.ToString();
                                message.Channel.SendMessageAsync("Alright, i will post inventory updates in this channel.");
                            }
                            IDFiles.StoreObjectForId((message.message.Channel as IGuildChannel).GuildId, "server.json", TF2Module.instance, datab);
                            return true;
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("You need the manage channels permission to set the TF2 inventory tracking channel.");
                            return false;
                        }
                    case "track":
                        if (message.headless || !typeof(IGuildChannel).IsAssignableFrom(message.message.Channel.GetType()))
                        {
                            message.Channel.SendMessageAsync("Can only use this command inside a guild.");
                            return false;
                        }
                        if (!TF2Module.database.userIDtoSteamID.ContainsKey(message.message.Author.Id.ToString()))
                        {
                            message.Channel.SendMessageAsync("Register your 64 bit Steam ID with the steam command first.");
                            return false;
                        }
                        var db = IDFiles.ReturnObjectForId<TF2ServerDatabase>((message.message.Channel as IGuildChannel).GuildId, "server.json", TF2Module.instance);
                        if (db.trackedUsers == null)
                            db.trackedUsers = new List<string>();
                        if (db.trackedUsers.Contains(message.message.Author.Id.ToString()))
                        {
                            db.trackedUsers.Remove(message.message.Author.Id.ToString());
                            message.Channel.SendMessageAsync("This server is not tracking your inventory anymore.");
                        }
                        else
                        {
                            db.trackedUsers.Add(message.message.Author.Id.ToString());
                            message.Channel.SendMessageAsync("This server is now tracking your inventory.");
                        }
                        //db.trackingChannelID = message.message.Channel.Id.ToString();
                        IDFiles.StoreObjectForId((message.message.Channel as IGuildChannel).GuildId, "server.json", TF2Module.instance, db);
                        return true;
                }
                message.Channel.SendMessageAsync("Provide a valid command");
                return false;
            }
            return false;
        }
    }
}
