using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BS_Utils.Utilities;
using HarmonyLib;
using HMUI;
using SongCore;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
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

            /// Apply
            if (Config.GameTweaker.Enabled && Config.GameTweaker.DeleteSongButton)
                SetDeleteSongButtonEnabled(Config.GameTweaker.DeleteSongButton);
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
                l_View += "    <button text='Delete' font-size='3.4' stroke-color='#FF5555' preferred-width='30' on-click='confirm-delete-button-clicked' />";
                l_View += "    <button text='Cancel' font-size='3.4' preferred-width='30' click-event='hide-delete-confirmation-modal' />";
                l_View += "   </horizontal>";
                l_View += "  </vertical>";
                l_View += " </modal>";
                l_View += " <icon-button id='delete-button' icon='BeatSaberPlus.Plugins.GameTweaker.Resources.Delete.png' active='false' size-delta-x='7.25' size-delta-y='4.4' on-click='delete-button-clicked' pad='2' />";
                l_View += "</bg>";

                m_ParserParams = BSMLParser.instance.Parse(l_View, standardLevelDetailView.gameObject, this);

                var l_ActionButtons = standardLevelDetailView.transform.Find("ActionButtons");
                if (l_ActionButtons != null && l_ActionButtons)
                {
                    m_DeleteButton.transform.SetParent(l_ActionButtons, false);
                    m_DeleteButton.gameObject.SetActive(true);
                }
            }
            /// <summary>
            /// When the game object is destroyed
            /// </summary>
            void OnDestroy()
            {
                if (m_DeleteButton != null)
                    m_DeleteButton.transform.SetParent(this.transform, true);
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// On delete button pressed
            /// </summary>
            [UIAction("delete-button-clicked")]
            private void OnDeleteButton()
            {
                IBeatmapLevel l_LevelToDelete = standardLevelDetailView.GetField<IBeatmapLevel>("_level");
                if (l_LevelToDelete != null && l_LevelToDelete is CustomBeatmapLevel customLevel)
                {
                    m_DeleteConfirmationText.text = $"Are you sure you would like to delete '<color=#FFFFCC>{Utils.GameUI.EscapeTextMeshProTags(customLevel.songName)}</color>' by {Utils.GameUI.EscapeTextMeshProTags(customLevel.levelAuthorName)}?";
                    m_ParserParams.EmitEvent("show-delete-confirmation-modal");
                }
            }
            /// <summary>
            /// On confirm delete button pressed
            /// </summary>
            [UIAction("confirm-delete-button-clicked")]
            private void OnConfirmDeleteButton()
            {
                IBeatmapLevel l_LevelToDelete = standardLevelDetailView.GetField<IBeatmapLevel>("_level");
                if (l_LevelToDelete != null)
                {
                    TableView l_TableView = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().FirstOrDefault().GetComponentInChildren<TableView>();

                    int l_IndexOfTopCell = 0;
                    if (l_TableView != null)
                    {
                        if (l_TableView.visibleCells.Count() > 0)
                            l_IndexOfTopCell = l_TableView.visibleCells.Min(x => x.idx);
                    }

                    Loader.Instance.DeleteSong((l_LevelToDelete as CustomBeatmapLevel).customLevelPath);

                    /// Scroll back to where we were
                    if (l_TableView != null)
                        l_TableView.ScrollToCellWithIdx(l_IndexOfTopCell, TableViewScroller.ScrollPositionType.Beginning, false);
                }

                m_ParserParams.EmitEvent("hide-delete-confirmation-modal");
            }
        }
    }
}
