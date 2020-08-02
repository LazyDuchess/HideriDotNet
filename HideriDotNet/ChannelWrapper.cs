using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;

namespace HideriDotNet
{
    //Wrapper for RCON Channel
    public class RCONChannel : ChannelWrapper
    {
        public WebSocket instance;
        public override void SendMessageAsync(string text)
        {
            Console.WriteLine("(For RCON Connection) "+text);
            var js = new ConsolePacket();
            js.packet = 0;
            js.text = text;
            instance.Send(JsonConvert.SerializeObject(js));
           // Program.wssv.WebSocketServices.Broadcast(JsonConvert.SerializeObject(js));
        }
    }
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
        public virtual void SendMessageAsync(string text)
        {
            if (!headless)
                channel.SendMessageAsync(text);
            else
            {
                Console.WriteLine(text);
                var js = new ConsolePacket();
                js.packet = 0;
                js.text = text;
                Program.wssv.WebSocketServices.Broadcast(JsonConvert.SerializeObject(js));
            }
        }
    }
}
