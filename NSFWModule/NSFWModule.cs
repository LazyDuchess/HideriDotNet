using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace NSFWModule
{
    public class NSFWModule : BotModule
    {
        public override void Initialize()
        {
            Program.AddCommand("gelbooru", new GelbooruCommand());
        }

        public override void Unload()
        {
            Program.RemoveCommand("gelbooru");
        }
    }
}
