using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;
using System.IO;
using Newtonsoft.Json;

namespace BasicModule
{
    public class BasicSettings
    {
        public string UnknownCommandMessage = "Unknown command. Type {0}help for a list of commands.";
        public string StopMessage = "Stopping bot.";
        public bool logMessages = true;
    }
    public class BasicModule : BotModule
    {
        BasicSettings config;
        Program bot;
        public override void Initialize(Program botCore)
        {
            var configFile = Path.Combine(directory, "config.json");
            if (File.Exists(configFile))
                config = JsonConvert.DeserializeObject<BasicSettings>(File.ReadAllText(configFile));
            else
            {
                config = new BasicSettings();
                File.WriteAllText(configFile, JsonConvert.SerializeObject(config));
            }
            bot = botCore;
            bot.onUnknownCommand += UnknownCommand;
            bot.onStop += StopCommand;
            bot.onMessage += MessageLog;
            bot.AddCommand("help", new HelpCommand());
            bot.AddCommand("ping", new LatencyCommand());
            bot.AddCommand("tell", new TellCommand());
        }

        void UnknownCommand(MessageWrapper message)
        {
            message.Channel.SendMessageAsync(String.Format(config.UnknownCommandMessage,bot.botSettings.defaultPrefix));
        }

        void StopCommand(MessageWrapper message)
        {
            message.Channel.SendMessageAsync(String.Format(config.StopMessage, bot.botSettings.defaultPrefix));
        }

        void MessageLog(MessageWrapper message)
        {
            if (!message.headless && config.logMessages)
            {
                Console.WriteLine("[" + message.Channel.channel.Name + "] " + message.message.Author.Username + " : " + message.Content);
            }
        }

        public override void Unload()
        {
            bot.onUnknownCommand -= UnknownCommand;
            bot.onStop -= StopCommand;
            bot.onMessage -= MessageLog;
            bot.RemoveCommand("help");
            bot.RemoveCommand("ping");
            bot.RemoveCommand("tell");
        }
    }
}
