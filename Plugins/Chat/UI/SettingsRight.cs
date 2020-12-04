using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.Chat.UI
{
    /// <summary>
    /// Chat filters settings
    /// </summary>
    internal class SettingsRight : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("followenvironementrotations-toggle")]
        public ToggleSetting m_FollowEnvironementRotations;
        [UIComponent("chat-filterviewers")]
        private ToggleSetting m_ChatFitlerViewers;
        [UIComponent("chat-filterbroadcaster")]
        private ToggleSetting m_ChatFilterBroadcaster;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("followevent-toggle")]
        public ToggleSetting m_FollowEvents;
        [UIComponent("subscriptionevents-toggle")]
        public ToggleSetting m_SubscriptionEvents;
        [UIComponent("bitscheering-toggle")]
        public ToggleSetting m_BitsCheering;
        [UIComponent("channelpoints-toggle")]
        public ToggleSetting m_ChannelPoints;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if(p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Prepare
                PrepareSlider(m_FollowEnvironementRotations.gameObject);
                PrepareSlider(m_ChatFitlerViewers.gameObject);
                PrepareSlider(m_ChatFilterBroadcaster.gameObject);

                /// Set values
                m_FollowEnvironementRotations.Value = Config.Chat.FollowEnvironementRotation;
                m_ChatFitlerViewers.Value           = Config.Chat.FilterViewersCommands;
                m_ChatFilterBroadcaster.Value       = Config.Chat.FilterBroadcasterCommands;

                /// Bind events
                m_FollowEnvironementRotations.onChange  = l_Event;
                m_ChatFitlerViewers.onChange            = l_Event;
                m_ChatFilterBroadcaster.onChange        = l_Event;

                /// Prepare
                PrepareSlider(m_FollowEvents.gameObject);
                PrepareSlider(m_SubscriptionEvents.gameObject);
                PrepareSlider(m_BitsCheering.gameObject);
                PrepareSlider(m_ChannelPoints.gameObject);

                /// Set values
                m_FollowEvents.Value                = Config.Chat.ShowFollowEvents;
                m_SubscriptionEvents.Value          = Config.Chat.ShowSubscriptionEvents;
                m_BitsCheering.Value                = Config.Chat.ShowBitsCheeringEvents;
                m_ChannelPoints.Value               = Config.Chat.ShowChannelPointsEvent;

                /// Bind events
                m_FollowEvents.onChange                 = l_Event;
                m_SubscriptionEvents.onChange           = l_Event;
                m_BitsCheering.onChange                 = l_Event;
                m_ChannelPoints.onChange                = l_Event;
            }

            m_PreventChanges = true;

            /// Set values
            m_FollowEnvironementRotations.Value = Config.Chat.FollowEnvironementRotation;
            m_ChatFitlerViewers.Value           = Config.Chat.FilterViewersCommands;
            m_ChatFilterBroadcaster.Value       = Config.Chat.FilterBroadcasterCommands;

            /// Set values
            m_FollowEvents.Value                = Config.Chat.ShowFollowEvents;
            m_SubscriptionEvents.Value          = Config.Chat.ShowSubscriptionEvents;
            m_BitsCheering.Value                = Config.Chat.ShowBitsCheeringEvents;
            m_ChannelPoints.Value               = Config.Chat.ShowChannelPointsEvent;

            m_PreventChanges = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prepare slider widget
        /// </summary>
        /// <param name="p_Object">Slider instance</param>
        /// <param name="p_AddControls">Should add left & right controls</param>
        private void PrepareSlider(GameObject p_Object)
        {
            GameObject.Destroy(p_Object.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

            RectTransform l_RectTransform = p_Object.transform.GetChild(1) as RectTransform;
            l_RectTransform.anchorMin = new Vector2(0f, 0f);
            l_RectTransform.anchorMax = new Vector2(1f, 1f);
            l_RectTransform.sizeDelta = new Vector2(1, 1);

            p_Object.GetComponent<LayoutElement>().preferredWidth = -1f;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update config
            Config.Chat.FollowEnvironementRotation  = m_FollowEnvironementRotations.Value;
            Config.Chat.FilterViewersCommands       = m_ChatFitlerViewers.Value;
            Config.Chat.FilterBroadcasterCommands   = m_ChatFilterBroadcaster.Value;

            /// Set values
            Config.Chat.ShowFollowEvents            = m_FollowEvents.Value;
            Config.Chat.ShowSubscriptionEvents      = m_SubscriptionEvents.Value;
            Config.Chat.ShowBitsCheeringEvents      = m_BitsCheering.Value;
            Config.Chat.ShowChannelPointsEvent      = m_ChannelPoints.Value;

            /// Update floating view
            Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.Utils.Game.ActiveScene, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            /// Refresh values
            DidActivate(false, false, false);
        }
    }
}
