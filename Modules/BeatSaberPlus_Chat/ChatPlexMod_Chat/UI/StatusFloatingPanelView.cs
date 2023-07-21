using CP_SDK.Chat.Interfaces;
using CP_SDK.XUI;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Status floating panel view
    /// </summary>
    internal sealed class StatusFloatingPanelView : CP_SDK.UI.ViewController<StatusFloatingPanelView>
    {
        public static Vector2 SIZE = new Vector2(25, 8);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIText m_Text;

        private ConcurrentDictionary<string, (bool, int)> m_ChannelsLiveStatuses = new ConcurrentDictionary<string, (bool, int)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            var l_Assembly          = Assembly.GetExecutingAssembly();
            var l_ViewerIconSprite  = CP_SDK.Unity.SpriteU.CreateFromRawWithBorders(CP_SDK.Misc.Resources.FromRelPath(l_Assembly, "ChatPlexMod_Chat.Resources.ViewerIcon.png"));

            XUIHLayout.Make(
                XUIImage.Make(l_ViewerIconSprite)
                    .SetWidth(8f).SetHeight(8f),

                XUIText.Make("Offline")
                    .SetFontSize(5f)
                    .Bind(ref m_Text)
            )
            .SetPadding(0).SetSpacing(0)
            .OnReady(x => {
                x.CSizeFitter.enabled           = false;
                x.HLayoutGroup.childAlignment   = TextAnchor.MiddleLeft;
            })
            .BuildUI(transform);

            CP_SDK.Chat.Service.Acquire();
            CP_SDK.Chat.Service.Multiplexer.OnLiveStatusUpdated += Mutiplixer_OnLiveStatusUpdated;
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            m_ChannelsLiveStatuses.Clear();
            UpdateViewerCount(false, 0);
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected sealed override void OnViewDestruction()
        {
            if (CP_SDK.Chat.Service.Multiplexer != null)
                CP_SDK.Chat.Service.Multiplexer.OnLiveStatusUpdated -= Mutiplixer_OnLiveStatusUpdated;
            CP_SDK.Chat.Service.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On room video playback updated
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_StreamUP">Is the stream up</param>
        /// <param name="p_ViewerCount">Viewer count</param>
        private void Mutiplixer_OnLiveStatusUpdated(IChatService p_ChatService, IChatChannel p_Channel, bool p_StreamUP, int p_ViewerCount)
        {
            var l_Key = "[" + p_ChatService.DisplayName + "]_" + p_Channel.Name.ToLower();

            if (!m_ChannelsLiveStatuses.ContainsKey(l_Key))
                m_ChannelsLiveStatuses.TryAdd(l_Key, (p_StreamUP, p_ViewerCount));
            else
                m_ChannelsLiveStatuses[l_Key] = (p_StreamUP, p_ViewerCount);

            var l_ShowUp        = false;
            var l_SumViewers    = 0;

            foreach (var l_KVP in m_ChannelsLiveStatuses)
            {
                if (!l_KVP.Value.Item1)
                    continue;

                l_ShowUp = true;
                l_SumViewers += l_KVP.Value.Item2;
            }

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => UpdateViewerCount(l_ShowUp, l_SumViewers));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update viewer count
        /// </summary>
        /// <param name="p_IsUp">Is up?</param>
        /// <param name="p_Viewers">Viewers count</param>
        private void UpdateViewerCount(bool p_IsUp, int p_Viewers)
        {
            if (p_IsUp) m_Text.SetText(p_Viewers.ToString());
            else        m_Text.SetText("<color=red>Offline");
        }
    }
}
