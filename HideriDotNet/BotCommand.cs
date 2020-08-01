using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;


namespace HideriDotNet
{
    //Command class
    public abstract class BotCommand
    {
        public virtual string GetCategory()
        {
            return "General";
        }
        public virtual int GetPriority()
        {
            return 0;
        }
        //Command usage, eg [Argument 0] [Argument 1]
        public virtual string GetUsage()
        {
            return "";
        }
        //Description of what the command does for help commands
        public virtual string GetHelp()
        {
            return "";
        }
        //Split the arguments
        public virtual string[] splitArgs( string arguments )
        {
            var split = arguments.Split(' ');
            if (split.Length > 1)
                arguments = arguments.Substring(split[0].Length + 1);
            else
                return new string[] { };
            if (getArguments() >= 0)
            {
                var instring = false;
                var argList = new List<string>();
                //var arg = new string[getArguments()];
                var currentArg = 0;
                var currentArgString = "";
                for (var i=0;i<arguments.Length;i++)
                {
                    if (arguments[i] == '"')
                    {
                        instring = !instring;
                        continue;
                    }
                    if (currentArg + 1 != getArguments() && instring == false)
                    {
                        if (arguments[i] != ' ')
                        {
                            currentArgString += arguments[i];
                        }
                        else
                        {
                            argList.Add(currentArgString);
                            currentArg += 1;
                            currentArgString = "";
                        }
                    }
                    else
                    {
                        currentArgString += arguments[i];
                    }
                }
                argList.Add(currentArgString);
                return argList.ToArray();
            }
            else
                return new string[] { };
        }

        //Only really necessary for commands that accept arguments with spaces and the like for the last argument, so that it doesn't get split into arguments and instead counts as 1
        public virtual int getArguments()
        {
            return 0;
        }
        //Only allow console and bot owners to use this command?
        public virtual bool IsOwnerOnly()
        {
            return false;
        }
        //Perform command run check, can override
        public virtual bool CanRunCommand( string[] arguments, MessageWrapper message)
        {
            if (IsOwnerOnly())
            {
                if (Program.botSettings.owners.Contains(message.Author.Id.ToString())) //Bot owner
                    return true;
                return false;
            }
            return true;
        }
        //Run the command!
        public virtual bool Run( string[] arguments, MessageWrapper message)
        {
            if (!CanRunCommand( arguments, message))
            {
                message.Channel.SendMessageAsync("Can't execute that command in this context.");
                return false;
            }
            return true;
        }

    }
}
