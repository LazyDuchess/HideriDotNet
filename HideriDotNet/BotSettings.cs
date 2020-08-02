using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideriDotNet
{
    //Bot settings
    public class BotSettings
    {
        public string defaultPrefix = "/";
        public string botName = "Hideri";
        public string botToken = "-";
        public List<string> owners = new List<string>();
        public ulong defaultUserID;
        public string defaultUsername;
        public string defaultUserDiscriminator;
    }
}
