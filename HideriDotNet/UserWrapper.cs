using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    //Generic wrapper for a discord channel, to make it easier to make commands that work both in console and discord
    public class UserWrapper
    {
        //Console?
        public bool headless = true;
        //Actual discord channel
        public SocketUser user;
        string username;
        string discriminator;
        ulong id;
        string avatarurl;
        public string AvatarURL
        {
            get
            {
                if (headless)
                    return avatarurl;
                else
                    return user.GetAvatarUrl(Discord.ImageFormat.Auto, 2048);
            }
        }
        public string Username
        {
            get
            {
                if (headless)
                    return username;
                else
                    return user.Username;
            }
        }
        public string Discriminator
        {
            get
            {
                if (headless)
                    return discriminator;
                else
                    return user.Discriminator;
            }
        }
        public ulong Id
        {
            get
            {
                if (headless)
                    return id;
                else
                    return user.Id;
            }
        }
        public UserWrapper()
        {
        }
        public UserWrapper(SocketUser user)
        {
            this.headless = false;
            this.user = user;
        }

        public UserWrapper(string username, string discriminator, ulong id, string avatar = "")
        {
            this.headless = true;
            this.username = username;
            this.discriminator = discriminator;
            this.id = id;
            this.avatarurl = avatar;
        }
        /*
        public void SendMessageAsync(string text)
        {
            if (!headless)
                channel.SendMessageAsync(text);
            else
            {
                Console.WriteLine(text);
                Program.wssv.WebSocketServices.Broadcast(text);
            }
        }*/
    }
}
