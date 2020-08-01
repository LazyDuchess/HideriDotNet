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
        public override void Initialize()
        {
            Program.AddCommand("modules", new PingCommand());
            Program.AddCommand("unload", new UnloadCommand());
            Program.AddCommand("load", new LoadCommand());
            Program.AddCommand("netload", new NetLoadCommand());
            Program.AddCommand("reload", new ReloadCommand());
        }

        public override void Unload()
        {
            Program.RemoveCommand("modules");
            Program.RemoveCommand("unload");
            Program.RemoveCommand("load");
            Program.RemoveCommand("netload");
            Program.RemoveCommand("reload");
        }
    }
}