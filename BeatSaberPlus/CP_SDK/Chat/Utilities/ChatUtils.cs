using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models.Twitch;

namespace CP_SDK.Chat
{
    /// <summary>
    /// Chat utils
    /// </summary>
    public static class ChatUtils
    {
        /// <summary>
        /// Min RGB component for color scaling
        /// </summary>
        private const int MIN_RGB_OFFSET = 80;
        /// <summary>
        /// RGB scale factor
        /// </summary>
        private const float RGB_SCALE = (255f - (float)MIN_RGB_OFFSET) / 255f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Hexadecimal digits
        /// </summary>
        private static char[] m_HexDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get name color based on name hash
        /// </summary>
        /// <param name="p_Name">User name</param>
        /// <returns>HEX color in string</returns>
        public static string GetNameColor(string p_Name)
        {
            int l_NameHash = p_Name.GetHashCode();
            int l_R = (int)(MIN_RGB_OFFSET + (RGB_SCALE * ((float)((l_NameHash >> 16) & 0xFF))));
            int l_G = (int)(MIN_RGB_OFFSET + (RGB_SCALE * ((float)((l_NameHash >>  8) & 0xFF))));
            int l_B = (int)(MIN_RGB_OFFSET + (RGB_SCALE * ((float)((l_NameHash >>  0) & 0xFF))));

            var l_NameColor = new string(new char[]
            {
                '#',
                m_HexDigits[(l_R >> 4) & 0xF],
                m_HexDigits[(l_R >> 0) & 0xF],
                m_HexDigits[(l_G >> 4) & 0xF],
                m_HexDigits[(l_G >> 0) & 0xF],
                m_HexDigits[(l_B >> 4) & 0xF],
                m_HexDigits[(l_B >> 0) & 0xF],
                'F',
                'F'
            });

            return l_NameColor;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static TwitchChannel AsTwitchChannel(this IChatChannel channel)
        {
            return channel as TwitchChannel;
        }
        public static TwitchUser AsTwitchUser(this IChatUser user)
        {
            return user as TwitchUser;
        }
    }
}
