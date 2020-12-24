using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.RankedAssistant.UI
{
    internal class Main : BSMLResourceViewController, IProgress<double>
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SongUpButton")]
        private Button m_SongUpButton;
        [UIComponent("SongList")]
        private CustomListTableData m_SongList = null;
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
                m_SongsProvider = Plugins.ChatRequest.ChatRequest.Instance.SongHistory;

                /// Scale down up & down button
                m_SongUpButton.transform.localScale     = Vector3.one * 0.6f;
                m_SongDownButton.transform.localScale   = Vector3.one * 0.6f;

                /// Create type selector
                m_TypeSegmentControl = Utils.GameUI.CreateTextSegmentedControl(m_TypeSegmentPanel.transform as RectTransform, false);
                m_TypeSegmentControl.SetTexts(new string[] { "Playlist Generator", "Playlist" });
                m_TypeSegmentControl.ReloadData();
                m_TypeSegmentControl.didSelectCellEvent += OnTabChanged;

                /// Bind events
                m_SongUpButton.onClick.AddListener(OnSongPageUpPressed);
                m_SongList.tableView.didSelectCellWithIdxEvent += OnSongSelected;
                m_SongDownButton.onClick.AddListener(OnSongPageDownPressed);

                /// Find song preview object
                m_SongPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First();

                /// Show song info panel
                m_SongInfo_Detail = new BeatSaberPlus.UI.Widget.SongDetail(m_SongInfoPanel.transform);
                UnselectSong();

                m_SongInfo_Detail.SetPlayButtonText("Play");
                m_SongInfo_Detail.SetPlayButtonEnabled(true);
                m_SongInfo_Detail.SetPlayButtonAction(PlaySong);
                m_SongInfo_Detail.OnActiveDifficultyChanged += (x) => {
                    /// Hide if no data
                    if (x == null)
                        BeatSaberPlus.UI.ViewFlowCoordinator.Instance.SetRightScreen(null);

                    /// Show score board
                    var l_ScoreBoard = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().FirstOrDefault();
                    if (l_ScoreBoard != null)
                    {
                        if (!l_ScoreBoard.isInViewControllerHierarchy)
                            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.SetRightScreen(l_ScoreBoard);

                        l_ScoreBoard.SetData(x);
                    }
                };

                /// Init loading modal
                m_LoadingModalSpinner = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<LoadingControl>().First(), m_LoadingModal.transform);
                m_LoadingModalSpinner.transform.SetAsLastSibling();

                Destroy(m_LoadingModalSpinner.GetComponent<Touchable>());

                /// Force change to tab Request
                OnTabChanged(null, 0);
            }

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the queue type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnTabChanged(SegmentedControl p_Sender, int p_Index)
        {
            UnselectSong();

            /// todo...
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
            m_SongList.tableView.ClearSelection();
            m_SongList.data.Clear();

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
                        var l_Entry     = new BeatSaverCustomSongCellInfo(l_Current.BeatMap.Hash, "", "");
                        var l_LocalSong = SongCore.Loader.GetLevelByHash(l_Current.BeatMap.Hash);

                        if (!l_Current.BeatMap.Partial)
                        {
                            l_Entry.text    = (l_LocalSong != null ? "<#7F7F7F>" : "") + l_Current.BeatMap.Name;
                            l_Entry.subtext = l_Current.BeatMap.Metadata.SongAuthorName + " [" + l_Current.BeatMap.Metadata.LevelAuthorName + "]";

                            if (l_Current.Cover != null)
                                l_Entry.icon = l_Current.Cover;
                            else
                            {
                                /// Fetch cover
                                var l_CoverTask = l_Current.BeatMap.FetchCoverImage();
                                _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                                {
                                    HMMainThreadDispatcher.instance.Enqueue(() =>
                                    {
                                        var l_Texture = new Texture2D(2, 2);

                                        if (l_Texture.LoadImage(p_CoverTaskResult.Result))
                                        {
                                            l_Entry.icon = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);
                                            l_Current.Cover = l_Entry.icon;
                                        }
                                        else
                                            l_Entry.icon = null;

                                        /// Refresh cells
                                        m_SongList.tableView.RefreshCellsContent();
                                    });
                                });
                            }
                        }
                        else
                        {
                            l_Entry.text    = "Loading from beatsaver...";
                            l_Entry.subtext = "";
                        }

                        m_SongList.data.Add(l_Entry);

                        if (m_SelectedSong != null && m_SelectedSong.BeatMap.Hash == l_Current.BeatMap.Hash)
                        {
                            m_SongList.tableView.SelectCellWithIdx(m_SongList.data.Count - 1);
                            OnSongSelected(m_SongList.tableView, m_SongList.data.Count - 1);
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
                m_SongList.tableView.ReloadData();

                /// Update UI
                m_SongUpButton.interactable   = m_CurrentPage != 1;
                m_SongDownButton.interactable = m_SongsProvider.Count > (m_CurrentPage * 7);
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
            BeatSaverCustomSongCellInfo l_SongRow = m_SongList.data[p_Row] as BeatSaverCustomSongCellInfo;

            /// Get song entry
            var l_SongEntry = m_SongsProvider.Where(x => x.BeatMap.Hash == l_SongRow.SongHash).FirstOrDefault();

            /// Hide if invalid song
            if (l_SongEntry == null)
            {
                /// Hide song info panel
                UnselectSong();

                return;
            }

            /// Show UIs
            m_SongInfoPanel.SetActive(true);

            /// Update UIs
            m_SongInfo_Detail.FromBeatSaver(l_SongEntry.BeatMap, l_SongEntry.Cover);

            /// Set selected song
            m_SelectedSong = l_SongEntry;

            /// Launch preview music if local map
            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_SelectedSong.BeatMap.Hash);
            if (l_LocalSong != null)
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
            //FlowCoordinator.DetailView.SetVisible(false);

            m_SelectedSong = null;

            /// Stop preview music if any
            m_SongPreviewPlayer.CrossfadeToDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            if (l_LocalSong != null)
            {
                /// Show loading modal
                ShowLoadingModal();

                _ = Utils.Songs.LoadSong(l_LocalSong.levelID, (p_LoadedLevel) =>
                {
                    /// Hide loading modal
                    HideLoadingModal();

                    /// Fetch game settings
                    var l_PlayerData        = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
                    var l_PlayerSettings    = l_PlayerData.playerSpecificSettings;
                    var l_ColorScheme       = l_PlayerData.colorSchemesSettings.overrideDefaultColors ? l_PlayerData.colorSchemesSettings.GetSelectedColorScheme() : null;

                    Utils.Songs.PlaySong(p_LoadedLevel, m_SongInfo_Detail.SelectedBeatmapCharacteristicSO, m_SongInfo_Detail.SelecteBeatmapDifficulty, l_PlayerData.overrideEnvironmentSettings, l_ColorScheme, l_PlayerData.gameplayModifiers, l_PlayerSettings,
                    (p_Transition, p_LevelPlayResult, p_Difficulty) =>
                    {
                        Logger.Instance.Error("rank " + p_Difficulty.difficultyRank);
                        if (p_LevelPlayResult.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
                            return;


                        /// Show loading modal
                        ShowLoadingModal("Uploading score");
                    });
                });
            }
            else
            {
                /// Show download modal
                ShowLoadingModal("Downloading", true);

                /// Start downloading
                Utils.SongDownloader.DownloadSong(m_SelectedSong.BeatMap, CancellationToken.None, this).ContinueWith((x) =>
                {
                    /// Bind callback
                    SongCore.Loader.SongsLoadedEvent += OnDownloadedSongLoaded;
                    /// Refresh loaded songs
                    SongCore.Loader.Instance.RefreshSongs(false);
                });

                return;
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
    /// BeatSaver custom song cell info
    /// </summary>
    internal class BeatSaverCustomSongCellInfo : CustomListTableData.CustomCellInfo
    {
        /// <summary>
        /// Hash of the song
        /// </summary>
        internal readonly string SongHash;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_SongHash">Hash of the song</param>
        /// <param name="p_RequesterName">Requester name</param>
        /// <param name="p_Text">Text</param>
        /// <param name="p_SubText">Sub text</param>
        internal BeatSaverCustomSongCellInfo(string p_SongHash, string p_Text, string p_SubText = null)
            : base(p_Text, p_SubText, null)
        {
            SongHash = p_SongHash;
        }
    }
}
