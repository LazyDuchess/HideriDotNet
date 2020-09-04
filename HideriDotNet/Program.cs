using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;
using System.Net;

namespace HideriDotNet
{
    public class RCONConnection
    {
        public WebSocket socket;
        public bool logged = false;
        public string token = "";
        public RCONConnection(WebSocket socket)
        {
            this.socket = socket;
        }
    }
    public class RCONUser
    {
        public string Username;
        public string Discriminator;
        public ulong Id;
        public string AvatarURL = "";
    }
    public class RCONSettings
    {
        public bool enabled = false;
        public string ip = "192.168.0.9";
        public int port = 27015;
        public bool secure = true;
        public string certificate = "rcon.pfx";
        public string certificatePassword = "1234";
        public bool autoUpdateUsers = false;
    }
    public class RCON : WebSocketBehavior
    {
        public static Dictionary<string, RCONUser> RCONUsersByToken = new Dictionary<string, RCONUser>();
        //private string _suffix;

        public RCON()
        {
            
        }
        protected override void OnOpen()
        {
            Program.rconConnections[Context.WebSocket] = new RCONConnection(Context.WebSocket);
            Console.WriteLine("There's a new RCON connection from " + Context.UserEndPoint.Address.ToString());
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Program.rconConnections.Remove(Context.WebSocket);
            Console.WriteLine("Dropped RCON connection from " + Context.UserEndPoint.Address.ToString());
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            //Console.WriteLine("Packet: "+e.Data);
            var pack = JsonConvert.DeserializeObject<WebPacket>(e.Data);
            switch(pack.packet)
            {
                case 0:
                    if (!RCONUsersByToken.ContainsKey(pack.token))
                        return;
                    var cs = JsonConvert.DeserializeObject<ConsolePacket>(e.Data);
                    Console.WriteLine("From RCON Connection: " + cs.text);
                    var console = cs.text;
                    var msg = new MessageWrapper(console);
                    var rconuser = RCONUsersByToken[pack.token];
                    var us = new UserWrapper(rconuser.Username, rconuser.Discriminator, rconuser.Id);
                    msg.Channel = new RCONChannel()
                    {
                        instance = Context.WebSocket
                    };
                    msg.Author = us;
                    if (console.Length >= Program.botSettings.defaultPrefix.Length)
                    {
                        var args = console.Split(' ');
                        var cmd = args[0];
                        if (cmd.Substring(0, Program.botSettings.defaultPrefix.Length) == Program.botSettings.defaultPrefix)
                        {
                            cmd = cmd.Substring(Program.botSettings.defaultPrefix.Length);
                            if (Program.commands.ContainsKey(cmd))
                            {
                                try
                                {
                                    Program.commands[cmd].Run(Program.commands[cmd].splitArgs(console), msg);
                                    var returnString = "";
                                    var returnInfo = Program.commands[cmd].GetInputField(Program.commands[cmd].splitArgs(console), msg);
                                    if (returnInfo.keepCommand)
                                        returnString = args[0];
                                    if (returnInfo.keepArguments.Length > 0)
                                        returnString += " ";
                                    foreach (var element in returnInfo.keepArguments)
                                        returnString += element + " ";
                                    returnString += returnInfo.suffix;
                                    var inputPacket = new ConsolePacket();
                                    inputPacket.packet = 3;
                                    inputPacket.text = returnString;
                                    Send(JsonConvert.SerializeObject(inputPacket));
                                }
                                catch(Exception se)
                                {
                                    var errorPacket = new ConsolePacket();
                                    errorPacket.packet = 0;
                                    errorPacket.text = "Failed to run command. Exception:" + Environment.NewLine + se.ToString();
                                    Send(JsonConvert.SerializeObject(errorPacket));
                                }
                            }
                            else
                            {
                                Program.onUnknownCommand?.Invoke(msg);
                            }
                        }
                        else
                        {
                            var inputPacket = new ConsolePacket();
                            inputPacket.packet = 3;
                            inputPacket.text = "";
                            Send(JsonConvert.SerializeObject(inputPacket));
                            foreach (var element in Program.rconConnections)
                            {
                                if (element.Key != Context.WebSocket)
                                {
                                    var messagePack = new ConsolePacket();
                                    messagePack.packet = 0;
                                    messagePack.token = pack.token;
                                    messagePack.text = "[RCON] " + rconuser.Username + ": " + console;
                                    element.Key.Send(JsonConvert.SerializeObject(messagePack));
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    var returnPacket = new TokenReturnPacket();
                    returnPacket.packet = 1;
                    if (RCONUsersByToken.ContainsKey(pack.token))
                    {
                        returnPacket.status = true;
                        returnPacket.username = RCONUsersByToken[pack.token].Username;
                        Program.rconConnections[Context.WebSocket].logged = true;
                        Program.rconConnections[Context.WebSocket].token = pack.token;
                        Console.WriteLine("RCON connection from " + Context.UserEndPoint.Address.ToString()+ " logged in as "+returnPacket.username);
                    }
                    else
                    {
                        returnPacket.status = false;
                        Program.rconConnections[Context.WebSocket].logged = false;
                        Program.rconConnections[Context.WebSocket].token = "";
                        Console.WriteLine("RCON connection from " + Context.UserEndPoint.Address.ToString() + " failed to log in, used incorrect token '"+pack.token+"'");
                    }
                    Send(JsonConvert.SerializeObject(returnPacket));
                    break;
            }
            
        }
    }
    public class Program
    {
        public static Dictionary<WebSocket, RCONConnection> rconConnections = new Dictionary<WebSocket, RCONConnection>();
        public static RCONSettings rconSettings = new RCONSettings();
        public static UserWrapper ConsoleAuthor;
        public static bool Debug = false;
        public delegate void MessageEvent(MessageWrapper message);

        public static MessageEvent onMessage;
        public static MessageEvent onUnknownCommand;
        public static MessageEvent onStop;

        public Form1 form;
        public static Dictionary<string, BotModule> modules = new Dictionary<string, BotModule>();
        public static Dictionary<string, BotCommand> commands = new Dictionary<string, BotCommand>();
        public static BotSettings botSettings;
        public Thread uiThread;
        public static DiscordSocketClient _client;
        public static WebSocketServer wssv;
        public static bool loadedConfig = false;
        //public ThreadStart 

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (File.Exists("rconusers.json"))
            {
                try
                {
                    Console.WriteLine("Loading RCON Users from rconusers.json.");
                    RCON.RCONUsersByToken = JsonConvert.DeserializeObject<Dictionary<string, RCONUser>>(File.ReadAllText("rconusers.json"));
                    Console.WriteLine("RCON Users loaded.");
                }
                catch(Exception e)
                {
                    Console.WriteLine("Failed to load RCON Users. Exception:"+Environment.NewLine+e.ToString());
                }
            }
            else
            {
                Console.WriteLine("Can't find rconusers.json, skipping loading of RCON users.");
            }
            if (File.Exists("rcon.json"))
            {
                try
                {
                    Console.WriteLine("Loading RCON settings from rcon.json.");
                    rconSettings = JsonConvert.DeserializeObject<RCONSettings>(File.ReadAllText("rcon.json"));
                    Console.WriteLine("RCON settings loaded.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load RCON settings. Exception:" + Environment.NewLine + e.ToString());
                }
            }
            else
            {
                File.WriteAllText("rcon.json", JsonConvert.SerializeObject(rconSettings));
                Console.WriteLine("Can't find rcon.json, loading default RCON settings.");
            }
            if (rconSettings.enabled)
            {
                wssv = new WebSocketServer(IPAddress.Parse(rconSettings.ip), rconSettings.port, rconSettings.secure);
                wssv.AddWebSocketService<RCON>("/RCON");
                if (rconSettings.secure)
                {
                    wssv.SslConfiguration.ServerCertificate =
          new X509Certificate2(rconSettings.certificate, rconSettings.certificatePassword);
                }
                wssv.Start();
                Console.WriteLine("Starting rcon server at ws"+ (rconSettings.secure ? "s" : "") +"://" + rconSettings.ip + ":" + rconSettings.port.ToString() + "/RCON");
            }
            var program = new Program();
            AddCommand("stop", new StopCommand());
            var moduleFolders = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "Modules"));
            foreach(var element in moduleFolders)
            {
                LoadModule(Path.GetFileName(element));
            }
            //program.LoadModule("PingModule");
            if (File.Exists("config.json"))
            {
                try
                {
                    botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText("config.json"));
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
                botSettings = new BotSettings();
                
            }
            Console.WriteLine("Default prefix is " + botSettings.defaultPrefix);
            if (args.Contains("-invisible"))
            {
                var handle = GetConsoleWindow();

                // Hide
                ShowWindow(handle, SW_HIDE);
            }
            if (args.Contains("-debug"))
            {
                Debug = true;
            }
                Application.ApplicationExit += Application_ApplicationExit;
            program.MainAsync().GetAwaiter().GetResult();
            
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting");
        }

        public static bool UnloadModule(string moduleName)
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
        public static void ConsoleWrite(string text)
        {
            Console.Write(text);
            var js = new ConsolePacket();
            js.packet = 0;
            js.text = text;
            if (rconSettings.enabled)
            {
                //wssv.WebSocketServices.Broadcast(JsonConvert.SerializeObject(js));
                foreach(var element in rconConnections)
                {
                    if (element.Value.logged)
                    {
                        element.Key.Send(JsonConvert.SerializeObject(js));
                    }
                }
            }
        }
        public static void ConsoleWriteLine(string text)
        {
            Console.WriteLine(text);
            var js = new ConsolePacket();
            js.packet = 0;
            js.text = text;
            if (rconSettings.enabled)
            {
                //wssv.WebSocketServices.Broadcast(JsonConvert.SerializeObject(js));
                foreach (var element in rconConnections)
                {
                    if (element.Value.logged)
                    {
                        element.Key.Send(JsonConvert.SerializeObject(js));
                    }
                }
            }
        }
        public static bool LoadModule(string moduleName)
        {
            
            Console.WriteLine("Loading module " + Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName + "/" + moduleName + ".dll"));
            try
            {
                var moduleData = JsonConvert.DeserializeObject<ModuleData>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName + "/data.json")));
                var data = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName + "/" + moduleName + ".dll"));
                //var asm = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(),"Modules/" + moduleName + "/" + moduleName + ".dll"));
                var asm = Assembly.Load(data);
                var type = asm.GetType(moduleData.main);
                var runnable = Activator.CreateInstance(type) as BotModule;
                if (runnable != null)
                {
                    runnable.data = moduleData;
                    runnable.directory = Path.Combine(Directory.GetCurrentDirectory(), "Modules/" + moduleName);
                    modules[moduleName] = runnable;
                    runnable.Initialize();
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

        public static void AddCommand(string cmd, BotCommand command)
        {
            Console.WriteLine("Registering " + cmd + " command.");
            commands[cmd] = command;
        }

        public static void RemoveCommand(string command)
        {
            if (commands.ContainsKey(command))
                commands.Remove(command);
        }

        public static void Update()
        {
            /*
            if (form != null)
                form.Update();*/
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
        public static void Stop()
        {
            foreach(var element in modules)
            {
                element.Value.CleanUp();
            }
            _client.Dispose();
            if (rconSettings.enabled)
                wssv.Stop();
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
                if (console == "hide")
                {
                    var handle = GetConsoleWindow();

                    // Hide
                    ShowWindow(handle, SW_HIDE);
                }
                if (console.Length >= botSettings.defaultPrefix.Length)
                {
                    var args = console.Split(' ');
                    var cmd = args[0];
                    if (cmd.Substring(0, botSettings.defaultPrefix.Length) == botSettings.defaultPrefix)
                    {
                        cmd = cmd.Substring(botSettings.defaultPrefix.Length);
                        if (commands.ContainsKey(cmd))
                        {
                            commands[cmd].Run( commands[cmd].splitArgs(console), new MessageWrapper(console));
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
            if (!loadedConfig)
            {
                ConsoleAuthor = new UserWrapper(_client.CurrentUser.Username, _client.CurrentUser.Discriminator, _client.CurrentUser.Id, _client.CurrentUser.GetAvatarUrl(ImageFormat.Auto,2048));
                botSettings.defaultUsername = ConsoleAuthor.Username;
                botSettings.defaultUserDiscriminator = ConsoleAuthor.Discriminator;
                botSettings.defaultUserID = ConsoleAuthor.Id;
                botSettings.defaultUserAvatar = ConsoleAuthor.AvatarURL;
                var newSettings = JsonConvert.SerializeObject(botSettings);
                File.WriteAllText("config.json", newSettings);
            }
            else
                ConsoleAuthor = new UserWrapper(botSettings.defaultUsername, botSettings.defaultUserDiscriminator, botSettings.defaultUserID, botSettings.defaultUserAvatar);
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            onMessage?.Invoke(new MessageWrapper(message));
            if (!botSettings.selfCommands)
            {
                if (message.Author.Id == _client.CurrentUser.Id)
                    return;
            }
            if (message.Content.Length < botSettings.defaultPrefix.Length)
                return;
            var args = message.Content.Split(' ');
            var cmd = args[0];
            if (cmd.Substring(0,botSettings.defaultPrefix.Length) == botSettings.defaultPrefix)
            {
                cmd = cmd.Substring(botSettings.defaultPrefix.Length);
                    if (commands.ContainsKey(cmd))
                    {
                    try
                    {
                        commands[cmd].Run( commands[cmd].splitArgs(message.Content), new MessageWrapper(message));
                    }
                    catch(Exception ex)
                    {
                        if (!Debug)
                            message.Channel.SendMessageAsync("Something went wrong, sorry!");
                        else
                        {
                            message.Channel.SendMessageAsync("Something went wrong. Stack trace: "+Environment.NewLine + "```" + ex.ToString() + "```");
                        }
                    }
                    }
                    else
                    {
                        onUnknownCommand?.Invoke(new MessageWrapper(message));
                    }
            }
          
        }

    }
}
