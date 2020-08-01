using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    class WebPacket
    {
        public string token;
        public int packet;
    }
    //Packet 0 - on receive on server parses as command to bot, on receive on client writes on console
    class ConsolePacket : WebPacket
    {
        public string text;
    }
    //Packet 1 - on receive on client, status tells if token was accepted or not, username is who you logged in as
    class TokenReturnPacket : WebPacket
    {
        public bool status;
        public string username = "";
    }
    //Packet 2 - on receive on server, verify the token and send back a tokenreturnpacket with results
    class TokenVerifyPacket : WebPacket
    {

    }
}
