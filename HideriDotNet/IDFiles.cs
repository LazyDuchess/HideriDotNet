using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HideriDotNet.Database
{
    public static class IDFiles
    {
        //To save files for specific ids like users, channels, server specific configs.
        public static string RetrieveFileForId(ulong id, string filename, BotModule module)
        {
            var fold = Path.Combine(module.directory, "db", id.ToString());
            Directory.CreateDirectory(fold);
            return Path.Combine(fold,filename);
        }

        public static T ReturnObjectForId<T>(ulong id, string filename, BotModule module) where T : new()
        {
            var folder = RetrieveFileForId(id, filename, module);
            if (!File.Exists(folder))
            {
                var obj = new T();
                File.WriteAllText(folder,JsonConvert.SerializeObject(obj));
                return new T();
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(folder));
            }
        }

        public static void StoreObjectForId(ulong id, string filename, BotModule module, object obj)
        {
            var folder = RetrieveFileForId(id, filename, module);
            File.WriteAllText(folder, JsonConvert.SerializeObject(obj));
            /*
            if (!File.Exists(folder))
            {
                var obj = new T();
                File.WriteAllText(folder, JsonConvert.SerializeObject(obj));
                return new T();
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(folder);
            }*/
        }
    }
}
