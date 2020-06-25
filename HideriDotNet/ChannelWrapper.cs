using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    public class ChannelWrapper
    {
        public bool headless = true;
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
