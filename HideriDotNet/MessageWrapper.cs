using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    //Generic message wrapper to make it easier to access messages sent from console or Discord in the same way
    public class MessageWrapper
    {
        //Channel wrapper for the channel this message was sent in
        public ChannelWrapper Channel;
        //Actual Discord message
        public SocketMessage message;
        //Console?
        public bool headless = false;
        private readonly string content;
        //Was gonna use this for something but eh, might still be useful?
        private readonly DateTimeOffset timestamp;
        public UserWrapper Author;
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

        //Generate a message wrapper for an arbitrary non-discord message
        public MessageWrapper(string content)
        {
            this.content = content;
            this.headless = true;
            this.Channel = new ChannelWrapper();
            this.Author = Program.ConsoleAuthor;
        }
        //Generate a message wrapper for an actual discord message
        public MessageWrapper(SocketMessage message)
        {
            this.message = message;
            this.Channel = new ChannelWrapper(message.Channel);
            this.Author = new UserWrapper(message.Author);
        }
    }
}
