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
        public bool CommandSuggest = true;
        public string CommandSuggestMessage = "Unknown command. Did you mean to type {0}{1}?";
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
            bot.AddCommand("show", new ShowCommand());
        }

        void UnknownCommand(MessageWrapper message)
        {
            if (config.CommandSuggest)
            {
                var lastSuggest = "";
                var lastScore = 0;
                foreach (var element in bot.commands)
                {
                    var split = message.Content.Split(' ');
                    var cmd = split[0].Substring(1).ToLowerInvariant();
                    
                    if (element.Key.Length >= cmd.Length)
                    {
                        var score = 0;
                        var iMiss = 0;
                        var key = element.Key.ToLowerInvariant();
                        for (var i = 0; i < key.Length; i++)
                        {
                            if (key[i] == cmd[iMiss])
                            {
                                score += 1;
                                iMiss += 1;
                            }
                            if (iMiss != i && cmd.Length > i && key[i] == cmd[i])
                            {
                                score += 1;
                            }
                            if (iMiss >= cmd.Length)
                                break;
                        }
                        if (score >= cmd.Length*0.5 && score >= lastScore)
                        {
                            //message.Channel.SendMessageAsync(String.Format(config.CommandSuggestMessage, bot.botSettings.defaultPrefix, element.Key));
                            lastSuggest = element.Key;
                            lastScore = score;
                        }
                        /*
                        if (element.Key.ToLowerInvariant().Substring(0, cmd.Length) == cmd)
                        {
                            message.Channel.SendMessageAsync(String.Format(config.CommandSuggestMessage, bot.botSettings.defaultPrefix, element.Key));
                            return;
                        }*/
                    }
                }
                if (lastSuggest != "")
                {
                    message.Channel.SendMessageAsync(String.Format(config.CommandSuggestMessage, bot.botSettings.defaultPrefix, lastSuggest));
                    return;
                }
            }
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
                Console.WriteLine("[("+ message.Channel.channel.Id.ToString() +")" + message.Channel.channel.Name + "] " + message.message.Author.Username + " : " + message.Content);
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
            bot.RemoveCommand("show");
        }
    }
}
