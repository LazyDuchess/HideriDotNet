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
        public override void Initialize()
        {
            var configFile = Path.Combine(directory, "config.json");
            if (File.Exists(configFile))
                config = JsonConvert.DeserializeObject<BasicSettings>(File.ReadAllText(configFile));
            else
            {
                config = new BasicSettings();
                File.WriteAllText(configFile, JsonConvert.SerializeObject(config));
            }
            Program.onUnknownCommand += UnknownCommand;
            Program.onStop += StopCommand;
            Program.onMessage += MessageLog;
            Program.AddCommand("help", new HelpCommand());
            Program.AddCommand("ping", new LatencyCommand());
            Program.AddCommand("tell", new TellCommand());
            Program.AddCommand("show", new ShowCommand());
        }

        void UnknownCommand(MessageWrapper message)
        {
            if (config.CommandSuggest)
            {
                var lastSuggest = "";
                var lastScore = 0;
                foreach (var element in Program.commands)
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
                    message.Channel.SendMessageAsync(String.Format(config.CommandSuggestMessage, Program.botSettings.defaultPrefix, lastSuggest));
                    return;
                }
            }
            message.Channel.SendMessageAsync(String.Format(config.UnknownCommandMessage, Program.botSettings.defaultPrefix));
        }

        void StopCommand(MessageWrapper message)
        {
            message.Channel.SendMessageAsync(String.Format(config.StopMessage, Program.botSettings.defaultPrefix));
        }

        void MessageLog(MessageWrapper message)
        {
            if (!message.headless && config.logMessages)
            {
                Program.ConsoleWriteLine("[("+ message.Channel.channel.Id.ToString() +")" + message.Channel.channel.Name + "] " + message.message.Author.Username + " : " + message.Content);
            }
        }

        public override void Unload()
        {
            Program.onUnknownCommand -= UnknownCommand;
            Program.onStop -= StopCommand;
            Program.onMessage -= MessageLog;
            Program.RemoveCommand("help");
            Program.RemoveCommand("ping");
            Program.RemoveCommand("tell");
            Program.RemoveCommand("show");
        }
    }
}
