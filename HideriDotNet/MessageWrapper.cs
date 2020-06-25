using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    public class MessageWrapper
    {
        public ChannelWrapper Channel;
        public SocketMessage message;
        public bool headless = false;
        private readonly string content;
        private readonly DateTimeOffset timestamp;
        public string Content
        {
            get
            {
                if (headless)
                    return content;
                else
                    return message.Content;
            }
        }

        public DateTimeOffset Timestamp
        {
            get
            {
                if (headless)
                    return timestamp;
                else
                    return message.Timestamp;
            }
        }

        public MessageWrapper(string content)
        {
            this.content = content;
            this.headless = true;
            this.Channel = new ChannelWrapper();
        }

        public MessageWrapper(SocketMessage message)
        {
            this.message = message;
            this.Channel = new ChannelWrapper(message.Channel);
        }
    }
}
