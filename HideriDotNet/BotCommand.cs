﻿using System;
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
            var args = arguments.Split(' ');
            return args.Where((val, idx) => idx != 0).ToArray();
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
        public virtual bool CanRunCommand( Program bot, string[] arguments, MessageWrapper message)
        {
            if (IsOwnerOnly())
            {
                if (message.headless) //Console
                    return true;
                if (bot.botSettings.owners.Contains(message.message.Author.Id.ToString())) //Bot owner
                    return true;
                return false;
            }
            return true;
        }
        //Run the command!
        public virtual bool Run( Program bot, string[] arguments, MessageWrapper message)
        {
            if (!CanRunCommand( bot, arguments, message))
            {
                message.Channel.SendMessageAsync("Can't execute that command in this context.");
                return false;
            }
            return true;
        }

    }
}
