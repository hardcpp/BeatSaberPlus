using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.Models.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch data provider
    /// </summary>
    public class TwitchDataProvider
    {
        private SemaphoreSlim   m_GlobalLock        = new SemaphoreSlim(1, 1), m_ChannelLock = new SemaphoreSlim(1, 1);
        private HashSet<string> m_ChannelDataCached = new HashSet<string>();

        private ChatPlexGradientNamesDataProvider   m_ChatPlexGradientNamesDataProvider = new ChatPlexGradientNamesDataProvider("https://data.chatplex.org/twitch_gradient_names.json");
        private TwitchBadgeProvider                 m_TwitchBadgeProvider               = new TwitchBadgeProvider();
        private TwitchCheermoteProvider             m_TwitchCheermoteProvider           = new TwitchCheermoteProvider();
        private BTTVDataProvider                    m_BTTVDataProvider                  = new BTTVDataProvider();
        private FFZDataProvider                     m_FFZDataProvider                   = new FFZDataProvider();
        private _7TVDataProvider                    m_7TVDataProvider                   = new _7TVDataProvider();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public bool IsReady { get; internal set; } = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request global resources
        /// </summary>
        public void TryRequestGlobalResources(string p_Token)
        {
            Task.Run(async () =>
            {
                await m_GlobalLock.WaitAsync().ConfigureAwait(false);

                try
                {
                    await m_ChatPlexGradientNamesDataProvider.TryRequestResources(null, null, p_Token).ConfigureAwait(false);
                    await m_TwitchBadgeProvider.TryRequestResources(null, null, p_Token).ConfigureAwait(false);
                    await m_BTTVDataProvider.TryRequestResources(null, null, p_Token).ConfigureAwait(false);
                    await m_FFZDataProvider.TryRequestResources(null, null, p_Token).ConfigureAwait(false);
                    await m_7TVDataProvider.TryRequestResources(null, null, p_Token).ConfigureAwait(false);
                    ///ChatPlexSDK.Logger.Information("Finished caching global emotes/badges.");
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"An exception occurred while trying to request global Twitch resources.");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
                finally
                {
                    m_GlobalLock.Release();
                }
            }).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try request channel resources
        /// </summary>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_OnChannelResourceDataCached">Callback</param>
        public void TryRequestChannelResources(IChatChannel p_Channel, string p_Token, Action<Dictionary<string, IChatResourceData>> p_OnChannelResourceDataCached)
        {
            Task.Run(async () =>
            {
                await m_ChannelLock.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (!m_ChannelDataCached.Contains(p_Channel.Id))
                    {
                        var l_ChannelID     = p_Channel.AsTwitchChannel().Roomstate.RoomId;
                        var l_ChannelName   = p_Channel.Id;

                        await m_TwitchBadgeProvider.TryRequestResources(l_ChannelID, l_ChannelName, p_Token).ConfigureAwait(false);
                        await m_TwitchCheermoteProvider.TryRequestResources(l_ChannelID, l_ChannelName, p_Token).ConfigureAwait(false);
                        await m_BTTVDataProvider.TryRequestResources(l_ChannelID, l_ChannelName, p_Token).ConfigureAwait(false);
                        await m_FFZDataProvider.TryRequestResources(l_ChannelID, l_ChannelName, p_Token).ConfigureAwait(false);
                        await m_7TVDataProvider.TryRequestResources(l_ChannelID, l_ChannelName, p_Token).ConfigureAwait(false);

                        var l_Result = new Dictionary<string, IChatResourceData>();

                        m_TwitchBadgeProvider.Resources.ToList().ForEach(x =>
                        {
                            var l_Parts = x.Key.Split(new char[] { '_' }, 2);
                            l_Result[$"{x.Value.Type}_{(l_Parts.Length > 1 ? l_Parts[1] : l_Parts[0])}"] = x.Value;
                        });
                        m_TwitchCheermoteProvider.Resources.ToList().ForEach(x =>
                        {
                            var l_Parts = x.Key.Split(new char[] { '_' }, 2);
                            foreach (var l_Tier in x.Value.Tiers)
                                l_Result[$"{l_Tier.Type}_{(l_Parts.Length > 1 ? l_Parts[1] : l_Parts[0])}{l_Tier.MinBits}"] = l_Tier;
                        });
                        m_BTTVDataProvider.Resources.ToList().ForEach(x =>
                        {
                            var l_Parts = x.Key.Split(new char[] { '_' }, 2);
                            l_Result[$"{x.Value.Type}_{(l_Parts.Length > 1 ? l_Parts[1] : l_Parts[0])}"] = x.Value;
                        });
                        m_FFZDataProvider.Resources.ToList().ForEach(x =>
                        {
                            var l_Parts = x.Key.Split(new char[] { '_' }, 2);
                            l_Result[$"{x.Value.Type}_{(l_Parts.Length > 1 ? l_Parts[1] : l_Parts[0])}"] = x.Value;
                        });
                        m_7TVDataProvider.Resources.ToList().ForEach(x =>
                        {
                            var l_Parts = x.Key.Split(new char[] { '_' }, 2);
                            l_Result[$"{x.Value.Type}_{(l_Parts.Length > 1 ? l_Parts[1] : l_Parts[0])}"] = x.Value;
                        });

                        p_OnChannelResourceDataCached?.Invoke(l_Result);
                        m_ChannelDataCached.Add(p_Channel.Id);

                        IsReady = true;
                        //ChatPlexSDK.Logger.Information($"Finished caching emotes for channel {channel.Id}.");
                    }
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"An exception occurred while trying to request Twitch channel resources for {p_Channel.Id}.");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
                finally
                {
                    m_ChannelLock.Release();
                }
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// Release channel
        /// </summary>
        /// <param name="p_Channel">Channel instance</param>
        public async void TryReleaseChannelResources(IChatChannel p_Channel)
        {
            await m_ChannelLock.WaitAsync();

            try
            {
                /// TODO: read a way to actually clear channel resources
                ChatPlexSDK.Logger.Info($"Releasing resources for channel {p_Channel.Id}");
                m_ChannelDataCached.Remove(p_Channel.Id);
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An exception occurred while trying to release Twitch channel resources for {p_Channel.Id}.");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            finally
            {
                m_ChannelLock.Release();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get a custom user display name
        /// </summary>
        /// <param name="p_UserID">UserID</param>
        /// <param name="p_Default">Default display name</param>
        /// <param name="p_PaintedName">Output painted name</param>
        /// <returns></returns>
        internal bool TryGetUserDisplayName(string p_UserID, string p_Default, out string p_PaintedName)
        {
            if (m_ChatPlexGradientNamesDataProvider.TryGetUserDisplayName(p_UserID, p_Default, out p_PaintedName)
                || m_7TVDataProvider.TryGetUserDisplayName(p_UserID, p_Default, out p_PaintedName))
                return true;

            p_PaintedName = p_Default;
            return false;
        }
        /// <summary>
        /// Get third party emote
        /// </summary>
        /// <param name="p_Word">Word</param>
        /// <param name="p_Channel">Channel</param>
        /// <param name="p_Data"></param>
        /// <returns></returns>
        internal bool TryGetThirdPartyEmote(string p_Word, string p_Channel, out ChatResourceData p_Data)
        {
            if (  m_BTTVDataProvider.TryGetResource(p_Word, p_Channel, out p_Data)
                || m_FFZDataProvider.TryGetResource(p_Word, p_Channel, out p_Data)
                || m_7TVDataProvider.TryGetResource(p_Word, p_Channel, out p_Data))
                return true;

            p_Data = null;
            return false;
        }
        /// <summary>
        /// Try get cheermote
        /// </summary>
        /// <param name="p_Word">Word</param>
        /// <param name="p_RoomID">Room ID</param>
        /// <param name="p_Data">Result</param>
        /// <param name="p_NumBits">Num bits</param>
        /// <returns></returns>
        internal bool TryGetCheermote(string p_Word, string p_RoomID, out TwitchCheermoteData p_Data, out int p_NumBits)
        {
            p_NumBits = 0;
            p_Data = null;

            if (string.IsNullOrEmpty(p_RoomID))
                return false;

            if (!char.IsLetter(p_Word[0]) || !char.IsDigit(p_Word[p_Word.Length - 1]))
                return false;

            int l_PrefixLength = -1;
            for (int l_I = p_Word.Length - 1; l_I > 0; l_I--)
            {
                if (char.IsDigit(p_Word[l_I]))
                    continue;

                l_PrefixLength = l_I + 1;
                break;
            }

            if (l_PrefixLength == -1)
                return false;

            string l_Prefix = p_Word.Substring(0, l_PrefixLength).ToLower();

            if (!m_TwitchCheermoteProvider.TryGetResource(l_Prefix, p_RoomID, out p_Data))
                return false;

            if (int.TryParse(p_Word.Substring(l_PrefixLength), out p_NumBits))
                return true;

            return false;
        }
        /// <summary>
        /// Try get badge info
        /// </summary>
        /// <param name="p_BadgeID">Badge ID</param>
        /// <param name="p_RoomID">Room ID</param>
        /// <param name="p_Badge">Result</param>
        /// <returns></returns>
        internal bool TryGetBadgeInfo(string p_BadgeID, string p_RoomID, out ChatResourceData p_Badge)
        {
            if (m_TwitchBadgeProvider.TryGetResource(p_BadgeID, p_RoomID, out p_Badge))
                return true;

            p_Badge = null;
            return false;
        }
    }
}
