using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    internal class ManagerMain : BSMLResourceViewController, IProgress<double>
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Flow coordinator instance
        /// </summary>
        internal ManagerViewFlowCoordinator FlowCoordinator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIValue("Title")]
        internal string m_Title = "Requests";
        [UIComponent("LoadingModal")]
        private ModalView m_LoadingModal = null;
        [UIValue("LoadingModalText")]
        private string m_LoadingModalText;
        private LoadingControl m_LoadingModalSpinner = null;
        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SongUpButton")]
        private Button m_SongUpButton;
        [UIObject("SongList")]
        private GameObject m_SongListView = null;
        private SongListDataSource m_SongList = null;
        [UIComponent("SongDownButton")]
        private Button m_SongDownButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongInfoPanel")]
        private GameObject m_SongInfoPanel;
        private BeatSaberPlus.UI.Widget.SongDetail m_SongInfo_Detail;
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
        Plugins.ChatRequest.ChatRequest.SongEntry m_SelectedSong = null;
        /// <summary>
        /// Preview song player
        /// </summary>
        private SongPreviewPlayer m_SongPreviewPlayer;
        /// <summary>
        /// Song list provider
        /// </summary>
        private List<Plugins.ChatRequest.ChatRequest.SongEntry> m_SongsProvider = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pending filter song
        /// </summary>
        static CustomPreviewBeatmapLevel m_PendingFilterSong = null;

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
                /// Scale down up & down button
                m_SongUpButton.transform.localScale     = Vector3.one * 0.6f;
                m_SongDownButton.transform.localScale   = Vector3.one * 0.6f;

                /// Create type selector
                m_TypeSegmentControl = Utils.GameUI.CreateTextSegmentedControl(m_TypeSegmentPanel.transform as RectTransform, false);
                m_TypeSegmentControl.SetTexts(new string[] { "Requests", "History", "Blacklist" });
                m_TypeSegmentControl.ReloadData();
                m_TypeSegmentControl.didSelectCellEvent += OnQueueTypeChanged;

                /// Prepare song list
                var l_BSMLTableView = m_SongListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_SongListView.GetComponentInChildren<CustomListTableData>());
                m_SongList = l_BSMLTableView.gameObject.AddComponent<SongListDataSource>();
                m_SongList.TableViewInstance = l_BSMLTableView;
                l_BSMLTableView.SetDataSource(m_SongList, false);

                /// Bind events
                m_SongUpButton.onClick.AddListener(OnSongPageUpPressed);
                m_SongList.TableViewInstance.didSelectCellWithIdxEvent += OnSongSelected;
                m_SongDownButton.onClick.AddListener(OnSongPageDownPressed);

                /// Find song preview object
                m_SongPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();

                /// Show song info panel
                m_SongInfo_Detail = new BeatSaberPlus.UI.Widget.SongDetail(m_SongInfoPanel.transform);
                UnselectSong();

                m_SongInfo_Detail.SetFavoriteToggleEnabled(true);
                m_SongInfo_Detail.SetFavoriteToggleImage("BeatSaberPlus.Plugins.ChatRequest.Resources.Blacklist.png", "BeatSaberPlus.Plugins.ChatRequest.Resources.Unblacklist.png");
                m_SongInfo_Detail.SetFavoriteToggleHoverHint("Add/Remove to blacklist");
                m_SongInfo_Detail.SetFavoriteToggleCallback(OnBlacklistButtonPressed);

                m_SongInfo_Detail.SetPracticeButtonEnabled(true);
                m_SongInfo_Detail.SetPracticeButtonText("Skip");
                m_SongInfo_Detail.SetPracticeButtonAction(SkipSong);

                m_SongInfo_Detail.SetPlayButtonText("Play");
                m_SongInfo_Detail.SetPlayButtonEnabled(true);
                m_SongInfo_Detail.SetPlayButtonAction(PlaySong);

                /// Init loading modal
                m_LoadingModalSpinner = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LoadingControl>().First(), m_LoadingModal.transform);
                m_LoadingModalSpinner.transform.SetAsLastSibling();

                Destroy(m_LoadingModalSpinner.GetComponent<Touchable>());

                /// Force change to tab Request
                OnQueueTypeChanged(null, 0);
            }

            /// Go back to request tab
            if (!p_FirstActivation && m_TypeSegmentControl.selectedCellNumber != 0)
            {
                m_TypeSegmentControl.SelectCellWithNumber(0);
                OnQueueTypeChanged(null, 0);
            }
            else
                RebuildSongList(true);
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            /// Close all remaining modals
            m_ParserParams.EmitEvent("CloseAllModals");

            /// Stop preview music if any
            m_SongPreviewPlayer.CrossfadeToDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the loading modal
        /// </summary>
        private void ShowLoadingModal(string p_Message = "Loading", bool p_Download = false)
        {
            /// Close old loading modal
            HideLoadingModal();

            /// Change modal text
            m_LoadingModalText = p_Download ? "" : p_Message;

            /// Update UI
            NotifyPropertyChanged("LoadingModalText");

            /// Show the modal
            m_ParserParams.EmitEvent("ShowLoadingModal");

            /// Show animator
            if (!p_Download)
                m_LoadingModalSpinner.ShowLoading();
            else
                m_LoadingModalSpinner.ShowDownloadingProgress(p_Message, 0);
        }
        /// <summary>
        /// Hide the loading modal
        /// </summary>
        private void HideLoadingModal()
        {
            m_ParserParams.EmitEvent("CloseLoadingModal");
            m_LoadingModalSpinner.Hide();
        }
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
        /// When the queue type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnQueueTypeChanged(SegmentedControl p_Sender, int p_Index)
        {
            var l_Plugin = Plugins.ChatRequest.ChatRequest.Instance;

            UnselectSong();
            m_SongsProvider = p_Index == 0 ? l_Plugin.SongQueue : (p_Index == 1 ? l_Plugin.SongHistory : l_Plugin.SongBlackList);
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
                    if (((m_CurrentPage - 1) * 7) > m_SongsProvider.Count)
                        m_CurrentPage = (m_SongsProvider.Count / 7) + 1;

                    for (int l_I = (m_CurrentPage - 1) * 7; l_I < (m_CurrentPage * 7); ++l_I)
                    {
                        if (l_I >= m_SongsProvider.Count)
                            break;

                        var l_Current   = m_SongsProvider[l_I];
                        m_SongList.Data.Add(l_Current);

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
                m_SongDownButton.interactable   = m_SongsProvider.Count > (m_CurrentPage * 7);
            }
        }
        /// <summary>
        /// When a song is selected
        /// </summary>
        /// <param name="p_TableView">Source table</param>
        /// <param name="p_Row">Selected row</param>
        private void OnSongSelected(TableView p_TableView, int p_Row)
        {
            /// Fetch song entry
            var l_SongEntry = m_SongList.Data[p_Row];

            /// Hide if invalid song
            if (l_SongEntry == null)
            {
                /// Hide song info panel
                UnselectSong();

                return;
            }

            /// Show UIs
            m_SongInfoPanel.SetActive(true);
            FlowCoordinator.DetailView.SetVisible(true);

            /// Update UIs
            m_SongInfo_Detail.FromBeatSaver(l_SongEntry.BeatMap, l_SongEntry.Cover);
            m_SongInfo_Detail.SetFavoriteToggleValue(m_TypeSegmentControl.selectedCellNumber == 2/* Blacklist */);
            FlowCoordinator.DetailView.SetDetail(l_SongEntry.BeatMap);

            /// Set selected song
            m_SelectedSong = l_SongEntry;

            /// Launch preview music if local map
            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatMap.Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                m_SongInfo_Detail.SetPlayButtonText("Play");

                /// Load the song clip to get duration
                if (Config.ChatRequest.PlayPreviewMusic)
                {
                    l_LocalSong.GetPreviewAudioClipAsync(CancellationToken.None).ContinueWith(x =>
                    {
                        if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                        {
                            HMMainThreadDispatcher.instance.Enqueue(() =>
                            {
                                /// Start preview song
                                m_SongPreviewPlayer.CrossfadeTo(x.Result, l_LocalSong.previewStartTime, l_LocalSong.previewDuration, 1f);
                            });
                        }
                    });
                }
            }
            /// Stop preview music if any
            else
            {
                m_SongInfo_Detail.SetPlayButtonText("Download");
                m_SongPreviewPlayer.CrossfadeToDefault();
            }
        }
        /// <summary>
        /// Unselect active song
        /// </summary>
        private void UnselectSong()
        {
            m_SongInfoPanel.SetActive(false);
            FlowCoordinator.DetailView.SetVisible(false);

            m_SelectedSong = null;

            /// Stop preview music if any
            m_SongPreviewPlayer.CrossfadeToDefault();
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

            Plugins.ChatRequest.ChatRequest.Instance.DequeueSong(m_SelectedSong, false);

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

            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatMap.Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                Plugins.ChatRequest.ChatRequest.Instance.DequeueSong(m_SelectedSong, true);
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

                UnselectSong();
                FlowCoordinator.Hide();
            }
            else
            {
                /// Show download modal
                ShowLoadingModal("Downloading", true);

                /// Start downloading
                Utils.SongDownloader.DownloadSong(m_SelectedSong.BeatMap, CancellationToken.None, this).ContinueWith((x) =>
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
                        HMMainThreadDispatcher.instance.Enqueue(() => {
                            HideLoadingModal();
                            ShowMessageModal("Download failed!");
                        });
                    }
                });

                return;
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
                m_ParserParams.EmitEvent("BlacklistMessageModal");
        }
        /// <summary>
        /// Blacklist no button
        /// </summary>
        [UIAction("click-btn-blacklist-no")]
        private void OnBlacklistNoButton()
        {
            /// Update UI
            m_SongInfo_Detail.SetFavoriteToggleValue(false);

            /// Close modal
            m_ParserParams.EmitEvent("CloseBlacklistMessageModal");
        }
        /// <summary>
        /// Blacklist yes button
        /// </summary>
        [UIAction("click-btn-blacklist-yes")]
        private void OnBlacklistYesButton()
        {
            /// Update UI
            m_SongInfo_Detail.SetFavoriteToggleValue(true);

            /// Blacklist the song
            Plugins.ChatRequest.ChatRequest.Instance.BlacklistSong(m_SelectedSong);

            /// Close modal
            m_ParserParams.EmitEvent("CloseBlacklistMessageModal");
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

            LevelFilteringNavigationController l_LevelFilteringNavigationController = l_LevelSelectionNavigationController.GetField<LevelFilteringNavigationController>("_levelFilteringNavigationController");

            try
            {
                if (l_LevelFilteringNavigationController != null)
                {
                    var l_Selector = l_LevelFilteringNavigationController.GetField<SelectLevelCategoryViewController>("_selectLevelCategoryViewController");
                    if (l_Selector != null && l_Selector)
                    {
                        var l_Tags = l_Selector.GetField<SelectLevelCategoryViewController.LevelCategoryInfo[]>("_allLevelCategoryInfos");
                        for (int l_I = 0; l_I < l_Tags.Length; ++l_I)
                        {
                            if (l_Tags[l_I].levelCategory != SelectLevelCategoryViewController.LevelCategory.All)
                                continue;

                            var l_SegmentControl = l_Selector.GetField<IconSegmentedControl>("_levelFilterCategoryIconSegmentedControl");
                            var l_Cells          = l_Selector != null ? l_SegmentControl.GetField<List<SegmentedControlCell>>("_cells") : null as List<SegmentedControlCell>;

                            if (l_Cells != null)
                            {
                                /// Multiplayer fix
                                if (l_Cells.Count == 4)
                                    l_I = l_I - 1;

                                l_SegmentControl.SelectCellWithNumber(l_I);
                                l_Selector.LevelFilterCategoryIconSegmentedControlDidSelectCell(l_SegmentControl, l_I);
                                //l_SegmentControl.HandleCellSelectionDidChange(l_Cells.ElementAt(l_I), SelectableCell.TransitionType.Instant, null);
                                //l_SegmentControl.SelectCellWithNumber(l_I);
                            }
                            break;
                        }
                    }

                    /// Wait next frame
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        try
                        {
                            var l_LevelSearchViewController = l_LevelFilteringNavigationController.GetField<LevelSearchViewController>("_levelSearchViewController");
                            if (l_LevelSearchViewController != null
                                && l_LevelSearchViewController
                                && l_LevelSearchViewController.isInViewControllerHierarchy
                                && !l_LevelSearchViewController.isInTransition
                                && l_LevelSearchViewController.gameObject.activeInHierarchy)
                            {
                                l_LevelSearchViewController.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;
                                l_LevelSearchViewController.ResetCurrentFilterParams();
                                var l_InputFieldView = l_LevelSearchViewController.GetField<InputFieldView>("_searchTextInputFieldView");
                                if (l_InputFieldView != null && l_InputFieldView)
                                {
                                    l_InputFieldView.SetText(m_PendingFilterSong.songName);
                                    l_InputFieldView.UpdateClearButton();
                                    l_InputFieldView.UpdatePlaceholder();
                                }

                                l_LevelSearchViewController.UpdateSearchLevelFilterParams(LevelFilterParams.ByBeatmapLevelIds(new HashSet<string>() { m_PendingFilterSong.levelID }));
                                l_LevelSearchViewController.didStartLoadingEvent += LevelSearchViewController_didStartLoadingEvent;
                            }
                        }
                        catch (System.Exception p_Exception)
                        {
                            Logger.Instance.Error("[ChatRequest] LevelSelectionNavigationController_didActivateEvent coroutine failed : ");
                            Logger.Instance.Error(p_Exception);

                            LevelSearchViewController_didStartLoadingEvent(l_LevelFilteringNavigationController.GetField<LevelSearchViewController>("_levelSearchViewController"));
                        }
                    });
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatRequest] LevelSelectionNavigationController_didActivateEvent failed : ");
                Logger.Instance.Error(p_Exception);

                LevelSearchViewController_didStartLoadingEvent(l_LevelFilteringNavigationController.GetField<LevelSearchViewController>("_levelSearchViewController"));
            }
        }
        /// <summary>
        /// Handle full reset on exit
        /// </summary>
        /// <param name="p_Object"></param>
        private static void LevelSearchViewController_didStartLoadingEvent(LevelSearchViewController p_Object)
        {
            if (p_Object != null)
                p_Object.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;

            if (!p_Object || !p_Object.isInViewControllerHierarchy || p_Object.isInTransition || !p_Object.gameObject.activeInHierarchy)
                return;

            try
            {
                var l_Filter = p_Object.GetField<LevelFilterParams>("_currentFilterParams");
                if (l_Filter != null && l_Filter.filterByLevelIds)
                {
                    p_Object.ResetCurrentFilterParams();
                    var l_InputFieldView = p_Object.GetField<InputFieldView>("_searchTextInputFieldView");
                    if (l_InputFieldView != null && l_InputFieldView)
                    {
                        l_InputFieldView.SetText("");
                        l_InputFieldView.UpdateClearButton();
                        l_InputFieldView.UpdatePlaceholder();
                    }

                    p_Object.UpdateSearchLevelFilterParams(LevelFilterParams.NoFilter());
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
            /// Avoid refresh if not active view anymore
            if (!isInViewControllerHierarchy)
                return;

            m_LoadingModalSpinner.ShowDownloadingProgress("Downloading", (float)p_Value);
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
            if (!isInViewControllerHierarchy)
                return;

            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                /// Reselect the cell
                PlaySong();

                /// Hide loading modal
                HideLoadingModal();
            });
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Song entry list source
    /// </summary>
    internal class SongListDataSource : MonoBehaviour, TableView.IDataSource
    {
        /// <summary>
        /// Cell template
        /// </summary>
        private LevelListTableCell m_SongListTableCellInstance;
        /// <summary>
        /// Default cover image
        /// </summary>
        private Sprite m_DefaultCover = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        internal TableView TableViewInstance;
        /// <summary>
        /// Data
        /// </summary>
        internal List<ChatRequest.SongEntry> Data = new List<ChatRequest.SongEntry>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Index">Cell index</param>
        /// <returns></returns>
        public TableCell CellForIdx(TableView p_TableView, int p_Index)
        {
            LevelListTableCell l_Cell = GetTableCell();

            TextMeshProUGUI l_Text      = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText");
            TextMeshProUGUI l_SubText   = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songAuthorText");
            l_Cell.GetField<Image, LevelListTableCell>("_favoritesBadgeImage").gameObject.SetActive(false);

            var l_HoverHint = l_Cell.gameObject.GetComponent<HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            var l_SongEntry = Data[p_Index];
            if (!l_SongEntry.BeatMap.Partial)
            {
                var l_LocalSong = SongCore.Loader.GetLevelByHash(l_SongEntry.BeatMap.Hash);
                l_Text.text     = l_SongEntry.NamePrefix + (l_SongEntry.NamePrefix.Length != 0 ? " " : "") + ((l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath)) ? "<#7F7F7F>" : "") + l_SongEntry.BeatMap.Name;
                l_SubText.text  = l_SongEntry.BeatMap.Metadata.SongAuthorName + " [" + l_SongEntry.BeatMap.Metadata.LevelAuthorName + "]";

                if (l_Text.text.Length > (28 + (l_LocalSong != null ? "<#7F7F7F>".Length : 0)))
                    l_Text.text = l_Text.text.Substring(0, 28 + (l_LocalSong != null ? "<#7F7F7F>".Length : 0)) + "...";
                if (l_SubText.text.Length > 28)
                    l_SubText.text = l_SubText.text.Substring(0, 28) + "...";

                var l_FirstDiff = l_SongEntry.BeatMap.Metadata.Characteristics.First().Difficulties.Where(x => x.Value.HasValue).LastOrDefault();

                var l_BPMText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText");
                l_BPMText.gameObject.SetActive(true);
                l_BPMText.text = ((int)l_SongEntry.BeatMap.Metadata.BPM).ToString();

                var l_DurationText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText");
                l_DurationText.gameObject.SetActive(true);
                l_DurationText.text = l_FirstDiff.Value.Value.Length >= 0.0 ? $"{Math.Floor((double)l_FirstDiff.Value.Value.Length / 60):N0}:{Math.Floor((double)l_FirstDiff.Value.Value.Length % 60):00}" : "--";

                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(true);

                if (l_SongEntry.Cover != null)
                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                else
                {
                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;

                    /// Fetch cover
                    var l_CoverTask = l_SongEntry.BeatMap.FetchCoverImage();
                    _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                    {
                        if (l_Cell.idx >= Data.Count || l_SongEntry.BeatMap.Hash != Data[l_Cell.idx].BeatMap.Hash)
                            return;

                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            var l_Texture = new Texture2D(2, 2);

                            if (l_Texture.LoadImage(p_CoverTaskResult.Result))
                            {
                                l_SongEntry.Cover = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);

                                if (l_Cell.idx < Data.Count && l_SongEntry.BeatMap.Hash == Data[l_Cell.idx].BeatMap.Hash)
                                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                            }
                        });
                    });
                }
            }
            else
            {
                l_Text.text     = "Loading from BeatSaver...";
                l_SubText.text  = "";

                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText").gameObject.SetActive(false);
                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText").gameObject.SetActive(false);
                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

                l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;
            }

            l_HoverHint.text = "Requested by <u>" + l_SongEntry.RequesterName + "</u>";

            return l_Cell;
        }
        /// <summary>
        /// Get cell size
        /// </summary>
        /// <returns></returns>
        public float CellSize()
        {
            return 8.5f;
        }
        /// <summary>
        /// Get number of cell
        /// </summary>
        /// <returns></returns>
        public int NumberOfCells()
        {
            return Data.Count();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get new table cell or reuse old one
        /// </summary>
        /// <returns></returns>
        private LevelListTableCell GetTableCell()
        {
            LevelListTableCell l_Cell = (LevelListTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_ChatRequest_Cell");
            if (!l_Cell)
            {
                if (m_SongListTableCellInstance == null)
                    m_SongListTableCellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                l_Cell = Instantiate(m_SongListTableCellInstance);
            }

            l_Cell.SetField("_notOwned", false);
            l_Cell.reuseIdentifier = "BSP_ChatRequest_Cell";

            if (m_DefaultCover == null)
                m_DefaultCover = l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite;

            return l_Cell;
        }
    }
}
