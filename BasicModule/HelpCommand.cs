using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideriDotNet;

namespace BasicModule
{
    public class HelpCommand : BotCommand
    {
        public override string GetHelp()
        {
            return "Display help.";
        }

        public override bool Run( string[] arguments, MessageWrapper message)
        {
            if (base.Run( arguments, message))
            {
                var builtstring = "";
                foreach (var element in Program.commands)
                {
                    builtstring += Program.botSettings.defaultPrefix + element.Key;
                    if (element.Value.GetUsage() != "")
                        builtstring += " " + element.Value.GetUsage();
                    if (element.Value.GetHelp() != "")
                        builtstring += " - " + element.Value.GetHelp();
                    builtstring += Environment.NewLine;
                    
                }
                message.Channel.SendMessageAsync(builtstring);
                return true;
            }
            return false;
        }
    }
}
