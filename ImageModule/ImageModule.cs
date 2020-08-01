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
        public override void Initialize()
        {
            Program.AddCommand("avatar", new AvatarCommand());
        }

        public override void Unload()
        {
            Program.RemoveCommand("avatar");
        }
    }
}
