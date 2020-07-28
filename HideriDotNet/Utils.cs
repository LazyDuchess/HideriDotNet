using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace HideriDotNet
{
    public class MentionResult
    {
        public ulong id;
        public bool success;
    }
    public static class Utils
    {
        readonly static MentionResult MentionError = new MentionResult()
        {
            success = false
        };
        public static async Task<MentionResult> GetMention(string mention, IGuild guild = null)
        {
            ulong result;
            if (ulong.TryParse(mention, out result))
            {
                return new MentionResult()
                {
                    id = result,
                    success = true
                };
            }
            else if (mention.Substring(0,2) == "<@" && mention.Substring(mention.Length-1,1) == ">")
            {
                var offset = 2;
                if (mention.Substring(0, 3) == "<@!")
                    offset = 3;
                if (ulong.TryParse(mention.Substring(offset, mention.Length - offset - 1), out result))
                    return new MentionResult()
                    {
                        id = result,
                        success = true
                    };
            }
            else
            {
                if (guild != null)
                {
                    var users = await guild.GetUsersAsync();
                    foreach(var element in users)
                    {
                        if (element.Nickname != null && element.Nickname != "")
                        {
                            if (element.Nickname.Length >= mention.Length)
                            {
                                if (element.Nickname.Substring(0, mention.Length).ToLowerInvariant() == mention.ToLowerInvariant())
                                    return new MentionResult()
                                    {
                                        id = element.Id,
                                        success = true
                                    };
                            }
                        }
                        if (element.Username.Length >= mention.Length)
                        {
                            if (element.Username.Substring(0, mention.Length).ToLowerInvariant() == mention.ToLowerInvariant())
                                return new MentionResult()
                                {
                                    id = element.Id,
                                    success = true
                                };
                        }
                    }
                }
            }
            return MentionError;
        }
    }
}
