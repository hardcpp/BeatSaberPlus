using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    /// <summary>
    /// Manager view detail
    /// </summary>
    internal class ManagerDetail : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("DetailBackground")]
        internal GameObject DetailBackground = null;
        [UIObject("SubDetailBackground")]
        internal GameObject SubDetailBackground = null;
        [UIObject("DetailText")]
        internal GameObject DetailText = null;
        [UIObject("SubDetailText")]
        internal GameObject SubDetailText = null;
        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pending detail
        /// </summary>
        private BeatSaverSharp.Beatmap m_PendingDetail = null;
        /// <summary>
        /// Last detail
        /// </summary>
        private BeatSaverSharp.Beatmap m_LastDetail = null;

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

            if (p_FirstActivation)
            {
                /// Update background color
                var l_Color = DetailBackground.GetComponent<ImageView>().color;
                l_Color.a = 0.5f;

                DetailBackground.GetComponent<ImageView>().color = l_Color;
                SubDetailBackground.GetComponent<ImageView>().color = l_Color;

                var l_DetailText = DetailText.GetComponent<CurvedTextMeshPro>();
                l_DetailText.autoSizeTextContainer  = false;
                l_DetailText.enableAutoSizing       = false;
                l_DetailText.overflowMode           = TextOverflowModes.ScrollRect;
                l_DetailText.alignment              = TextAlignmentOptions.TopLeft;

                SubDetailText.GetComponent<CurvedTextMeshPro>().alignment = TextAlignmentOptions.Center;
            }

            if (m_PendingDetail != null)
                SetDetail(m_PendingDetail);
            else
                SetVisible(false);
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            m_ParserParams.EmitEvent("CloseAllModals");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set detail view visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        internal void SetVisible(bool p_Visible)
        {
            if (transform.childCount == 0)
                return;

            transform.GetChild(0).gameObject.SetActive(p_Visible);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set details
        /// </summary>
        /// <param name="p_Detail">p_Detail</param>
        internal void SetDetail(BeatSaverSharp.Beatmap p_Detail, bool p_SecondTime = false)
        {
            if (DetailText != null)
            {
                string l_Description = p_Detail.Description;

                if (l_Description.Trim().Length == 0)
                    l_Description = "<align=\"center\"><alpha=#AA><i>No description...</i></align>";

                l_Description += "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

                DetailText.GetComponent<CurvedTextMeshPro>().text = l_Description;

                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    var l_ScrollView = DetailBackground.transform.GetChild(0).GetComponent<ScrollView>();
                    var l_Child = l_ScrollView.transform.GetChild(0);

                    l_ScrollView.SetContentHeight((l_Child.transform as RectTransform).rect.height);
                    l_ScrollView.SetDestinationPosY(0f);
                    l_ScrollView.ScrollTo(0f, false);
                    l_ScrollView.Update();
                    l_ScrollView.UpdateVerticalScrollIndicator(0f);
                    l_ScrollView.RefreshButtons();

                    if (!p_SecondTime)
                        SetDetail(p_Detail, true);
                });

                float   l_Vote      = (float)Math.Round((double)p_Detail.Stats.Rating * 100f, 0);
                string  l_SubText   = $"Votes +{p_Detail.Stats.UpVotes}/-{p_Detail.Stats.DownVotes} ({l_Vote}%) {p_Detail.Stats.Downloads} downloads";

                SubDetailText.GetComponent<CurvedTextMeshPro>().text = l_SubText;

                m_LastDetail    = p_Detail;
                m_PendingDetail = null;
            }
            else
                m_PendingDetail = p_Detail;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On link pressed
        /// </summary>
        [UIAction("click-btn-link")]
        private void OnLinkPressed()
        {
            ShowMessageModal("Song sent to the chat.");

            var l_Plugin = Plugins.ChatRequest.ChatRequest.Instance;
            l_Plugin.SendChatMessage($"\"{m_LastDetail.Name}\" by {m_LastDetail.Metadata.LevelAuthorName} => https://beatsaver.com/beatmap/{m_LastDetail.Key}");
        }
        /// <summary>
        /// On beat saver pressed
        /// </summary>
        [UIAction("click-btn-beatsaver")]
        private void OnBeatsaverPressed()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://beatsaver.com/beatmap/" + m_LastDetail.Key);
        }
    }
}
