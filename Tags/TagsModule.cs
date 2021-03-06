﻿using HideriDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Tags
{
    public class TagsModule : BotModule
    {
        public static TagsModule instance;
        public static TagDatabase database;
        public override void Initialize()
        {
            instance = this;
            if (File.Exists(Path.Combine(directory, "database.json")))
                database = JsonConvert.DeserializeObject<TagDatabase>(File.ReadAllText(Path.Combine(directory, "database.json")));
            else
                database = new TagDatabase();
            var tagCommand = new TagCommand();
            Program.AddCommand("t", new TagCommand());
            Program.AddCommand("tag", new TagCommand());
        }

        public static void SaveTags()
        {
            File.WriteAllText(Path.Combine(instance.directory, "database.json"), JsonConvert.SerializeObject(database));
        }

        public override void Unload()
        {
            Program.RemoveCommand("t");
            Program.RemoveCommand("tag");
            File.WriteAllText(Path.Combine(directory, "database.json"), JsonConvert.SerializeObject(database));
        }

        public override void CleanUp()
        {
            base.CleanUp();
            File.WriteAllText(Path.Combine(directory, "database.json"), JsonConvert.SerializeObject(database));
        }
    }

    public class TagDatabase
    {
        public Dictionary<string,Tag> tags = new Dictionary<string,Tag>();
    }

    public class Tag
    {
        public string ownerName;
        public string ownerTag;
        public string ownerID;
        public string content;
        public string attachmentFilename = "";
        public string attachmentUrl = "";

        public Tag(string ownerName, string ownerTag, string ownerID, string content, string attachmentFilename = "", string attachmentUrl = "")
        {
            this.ownerID = ownerID;
            this.ownerName = ownerName;
            this.ownerTag = ownerTag;
            this.content = content;
            this.attachmentFilename = attachmentFilename;
            this.attachmentUrl = attachmentUrl;
        }
    }
}
