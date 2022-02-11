using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using BeatSaberPlus.SDK.Chat.Services.Twitch;
using System;
using System.Collections.Concurrent;

namespace BeatSaberPlus.SDK.Chat
{
    public static class ChatUtils
    {


        public static TwitchChannel AsTwitchChannel(this IChatChannel channel)
        {
            return channel as TwitchChannel;
        }

        public static TwitchUser AsTwitchUser(this IChatUser user)
        {
            return user as TwitchUser;
        }


        private static ConcurrentDictionary<int, string> _userColors = new ConcurrentDictionary<int, string>();
        public static string GetNameColor(string name)
        {
            int nameHash = name.GetHashCode();
            if (!_userColors.TryGetValue(nameHash, out var nameColor))
            {
                // Generate a psuedo-random color based on the users display name
                Random rand = new Random(nameHash);
                int argb = (rand.Next(255) << 16) + (rand.Next(255) << 8) + rand.Next(255);
                string colorString = string.Format("#{0:X6}FF", argb);
                _userColors.TryAdd(nameHash, colorString);
                nameColor = colorString;
            }
            return nameColor;
        }
    }
}
