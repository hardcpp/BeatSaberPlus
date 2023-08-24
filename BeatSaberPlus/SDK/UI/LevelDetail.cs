using IPA.Utilities;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using CP_SDK.UI.Components;
using CP_SDK.UI;
using System.Reflection;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Song detail widget
    /// </summary>
    public class LevelDetail
    {
        /// <summary>
        /// Song detail view template
        /// </summary>
        private static GameObject m_SongDetailViewTemplate = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            if (!m_SongDetailViewTemplate)
            {
                m_SongDetailViewTemplate = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First(x => x.gameObject.name == "LevelDetail").gameObject);
                m_SongDetailViewTemplate.name = "BSP_SongDetailViewTemplate";

                GameObject.DestroyImmediate(m_SongDetailViewTemplate.GetComponent<StandardLevelDetailView>());
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private GameObject                                      m_GameObject;
        private TextMeshProUGUI                                 m_SongNameText;
        private TextMeshProUGUI                                 m_AuthorNameText;
        private HMUI.ImageView                                  m_SongCoverImage;
        private TextMeshProUGUI                                 m_SongTimeText;
        private TextMeshProUGUI                                 m_SongBPMText;
        private TextMeshProUGUI                                 m_SongNPSText;
        private TextMeshProUGUI                                 m_SongNJSText;
        private TextMeshProUGUI                                 m_SongOffsetText;
        private TextMeshProUGUI                                 m_SongNotesText;
        private TextMeshProUGUI                                 m_SongObstaclesText;
        private TextMeshProUGUI                                 m_SongBombsText;
        private BeatmapDifficultySegmentedControlController     m_DifficultiesSegmentedControllerClone;
        private BeatmapCharacteristicSegmentedControlController m_CharacteristicSegmentedControllerClone;
        private HMUI.TextSegmentedControl                       m_SongDiffSegmentedControl;
        private HMUI.IconSegmentedControl                       m_SongCharacteristicSegmentedControl;
        private CSecondaryButton                                m_SecondaryButton                           = null;
        private CPrimaryButton                                  m_PrimaryButton                             = null;
        private GameObject                                      m_FavoriteToggle                            = null;
        private CustomPreviewBeatmapLevel                       m_LocalBeatMap                              = null;
        private Game.BeatMaps.MapDetail                         m_BeatMap                                   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private double  m_Time = 0;
        private float   m_BPM = 0;
        private float   m_NPS = 0;
        private int     m_NJS = 0;
        private float   m_Offset = 0;
        private int     m_Notes = 0;
        private int     m_Obstacles = 0;
        private int     m_Bombs = 0;
        private string  m_Difficulty = "";

        private HMUI.IconSegmentedControl.DataItem m_Characteristic = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public Action OnSecondaryButton;
        public Action OnPrimaryButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string Name {
            get => m_SongNameText.text;
            set {
                var l_HTMLStripped = Regex.Replace(value, "<.*?>", String.Empty);

                if (l_HTMLStripped.Length < 30)
                    m_SongNameText.text = value;
                else
                    m_SongNameText.text = value.Substring(0, 27) + "...";
            }
        }
        public string AuthorNameText
        {
            get => m_AuthorNameText.text;
            set {
                var l_HTMLStripped = Regex.Replace(value, "<.*?>", String.Empty);

                if (l_HTMLStripped.Length < 32)
                    m_AuthorNameText.text = value;
                else
                    m_AuthorNameText.text = value.Substring(0, 32) + "...";
            }
        }
        public UnityEngine.Sprite Cover {
            get => m_SongCoverImage.sprite as UnityEngine.Sprite;
            set => m_SongCoverImage.sprite = value;
        }
        public double Time {
            get => m_Time;
            set {
                m_Time = value;
                m_SongTimeText.text = value >= 0.0 ? $"{Math.Floor(value / 60):N0}:{Math.Floor(value % 60):00}" : "--";
            }
        }
        public float BPM {
            get => m_BPM;
            set {
                m_BPM = value;
                m_SongBPMText.text = value.ToString("F0");
            }
        }
        public float NPS {
            get => m_NPS;
            set {
                m_NPS = value;
                m_SongNPSText.text = value >= 0f ? value.ToString("F2") : "--";
            }
        }
        public int NJS {
            get => m_NJS;
            set {
                m_NJS = value;
                m_SongNJSText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public float Offset {
            get => m_Offset;
            set {
                m_Offset = value;
                m_SongOffsetText.text = !float.IsNaN(value) ? value.ToString("F1") : "--";
            }
        }
        public int Notes {
            get => m_Notes;
            set {
                m_Notes = value;
                m_SongNotesText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public int Obstacles {
            get => m_Obstacles;
            set {
                m_Obstacles = value;
                m_SongObstaclesText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public int Bombs {
            get => m_Bombs;
            set {
                m_Bombs = value;
                m_SongBombsText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public HMUI.IconSegmentedControl.DataItem Characteristic {
            get => m_Characteristic;
            set {
                m_Characteristic = value;
                m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(new List<HMUI.IconSegmentedControl.DataItem>() {
                    value
                }.ToArray());
            }
        }
        public string Difficulty {
            get => m_Difficulty;
            set
            {
                m_Difficulty = value;
                m_SongDiffSegmentedControl.SetTexts(new string[] {
                    value
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public BeatmapCharacteristicSO          SelectedBeatmapCharacteristicSO = null;
        public BeatmapDifficulty                SelecteBeatmapDifficulty        = BeatmapDifficulty.Easy;
        public event Action<IDifficultyBeatmap> OnActiveDifficultyChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Parent"></param>
        public LevelDetail(UnityEngine.Transform p_Parent)
        {
            /// Close music preview details panel
            m_GameObject = UnityEngine.GameObject.Instantiate(m_SongDetailViewTemplate, p_Parent);

            var l_BSMLObjects     = m_GameObject.GetComponentsInChildren<RectTransform>().Where(x => x.gameObject.name.StartsWith("BSML"));
            var l_HoverHints      = m_GameObject.GetComponentsInChildren<HMUI.HoverHint>(true);
            var l_LocalHoverHints = m_GameObject.GetComponentsInChildren<LocalizedHoverHint>(true);

            foreach (var l_Current in l_BSMLObjects)        GameObject.Destroy(l_Current.gameObject);
            foreach (var l_Current in l_HoverHints)         GameObject.Destroy(l_Current);
            foreach (var l_Current in l_LocalHoverHints)    GameObject.Destroy(l_Current);

            /// Favorite toggle
            m_FavoriteToggle = m_GameObject.transform.Find("FavoriteToggle").gameObject;
            m_FavoriteToggle.SetActive(false);

            /// Find play buttons
            var l_ActionButtons     = m_GameObject.transform.Find("ActionButtons");
            var l_PracticeButton    = l_ActionButtons.Find("PracticeButton");
            var l_PlayButton        = l_ActionButtons.Find("ActionButton");

            /// Re-bind play button
            if (l_PlayButton.GetComponent<UnityEngine.UI.Button>())
            {
                var l_ActionButtonsRTransform = l_ActionButtons.transform as RectTransform;
                l_ActionButtonsRTransform.anchoredPosition = new Vector2(-0.5f, l_ActionButtonsRTransform.anchoredPosition.y);

                var l_ButtonsParent = l_PlayButton.transform.parent;
                GameObject.Destroy(l_PracticeButton.gameObject);
                GameObject.Destroy(l_PlayButton.gameObject);

                m_SecondaryButton = UISystem.SecondaryButtonFactory.Create("Secondary", l_ButtonsParent);
                m_SecondaryButton.SetText("Secondary");
                m_SecondaryButton.SetHeight(8f).SetWidth(30f);
                m_SecondaryButton.OnClick(OnSecondaryButtonClicked);

                m_PrimaryButton = UISystem.PrimaryButtonFactory.Create("Primary", l_ButtonsParent);
                m_PrimaryButton.SetText("Primary");
                m_PrimaryButton.SetHeight(8f).SetWidth(30f);
                m_PrimaryButton.OnClick(OnPrimaryButtonClicked);

                SetSecondaryButtonEnabled(false);
                SetSecondaryButtonText("?");
                SetPrimaryButtonEnabled(true);
                SetPrimaryButtonText("?");
            }

            m_CharacteristicSegmentedControllerClone    = m_GameObject.transform.Find("BeatmapCharacteristic").Find("BeatmapCharacteristicSegmentedControl").GetComponent<BeatmapCharacteristicSegmentedControlController>();
            m_SongCharacteristicSegmentedControl        = HMUIIconSegmentedControl.Create(m_CharacteristicSegmentedControllerClone.transform as RectTransform, true);

            m_DifficultiesSegmentedControllerClone  = m_GameObject.transform.Find("BeatmapDifficulty").GetComponentInChildren<BeatmapDifficultySegmentedControlController>();
            m_SongDiffSegmentedControl              = HMUITextSegmentedControl.Create(m_DifficultiesSegmentedControllerClone.transform as RectTransform, true);

            var l_LevelBarBig = m_GameObject.transform.Find("LevelBarBig");

            m_SongNameText      = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "SongNameText");
            m_AuthorNameText    = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "AuthorNameText");
            m_SongCoverImage    = l_LevelBarBig.Find("SongArtwork").GetComponent<HMUI.ImageView>();

            m_SongCoverImage.rectTransform.anchoredPosition = new Vector2( 2.000f, m_SongCoverImage.rectTransform.anchoredPosition.y);
            m_SongNameText.rectTransform.anchoredPosition   = new Vector2(-0.195f, m_SongNameText.rectTransform.anchoredPosition.y);
            m_AuthorNameText.richText = true;

            /// Disable multiline
            l_LevelBarBig.Find("MultipleLineTextContainer").gameObject.SetActive(false);

            var l_BeatmapParamsPanel = m_GameObject.transform.Find("BeatmapParamsPanel");
            l_BeatmapParamsPanel.transform.localPosition = l_BeatmapParamsPanel.transform.localPosition + (2 * Vector3.up);

            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>().childControlHeight=false;
            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();

            m_SongNPSText       = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NPS");
            m_SongNotesText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NotesCount");
            m_SongObstaclesText = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "ObstaclesCount");
            m_SongBombsText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "BombsCount");

            var l_SizeDelta = (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta;
            l_SizeDelta.y *= 2;

            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNotesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongObstaclesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongBombsText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            /// Patch
            var l_OffsetTexture = CP_SDK.Unity.Texture2DU.CreateFromRaw(CP_SDK.Misc.Resources.FromPath(Assembly.GetExecutingAssembly(), "BeatSaberPlus.SDK.UI.Resources.Offset.png"));
            var l_OffsetSprite = CP_SDK.Unity.SpriteU.CreateFromTextureWithBorders(l_OffsetTexture, 100f, Vector2.one * 16f);
            m_SongOffsetText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongOffsetText.transform.parent.SetAsFirstSibling();
            m_SongOffsetText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_OffsetSprite;

            var l_NJSTexture = CP_SDK.Unity.Texture2DU.CreateFromRaw(CP_SDK.Misc.Resources.FromPath(Assembly.GetExecutingAssembly(), "BeatSaberPlus.SDK.UI.Resources.NJS.png"));
            var l_NJSSprite = CP_SDK.Unity.SpriteU.CreateFromTextureWithBorders(l_NJSTexture, 100f, Vector2.one * 16f);
            m_SongNJSText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongNJSText.transform.parent.SetAsFirstSibling();
            m_SongNJSText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_NJSSprite;

            m_SongNPSText.transform.parent.SetAsFirstSibling();

            m_SongBPMText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongBPMText.transform.parent.SetAsFirstSibling();
            m_SongBPMText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "MetronomeIcon");

            m_SongTimeText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongTimeText.transform.parent.SetAsFirstSibling();
            m_SongTimeText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "ClockIcon");

            /// Bind events
            m_SongCharacteristicSegmentedControl.didSelectCellEvent += OnCharacteristicChanged;
            m_SongDiffSegmentedControl.didSelectCellEvent           += OnDifficultyChanged;

            try
            {
                foreach (var l_Text in m_GameObject.GetComponentsInChildren<TextMeshProUGUI>(true))
                    l_Text.fontStyle &= ~FontStyles.Italic;

                foreach (var l_Image in m_GameObject.GetComponentsInChildren<HMUI.ImageView>(true))
                {
                    m_SongCoverImage._skew = 0f;
                    m_SongCoverImage.SetAllDirty();
                }
            }
            catch (System.Exception)
            {

            }

            m_GameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the game object is active
        /// </summary>
        /// <param name="p_Active"></param>
        public void SetActive(bool p_Active)
        {
            m_GameObject.SetActive(p_Active);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from SongCore
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <param name="p_Characteristic">Game mode</param>
        /// <param name="p_DifficultyRaw">Difficulty raw</param>
        /// <param name="p_CharacteristicSO">Out SO characteristic</param>
        /// <returns></returns>
        public bool FromSongCore(CustomPreviewBeatmapLevel p_BeatMap, Sprite p_Cover, string p_Characteristic, string p_DifficultyRaw, out BeatmapCharacteristicSO p_CharacteristicSO)
        {
            p_CharacteristicSO = null;
            m_LocalBeatMap = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] Null Beatmap provided!");
                return false;
            }

            /// Select characteristic
            PreviewDifficultyBeatmapSet l_PreviewBeatmap = null;
            foreach (var l_Current in p_BeatMap.previewDifficultyBeatmapSets)
            {
                if (l_Current.beatmapCharacteristic.serializedName.ToLower() != p_Characteristic.ToLower())
                    continue;

                l_PreviewBeatmap   = l_Current;
                p_CharacteristicSO = l_Current.beatmapCharacteristic;
                break;
            }

            if (l_PreviewBeatmap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] No preview beatmap found!");
                return false;
            }

            /// Display mode
            Characteristic = new HMUI.IconSegmentedControl.DataItem(l_PreviewBeatmap.beatmapCharacteristic.icon, Polyglot.Localization.Get(l_PreviewBeatmap.beatmapCharacteristic.descriptionLocalizationKey));

            /// Select difficulty
            BeatmapDataBasicInfo l_SelectedDifficulty = null;
            var l_Difficulties      = p_BeatMap.standardLevelInfoSaveData.difficultyBeatmapSets.Where(x => x.beatmapCharacteristicName.ToLower() == p_Characteristic.ToLower()).SingleOrDefault();
            var l_DifficultyBeatMap = null as StandardLevelInfoSaveData.DifficultyBeatmap;

            if (l_Difficulties != null)
            {
                foreach (var l_Current in l_Difficulties.difficultyBeatmaps)
                {
                    if (l_Current.difficulty.ToLower() != p_DifficultyRaw.ToLower())
                        continue;

                    l_DifficultyBeatMap = l_Current;

                    string              l_DifficultyPath = p_BeatMap.customLevelPath + "\\" + l_Current.beatmapFilename;
                    BeatmapDataLoader   l_Loader         = new BeatmapDataLoader();

                    try
                    {
                        var l_StandartLevelInformation = p_BeatMap.standardLevelInfoSaveData;
                        var l_JSON = System.IO.File.ReadAllText(l_DifficultyPath);

                        var l_BeatmapSaveData   = BeatmapSaveDataVersion3.BeatmapSaveData.DeserializeFromJSONString(l_JSON);
                        var l_Info              = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(l_BeatmapSaveData);
                        if (l_Info != null)
                        {
                            l_SelectedDifficulty = l_Info;
                            break;
                        }
                    }
                    catch (Exception p_Exception)
                    {
                        CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] Error:");
                        CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
                    }
                }
            }

            if (l_SelectedDifficulty == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] No valid difficulty found!");
                return false;
            }

            /// Display difficulty
            Difficulty = Game.Levels.SerializedToDifficultyName(p_DifficultyRaw);

            Name            = p_BeatMap.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            Time            = p_BeatMap.songDuration;
            BPM             = p_BeatMap.standardLevelInfoSaveData.beatsPerMinute;
            NPS             = ((float)l_SelectedDifficulty.cuttableNotesCount / (float)p_BeatMap.songDuration);
            NJS             = (int)l_DifficultyBeatMap.noteJumpMovementSpeed;
            Offset          = l_DifficultyBeatMap.noteJumpStartBeatOffset;
            Notes           = l_SelectedDifficulty.cuttableNotesCount;
            Obstacles       = l_SelectedDifficulty.obstaclesCount;
            Bombs           = l_SelectedDifficulty.bombsCount;

            return true;
        }
        /// <summary>
        /// Set from SongCore
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <param name="p_Characteristic">Game mode</param>
        /// <param name="p_Difficulty">Difficulty</param>
        /// <returns></returns>
        public bool FromSongCore(IBeatmapLevel p_BeatMap, Sprite p_Cover, BeatmapCharacteristicSO p_Characteristic, BeatmapDifficulty p_Difficulty)
        {
            m_LocalBeatMap      = null;
            m_BeatMap           = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] Null Beatmap provided!");
                return false;
            }

            /// Display mode
            Characteristic = new HMUI.IconSegmentedControl.DataItem(p_Characteristic.icon, Polyglot.Localization.Get(p_Characteristic.descriptionLocalizationKey));

            var l_IDifficultyBeatmap = p_BeatMap.beatmapLevelData.GetDifficultyBeatmap(p_Characteristic, p_Difficulty);

            /// Display difficulty
            Difficulty = Game.Levels.SerializedToDifficultyName(p_Difficulty.ToString());

            Name            = p_BeatMap.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            Time            = p_BeatMap.songDuration;
            BPM             = p_BeatMap.beatsPerMinute;
            NJS             = (int)l_IDifficultyBeatmap.noteJumpMovementSpeed;
            Offset          = l_IDifficultyBeatmap.noteJumpStartBeatOffset;

            if (l_IDifficultyBeatmap is BeatmapLevelSO.DifficultyBeatmap l_DifficultyBeatmap)
            {
                try
                {
                    /* var l_Task = l_DifficultyBeatmap.GetBeatmapDataBasicInfoAsync();
                    l_Task.ConfigureAwait(false);
                    l_Task.Wait();
                   var l_Info = l_Task.Result;
                    l_DifficultyBeatmap.beatmapLevelData.
                    NPS         = ((float)l_Info.cuttableNotesCount / (float)p_BeatMap.beatsPerMinute);
                    Notes       = l_Info.cuttableNotesCount;
                    Obstacles   = l_Info.obstaclesCount;
                    Bombs       = l_Info.bombsCount;*/
                }
                catch
                {
                    NPS         = -1;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                }
            }
            else if (l_IDifficultyBeatmap is CustomDifficultyBeatmap l_CustomDifficultyBeatmap)
            {
                try
                {
                    NPS         = ((float)l_CustomDifficultyBeatmap.beatmapDataBasicInfo.cuttableNotesCount / (float)p_BeatMap.songDuration);
                    Notes       = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.cuttableNotesCount;
                    Obstacles   = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.obstaclesCount;
                    Bombs       = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.bombsCount;
                }
                catch
                {
                    NPS         = -1;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                }
            }

            return true;
        }
        /// <summary>
        /// Set from SongCore
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <returns></returns>
        public bool FromSongCore(CustomPreviewBeatmapLevel p_BeatMap, Sprite p_Cover)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore] Null Beatmap provided!");
                return false;
            }

            /// Display modes
            var l_Characteristics = new List<HMUI.IconSegmentedControl.DataItem>();
            foreach (var l_Current in p_BeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct())
                l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_Current.icon, Polyglot.Localization.Get(l_Current.descriptionLocalizationKey)));

            if (l_Characteristics.Count == 0)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromSongCore2] No valid characteristics found for map \"{p_BeatMap.levelID}\"!");
                return false;
            }

            /// Store beatmap
            m_LocalBeatMap = p_BeatMap;

            m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(l_Characteristics.ToArray());
            m_SongCharacteristicSegmentedControl.SelectCellWithNumber(0);
            OnCharacteristicChanged(null, 0);

            /// Display informations
            Name            = p_BeatMap.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            BPM             = p_BeatMap.standardLevelInfoSaveData.beatsPerMinute;

            return true;
        }
        /// <summary>
        /// Set from BeatSaver
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <param name="p_Characteristic">Game mode</param>
        /// <param name="p_DifficultyRaw">Difficulty raw</param>
        /// <param name="p_CharacteristicSO">Out SO characteristic</param>
        /// <returns></returns>
        public bool FromBeatSaver(Game.BeatMaps.MapDetail p_BeatMap, Sprite p_Cover, string p_Characteristic, string p_DifficultyRaw, out BeatmapCharacteristicSO p_CharacteristicSO)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;
            p_CharacteristicSO = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver1] Null Beatmap provided!");
                return false;
            }

            var l_Version = p_BeatMap.SelectMapVersion();
            if (l_Version == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver1] Null version provided!");
                return false;
            }

            var l_Difficulties = l_Version.GetDifficultiesPerCharacteristic(Game.Levels.SanitizeCharacteristic(p_Characteristic).ToLower());
            if (l_Difficulties.Count == 0)
                return false;

            /// Display mode
            BeatmapCharacteristicSO l_CharacteristicDetails = Game.Levels.GetCharacteristicSOBySerializedName(p_Characteristic);

            if (l_CharacteristicDetails == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver1] Characteristic \"{p_Characteristic}\" not found in song core");
                return false;
            }

            Characteristic      = new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey));
            p_CharacteristicSO  = l_CharacteristicDetails;

            /// Select difficulty
            Game.BeatMaps.MapDifficulty l_SelectedDifficulty = null;

            foreach (var l_Current in l_Difficulties)
            {
                if (l_Current.difficulty.ToLower() != p_DifficultyRaw.ToLower())
                    continue;

                l_SelectedDifficulty    = l_Current;
                break;
            }

            if (l_SelectedDifficulty == null)
                return false;

            /// Display difficulty
            Difficulty = Game.Levels.SerializedToDifficultyName(p_DifficultyRaw);

            /// Display informations
            Name            = p_BeatMap.metadata.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.metadata.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            Time            = (double)p_BeatMap.metadata.duration;
            BPM             = p_BeatMap.metadata.bpm;
            NPS             = l_SelectedDifficulty.nps;
            NJS             = (int)l_SelectedDifficulty.njs;
            Offset          = l_SelectedDifficulty.offset;
            Notes           = l_SelectedDifficulty.notes;
            Obstacles       = l_SelectedDifficulty.obstacles;
            Bombs           = l_SelectedDifficulty.bombs;

            return true;
        }
        /// <summary>
        /// Set from BeatSaver
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <returns></returns>
        public bool FromBeatSaver(Game.BeatMaps.MapDetail p_BeatMap, Sprite p_Cover)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Null Beatmap provided!");
                return false;
            }

            var l_Version = p_BeatMap.SelectMapVersion();
            if (l_Version == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Invalid version!");
                return false;
            }

            /// Display modes
            var l_Characteristics = new List<HMUI.IconSegmentedControl.DataItem>();
            foreach (var l_Current in l_Version.GetCharacteristicsInOrder())
            {
                var l_SerializedName = Game.Levels.SanitizeCharacteristic(l_Current);

                if (string.IsNullOrEmpty(l_SerializedName))
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Invalid characteristic \"{l_Current}\"!");
                    return false;
                }

                BeatmapCharacteristicSO l_CharacteristicDetails = Game.Levels.GetCharacteristicSOBySerializedName(l_SerializedName);

                if (l_CharacteristicDetails != null)
                    l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey)));
            }

            if (l_Characteristics.Count == 0)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] No valid characteristics found for map \"{p_BeatMap.id}\"!");
                return false;
            }

            /// Store beatmap
            m_BeatMap = p_BeatMap;

            m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(l_Characteristics.ToArray());
            m_SongCharacteristicSegmentedControl.SelectCellWithNumber(0);
            OnCharacteristicChanged(null, 0);

            /// Display informations
            Name            = p_BeatMap.metadata.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.metadata.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            BPM             = p_BeatMap.metadata.bpm;

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set favorite toggle enabled
        /// </summary>
        /// <param name="p_Value"></param>
        public void SetFavoriteToggleEnabled(bool p_Value)
        {
            m_FavoriteToggle.SetActive(p_Value);
        }
        /// <summary>
        /// Set favorite toggle images
        /// </summary>
        /// <param name="p_Default">Default image</param>
        /// <param name="p_Enabled">Enable image</param>
        public void SetFavoriteToggleImage(Sprite p_Default, Sprite p_Enabled)
        {
            var l_IVDefault = m_FavoriteToggle.transform.GetChild(0).GetComponent<HMUI.ImageView>();
            var l_IVMarked  = m_FavoriteToggle.transform.GetChild(1).GetComponent<HMUI.ImageView>();

            l_IVDefault.sprite  = p_Default;
            l_IVMarked.sprite   = p_Enabled;
        }
        /// <summary>
        /// Set favorite toggle hover hint
        /// </summary>
        /// <param name="p_Hint">New hint</param>
        public void SetFavoriteToggleHoverHint(string p_Hint)
        {
            var l_HoverHint = m_FavoriteToggle.GetComponent<HMUI.HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = m_FavoriteToggle.AddComponent<HMUI.HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            l_HoverHint.text = p_Hint;
        }
        /// <summary>
        /// Set favorite toggle value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetFavoriteToggleValue(bool p_Value)
        {
            m_FavoriteToggle.GetComponent<HMUI.ToggleWithCallbacks>().isOn = p_Value;
        }
        /// <summary>
        /// Set favorite toggle callback
        /// </summary>
        /// <param name="p_Action">New callback</param>
        public void SetFavoriteToggleCallback(Action<HMUI.ToggleWithCallbacks.SelectionState> p_Action)
        {
            m_FavoriteToggle.GetComponent<HMUI.ToggleWithCallbacks>().stateDidChangeEvent += p_Action;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reverse button order
        /// </summary>
        public void ReverseButtonsOrder()
        {
            m_SecondaryButton.transform.SetAsLastSibling();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetSecondaryButtonEnabled(bool p_Value)
        {
            m_SecondaryButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonEnabled(bool p_Value)
        {
            m_PrimaryButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonInteractable(bool p_Value)
        {
            m_SecondaryButton.SetInteractable(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonInteractable(bool p_Value)
        {
            m_PrimaryButton.SetInteractable(p_Value);
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetSecondaryButtonText(string p_Value)
        {
            m_SecondaryButton.SetText(p_Value);
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonText(string p_Value)
        {
            m_PrimaryButton.SetText(p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the characteristic changed
        /// </summary>
        /// <param name="p_SegmentControl">Control instance</param>
        /// <param name="p_Index">New selected index</param>
        private void OnCharacteristicChanged(HMUI.SegmentedControl p_SegmentControl, int p_Index)
        {
            if (m_LocalBeatMap != null)
            {
                var l_Characs = m_LocalBeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct();

                if (p_Index > l_Characs.Count())
                    return;

                SelectedBeatmapCharacteristicSO = l_Characs.ElementAt(p_Index);

                List<string> l_Difficulties = m_LocalBeatMap.previewDifficultyBeatmapSets
                    .Where(x => x.beatmapCharacteristic == SelectedBeatmapCharacteristicSO)
                    .FirstOrDefault().beatmapDifficulties.Select(x => x.SerializedName()).ToList();

                m_SongDiffSegmentedControl.SetTexts(l_Difficulties.ToArray());
                m_SongDiffSegmentedControl.SelectCellWithNumber(l_Difficulties.Count - 1);
                OnDifficultyChanged(null, l_Difficulties.Count - 1);
            }
            else if (m_BeatMap != null)
            {
                var l_Version = m_BeatMap.SelectMapVersion();
                var l_Characs = l_Version.GetCharacteristicsInOrder();

                if (p_Index > l_Characs.Count)
                    return;

                SelectedBeatmapCharacteristicSO = Game.Levels.GetCharacteristicSOBySerializedName(Game.Levels.SanitizeCharacteristic(l_Characs[p_Index]));

                List<string> l_Difficulties = new List<string>();
                foreach (var l_Current in l_Version.GetDifficultiesPerCharacteristic(l_Characs[p_Index]))
                    l_Difficulties.Add(Game.Levels.SerializedToDifficultyName(l_Current.difficulty.ToString()));

                m_SongDiffSegmentedControl.SetTexts(l_Difficulties.ToArray());
                m_SongDiffSegmentedControl.SelectCellWithNumber(l_Difficulties.Count - 1);
                OnDifficultyChanged(null, l_Difficulties.Count - 1);
            }
        }
        /// <summary>
        /// When the difficulty is changed
        /// </summary>
        /// <param name="p_SegmentControl">Control instance</param>
        /// <param name="p_Index">New selected index</param>
        private void OnDifficultyChanged(HMUI.SegmentedControl p_SegmentControl, int p_Index)
        {
            if (m_LocalBeatMap != null)
            {
                var l_Characs = m_LocalBeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct();

                if (m_SongCharacteristicSegmentedControl.selectedCellNumber > l_Characs.Count())
                    return;

                var l_Difficulties = m_LocalBeatMap.standardLevelInfoSaveData.difficultyBeatmapSets
                    .Where(x => x.beatmapCharacteristicName == SelectedBeatmapCharacteristicSO.serializedName)
                    .SingleOrDefault();
                if (p_Index < 0 || p_Index >= l_Difficulties.difficultyBeatmaps.Length)
                {
                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }

                var l_DifficultyBeatMap = l_Difficulties.difficultyBeatmaps.ElementAt(p_Index);
                var l_DifficultyPath    = m_LocalBeatMap.customLevelPath + "\\" + l_DifficultyBeatMap.beatmapFilename;
                var l_Loader            = new BeatmapDataLoader();

                try
                {
                    var l_JSON = System.IO.File.ReadAllText(l_DifficultyPath);

                    var l_BeatmapSaveData   = BeatmapSaveDataVersion3.BeatmapSaveData.DeserializeFromJSONString(l_JSON);
                    var l_Info              = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(l_BeatmapSaveData);
                    if (l_Info != null)
                    {
                        Time            = m_LocalBeatMap.songDuration;
                        NPS             = ((float)l_Info.cuttableNotesCount / (float)m_LocalBeatMap.songDuration);
                        NJS             = (int)l_DifficultyBeatMap.noteJumpMovementSpeed;
                        Offset          = l_DifficultyBeatMap.noteJumpStartBeatOffset;
                        Notes           = l_Info.cuttableNotesCount;
                        Obstacles       = l_Info.obstaclesCount;
                        Bombs           = l_Info.bombsCount;

                        if (OnActiveDifficultyChanged != null)
                            OnActiveDifficultyChanged.Invoke(GetIDifficultyBeatMap());
                    }
                }
                catch (Exception p_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[SDK.UI][LevelDetail.OnDifficultyChanged] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);

                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }
            }
            else if (m_BeatMap != null)
            {
                var l_Version = m_BeatMap.SelectMapVersion();
                var l_Characs = l_Version.GetCharacteristicsInOrder();

                if (m_SongCharacteristicSegmentedControl.selectedCellNumber > l_Characs.Count)
                    return;

                var l_Difficulties = l_Version.GetDifficultiesPerCharacteristic(l_Characs[m_SongCharacteristicSegmentedControl.selectedCellNumber]);
                if (p_Index < 0 || p_Index >= l_Difficulties.Count)
                {
                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }

                var l_SelectedBeatmapCharacteristicDifficulty   = l_Difficulties.ElementAt(p_Index);
                SelecteBeatmapDifficulty                        = Game.Levels.SerializedToDifficulty(l_SelectedBeatmapCharacteristicDifficulty.difficulty);

                /// Display informations
                Time        = (double)m_BeatMap.metadata.duration;
                NPS         = (float)l_SelectedBeatmapCharacteristicDifficulty.nps;
                NJS         = (int)l_SelectedBeatmapCharacteristicDifficulty.njs;
                Offset      = l_SelectedBeatmapCharacteristicDifficulty.offset;
                Notes       = l_SelectedBeatmapCharacteristicDifficulty.notes;
                Obstacles   = l_SelectedBeatmapCharacteristicDifficulty.obstacles;
                Bombs       = l_SelectedBeatmapCharacteristicDifficulty.bombs;

                if (OnActiveDifficultyChanged != null)
                    OnActiveDifficultyChanged.Invoke(GetIDifficultyBeatMap());
            }
        }
        /// <summary>
        /// Secondary button on click
        /// </summary>
        private void OnSecondaryButtonClicked()
            => OnSecondaryButton?.Invoke();
        /// <summary>
        /// Primary button on click
        /// </summary>
        private void OnPrimaryButtonClicked()
            => OnPrimaryButton?.Invoke();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get IDifficultyBeatmap
        /// </summary>
        /// <returns></returns>
        private IDifficultyBeatmap GetIDifficultyBeatMap()
        {
            //if (m_BeatMap == null)
                return null;
            /*
            var l_CharacIndex = m_SongCharacteristicSegmentedControl.selectedCellNumber;
            if (l_CharacIndex >= m_BeatMap.Metadata.Characteristics.Count)
                return null;

            var l_LocalSong = SongCore.Loader.GetLevelByHash(m_BeatMap.Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                IBeatmapLevel l_Level = null;

                var task = Task.Run(async () => { await Game.Level.LoadSong(l_LocalSong.levelID, (x) => l_Level = x); });
                task.Wait();

                return l_Level.beatmapLevelData.GetDifficultyBeatmap(SelectedBeatmapCharacteristicSO, SelecteBeatmapDifficulty);
            }
            else
            {
                var l_BSBeatmapLevel            = Game.BeatSaver.CreateFakeCustomBeatmapLevelFromBeatMap(m_BeatMap);
                var l_BSIDifficultyBeatmapSet   = l_BSBeatmapLevel.beatmapLevelData.difficultyBeatmapSets.Where(x => x.beatmapCharacteristic == SelectedBeatmapCharacteristicSO).FirstOrDefault();

                return l_BSIDifficultyBeatmapSet.difficultyBeatmaps.Where(x => x.difficulty == SelecteBeatmapDifficulty).FirstOrDefault();
            }*/
        }
    }
}
