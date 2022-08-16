#define WITH_SONG_CORE

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
#if WITH_SONG_CORE
using SongCore;
#endif
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_GameTweaker.Patches
{
    internal class SongBrowserDeleteAlias : MonoBehaviour
    {

    }

    /// <summary>
    /// StandardLevelDetailView patcher
    /// </summary>
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch(nameof(StandardLevelDetailView.SetContent))]
    internal class PStandardLevelDetailView : StandardLevelDetailView
    {
        /// <summary>
        /// StandardLevelDetailView instance
        /// </summary>
        private static StandardLevelDetailView m_StandardLevelDetailView = null;
        /// <summary>
        /// Patch instance
        /// </summary>
        private static UIPatch m_Patch = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">LevelCollectionViewController instance</param>
        internal static void Postfix(ref StandardLevelDetailView __instance)
        {
            m_StandardLevelDetailView = __instance;

            /// BetterSongList already have a trashcan
            if (IPA.Loader.PluginManager.GetPluginFromId("BetterSongList") != null)
                return;

            /// Apply
            if (GTConfig.Instance.Enabled && GTConfig.Instance.LevelSelection.DeleteSongButton)
                SetDeleteSongButtonEnabled(GTConfig.Instance.LevelSelection.DeleteSongButton);

            Transform l_SongBrowserButton = null;

            var l_ActionButtons = m_StandardLevelDetailView.transform.Find("ActionButtons");
            if (l_ActionButtons)
            {
                l_SongBrowserButton = l_ActionButtons.Find("DeleteLevelButton_hoverHintText");

                if (l_SongBrowserButton && !l_SongBrowserButton.gameObject.GetComponent<SongBrowserDeleteAlias>())
                    l_SongBrowserButton.gameObject.AddComponent<SongBrowserDeleteAlias>();
                else
                    l_SongBrowserButton = Resources.FindObjectsOfTypeAll<SongBrowserDeleteAlias>().FirstOrDefault()?.transform;
            }

            if (l_SongBrowserButton)
            {
                if (GTConfig.Instance.Enabled && GTConfig.Instance.LevelSelection.DeleteSongBrowserTrashcan)
                {
                    l_SongBrowserButton.transform.localScale = Vector3.zero;
                    l_SongBrowserButton.transform.SetParent(null);
                }
                else
                {
                    l_SongBrowserButton.transform.localScale = Vector3.one;
                    l_SongBrowserButton.transform.SetParent(l_ActionButtons.transform);
                    l_SongBrowserButton.transform.SetAsFirstSibling();
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the song delete button is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetDeleteSongButtonEnabled(bool p_Enabled)
        {
            /// Wait until it's ready
            if (m_StandardLevelDetailView == null)
                return;

            /// BetterSongSearch already have a trashcan
            if (IPA.Loader.PluginManager.GetPluginFromId("BetterSongSearch") != null)
                return;

            if (p_Enabled && (m_Patch == null || !m_Patch))
            {
                m_Patch = new GameObject("BeatSaberPlus_SongDeleteButton").AddComponent<UIPatch>();
                m_Patch.standardLevelDetailView = m_StandardLevelDetailView;
            }
            else if (!p_Enabled && m_Patch != null)
            {
                GameObject.DestroyImmediate(m_Patch);
                m_Patch = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// UI Patch
        /// </summary>
        internal class UIPatch : MonoBehaviour
        {
            /// <summary>
            /// BSML parser parameters
            /// </summary>
            [UIParams]
            private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
            [UIComponent("delete-confirmation-text")]
            private TextMeshProUGUI m_DeleteConfirmationText;
            [UIComponent("delete-button")]
            private Button m_DeleteButton;
            [UIComponent("dodelete-button")]
            private Button m_DoDeleteButton;
            [UIComponent("docancel-button")]
            private Button m_DoCancelButton;
#pragma warning restore CS0649

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Instance StandardLevelDetailView
            /// </summary>
            public StandardLevelDetailView standardLevelDetailView = null;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// On start
            /// </summary>
            void Start()
            {
                string l_View = "";
                l_View += "<bg>";
                l_View += " <modal show-event='show-delete-confirmation-modal' hide-event='hide-delete-confirmation-modal' size-delta-x='80' size-delta-y='40' click-off-closes='true'>";
                l_View += "  <vertical>";
                l_View += "   <vertical child-align='UpperCenter' preferred-width='80' preferred-height='26' pad='4'>";
                l_View += "    <text id='delete-confirmation-text' font-size='3.6' font-align='Center' overflow-mode='Overflow' word-wrapping='true' />";
                l_View += "   </vertical>";
                l_View += "   <horizontal preferred-width='80' preferred-height='12' spacing='6' pad-bottom='4' pad-left='4' pad-right='4'> ";
                l_View += "    <button id='dodelete-button' text='Delete' font-size='3.4' stroke-color='#FF5555' preferred-width='30' on-click='confirm-delete-button-clicked' />";
                l_View += "    <button id='docancel-button' text='Cancel' font-size='3.4' preferred-width='30' click-event='hide-delete-confirmation-modal' />";
                l_View += "   </horizontal>";
                l_View += "  </vertical>";
                l_View += " </modal>";
                l_View += " <icon-button id='delete-button' icon='BeatSaberPlus_GameTweaker.Resources.Delete.png' active='false' size-delta-x='7.25' size-delta-y='4.4' on-click='delete-button-clicked' pad='2' />";
                l_View += "</bg>";

                m_ParserParams = BSMLParser.instance.Parse(l_View, standardLevelDetailView.gameObject, this);

                var l_ActionButtons = standardLevelDetailView.transform.Find("ActionButtons");
                if (l_ActionButtons != null && l_ActionButtons)
                {
                    m_DeleteButton.transform.SetParent(l_ActionButtons, false);
                    m_DeleteButton.gameObject.SetActive(true);
                }

                m_DoDeleteButton.gameObject.SetActive(true);
                m_DoCancelButton.gameObject.SetActive(true);

                var l_LevelCollectionViewController = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().FirstOrDefault();
                if (l_LevelCollectionViewController != null)
                    l_LevelCollectionViewController.didSelectLevelEvent += LevelCollectionViewController_didSelectLevelEvent;
            }
            /// <summary>
            /// When the game object is destroyed
            /// </summary>
            void OnDestroy()
            {
                var l_LevelCollectionViewController = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().FirstOrDefault();
                if (l_LevelCollectionViewController != null)
                    l_LevelCollectionViewController.didSelectLevelEvent -= LevelCollectionViewController_didSelectLevelEvent;

                if (m_DeleteButton != null)
                    m_DeleteButton.transform.SetParent(this.transform, true);
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// On level selected
            /// </summary>
            /// <param name="p_Sender">Event sender</param>
            /// <param name="p_Level">Selected level</param>
            private void LevelCollectionViewController_didSelectLevelEvent(LevelCollectionViewController p_Sender, IPreviewBeatmapLevel p_Level)
            {
                if (p_Level is CustomPreviewBeatmapLevel l_CustomPreviewBeatmapLevel)
                    m_DeleteButton.gameObject.SetActive(!l_CustomPreviewBeatmapLevel.customLevelPath.Contains("CustomWIPLevels"));
                else
                    m_DeleteButton.gameObject.SetActive(false);
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// On delete button pressed
            /// </summary>
            [UIAction("delete-button-clicked")]
            private void OnDeleteButton()
            {
                IBeatmapLevel l_LevelToDelete = standardLevelDetailView.GetField<IBeatmapLevel, StandardLevelDetailView>("_level");
                if (l_LevelToDelete != null && l_LevelToDelete is CustomBeatmapLevel customLevel)
                {
                    m_DeleteConfirmationText.text = $"Are you sure you would like to delete '<color=#FFFFCC>{CP_SDK.Unity.TextMeshProU.EscapeString(customLevel.songName)}</color>' by {CP_SDK.Unity.TextMeshProU.EscapeString(customLevel.levelAuthorName)}?";
                    m_ParserParams.EmitEvent("show-delete-confirmation-modal");
                }
            }
            /// <summary>
            /// On confirm delete button pressed
            /// </summary>
            [UIAction("confirm-delete-button-clicked")]
            private void OnConfirmDeleteButton()
            {
                IBeatmapLevel l_LevelToDelete = standardLevelDetailView.GetField<IBeatmapLevel, StandardLevelDetailView>("_level");
                if (l_LevelToDelete != null)
                {
                    TableView l_TableView = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().FirstOrDefault().GetComponentInChildren<TableView>();

                    int l_IndexOfTopCell = 0;
                    if (l_TableView != null)
                    {
                        if (l_TableView.visibleCells.Count() > 0)
                            l_IndexOfTopCell = l_TableView.visibleCells.Min(x => x.idx);
                    }

#if WITH_SONG_CORE
                    Loader.Instance.DeleteSong((l_LevelToDelete as CustomBeatmapLevel).customLevelPath);
#endif
                    /// Scroll back to where we were
                    if (l_TableView != null)
                        l_TableView.ScrollToCellWithIdx(l_IndexOfTopCell, TableView.ScrollPositionType.Beginning, false);
                }

                m_ParserParams.EmitEvent("hide-delete-confirmation-modal");
            }
        }
    }
}
