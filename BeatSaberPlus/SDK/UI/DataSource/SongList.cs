using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.SDK.UI.DataSource
{
    /// <summary>
    /// Song entry list source
    /// </summary>
    public class SongList : UnityEngine.MonoBehaviour, HMUI.TableView.IDataSource
    {
        /// <summary>
        /// Cell template
        /// </summary>
        private LevelListTableCell m_SongListTableCellInstance;
        /// <summary>
        /// Default cover image
        /// </summary>
        private UnityEngine.Sprite m_DefaultCover = null;
        /// <summary>
        /// Song preview player
        /// </summary>
        private SongPreviewPlayer m_SongPreviewPlayer = null;
        /// <summary>
        /// Old loading coroutine
        /// </summary>
        private Coroutine m_OldCoroutine = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        public HMUI.TableView TableViewInstance;
        /// <summary>
        /// Data
        /// </summary>
        public List<Entry> Data = new List<Entry>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cover cache
        /// </summary>
        public static Dictionary<string, UnityEngine.Sprite> CoverCache = new Dictionary<string, UnityEngine.Sprite>();
        /// <summary>
        /// Audio clip cache
        /// </summary>
        public static Dictionary<string, UnityEngine.AudioClip> AudioClipCache = new Dictionary<string, UnityEngine.AudioClip>();
        /// <summary>
        /// Play preview audio ?
        /// </summary>
        public bool PlayPreviewAudio = false;
        /// <summary>
        /// Preview volume
        /// </summary>
        public float PreviewAudioVolume = 1f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cover fetched event
        /// </summary>
        public event Action<int, Entry> OnCoverFetched;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Song entry
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Was init
            /// </summary>
            private bool m_WasInit = false;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Title prefix
            /// </summary>
            public string TitlePrefix = "";
            /// <summary>
            /// Hover hint text
            /// </summary>
            public string HoverHint = null;
            /// <summary>
            /// Hover hint time argument
            /// </summary>
            public DateTime? HoverHintTimeArg = null;
            /// <summary>
            /// Beat saver map
            /// </summary>
            public Game.BeatMaps.MapDetail BeatSaver_Map = null;
            /// <summary>
            /// Custom level instance
            /// </summary>
            public CustomPreviewBeatmapLevel CustomLevel = null;
            /// <summary>
            /// Cover
            /// </summary>
            public UnityEngine.Sprite Cover;
            /// <summary>
            /// Custom data
            /// </summary>
            public object CustomData = null;
            /// <summary>
            /// Is invalid
            /// </summary>
            public bool Invalid => (CustomLevel == null && BeatSaver_Map == null);

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Init the entry
            /// </summary>
            public void Init()
            {
                if (m_WasInit)
                    return;

                if (CustomLevel == null && BeatSaver_Map != null && !BeatSaver_Map.Partial)
                {
                    var l_LocalLevel = SongCore.Loader.GetLevelByHash(GetLevelHash().ToUpper());
                    if (l_LocalLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalLevel.customLevelPath))
                        CustomLevel = l_LocalLevel;
                }

                m_WasInit = true;
            }
            /// <summary>
            /// Get entry level hash
            /// </summary>
            /// <returns></returns>
            public string GetLevelHash()
            {
                if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                    return BeatSaver_Map.SelectMapVersion().hash.ToLower();
                else if (CustomLevel != null && CustomLevel.levelID.StartsWith("custom_level_"))
                    return CustomLevel.levelID.Replace("custom_level_", "").ToLower();

                return "";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        public void OnDestroy()
        {
            /// Bind event
            TableViewInstance.didSelectCellWithIdxEvent -= DidSelectCellWithIdxEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        public void Init()
        {
            /// Find preview player
            m_SongPreviewPlayer = UnityEngine.Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();

            /// Bind event
            TableViewInstance.didSelectCellWithIdxEvent -= DidSelectCellWithIdxEvent;
            TableViewInstance.didSelectCellWithIdxEvent += DidSelectCellWithIdxEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Index">Cell index</param>
        /// <returns></returns>
        public HMUI.TableCell CellForIdx(HMUI.TableView p_TableView, int p_Index)
        {
            LevelListTableCell l_Cell = GetTableCell();

            TextMeshProUGUI l_Text      = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText");
            TextMeshProUGUI l_SubText   = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songAuthorText");
            l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_favoritesBadgeImage").gameObject.SetActive(false);

            var l_HoverHint = l_Cell.gameObject.GetComponent<HMUI.HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HMUI.HoverHint>();
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                UnityEngine.GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            var l_SongEntry = Data[p_Index];
            l_SongEntry.Init();

            if ((l_SongEntry.BeatSaver_Map != null && !l_SongEntry.BeatSaver_Map.Partial) || l_SongEntry.CustomLevel != null)
            {
                var l_HaveSong  = l_SongEntry.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongEntry.CustomLevel.customLevelPath);
                var l_Scores    = Game.Levels.GetScoresByHash(l_SongEntry.GetLevelHash(), out var l_HaveAnyScore, out var l_HaveAllScores);

                string l_MapName        = "";
                string l_MapAuthor      = "";
                string l_MapSongAuthor  = "";
                float  l_Duration       = 0f;
                float  l_BPM            = 0f;

                if (l_HaveAnyScore && l_Scores.Count != 0)
                {
                    string l_HoverHintSuffix = "";
                    foreach (var l_Row in l_Scores)
                    {
                        l_HoverHintSuffix += $"\n{l_Row.Key.serializedName} ";
                        foreach (var l_SubRow in l_Row.Value)
                            l_HoverHintSuffix += (l_SubRow.Item2 != -1 ? "<color=green>✔</color> " : "<color=red>❌</color> ");
                    }

                    l_SongEntry.HoverHint += "\n" + l_HoverHintSuffix;
                }

                if (l_SongEntry.CustomLevel != null)
                {
                    l_MapName       = l_SongEntry.CustomLevel.songName;
                    l_MapAuthor     = l_SongEntry.CustomLevel.levelAuthorName;
                    l_MapSongAuthor = l_SongEntry.CustomLevel.songAuthorName;
                    l_BPM           = l_SongEntry.CustomLevel.beatsPerMinute;
                    l_Duration      = l_SongEntry.CustomLevel.songDuration;
                }
                else
                {
                    l_MapName       = l_SongEntry.BeatSaver_Map.name;
                    l_MapAuthor     = l_SongEntry.BeatSaver_Map.metadata.levelAuthorName;
                    l_MapSongAuthor = l_SongEntry.BeatSaver_Map.metadata.songAuthorName;
                    l_BPM           = l_SongEntry.BeatSaver_Map.metadata.bpm;
                    l_Duration      = l_SongEntry.BeatSaver_Map.metadata.duration;
                }

                var l_ColorPrefix = "<#FFFFFF>";
                if (l_HaveAllScores)
                    l_ColorPrefix = "<#52F700>";
                else if (l_HaveAnyScore)
                    l_ColorPrefix = "<#F8E600>";
                else if (l_HaveSong)
                    l_ColorPrefix = "<#7F7F7F>";

                string l_Title          = l_SongEntry.TitlePrefix + (l_SongEntry.TitlePrefix.Length != 0 ? " " : "") + l_ColorPrefix + l_MapName;
                string l_SubTitle       = l_MapSongAuthor + " [<size=85%>" + l_MapAuthor + "]";

                if (Regex.Replace(l_Title, "<.*?>", String.Empty).Length > (28 + l_ColorPrefix.Length))
                    l_Title = l_Title.Substring(0, 28 + l_ColorPrefix.Length) + "...";
                if (Regex.Replace(l_SubTitle, "<.*?>", String.Empty).Length > 35)
                    l_SubTitle = l_SubTitle.Substring(0, 35) + "...";

                /// Enable rich text support for the lower row text
                l_SubText.richText = true;

                l_Text.text     = l_Title;
                l_SubText.text  = l_SubTitle;

                var l_BPMText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText");
                l_BPMText.gameObject.SetActive(true);
                l_BPMText.text = ((int)l_BPM).ToString();

                var l_DurationText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText");
                l_DurationText.gameObject.SetActive(true);
                l_DurationText.text = l_Duration >= 0.0 ? $"{Math.Floor((double)l_Duration / 60):N0}:{Math.Floor((double)l_Duration % 60):00}" : "--";

                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(true);

                if (l_SongEntry.Cover != null)
                    l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                else if (CoverCache.TryGetValue(l_SongEntry.GetLevelHash(), out var l_Cover))
                {
                    l_SongEntry.Cover = l_Cover;
                    l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;

                    OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                }
                else
                {
                    l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;

                    if (l_HaveSong)
                    {
                        var l_CoverTask = l_SongEntry.CustomLevel.GetCoverImageAsync(CancellationToken.None);
                        _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                        {
                            if (l_Cell.idx >= Data.Count || l_SongEntry != Data[l_Cell.idx])
                                return;

                            Unity.MainThreadInvoker.Enqueue(() =>
                            {
                                /// Update infos
                                l_SongEntry.Cover = p_CoverTaskResult.Result;

                                /// Cache cover
                                if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                    CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                if (l_Cell.idx < Data.Count && l_SongEntry == Data[l_Cell.idx])
                                {
                                    l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                    l_Cell.RefreshVisuals();

                                    OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                                }
                            });
                        });
                    }
                    else if (l_SongEntry.BeatSaver_Map != null)
                    {
                        var l_CoverByte = Game.BeatMapsClient.GetCoverImageFromCacheByKey(l_SongEntry.BeatSaver_Map.id);
                        if (l_CoverByte != null && l_CoverByte.Length > 0)
                        {
                            var l_Texture = Unity.Texture2D.CreateFromRaw(l_CoverByte);
                            if (l_Texture != null)
                            {
                                l_SongEntry.Cover = UnityEngine.Sprite.Create(l_Texture, new UnityEngine.Rect(0, 0, l_Texture.width, l_Texture.height), new UnityEngine.Vector2(0.5f, 0.5f), 100);

                                /// Cache cover
                                if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                    CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                            }
                        }
                        else
                        {
                            /// Fetch cover
                            l_SongEntry.BeatSaver_Map.SelectMapVersion().CoverImageBytes((p_Valid, p_CoverTaskResult) =>
                            {
                                if (p_Valid)
                                    Game.BeatMapsClient.CacheCoverImage(l_SongEntry.BeatSaver_Map, p_CoverTaskResult);

                                if (l_Cell.idx >= Data.Count || l_SongEntry != Data[l_Cell.idx])
                                    return;

                                Unity.MainThreadInvoker.Enqueue(() =>
                                {
                                    var l_Texture = Unity.Texture2D.CreateFromRaw(p_CoverTaskResult);
                                    if (l_Texture != null)
                                    {
                                        l_SongEntry.Cover = UnityEngine.Sprite.Create(l_Texture, new UnityEngine.Rect(0, 0, l_Texture.width, l_Texture.height), new UnityEngine.Vector2(0.5f, 0.5f), 100);

                                        /// Cache cover
                                        if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                            CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                        if (l_Cell.idx < Data.Count && l_SongEntry == Data[l_Cell.idx])
                                        {
                                            l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                            l_Cell.RefreshVisuals();

                                            OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                                        }
                                    }
                                });
                            });
                        }
                    }
                }
            }
            else if (l_SongEntry.BeatSaver_Map != null && l_SongEntry.BeatSaver_Map.Partial)
            {
                l_Text.text     = "Loading from BeatSaver...";
                l_SubText.text  = "";

                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText").gameObject.SetActive(false);
                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText").gameObject.SetActive(false);
                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

                l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;
            }
            else
            {
                l_Text.text     = "<#FF0000>Invalid song";
                l_SubText.text  = l_SongEntry.CustomLevel != null ? l_SongEntry.CustomLevel.levelID.Replace("custom_level_", "") : "";

                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText").gameObject.SetActive(false);
                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText").gameObject.SetActive(false);
                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

                l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;
            }

            if (!string.IsNullOrEmpty(l_SongEntry.HoverHint))
            {
                var l_HoverHintText = l_SongEntry.HoverHint;
                if (l_SongEntry.HoverHintTimeArg.HasValue && l_HoverHintText.Contains("$$time$$"))
                {
                    var l_Replace = "";
                    var l_Elapsed = Misc.Time.UnixTimeNow() - Misc.Time.ToUnixTime(l_SongEntry.HoverHintTimeArg.Value);
                    if (l_Elapsed < (60 * 60))
                        l_Replace = Math.Max(1, l_Elapsed / 60).ToString() + " minute(s) ago";
                    else if (l_Elapsed < (60 * 60 * 24))
                        l_Replace = Math.Max(1, l_Elapsed / (60 * 60)).ToString() + " hour(s) ago";
                    else
                        l_Replace = Math.Max(1, l_Elapsed / (60 * 60 * 24)).ToString() + " day(s) ago";

                    l_HoverHintText = l_HoverHintText.Replace("$$time$$", l_Replace);
                }

                l_HoverHint.enabled = true;
                l_HoverHint.text    = l_HoverHintText;
            }
            else
                l_HoverHint.enabled = false;

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
        /// Stop preview music if any
        /// </summary>
        public void StopPreviewMusic()
        {
            if (m_SongPreviewPlayer != null && m_SongPreviewPlayer)
                m_SongPreviewPlayer.CrossfadeToDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a cell is selected
        /// </summary>
        /// <param name="p_Table">Table instance</param>
        /// <param name="p_Row">Row index</param>
        public void DidSelectCellWithIdxEvent(HMUI.TableView p_Table, int p_Row)
        {
            if (m_SongPreviewPlayer == null || !m_SongPreviewPlayer || !PlayPreviewAudio || p_Row > Data.Count)
                return;

            /// Fetch song entry
            var l_SongRowData = Data[p_Row];

            /// Hide if invalid song
            if (l_SongRowData == null || l_SongRowData.Invalid)
                return;

            if (l_SongRowData.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongRowData.CustomLevel.customLevelPath))
            {
                if (AudioClipCache.TryGetValue(l_SongRowData.GetLevelHash(), out var l_AudioClip))
                    m_SongPreviewPlayer.CrossfadeTo(l_AudioClip, PreviewAudioVolume, l_SongRowData.CustomLevel.previewStartTime, l_SongRowData.CustomLevel.previewDuration, () => { });
                else
                {
                    if (m_OldCoroutine != null)
                    {
                        StopCoroutine(m_OldCoroutine);
                        m_OldCoroutine = null;
                    }

                    m_OldCoroutine = StartCoroutine(LoadAudioClip(l_SongRowData, l_SongRowData.CustomLevel.songPreviewAudioClipPath));
                }
            }
            else
            {
                /// Stop preview music if any
                m_SongPreviewPlayer.CrossfadeToDefault();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get new table cell or reuse old one
        /// </summary>
        /// <returns></returns>
        private LevelListTableCell GetTableCell()
        {
            LevelListTableCell l_Cell = (LevelListTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_SongList_Cell");
            if (!l_Cell)
            {
                if (m_SongListTableCellInstance == null)
                    m_SongListTableCellInstance = UnityEngine.Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                l_Cell = Instantiate(m_SongListTableCellInstance);
            }

            l_Cell.SetField("_notOwned", false);
            l_Cell.reuseIdentifier = "BSP_SongList_Cell";

            if (m_DefaultCover == null)
                m_DefaultCover = l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite;

            return l_Cell;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        IEnumerator LoadAudioClip(Entry p_SongRowData, string p_Path)
        {
            /// Skip if it's not the menu
            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Menu)
            {
                m_OldCoroutine = null;
                yield break;
            }

            UnityWebRequest l_Song = UnityWebRequestMultimedia.GetAudioClip(p_Path, AudioType.OGGVORBIS);
            yield return l_Song.SendWebRequest();

            /// Skip if it's not the menu
            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Menu)
            {
                m_OldCoroutine = null;
                yield break;
            }

            try
            {
                ((DownloadHandlerAudioClip)l_Song.downloadHandler).streamAudio = true;

                var l_Clip = DownloadHandlerAudioClip.GetContent(l_Song);

                if (l_Clip != null)
                {
                    if (!AudioClipCache.ContainsKey(p_SongRowData.GetLevelHash()))
                        AudioClipCache.Add(p_SongRowData.GetLevelHash(), l_Clip);

                    m_SongPreviewPlayer.CrossfadeTo(l_Clip, PreviewAudioVolume, p_SongRowData.CustomLevel.previewStartTime, p_SongRowData.CustomLevel.previewDuration, () => { });

                    m_OldCoroutine = null;
                    yield break;
                }
            }
            catch (Exception)
            {
                m_OldCoroutine = null;
                yield break;
            }
        }
    }
}
