using CP_SDK.UI.Data;
using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using HMUI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Chat request main view controller
    /// </summary>
    internal class ManagerMainView
        : CP_SDK.UI.ViewController<ManagerMainView>,
          CP_SDK_BS.UI.Data.SongListController,
          IProgress<float>
    {
        private static CustomPreviewBeatmapLevel m_SongToSelectAfterDismiss = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIText                 m_Title                 = null;
        private XUITextSegmentedControl m_TypeSegmentControl    = null;
        private XUIVVList               m_SongList              = null;
        private XUIVLayout              m_SongInfoPanelOwner    = null;
        private XUIVLayout              m_SongInfoPanelNoSong   = null;
        private XUIImage                m_CoverMask             = null;
        private XUIImage                m_Cover                 = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK_BS.UI.LevelDetail m_SongInfo_Detail = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private BeatSaberPlus_ChatRequest.Data.SongEntry            m_SelectedSong              = null;
        private List<BeatSaberPlus_ChatRequest.Data.SongEntry>      m_SongsProvider             = null;
        private string                                              m_SongReloadingExpectedPath = "";
        private int                                                 m_LastTab                   = 0;
        private float                                               m_Tab0Scroll                = 0.0f;
        private float                                               m_Tab1Scroll                = 0.0f;
        private float                                               m_Tab2Scroll                = 0.0f;


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Title")
                    .ForEachDirect<XUIText>(x => x.Bind(ref m_Title)),

                XUITextSegmentedControl.Make(new string[] { "Requests", "History", "Blacklist" })
                    .OnActiveChanged(OnQueueTypeChanged)
                    .Bind(ref m_TypeSegmentControl),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(ListCellPrefabs<CP_SDK_BS.UI.Data.SongListCell>.Get())
                            .OnListItemSelected(OnSongSelected)
                            .Bind(ref m_SongList)
                    )
                    .SetWidth(67)
                    .SetHeight(65)
                    .SetSpacing(0)
                    .SetPadding(0)
                    .SetBackground(true, CP_SDK.UI.UISystem.ListBGColor)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),


                    XUIVLayout.Make(
                        XUIImage.Make(CP_SDK.UI.UISystem.GetUIRoundBGSprite())
                            .SetType(Image.Type.Sliced)
                            .SetPixelsPerUnitMultiplier(4.0f)
                            .OnReady((x) =>
                            {
                                x.LElement.ignoreLayout = true;
                                x.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
                                x.RTransform.anchorMin          = new Vector2( 0.0f,  0.0f);
                                x.RTransform.anchorMax          = new Vector2( 1.0f,  1.0f);
                                x.RTransform.pivot              = new Vector2( 0.5f,  0.5f);
                                x.RTransform.sizeDelta          = new Vector2(-3.0f, -5.0f);
                                x.RTransform.anchoredPosition   = new Vector2( 1.0f,  0.0f);
                            })
                            .Bind(ref m_CoverMask)
                    )
                    .SetWidth(77)
                    .SetHeight(65)
                    .SetPadding(4, 0, 0, 2)
                    .SetActive(false)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
                    .Bind(ref m_SongInfoPanelOwner),

                    XUIVLayout.Make(
                        XUIText.Make("Please select a song in the list!")
                            .SetStyle(FontStyles.Bold)
                            .SetAlign(TextAlignmentOptions.MidlineGeoAligned)
                    )
                    .SetWidth(77)
                    .SetHeight(65)
                    .SetActive(true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
                    .Bind(ref m_SongInfoPanelNoSong)
                )
                .SetHeight(65)
                .SetSpacing(0)
                .SetPadding(2)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            XUIImage.Make(null)
                .SetColor(ColorU.WithAlpha(Color.white, 0.2f))
                .OnReady((x) =>
                {
                    x.RTransform.localScale         = 1.25f * Vector3.one;
                    x.RTransform.anchorMin          = new Vector2(0.0f, 0.0f);
                    x.RTransform.anchorMax          = new Vector2(1.0f, 1.0f);
                    x.RTransform.pivot              = new Vector2(0.5f, 0.5f);
                    x.RTransform.sizeDelta          = new Vector2(0.0f, 0.0f);
                    x.RTransform.anchoredPosition   = new Vector2(0.0f, 0.0f);
                })
                .Bind(ref m_Cover)
                .BuildUI(m_CoverMask.RTransform);

            /// Show song info panel
            m_SongInfo_Detail = new CP_SDK_BS.UI.LevelDetail(m_SongInfoPanelOwner.RTransform);
            UnselectSong();

            m_SongInfo_Detail.SetFavoriteToggleEnabled(true);
            m_SongInfo_Detail.SetFavoriteToggleImage(
                CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromPath(Assembly.GetExecutingAssembly(), "BeatSaberPlus_ChatRequest.Resources.Blacklist.png")),
                CP_SDK.Unity.SpriteU.CreateFromRaw(CP_SDK.Misc.Resources.FromPath(Assembly.GetExecutingAssembly(), "BeatSaberPlus_ChatRequest.Resources.Unblacklist.png"))
            );
            m_SongInfo_Detail.SetFavoriteToggleHoverHint("Add/Remove to blacklist");
            m_SongInfo_Detail.SetFavoriteToggleCallback(OnBlacklistButtonPressed);

            m_SongInfo_Detail.SetSecondaryButtonEnabled(true);
            m_SongInfo_Detail.SetSecondaryButtonText("Skip");
            m_SongInfo_Detail.OnSecondaryButton = SkipOrAddToQueueSong;

            m_SongInfo_Detail.SetPrimaryButtonText("Play");
            m_SongInfo_Detail.SetPrimaryButtonEnabled(true);
            m_SongInfo_Detail.OnPrimaryButton = PlaySong;

            /// Force change to tab Request
            OnQueueTypeChanged(0);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            /// Go back to request tab
            if (m_TypeSegmentControl.Element.GetActiveText() != 0)
                m_TypeSegmentControl.SetActiveText(0);
            else
                RebuildSongList();

            RebuiltTitle();
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            /// Stop preview music if any
            m_SelectedSong?.StopPreviewMusic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the queue type is changed
        /// </summary>
        /// <param name="p_Index">Tab index</param>
        private void OnQueueTypeChanged(int p_Index)
        {
            var l_OldScroll = m_SongList.Element.ScrollPosition;
            if (m_LastTab == 0) m_Tab0Scroll = l_OldScroll;
            if (m_LastTab == 1) m_Tab1Scroll = l_OldScroll;
            if (m_LastTab == 2) m_Tab2Scroll = l_OldScroll;

            UnselectSong();
            m_SongsProvider = p_Index == 0 ? ChatRequest.Instance.SongQueue : (p_Index == 1 ? ChatRequest.Instance.SongHistory : ChatRequest.Instance.SongBlackList);
            RebuildSongList();

            if (p_Index == 0) m_SongList.Element.ScrollTo(m_Tab0Scroll, false);
            if (p_Index == 1) m_SongList.Element.ScrollTo(m_Tab1Scroll, false);
            if (p_Index == 2) m_SongList.Element.ScrollTo(m_Tab2Scroll, false);
            m_LastTab = p_Index;

            m_SongInfo_Detail.SetSecondaryButtonText(p_Index == 0 ? "Skip" : "Add to queue");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuild song list
        /// </summary>
        /// <returns></returns>
        internal void RebuildSongList()
        {
            var l_OldScroll = m_SongList.Element.ScrollPosition;
            lock (m_SongsProvider)
            {
                var l_OldSelected = m_SelectedSong;

                for (var l_I = 0; l_I < m_SongsProvider.Count; ++l_I)
                    m_SongsProvider[l_I].SongListController = this;

                m_SongList.Element.SetListItems(m_SongsProvider);
                if (l_OldSelected != null)
                    m_SongList.Element.SetSelectedListItem(l_OldSelected);
            }
            m_SongList.Element.ScrollTo(l_OldScroll, false);

            RebuiltTitle();
        }
        /// <summary>
        /// Rebuild title
        /// </summary>
        internal void RebuiltTitle()
        {
            var l_Minutes = ChatRequest.Instance.QueueDuration / 60;
            var l_Seconds = (ChatRequest.Instance.QueueDuration - (l_Minutes * 60));

            if (ChatRequest.Instance.QueueOpen)
            {
                if (l_Minutes != 0 || l_Seconds != 0)
                    m_Title.SetText($"Queue is <color=green>open</color> | Duration {l_Minutes}m{l_Seconds}s");
                else
                    m_Title.SetText($"Queue is <color=green>open</color>");
            }
            else
            {
                if (l_Minutes != 0 || l_Seconds != 0)
                    m_Title.SetText($"Queue is <color=red>closed</color> | Duration {l_Minutes}m{l_Seconds}s");
                else
                    m_Title.SetText($"Queue is <color=red>closed</color>");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a song is selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnSongSelected(IListItem p_SelectedItem)
        {
            if (p_SelectedItem == null || !(p_SelectedItem is BeatSaberPlus_ChatRequest.Data.SongEntry l_SongEntry))
            {
                UnselectSong();
                return;
            }

            m_SelectedSong = null;

            /// Hide if invalid song
            if (p_SelectedItem == null
                || l_SongEntry.Invalid
                || l_SongEntry.BeatSaver_Map == null
                || (l_SongEntry.BeatSaver_Map != null && l_SongEntry.BeatSaver_Map.Partial))
            {
                UnselectSong();
                return;
            }

            /// Show UIs
            m_SongInfoPanelNoSong.SetActive(false);
            m_SongInfoPanelOwner.SetActive(true);
            ManagerRightView.Instance.SetVisible(true);

            /// Update UIs
            if (!m_SongInfo_Detail.FromBeatSaver(l_SongEntry.BeatSaver_Map, l_SongEntry.Cover))
            {
                UnselectSong();
                return;
            }

            m_SongInfo_Detail.SetFavoriteToggleValue(m_TypeSegmentControl.Element.GetActiveText() == 2/* Blacklist */);
            ManagerRightView.Instance.SetDetail(l_SongEntry.BeatSaver_Map);

            /// Set selected song
            m_SelectedSong = l_SongEntry;

            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatSaver_Map.SelectMapVersion().hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                m_SongInfo_Detail.SetPrimaryButtonText("Play");
            else
                m_SongInfo_Detail.SetPrimaryButtonText("Download");

            m_Cover.SetActive(CRConfig.Instance.BigCoverArt);
            m_Cover.SetSprite(l_SongEntry.Cover);
        }
        /// <summary>
        /// Select random song
        /// </summary>
        internal void SelectRandom()
        {
            m_TypeSegmentControl.SetActiveText(0/* Request */);

            var l_ToSelect = null as BeatSaberPlus_ChatRequest.Data.SongEntry;
            lock (ChatRequest.Instance.SongQueue)
            {
                var l_SongCount = ChatRequest.Instance.SongQueue.Count;
                if (l_SongCount > 0)
                    l_ToSelect = ChatRequest.Instance.SongQueue[UnityEngine.Random.Range(0, l_SongCount)];
            }

            if (l_ToSelect != null)
                m_SongList.Element.SetSelectedListItem(l_ToSelect);
        }
        /// <summary>
        /// Unselect active song
        /// </summary>
        private void UnselectSong()
        {
            m_SongInfoPanelOwner.SetActive(false);
            m_SongInfoPanelNoSong.SetActive(true);
            ManagerRightView.Instance.SetVisible(false);

            m_SelectedSong?.StopPreviewMusic();
            m_SelectedSong = null;

            m_SongList.Element.SetSelectedListItem(null, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Skip a song
        /// </summary>
        private void SkipOrAddToQueueSong()
        {
            if (m_SelectedSong == null)
            {
                UnselectSong();
                RebuildSongList();
                return;
            }

            if (m_TypeSegmentControl.Element.GetActiveText() == 0/* Request */)
                ChatRequest.Instance.DequeueSong(m_SelectedSong, false);
            else
                ChatRequest.Instance.ReEnqueueSong(m_SelectedSong);

            UnselectSong();
            RebuildSongList();
        }
        /// <summary>
        /// On play song pressed
        /// </summary>
        private void PlaySong()
        {
            if (m_SelectedSong == null)
            {
                UnselectSong();
                RebuildSongList();
                return;
            }

            try
            {
                var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatSaver_Map.SelectMapVersion().hash);
                if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                {
                    ChatRequest.Instance.DequeueSong(m_SelectedSong, true);

                    if (m_TypeSegmentControl.Element.GetActiveText() == 0)
                        m_SongList.RemoveListItem(m_SelectedSong);

                    m_SongToSelectAfterDismiss = l_LocalSong;

                    CP_SDK.UI.ScreenSystem.OnDismiss -= SelectSongAfterScreenSystemDismiss;
                    CP_SDK.UI.ScreenSystem.OnDismiss += SelectSongAfterScreenSystemDismiss;

                    ManagerViewFlowCoordinator.Instance().Dismiss();
                    UnselectSong();
                }
                else
                {
                    /// Show download modal
                    ShowLoadingModal("Downloading", true);

                    /// Start downloading
                    CP_SDK_BS.Game.BeatMapsClient.DownloadSong(
                        m_SelectedSong.BeatSaver_Map,
                        m_SelectedSong.BeatSaver_Map.SelectMapVersion(),
                        CancellationToken.None,
                        (p_IsSuccess, p_SongReloadingExpectedPath) => {
                            if (p_IsSuccess)
                            {
                                m_SongReloadingExpectedPath = p_SongReloadingExpectedPath;

                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                                {
                                    SongCore.Loader.SongsLoadedEvent += OnDownloadedSongLoaded;
                                    SongCore.Loader.Instance.RefreshSongs(false);
                                });
                            }
                            else
                            {
                                /// Show error message
                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => {
                                    CloseLoadingModal();
                                    ShowMessageModal("Download failed!");
                                });
                            }
                        },
                        this);

                    return;
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[UI][ManagerMain.PlaySong] Error:");
                Logger.Instance.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On black list button pressed
        /// </summary>
        private void OnBlacklistButtonPressed()
        {
            if (m_TypeSegmentControl.Element.GetActiveText() == 2/* Blacklist */)
                ChatRequest.Instance.UnBlacklistSong(m_SelectedSong);
            /// Show modal
            else
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to blacklist this song?", (p_Confirm) => {
                    if (!p_Confirm)
                        return;

                    /// Update UI
                    m_SongInfo_Detail.SetFavoriteToggleValue(true);

                    /// Blacklist the song
                    ChatRequest.Instance.BlacklistSong(m_SelectedSong);
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a downloaded song is downloaded
        /// </summary>
        /// <param name="p_Loader">Loader instance</param>
        /// <param name="p_Maps">All loaded songs</param>
        private void OnDownloadedSongLoaded(SongCore.Loader p_Loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> p_Maps)
        {
            /// Remove callback
            SongCore.Loader.SongsLoadedEvent -= OnDownloadedSongLoaded;

            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatSaver_Map.SelectMapVersion().hash);
            if (l_LocalSong == null)
            {
                foreach (var l_Current in p_Maps)
                {
                    if (!l_Current.Value.customLevelPath.ToLower().Contains(m_SongReloadingExpectedPath.ToLower()))
                        continue;

                    l_LocalSong = l_Current.Value;
                    break;
                }
            }

            if (l_LocalSong == null || l_LocalSong.levelID.Replace("custom_level_", "").ToLower() != m_SelectedSong.BeatSaver_Map.SelectMapVersion().hash)
            {
                CloseLoadingModal();
                ShowMessageModal("An error occurred while downloading this map.\nDownloaded song doesn't match.");
                return;
            }

            StartCoroutine(PlayDownloadedLevel());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Play download song
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayDownloadedLevel()
        {
            yield return new WaitForEndOfFrame();

            if (!CanBeUpdated)
                yield break;

            /// Hide loading modal
            CloseLoadingModal();

            /// Reselect the cell
            PlaySong();

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Select song after screen system dismiss
        /// </summary>
        private static void SelectSongAfterScreenSystemDismiss()
        {
            CP_SDK.UI.ScreenSystem.OnDismiss -= SelectSongAfterScreenSystemDismiss;

            CP_SDK_BS.Game.LevelSelection.FilterToSpecificSong(m_SongToSelectAfterDismiss);
            m_SongToSelectAfterDismiss = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On song list item cover fetched
        /// </summary>
        /// <param name="p_Item">Item</param>
        public void OnSongListItemCoverFetched(CP_SDK_BS.UI.Data.SongListItem p_Item)
        {
            if (m_SelectedSong != p_Item)
                return;

            m_SongInfo_Detail.Cover = p_Item.Cover;

            m_SongInfoPanelOwner.SetBackground(true);
            m_SongInfoPanelOwner.SetBackgroundSprite(p_Item.Cover);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should play preview audio?
        /// </summary>
        /// <returns></returns>
        public bool PlayPreviewAudio()
            => CRConfig.Instance.PlayPreviewMusic;
        /// <summary>
        /// Preview audio playback volume
        /// </summary>
        /// <returns></returns>
        public float PreviewAudioVolume()
            => 1.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On download progress reported
        /// </summary>
        /// <param name="p_Value"></param>
        void IProgress<float>.Report(float p_Value)
            => LoadingModal_SetMessage($"Downloading {Mathf.Round((float)(p_Value * 100.0))}%");
    }
}
