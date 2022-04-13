using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch data provider
    /// </summary>
    public class TwitchDataProvider
    {
        /// <summary>
        /// Global lock
        /// </summary>
        private SemaphoreSlim m_GlobalLock = new SemaphoreSlim(1, 1), m_ChannelLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Cached data hash set
        /// </summary>
        private HashSet<string> m_ChannelDataCached = new HashSet<string>();
        /// <summary>
        /// Twitch badge provider
        /// </summary>
        private TwitchBadgeProvider m_TwitchBadgeProvider = new TwitchBadgeProvider();
        /// <summary>
        /// Twitch cheermote provider
        /// </summary>
        private TwitchCheermoteProvider m_TwitchCheermoteProvider = new TwitchCheermoteProvider();
        /// <summary>
        /// BTTV data provider
        /// </summary>
        private BTTVDataProvider m_BTTVDataProvider = new BTTVDataProvider();
        /// <summary>
        /// FFZ data provider
        /// </summary>
        private FFZDataProvider m_FFZDataProvider = new FFZDataProvider();
        /// <summary>
        /// 7TV data provider
        /// </summary>
        private _7TVDataProvider m_7TVDataProvider = new _7TVDataProvider();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public _7TVDataProvider _7TVDataProvider => m_7TVDataProvider;

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
                await m_GlobalLock.WaitAsync();
                try
                {
                    await m_TwitchBadgeProvider.TryRequestResources(null, p_Token);
                    await m_BTTVDataProvider.TryRequestResources(null, p_Token);
                    await m_FFZDataProvider.TryRequestResources(null, p_Token);
                    await m_7TVDataProvider.TryRequestResources(null, p_Token);
                    ///Logger.Instance.Information("Finished caching global emotes/badges.");
                }
                catch (Exception l_Exception)
                {
                    Logger.Instance.Error($"An exception occurred while trying to request global Twitch resources.");
                    Logger.Instance.Error(l_Exception);
                }
                finally
                {
                    m_GlobalLock.Release();
                }
            });
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
                await m_ChannelLock.WaitAsync();
                try
                {
                    if (!m_ChannelDataCached.Contains(p_Channel.Id))
                    {
                        string l_RoomId = p_Channel.AsTwitchChannel().Roomstate.RoomId;

                        await m_TwitchBadgeProvider.TryRequestResources(l_RoomId, p_Token);
                        await m_TwitchCheermoteProvider.TryRequestResources(l_RoomId, p_Token);
                        await m_BTTVDataProvider.TryRequestResources(l_RoomId, p_Token);
                        await m_FFZDataProvider.TryRequestResources(p_Channel.Id, p_Token);
                        await m_7TVDataProvider.TryRequestResources(p_Channel.Id, p_Token);

                        Dictionary<string, IChatResourceData> l_Result = new Dictionary<string, IChatResourceData>();

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
                        //Logger.Instance.Information($"Finished caching emotes for channel {channel.Id}.");
                    }
                }
                catch (Exception l_Exception)
                {
                    Logger.Instance.Error($"An exception occurred while trying to request Twitch channel resources for {p_Channel.Id}.");
                    Logger.Instance.Error(l_Exception);
                }
                finally
                {
                    m_ChannelLock.Release();
                }
            });
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
                Logger.Instance.Info($"Releasing resources for channel {p_Channel.Id}");
                m_ChannelDataCached.Remove(p_Channel.Id);
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"An exception occurred while trying to release Twitch channel resources for {p_Channel.Id}.");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                m_ChannelLock.Release();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
