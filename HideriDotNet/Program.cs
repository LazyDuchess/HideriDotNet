using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using System.IO;
using Newtonsoft.Json;

namespace HideriDotNet
{
    public class Program
    {
        public List<BotModule> modules = new List<BotModule>();
        public BotSettings botSettings;
        public Thread uiThread;
        public DiscordSocketClient _client;
        //public ThreadStart 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            var program = new Program();
            
            var loadedConfig = false;
            if (File.Exists("config.json"))
            {
                try
                {
                    program.botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText("config.json"));
                    loadedConfig = true;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Couldn't load settings file");
                    Console.WriteLine(e.ToString());
                    loadedConfig = false;
                }
            }
            if (!loadedConfig)
            {
                Console.WriteLine("Creating a new settings file");
                program.botSettings = new BotSettings();
                var newSettings = JsonConvert.SerializeObject(program.botSettings);
                File.WriteAllText("config.json", newSettings);
                loadedConfig = true;
            }
            Console.WriteLine("Default prefix is " + program.botSettings.defaultPrefix);
            var uiThreadDelegate = new ThreadStart(program.UIThread);
            if (args.Contains("-ui"))
            {
                Console.WriteLine("Starting UI");
                program.uiThread = new Thread(uiThreadDelegate);
                program.uiThread.Start();
            }
            else
                Console.WriteLine("Starting without UI");
            program.MainAsync().GetAwaiter().GetResult();
        }

        void UIThread()
        {
            var form = new Form1();
            form.botProgram = this;
            form.Update();
            Application.Run(form);
        }

        public Program()
        {
            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, botSettings.botToken);
            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {

            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (botSettings.logMessages)
                Console.WriteLine("[" + message.Channel.Name + "] " + message.Author.Username + " : " + message.Content);
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Content == "/ping")
                await message.Channel.SendMessageAsync("Pingassi");
        }

    }
}
