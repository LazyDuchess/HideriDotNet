using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace HideriModules
{
    public class PingModule : BotModule
    {
        Program bot;
        public override void Initialize(Program botCore)
        {
            bot = botCore;
            botCore.AddCommand("modules", new PingCommand());
            botCore.AddCommand("unload", new UnloadCommand());
            botCore.AddCommand("load", new LoadCommand());
            botCore.AddCommand("reload", new ReloadCommand());
        }

        public override void Unload()
        {
            bot.RemoveCommand("modules");
            bot.RemoveCommand("unload");
            bot.RemoveCommand("load");
            bot.RemoveCommand("reload");
        }
    }
}