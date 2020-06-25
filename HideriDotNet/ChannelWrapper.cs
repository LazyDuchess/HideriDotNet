using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    //Generic wrapper for a discord channel, to make it easier to make commands that work both in console and discord
    public class ChannelWrapper
    {
        //Console?
        public bool headless = true;
        //Actual discord channel
        public ISocketMessageChannel channel;

        public ChannelWrapper(ISocketMessageChannel channel)
        {
            this.headless = false;
            this.channel = channel;
        }

        public ChannelWrapper()
        {

        }
        public void SendMessageAsync(string text)
        {
            if (!headless)
                channel.SendMessageAsync(text);
            else
                Console.WriteLine(text);
        }
    }
}
