using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;
using TF2Module.SchemaItems;
using Newtonsoft.Json;
using System.Threading;
using HideriDotNet.Database;
using Discord;
using System.Timers;

namespace TF2Module
{
    public class TF2Module : BotModule
    {
        
        int retries = 20;
        public static BotModule instance;
        public static TF2Database database;
        public static bool loaded = false;
        Program bot;
        public static string dbFolder;
        public static List<TF2SchemaItems> schemaItemsCache = new List<TF2SchemaItems>();
        public static Dictionary<string, TF2SchemaItem> schemaItemsByName = new Dictionary<string, TF2SchemaItem>();
        public static Dictionary<ulong, TF2SchemaItem> schemaItemsByIndex = new Dictionary<ulong, TF2SchemaItem>();
        void LoadItems()
        {
            try
            {
                var start = 0;
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetSchemaItems/v0001/?key=F7416B287CE89DC0407271739A22296C&format=json&language=en");
                TF2SchemaItems obj;
                using (StreamReader reader = new StreamReader(stream))
                {
                    var data = reader.ReadToEnd();
                    obj = JsonConvert.DeserializeObject<TF2SchemaItems>(data);
                    foreach (var element in obj.result.items)
                    {
                        schemaItemsByName[element.item_name.ToLowerInvariant()] = element;
                        schemaItemsByIndex[element.defindex] = element;
                    }
                    //schemaItemsCache.Add(obj);
                }

                while (obj.result.items.Count > 0)
                {
                    start += 1000;
                    stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetSchemaItems/v0001/?key=F7416B287CE89DC0407271739A22296C&format=json&language=en&start=" + start.ToString());
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var data = reader.ReadToEnd();
                        obj = JsonConvert.DeserializeObject<TF2SchemaItems>(data);
                        foreach (var element in obj.result.items)
                        {
                            schemaItemsByName[element.item_name.ToLowerInvariant()] = element;
                            schemaItemsByIndex[element.defindex] = element;
                        }
                    }
                }
                loaded = true;
                Console.WriteLine("TF2 items cache loaded.");
                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(announceBackpacks);
                aTimer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
                aTimer.Enabled = true;
            }
            catch(Exception e)
            {
                Console.WriteLine("There was an issue loading TF2 Item data:" + Environment.NewLine + e.ToString());
            }
        }

        public static void Save()
        {
            File.WriteAllText(dbFolder,JsonConvert.SerializeObject(database));
        }

        public override void Initialize()
        {
            instance = this;
            dbFolder = Path.Combine(directory, "database.json");
            if (File.Exists(dbFolder))
            {
                database = JsonConvert.DeserializeObject<TF2Database>(File.ReadAllText(dbFolder));
            }
            else
            {
                database = new TF2Database();
            }
            ThreadStart itemThreadStart = new ThreadStart(LoadItems);
            var itemThread = new Thread(itemThreadStart);
            itemThread.Start();
            Program.AddCommand("tf2", new TF2Command());
        }
        void announceBackpacks(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Announcing backpack changes.");
            var pendingAnnounces = new Dictionary<ulong, TF2BackpackPendingAnnounce>();
            foreach (var element in database.userIDtoSteamID)
            {
                var success = false;
                //var username = element.GetUser(ulong.Parse(element2)).Username;
                WebClient client = new WebClient();
                Stream stream = null;
                try
                {
                    stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetPlayerItems/v0001/?SteamID=" + element.Value + "&key=F7416B287CE89DC0407271739A22296C&format=json&language=en");
                }
                catch (Exception u)
                {
                    stream = null;
                    //yep
                }
                StreamReader reader = null;
                TF2SchemaItems obj = null;
                if (stream != null)
                {
                    try
                    {
                        reader = new StreamReader(stream);
                        var sdata = reader.ReadToEnd();
                        obj = JsonConvert.DeserializeObject<TF2SchemaItems>(sdata);
                    }
                    catch(Exception ex)
                    {
                        stream = null;
                    }
                }
                var retry = 0;
                if (stream != null && obj.result != null && obj.result.items != null)
                    success = true;
                if (stream != null)
                    reader.Dispose();
                while ((stream == null || obj.result == null || obj.result.items == null) && retries >= retry)
                {
                    try
                    {
                        stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetPlayerItems/v0001/?SteamID=" + element.Value + "&key=F7416B287CE89DC0407271739A22296C&format=json&language=en");
                    }
                    catch (Exception u)
                    {
                        stream = null;
                        //yup
                    }
                    if (stream != null)
                    {
                        try
                        {
                            reader = new StreamReader(stream);
                            var sdata = reader.ReadToEnd();
                            obj = JsonConvert.DeserializeObject<TF2SchemaItems>(sdata);
                        }
                        catch (Exception ex)
                        {
                            stream = null;
                        }
                    }
                    if (stream != null && obj.result != null && obj.result.items != null)
                        success = true;
                    if (stream != null)
                        reader.Dispose();
                    retry += 1;
                }
                if (success)
                {
                    var myPending = new TF2BackpackPendingAnnounce();
                    pendingAnnounces[ulong.Parse(element.Key)] = myPending;
                    var playerDb = IDFiles.ReturnObjectForId<TF2PlayerDatabase>(ulong.Parse(element.Key), "player.json", this);
                    if (playerDb != null)
                    {
                        if (!playerDb.cached)
                        {
                            playerDb.cached = true;
                            if (playerDb.ids == null)
                                playerDb.ids = new List<ulong>();
                            foreach (var element3 in obj.result.items)
                            {
                                playerDb.ids.Add(element3.id);
                                playerDb.defindex.Add(element3.defindex);
                            }
                            playerDb.timestamp = DateTime.Now;
                            IDFiles.StoreObjectForId(ulong.Parse(element.Key), "player.json", this, playerDb);
                        }
                        else
                        {
                            var receivedItems = new List<TF2SchemaItem>();
                            var lostItems = new List<TF2SchemaItem>();
                            var newCache = new List<ulong>();
                            var newDefindexCache = new List<ulong>();
                            foreach (var element3 in obj.result.items)
                            {
                                newCache.Add(element3.id);
                                newDefindexCache.Add(element3.defindex);
                                if (!playerDb.ids.Contains(element3.id))
                                {
                                    receivedItems.Add(schemaItemsByIndex[element3.defindex]);
                                }
                            }
                            for (var i = 0; i < playerDb.ids.Count; i++)
                            {
                                var element3 = playerDb.ids[i];
                                if (!newCache.Contains(element3))
                                {
                                    lostItems.Add(schemaItemsByIndex[playerDb.defindex[i]]);
                                }
                            }
                            if (receivedItems.Count > 0)
                            {
                                /*
                                var text = username + " has received the following items in TF2:" + Environment.NewLine;*/
                                var text = "";
                                foreach (var item in receivedItems)
                                {
                                    if (item.item_description != null && item.item_description != "")
                                    {
                                        text += "**" + item.item_name + "**" + " - " + item.item_description + Environment.NewLine;
                                    }
                                    else
                                    {
                                        text += "**" + item.item_name + "**" + Environment.NewLine;
                                    }
                                }
                                myPending.receivedItems = text;
                                //(bot._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                            }
                            if (lostItems.Count > 0)
                            {
                                //var text = username + " has lost the following items in TF2:" + Environment.NewLine;
                                var text = "";
                                foreach (var item in lostItems)
                                {
                                    if (item.item_description != null && item.item_description != "")
                                    {
                                        text += "**" + item.item_name + "**" + " - " + item.item_description + Environment.NewLine;
                                    }
                                    else
                                    {
                                        text += "**" + item.item_name + "**" + Environment.NewLine;
                                    }
                                }
                                myPending.lostItems = text;
                                //(bot._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                            }
                            playerDb.defindex = newDefindexCache;
                            playerDb.ids = newCache;
                            playerDb.timestamp = DateTime.Now;
                            IDFiles.StoreObjectForId(ulong.Parse(element.Key), "player.json", this, playerDb);
                            //(bot._client.GetChannel(ulong.Parse(arguments[0])) as IMessageChannel).SendMessageAsync(arguments[1]);
                        }
                    }
                }
            }
            foreach (var element in Program._client.Guilds)
            {
                var servid = IDFiles.ReturnObjectForId<TF2ServerDatabase>(element.Id, "server.json", this);
                if (servid.trackingChannelID != null && servid.trackingChannelID != "")
                {
                    foreach(var element2 in servid.trackedUsers)
                    {
                        if (pendingAnnounces.ContainsKey(ulong.Parse(element2)))
                        {
                            var pend = pendingAnnounces[ulong.Parse(element2)];
                            var user = element.GetUser(ulong.Parse(element2));
                            if (pend.receivedItems != null && pend.receivedItems != "")
                            {
                                var text = user.Username + " has received the following items in TF2:" + Environment.NewLine;
                                text += pend.receivedItems;
                                (Program._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                            }
                            if (pend.lostItems != null && pend.lostItems != "")
                            {
                                var text = user.Username + " has lost the following items in TF2:" + Environment.NewLine;
                                text += pend.lostItems;
                                (Program._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                            }
                        }
                    }
                }
            }
                    //PAST THIS POINT = BAD
                    /*
                    foreach(var element in bot._client.Guilds)
                    {
                        var servid = IDFiles.ReturnObjectForId<TF2ServerDatabase>(element.Id, "server.json", this);
                        if (servid.trackingChannelID != null && servid.trackingChannelID != "")
                        {
                            foreach(var element2 in servid.trackedUsers)
                            {
                                if (database.userIDtoSteamID.ContainsKey(element2))
                                {
                                    var success = false;
                                    var username = element.GetUser(ulong.Parse(element2)).Username;
                                    WebClient client = new WebClient();
                                    Stream stream = null;
                                    try
                                    {
                                        stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetPlayerItems/v0001/?SteamID=" + database.userIDtoSteamID[element2] + "&key=F7416B287CE89DC0407271739A22296C&format=json&language=en");
                                    }
                                    catch(Exception u)
                                    {
                                        //yep
                                    }
                                    StreamReader reader = null;
                                    TF2SchemaItems obj = null;
                                    if (stream != null)
                                    {
                                        reader = new StreamReader(stream);
                                        var sdata = reader.ReadToEnd();
                                        obj = JsonConvert.DeserializeObject<TF2SchemaItems>(sdata);
                                    }
                                        var retry = 0;
                                        if (stream != null && obj.result != null)
                                            success = true;
                                        if (stream != null)
                                    reader.Dispose();
                                        while( (stream == null || obj.result == null) && retries >= retry)
                                        {
                                        try
                                        {
                                            stream = client.OpenRead("http://api.steampowered.com/IEconItems_440/GetPlayerItems/v0001/?SteamID=" + database.userIDtoSteamID[element2] + "&key=F7416B287CE89DC0407271739A22296C&format=json&language=en");
                                        }
                                        catch(Exception u)
                                        {
                                           //yup
                                        }
                                        if (stream != null)
                                        {
                                            reader = new StreamReader(stream);
                                            var sdata = reader.ReadToEnd();
                                            obj = JsonConvert.DeserializeObject<TF2SchemaItems>(sdata);
                                        }
                                        if (stream != null && obj.result != null)
                                            success = true;
                                        if (stream != null)
                                            reader.Dispose();
                                            retry += 1;
                                        }
                                    if (success)
                                    {
                                        var playerDb = IDFiles.ReturnObjectForId<TF2PlayerDatabase>(ulong.Parse(element2), "player.json", this);
                                        if (!playerDb.cached)
                                        {
                                            playerDb.cached = true;
                                            if (playerDb.ids == null)
                                                playerDb.ids = new List<ulong>();
                                            foreach(var element3 in obj.result.items)
                                            {
                                                playerDb.ids.Add(element3.id);
                                                playerDb.defindex.Add(element3.defindex);
                                            }
                                            IDFiles.StoreObjectForId(ulong.Parse(element2), "player.json", this, playerDb);
                                        }
                                        else
                                        {
                                            var receivedItems = new List<TF2SchemaItem>();
                                            var lostItems = new List<TF2SchemaItem>();
                                            var newCache = new List<ulong>();
                                            var newDefindexCache = new List<ulong>();
                                            foreach(var element3 in obj.result.items)
                                            {
                                                newCache.Add(element3.id);
                                                newDefindexCache.Add(element3.defindex);
                                                if (!playerDb.ids.Contains(element3.id))
                                                {
                                                    receivedItems.Add(schemaItemsByIndex[element3.defindex]);
                                                }
                                            }
                                            for(var i=0;i<playerDb.ids.Count;i++)
                                            {
                                                var element3 = playerDb.ids[i];
                                                if (!newCache.Contains(element3))
                                                {
                                                    lostItems.Add(schemaItemsByIndex[playerDb.defindex[i]]);
                                                }
                                            }
                                            if (receivedItems.Count > 0)
                                            {
                                                var text = username + " has received the following items in TF2:" + Environment.NewLine;
                                                foreach(var item in receivedItems)
                                                {
                                                    text += "**" + item.item_name + "**" + " - " + item.item_description + Environment.NewLine;
                                                }
                                                (bot._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                                            }
                                            if (lostItems.Count > 0)
                                            {
                                                var text = username + " has lost the following items in TF2:" + Environment.NewLine;
                                                foreach (var item in lostItems)
                                                {
                                                    text += "**" + item.item_name + "**" + " - " + item.item_description + Environment.NewLine;
                                                }
                                                (bot._client.GetChannel(ulong.Parse(servid.trackingChannelID)) as IMessageChannel).SendMessageAsync(text);
                                            }
                                            playerDb.defindex = newDefindexCache;
                                            playerDb.ids = newCache;
                                            IDFiles.StoreObjectForId(ulong.Parse(element2), "player.json", this, playerDb);
                                            //(bot._client.GetChannel(ulong.Parse(arguments[0])) as IMessageChannel).SendMessageAsync(arguments[1]);
                                        }
                                    }
                                }
                            }
                        }
                    }*/
                }
        public override void Unload()
        {
            Program.RemoveCommand("tf2");
        }
    }
}
