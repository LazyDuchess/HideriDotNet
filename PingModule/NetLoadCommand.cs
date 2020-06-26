using HideriDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace HideriModules
{
    public class NetLoadCommand : BotCommand
    {
        public override int getArguments()
        {
            return 1;
        }
        public override string GetUsage()
        {
            return "[Module url]";
        }
        public override string GetHelp()
        {
            return "Load a module from the internet.";
        }
        public override bool IsOwnerOnly()
        {
            return true;
        }
        public override bool Run(Program bot, string[] arguments, MessageWrapper message)
        {
            if (base.Run( bot, arguments, message))
            {
                var filename = Path.GetFileName(arguments[0]);
                message.Channel.SendMessageAsync("Attempting to download module " + Path.GetFileNameWithoutExtension(arguments[0]));
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(arguments[0], Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + filename));
                message.Channel.SendMessageAsync("Download successful, extracting module...");
                var result = bot.UnloadModule(Path.GetFileNameWithoutExtension(arguments[0]));
                if (result)
                {
                    message.Channel.SendMessageAsync("Found module already loaded with the same name, unloaded.");
                }
                if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + Path.GetFileNameWithoutExtension(arguments[0]))))
                {
                    Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + Path.GetFileNameWithoutExtension(arguments[0])), true);
                    message.Channel.SendMessageAsync("Overwriting module with same name.");
                }
                ZipFile.ExtractToDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + filename), Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
                message.Channel.SendMessageAsync("Extraction successful, loading module...");
                result = bot.LoadModule(Path.GetFileNameWithoutExtension(arguments[0]));
                if (result)
                    message.Channel.SendMessageAsync("Module loaded.");
                else
                    message.Channel.SendMessageAsync("Couldn't load module.");
                return true;
            }
            return false;
        }
    }
}
