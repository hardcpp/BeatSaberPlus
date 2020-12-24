using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.ChatRequest.UI
{
    /// <summary>
    /// Chat request main view controller
    /// </summary>
    internal class ManagerMain : SDK.UI.ResourceViewController<ManagerMain>, IProgress<double>
    {
        /// <summary>
        /// Amount of song to display per page
        /// </summary>
        private static int s_SONG_PER_PAGE = 7;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIValue("Title")]
        internal string m_Title = "Requests";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SongUpButton")]
        private Button m_SongUpButton;
        [UIObject("SongList")]
        private GameObject m_SongListView = null;
        private SDK.UI.DataSource.SongList m_SongList = null;
        [UIComponent("SongDownButton")]
        private Button m_SongDownButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongInfoPanel")]
        private GameObject m_SongInfoPanel;
        private SDK.UI.LevelDetail m_SongInfo_Detail;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;
        /// <summary>
        /// Current song list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Selected song
        /// </summary>
        ChatRequest.SongEntry m_SelectedSong = null;
        /// <summary>
        /// Selected song index
        /// </summary>
        private int m_SelectedSongIndex = 0;
        /// <summary>
        /// Song list provider
        /// </summary>
        private List<ChatRequest.SongEntry> m_SongsProvider = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pending filter song
        /// </summary>
        static CustomPreviewBeatmapLevel m_PendingFilterSong = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Scale down up & down button
            m_SongUpButton.transform.localScale     = Vector3.one * 0.6f;
            m_SongDownButton.transform.localScale   = Vector3.one * 0.6f;

            /// Create type selector
            m_TypeSegmentControl = SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false);
            m_TypeSegmentControl.SetTexts(new string[] { "Requests", "History", "Blacklist" });
            m_TypeSegmentControl.ReloadData();
            m_TypeSegmentControl.didSelectCellEvent += OnQueueTypeChanged;

            /// Prepare song list
            var l_BSMLTableView = m_SongListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_SongListView.GetComponentInChildren<CustomListTableData>());
            m_SongList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SongList>();
            m_SongList.PlayPreviewAudio     = Config.ChatRequest.PlayPreviewMusic;
            m_SongList.PreviewAudioVolume   = 1.0f;
            m_SongList.TableViewInstance    = l_BSMLTableView;
            m_SongList.Init();
            l_BSMLTableView.SetDataSource(m_SongList, false);

            /// Bind events
            m_SongUpButton.onClick.AddListener(OnSongPageUpPressed);
            m_SongList.OnCoverFetched                               += OnSongCoverFetched;
            m_SongList.TableViewInstance.didSelectCellWithIdxEvent  += OnSongSelected;
            m_SongDownButton.onClick.AddListener(OnSongPageDownPressed);

            /// Show song info panel
            m_SongInfo_Detail = new SDK.UI.LevelDetail(m_SongInfoPanel.transform);
            UnselectSong();

            m_SongInfo_Detail.SetFavoriteToggleEnabled(true);
            m_SongInfo_Detail.SetFavoriteToggleImage("BeatSaberPlus.Modules.ChatRequest.Resources.Blacklist.png", "BeatSaberPlus.Modules.ChatRequest.Resources.Unblacklist.png");
            m_SongInfo_Detail.SetFavoriteToggleHoverHint("Add/Remove to blacklist");
            m_SongInfo_Detail.SetFavoriteToggleCallback(OnBlacklistButtonPressed);

            m_SongInfo_Detail.SetPracticeButtonEnabled(true);
            m_SongInfo_Detail.SetPracticeButtonText("Skip");
            m_SongInfo_Detail.SetPracticeButtonAction(SkipSong);

            m_SongInfo_Detail.SetPlayButtonText("Play");
            m_SongInfo_Detail.SetPlayButtonEnabled(true);
            m_SongInfo_Detail.SetPlayButtonAction(PlaySong);

            /// Force change to tab Request
            OnQueueTypeChanged(null, 0);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            m_SongList.PlayPreviewAudio = Config.ChatRequest.PlayPreviewMusic;

            /// Go back to request tab
            if (m_TypeSegmentControl.selectedCellNumber != 0)
            {
                m_TypeSegmentControl.SelectCellWithNumber(0);
                OnQueueTypeChanged(null, 0);
            }
            else
                RebuildSongList(true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            /// Stop preview music if any
            m_SongList.StopPreviewMusic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the queue type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnQueueTypeChanged(SegmentedControl p_Sender, int p_Index)
        {
            UnselectSong();
            m_SongsProvider = p_Index == 0 ? ChatRequest.Instance.SongQueue : (p_Index == 1 ? ChatRequest.Instance.SongHistory : ChatRequest.Instance.SongBlackList);
            RebuildSongList(false);
            m_SongInfo_Detail.SetPracticeButtonEnabled(p_Index == 0);
        }
        /// <summary>
        /// Go to previous song page
        /// </summary>
        private void OnSongPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Rebuild song list
            RebuildSongList();
        }
        /// <summary>
        /// Go to next song page
        /// </summary>
        private void OnSongPageDownPressed()
        {
            /// Increment current page
            m_CurrentPage++;

            /// Rebuild song list
            RebuildSongList();
        }
        /// <summary>
        /// Rebuild song list
        /// </summary>
        /// <param name="p_OnActivation">Is on activation</param>
        /// <returns></returns>
        internal void RebuildSongList(bool p_OnActivation = false)
        {
            /// Clear selection and items, then refresh the list
            m_SongList.TableViewInstance.ClearSelection();
            m_SongList.Data.Clear();

            lock (m_SongsProvider)
            {
                /// Append all songs
                if (m_SongsProvider.Count > 0)
                {
                    /// Handle page overflow
                    if (((m_CurrentPage - 1) * s_SONG_PER_PAGE) > m_SongsProvider.Count)
                        m_CurrentPage = (m_SongsProvider.Count / s_SONG_PER_PAGE) + 1;

                    for (int l_I = (m_CurrentPage - 1) * s_SONG_PER_PAGE; l_I < (m_CurrentPage * s_SONG_PER_PAGE); ++l_I)
                    {
                        if (l_I >= m_SongsProvider.Count)
                            break;

                        var l_Current   = m_SongsProvider[l_I];
                        var l_HoverHint = "<b><u>Requested by</b></u> " + l_Current.NamePrefix + (l_Current.NamePrefix.Length != 0 ? " " : "") + l_Current.RequesterName;

                        if (l_Current.RequestTime.HasValue)
                            l_HoverHint += "\n<b><u>$$time$$</b></u>";

                        m_SongList.Data.Add(new SDK.UI.DataSource.SongList.Entry() {
                            BeatSaver_Map       = l_Current.BeatMap,
                            TitlePrefix         = l_Current.NamePrefix,
                            HoverHint           = l_HoverHint,
                            HoverHintTimeArg    = l_Current.RequestTime,
                            CustomData          = l_Current
                        });;

                        if (m_SelectedSong != null && m_SelectedSong.BeatMap.Hash == l_Current.BeatMap.Hash)
                        {
                            m_SongList.TableViewInstance.SelectCellWithIdx(m_SongList.Data.Count - 1);
                            OnSongSelected(m_SongList.TableViewInstance, m_SongList.Data.Count - 1);
                        }
                    }

                    if (m_SelectedSong != null && m_SongsProvider.Where(x => x.BeatMap.Hash == m_SelectedSong.BeatMap.Hash).Count() == 0)
                    {
                        UnselectSong();
                        m_SelectedSong = null;
                    }
                }
                else
                {
                    m_CurrentPage = 1;
                    UnselectSong();
                }

                /// Refresh the list
                m_SongList.TableViewInstance.ReloadData();

                /// Update UI
                m_SongUpButton.interactable     = m_CurrentPage != 1;
                m_SongDownButton.interactable   = m_SongsProvider.Count > (m_CurrentPage * s_SONG_PER_PAGE);
            }
        }
        /// <summary>
        /// When a song is selected
        /// </summary>
        /// <param name="p_TableView">Source table</param>
        /// <param name="p_Row">Selected row</param>
        private void OnSongSelected(TableView p_TableView, int p_Row)
        {
            /// Unselect previous song
            m_SelectedSong      = null;
            m_SelectedSongIndex = p_Row;

            /// Hide if invalid song
            if (p_Row >= m_SongList.Data.Count || m_SongList.Data[p_Row].Invalid || m_SongList.Data[p_Row].BeatSaver_Map == null || (p_TableView != null && m_SongList.Data[p_Row].BeatSaver_Map != null && m_SongList.Data[p_Row].BeatSaver_Map.Partial))
            {
                /// Hide song info panel
                UnselectSong();

                return;
            }

            /// Fetch song entry
            var l_SongEntry = m_SongList.Data[p_Row];

            /// Show UIs
            m_SongInfoPanel.SetActive(true);
            ManagerRight.Instance.SetVisible(true);

            /// Update UIs
            if (!m_SongInfo_Detail.FromBeatSaver(l_SongEntry.BeatSaver_Map, l_SongEntry.Cover))
            {
                /// Hide song info panel
                UnselectSong();

                return;
            }

            m_SongInfo_Detail.SetFavoriteToggleValue(m_TypeSegmentControl.selectedCellNumber == 2/* Blacklist */);
            ManagerRight.Instance.SetDetail(l_SongEntry.BeatSaver_Map);

            /// Set selected song
            m_SelectedSong = l_SongEntry.CustomData as ChatRequest.SongEntry;

            /// Launch preview music if local map
            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatMap.Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                m_SongInfo_Detail.SetPlayButtonText("Play");
            else
                m_SongInfo_Detail.SetPlayButtonText("Download");
        }
        /// <summary>
        /// On song cover fetched
        /// </summary>
        /// <param name="p_RowData">Row data</param>
        private void OnSongCoverFetched(int p_Index, SDK.UI.DataSource.SongList.Entry p_RowData)
        {
            if (m_SelectedSongIndex != p_Index)
                return;

            OnSongSelected(null, m_SelectedSongIndex);
        }
        /// <summary>
        /// Unselect active song
        /// </summary>
        private void UnselectSong()
        {
            m_SongInfoPanel.SetActive(false);
            ManagerRight.Instance.SetVisible(false);

            m_SelectedSong = null;

            /// Stop preview music if any
            m_SongList.StopPreviewMusic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Skip a song
        /// </summary>
        private void SkipSong()
        {
            if (m_SelectedSong == null)
            {
                UnselectSong();
                RebuildSongList();
                return;
            }

            ChatRequest.Instance.DequeueSong(m_SelectedSong, false);

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
                var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatMap.Hash);
                if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                {
                    ChatRequest.Instance.DequeueSong(m_SelectedSong, true);
                    m_PendingFilterSong = l_LocalSong;

                    try
                    {
                        var l_LevelFilteringNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().FirstOrDefault();

                        if (l_LevelFilteringNavigationController != null)
                            l_LevelFilteringNavigationController.didActivateEvent += LevelSelectionNavigationController_didActivateEvent;
                    }
                    catch (System.Exception p_Exception)
                    {
                        Logger.Instance?.Critical(p_Exception);
                    }

                    ManagerViewFlowCoordinator.Instance().Dismiss();
                    UnselectSong();
                }
                else
                {
                    /// Show download modal
                    ShowLoadingModal("Downloading", true);

                    /// Start downloading
                    SDK.Game.BeatSaver.DownloadSong(m_SelectedSong.BeatMap, CancellationToken.None, this).ContinueWith((x) =>
                    {
                        if (x.Result)
                        {
                            /// Bind callback
                            SongCore.Loader.SongsLoadedEvent += OnDownloadedSongLoaded;
                            /// Refresh loaded songs
                            SongCore.Loader.Instance.RefreshSongs(false);
                        }
                        else
                        {
                            /// Show error message
                            SDK.Unity.MainThreadInvoker.Enqueue(() => {
                                HideLoadingModal();
                                ShowMessageModal("Download failed!");
                            });
                        }
                    });

                    return;
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance?.Critical(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On black list button pressed
        /// </summary>
        /// <param name="p_State">State</param>
        private void OnBlacklistButtonPressed(ToggleWithCallbacks.SelectionState p_State)
        {
            if (p_State != ToggleWithCallbacks.SelectionState.Pressed)
                return;

            if (m_TypeSegmentControl.selectedCellNumber == 2/* Blacklist */)
                ChatRequest.Instance.UnBlacklistSong(m_SelectedSong);
            /// Show modal
            else
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to blacklist this song?", () => {
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
        /// Change current song view to all songs view
        /// </summary>
        /// <param name="p_FirstActivation"></param>
        /// <param name="p_AddedToHierarchy"></param>
        /// <param name="p_ScreenSystemEnabling"></param>
        private static void LevelSelectionNavigationController_didActivateEvent(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            var l_LevelSelectionNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().FirstOrDefault();
            if (l_LevelSelectionNavigationController == null)
                return;

            l_LevelSelectionNavigationController.didActivateEvent -= LevelSelectionNavigationController_didActivateEvent;

            SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_SelectLevelCategory(l_LevelSelectionNavigationController));
        }
        /// <summary>
        /// Level selection, select level category
        /// </summary>
        /// <param name="p_LevelSelectionNavigationController">LevelSelectionNavigationController instance</param>
        /// <returns></returns>
        private static IEnumerator LevelSelection_SelectLevelCategory(LevelSelectionNavigationController p_LevelSelectionNavigationController)
        {
            yield return new WaitUntil(() => !p_LevelSelectionNavigationController || !p_LevelSelectionNavigationController.isInTransition);

            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Menu)
                yield break;

            if (!p_LevelSelectionNavigationController || !p_LevelSelectionNavigationController.isInViewControllerHierarchy || !p_LevelSelectionNavigationController.isActiveAndEnabled)
                yield break;

            var l_LevelFilteringNavigationController = p_LevelSelectionNavigationController.GetField<LevelFilteringNavigationController, LevelSelectionNavigationController>("_levelFilteringNavigationController");
            if (!l_LevelFilteringNavigationController)
                yield break;

            if (l_LevelFilteringNavigationController.selectedLevelCategory != SelectLevelCategoryViewController.LevelCategory.All)
            {
                var l_Selector = l_LevelFilteringNavigationController.GetField<SelectLevelCategoryViewController, LevelFilteringNavigationController>("_selectLevelCategoryViewController");
                if (l_Selector != null && l_Selector)
                {
                    var l_SegmentControl    = l_Selector.GetField<IconSegmentedControl, SelectLevelCategoryViewController>("_levelFilterCategoryIconSegmentedControl");
                    var l_Tags              = l_Selector.GetField<SelectLevelCategoryViewController.LevelCategoryInfo[], SelectLevelCategoryViewController>("_levelCategoryInfos");
                    var l_IndexToSelect     = l_Tags.Select((x => x.levelCategory)).ToList().IndexOf(SelectLevelCategoryViewController.LevelCategory.All);

                    /// Multiplayer : missing extension
                    if (l_IndexToSelect == -1)
                        yield break;

                    l_SegmentControl.SelectCellWithNumber(l_IndexToSelect);
                    l_Selector.LevelFilterCategoryIconSegmentedControlDidSelectCell(l_SegmentControl, l_IndexToSelect);

                    SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_FilterLevel(
                        l_LevelFilteringNavigationController.GetField<LevelSearchViewController, LevelFilteringNavigationController>("_levelSearchViewController"),
                        true
                    ));
                }
            }
            else
            {
                SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_FilterLevel(
                    l_LevelFilteringNavigationController.GetField<LevelSearchViewController, LevelFilteringNavigationController>("_levelSearchViewController"),
                    false
                ));
            }
        }
        /// <summary>
        /// Level selection, filter
        /// </summary>
        /// <param name="p_LevelSearchViewController">LevelSearchViewController instance</param>
        /// <param name="p_Wait">Should wait for any transition</param>
        /// <returns></returns>
        private static IEnumerator LevelSelection_FilterLevel(LevelSearchViewController p_LevelSearchViewController, bool p_Wait)
        {
            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Menu)
                yield break;

            if (p_LevelSearchViewController == null || !p_LevelSearchViewController || m_PendingFilterSong == null)
                yield break;

            if (p_Wait)
            {
                yield return new WaitUntil(() => !p_LevelSearchViewController || !p_LevelSearchViewController.isInTransition);

                if (!p_LevelSearchViewController || !p_LevelSearchViewController.isInViewControllerHierarchy || !p_LevelSearchViewController.isActiveAndEnabled)
                    yield break;

                if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Menu)
                    yield break;
            }

            try
            {
                p_LevelSearchViewController.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;
                p_LevelSearchViewController.ResetCurrentFilterParams();
                var l_InputFieldView = p_LevelSearchViewController.GetField<InputFieldView, LevelSearchViewController>("_searchTextInputFieldView");
                if (l_InputFieldView != null && l_InputFieldView)
                {
                    l_InputFieldView.SetText(m_PendingFilterSong.songName);
                    l_InputFieldView.UpdateClearButton();
                    l_InputFieldView.UpdatePlaceholder();
                }

                p_LevelSearchViewController.UpdateSearchLevelFilterParams(LevelFilterParams.ByBeatmapLevelIds(new HashSet<string>() { m_PendingFilterSong.levelID }));
                p_LevelSearchViewController.didStartLoadingEvent += LevelSearchViewController_didStartLoadingEvent;
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatRequest] LevelSelection_FilterLevel coroutine failed : ");
                Logger.Instance.Error(p_Exception);

                LevelSearchViewController_didStartLoadingEvent(p_LevelSearchViewController);
            }
        }
        /// <summary>
        /// LevelSearchViewController didStartLoadingEvent
        /// </summary>
        /// <param name="p_LevelSearchViewController">LevelSearchViewController instance</param>
        private static void LevelSearchViewController_didStartLoadingEvent(LevelSearchViewController p_LevelSearchViewController)
        {
            if (!p_LevelSearchViewController)
                return;

            p_LevelSearchViewController.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;

            try
            {
                var l_Filter = p_LevelSearchViewController.GetField<LevelFilterParams, LevelSearchViewController>("_currentFilterParams");
                if (l_Filter != null && l_Filter.filterByLevelIds)
                {
                    p_LevelSearchViewController.ResetCurrentFilterParams();

                    var l_InputFieldView = p_LevelSearchViewController.GetField<InputFieldView, LevelSearchViewController>("_searchTextInputFieldView");
                    if (l_InputFieldView != null && l_InputFieldView)
                    {
                        l_InputFieldView.UpdateClearButton();
                        l_InputFieldView.UpdatePlaceholder();
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatRequest] LevelSearchViewController_didStartLoadingEvent failed : ");
                Logger.Instance.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On download progress reported
        /// </summary>
        /// <param name="p_Value"></param>
        void IProgress<double>.Report(double p_Value)
        {
            SetLoadingModal_DownloadProgress("Downloading", (float)p_Value);
        }
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
            HideLoadingModal();

            /// Reselect the cell
            PlaySong();

            yield return null;
        }
    }
}
