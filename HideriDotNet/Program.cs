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
using System.Reflection;

namespace HideriDotNet
{
    public class Program
    {
        public delegate void MessageEvent(MessageWrapper message);

        public MessageEvent onMessage;
        public MessageEvent onUnknownCommand;
        public MessageEvent onStop;

        public Form1 form;
        public Dictionary<string, BotModule> modules = new Dictionary<string, BotModule>();
        public Dictionary<string, BotCommand> commands = new Dictionary<string, BotCommand>();
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
            program.AddCommand("stop", new StopCommand());
            var moduleFolders = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
            foreach(var element in moduleFolders)
            {
                program.LoadModule(Path.GetFileName(element));
            }
            //program.LoadModule("PingModule");
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
            //UI is kinda useless so it's been scrapped for now, you can do almost anything in the console anyways
            /*
            if (args.Contains("-ui"))
            {
                Console.WriteLine("Starting UI");
                program.uiThread = new Thread(uiThreadDelegate);
                program.uiThread.Start();
            }
            else
                Console.WriteLine("Starting without UI");*/
            //Ignore me
            Application.ApplicationExit += Application_ApplicationExit;
            program.MainAsync().GetAwaiter().GetResult();
            
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting");
        }

        public bool UnloadModule(string moduleName)
        {
            Console.WriteLine("Unloading module " + moduleName);
            if (modules.ContainsKey(moduleName))
            {
                modules[moduleName].Unload();
                modules.Remove(moduleName);
                Console.WriteLine(moduleName + " unloaded");
                Update();
                return true;
            }
            Console.WriteLine("Couldn't find module " + moduleName);
            
            return false;
        }
        public bool LoadModule(string moduleName)
        {
            
            Console.WriteLine("Loading module " + Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName + "/" + moduleName + ".dll"));
            try
            {
                var moduleData = JsonConvert.DeserializeObject<ModuleData>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName + "/data.json")));
                var asm = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(),"Modules/" + moduleName + "/" + moduleName + ".dll"));
                var type = asm.GetType(moduleData.main);
                var runnable = Activator.CreateInstance(type) as BotModule;
                if (runnable != null)
                {
                    runnable.data = moduleData;
                    runnable.directory = Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName);
                    this.modules[moduleName] = runnable;
                    runnable.Initialize(this);
                    Console.WriteLine("Module loaded successfully!");
                    Update();
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to load module " + moduleName + "!");
                Console.WriteLine(e.ToString());
                
                return false;
            }
        }

        public void AddCommand(string cmd, BotCommand command)
        {
            Console.WriteLine("Registering " + cmd + " command.");
            commands[cmd] = command;
        }

        public void RemoveCommand(string command)
        {
            if (commands.ContainsKey(command))
                commands.Remove(command);
        }

        public void Update()
        {
            if (form != null)
                form.Update();
        }

        void UIThread()
        {
            form = new Form1();
            form.botProgram = this;
            Update();
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
        public void Stop()
        {
            foreach(var element in modules)
            {
                element.Value.CleanUp();
            }
            this._client.Dispose();
            Application.Exit();
        }
        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, botSettings.botToken);
            await _client.StartAsync();
            while (true)
            {
                var console = Console.ReadLine();
                onMessage?.Invoke(new MessageWrapper(console));
                if (console.Length >= botSettings.defaultPrefix.Length)
                {
                    var args = console.Split(' ');
                    var cmd = args[0];
                    if (cmd.Substring(0, botSettings.defaultPrefix.Length) == botSettings.defaultPrefix)
                    {
                        cmd = cmd.Substring(botSettings.defaultPrefix.Length);
                        if (commands.ContainsKey(cmd))
                        {
                            commands[cmd].Run(this, commands[cmd].splitArgs(console), new MessageWrapper(console));
                        }
                        else
                        {
                            onUnknownCommand?.Invoke(new MessageWrapper(console));
                        }
                    }
                }
            }
            // Block the program until it is closed.
            //await Task.Delay(Timeout.Infinite);
            
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
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;
            onMessage?.Invoke(new MessageWrapper(message));
            if (message.Content.Length < botSettings.defaultPrefix.Length)
                return;
            var args = message.Content.Split(' ');
            var cmd = args[0];
            if (cmd.Substring(0,botSettings.defaultPrefix.Length) == botSettings.defaultPrefix)
            {
                cmd = cmd.Substring(botSettings.defaultPrefix.Length);
                if (commands.ContainsKey(cmd))
                {
                    commands[cmd].Run(this, commands[cmd].splitArgs(message.Content), new MessageWrapper(message));
                }
                else
                {
                    onUnknownCommand?.Invoke(new MessageWrapper(message));
                }
            }
            
            /*
            if (message.Content == "/ping")
                await message.Channel.SendMessageAsync("Pingassi");*/
        }

    }
}
