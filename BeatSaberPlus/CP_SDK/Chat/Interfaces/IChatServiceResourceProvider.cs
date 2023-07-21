using System;
using System.Collections.Generic;

namespace CP_SDK.Chat.Interfaces
{
    public interface IChatServiceResourceManager<t_EmoteType>
        where t_EmoteType : IChatResourceData
    {
        bool IsReady { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request global resources
        /// </summary>
        void TryRequestGlobalResources();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try request channel resources
        /// </summary>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_OnChannelResourceDataCached">Callback</param>
        void TryRequestChannelResources(IChatChannel p_Channel, Action<Dictionary<string, IChatResourceData>> p_OnChannelResourceDataCached);
        /// <summary>
        /// Release channel
        /// </summary>
        /// <param name="p_Channel">Channel instance</param>
        void TryReleaseChannelResources(IChatChannel p_Channel);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get a custom user display name
        /// </summary>
        /// <param name="p_UserID">UserID</param>
        /// <param name="p_Default">Default display name</param>
        /// <param name="p_PaintedName">Output painted name</param>
        /// <returns></returns>
        bool TryGetUserDisplayName(string p_UserID, string p_Default, out string p_PaintedName);
        /// <summary>
        /// Get third party emote
        /// </summary>
        /// <param name="p_Word">Word</param>
        /// <param name="p_ChannelID">ID of the channel</param>
        /// <param name="p_Data"></param>
        /// <returns></returns>
        bool TryGetThirdPartyEmote(string p_Word, string p_ChannelID, out t_EmoteType p_Data);
    }
}
