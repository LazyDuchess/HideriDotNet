using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace ImageModule
{
    public class ImageModule : BotModule
    {
        Program bot;
        public override void Initialize(Program botCore)
        {
            bot = botCore;
            botCore.AddCommand("test", new TestCommand());
        }

        public override void Unload()
        {
            bot.RemoveCommand("test");
        }
    }
}
