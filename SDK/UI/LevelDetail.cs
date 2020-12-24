using BeatSaverSharp;
using IPA.Utilities;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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
        private static UnityEngine.GameObject m_SongDetailViewTemplate = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            if (m_SongDetailViewTemplate == null)
                m_SongDetailViewTemplate = UnityEngine.GameObject.Instantiate(UnityEngine.Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First(x => x.gameObject.name == "LevelDetail").gameObject);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private GameObject m_GameObject;
        private TextMeshProUGUI m_SongNameText;
        private TextMeshProUGUI m_AuthorNameText;
        private HMUI.ImageView m_SongCoverImage;
        private TextMeshProUGUI m_SongTimeText;
        private TextMeshProUGUI m_SongBPMText;
        private TextMeshProUGUI m_SongNPSText;
        private TextMeshProUGUI m_SongNJSText;
        private TextMeshProUGUI m_SongOffsetText;
        private TextMeshProUGUI m_SongNotesText;
        private TextMeshProUGUI m_SongObstaclesText;
        private TextMeshProUGUI m_SongBombsText;
        private BeatmapDifficultySegmentedControlController m_DifficultiesSegmentedControllerClone;
        private BeatmapCharacteristicSegmentedControlController m_CharacteristicSegmentedControllerClone;
        private HMUI.TextSegmentedControl m_SongDiffSegmentedControl;
        private HMUI.IconSegmentedControl m_SongCharacteristicSegmentedControl;
        private UnityEngine.UI.Button m_PracticeButton = null;
        private UnityEngine.UI.Button m_PlayButton = null;
        private UnityEngine.GameObject m_FavoriteToggle = null;
        private Beatmap m_BeatMap = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private double m_Time = 0;
        private float m_BPM = 0;
        private float m_NPS = 0;
        private int m_NJS = 0;
        private float m_Offset = 0;
        private int m_Notes = 0;
        private int m_Obstacles = 0;
        private int m_Bombs = 0;
        private HMUI.IconSegmentedControl.DataItem m_Characteristic = null;
        private string m_Difficulty = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string Name {
            get => m_SongNameText.text;
            set {
                m_SongNameText.text = value;
            }
        }
        public string AuthorNameText
        {
            get => m_AuthorNameText.text;
            set
            {
                m_AuthorNameText.text = value;
            }
        }
        public UnityEngine.Sprite Cover {
            get => m_SongCoverImage.sprite as UnityEngine.Sprite;
            set {
                m_SongCoverImage.sprite = value;
            }
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
                m_SongNotesText.text = value.ToString();
            }
        }
        public int Obstacles {
            get => m_Obstacles;
            set {
                m_Obstacles = value;
                m_SongObstaclesText.text = value.ToString();
            }
        }
        public int Bombs {
            get => m_Bombs;
            set {
                m_Bombs = value;
                m_SongBombsText.text = value.ToString();
            }
        }
        public HMUI.IconSegmentedControl.DataItem Characteristic {
            get => m_Characteristic;
            set {
                m_Characteristic = value;
                m_SongCharacteristicSegmentedControl.SetData(new List<HMUI.IconSegmentedControl.DataItem>() {
                    value
                }.ToArray());
            }
        }
        public string Difficulty
        {
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

        public BeatmapCharacteristicSO SelectedBeatmapCharacteristicSO = null;
        public BeatmapDifficulty SelecteBeatmapDifficulty = BeatmapDifficulty.Easy;
        public BeatmapCharacteristicDifficulty SelectedBeatmapCharacteristicDifficulty = default;
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

            /// Clean unused elements
            UnityEngine.GameObject.Destroy(m_GameObject.GetComponent<StandardLevelDetailView>());

            var l_BSMLObjects     = m_GameObject.GetComponentsInChildren<UnityEngine.RectTransform>().Where(x => x.gameObject.name.StartsWith("BSML"));
            var l_HoverHints      = m_GameObject.GetComponentsInChildren<HMUI.HoverHint>();
            var l_LocalHoverHints = m_GameObject.GetComponentsInChildren<LocalizedHoverHint>();

            foreach (var l_Current in l_BSMLObjects)        UnityEngine.GameObject.Destroy(l_Current.gameObject);
            foreach (var l_Current in l_HoverHints)         UnityEngine.GameObject.Destroy(l_Current);
            foreach (var l_Current in l_LocalHoverHints)    UnityEngine.GameObject.Destroy(l_Current);

            /// Favorite toggle
            m_FavoriteToggle = m_GameObject.transform.Find("FavoriteToggle").gameObject;
            m_FavoriteToggle.SetActive(false);

            /// Find play buttons
            var l_PracticeButton = m_GameObject.transform.Find("ActionButtons").Find("PracticeButton");
            var l_PlayButton     = m_GameObject.transform.Find("ActionButtons").Find("ActionButton");

            /// Re-bind play button
            if (l_PlayButton.GetComponent<UnityEngine.UI.Button>())
            {
                m_PracticeButton = l_PracticeButton.GetComponent<UnityEngine.UI.Button>();
                m_PracticeButton.onClick.RemoveAllListeners();

                m_PlayButton = l_PlayButton.GetComponent<UnityEngine.UI.Button>();
                m_PlayButton.onClick.RemoveAllListeners();

                UnityEngine.GameObject.Destroy(m_PracticeButton.GetComponentInChildren<LocalizedTextMeshProUGUI>());
                UnityEngine.GameObject.Destroy(m_PlayButton.GetComponentInChildren<LocalizedTextMeshProUGUI>());

                SetPracticeButtonEnabled(false);
                SetPracticeButtonText("?");
                SetPlayButtonEnabled(true);
                SetPlayButtonText("?");
            }

            m_CharacteristicSegmentedControllerClone    = m_GameObject.transform.Find("BeatmapCharacteristic").Find("BeatmapCharacteristicSegmentedControl").GetComponent<BeatmapCharacteristicSegmentedControlController>();
            m_SongCharacteristicSegmentedControl        = HorizontalIconSegmentedControl.Create(m_CharacteristicSegmentedControllerClone.transform as UnityEngine.RectTransform, true);

            m_DifficultiesSegmentedControllerClone  = m_GameObject.transform.Find("BeatmapDifficulty").GetComponentInChildren<BeatmapDifficultySegmentedControlController>();
            m_SongDiffSegmentedControl              = TextSegmentedControl.Create(m_DifficultiesSegmentedControllerClone.transform as UnityEngine.RectTransform, true);

            var l_LevelBarBig = m_GameObject.transform.Find("LevelBarBig");

            m_SongNameText      = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "SongNameText");
            m_AuthorNameText    = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "AuthorNameText");
            m_AuthorNameText.richText = true;
            m_SongCoverImage    = l_LevelBarBig.Find("SongArtwork").GetComponent<HMUI.ImageView>();

            var l_BeatmapParamsPanel = m_GameObject.transform.Find("BeatmapParamsPanel");
            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>().childControlHeight=false;
            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();

            m_SongNPSText       = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NPS");
            m_SongNotesText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NotesCount");
            m_SongObstaclesText = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "ObstaclesCount");
            m_SongBombsText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "BombsCount");

            var l_SizeDelta = (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta;
            l_SizeDelta.y *= 2;

            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new UnityEngine.RectOffset(0, 0, 0, 3);
            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new UnityEngine.RectOffset(0, 0, 0, 3);
            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNotesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new UnityEngine.RectOffset(0, 0, 0, 3);
            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongObstaclesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new UnityEngine.RectOffset(0, 0, 0, 3);
            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongBombsText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            /// Patch
            var l_OffsetTexture = BeatSaberMarkupLanguage.Utilities.FindTextureInAssembly("BeatSaberPlus.SDK.UI.Resources.Offset.png");
            var l_OffsetSprite = SDK.Unity.Sprite.CreateFromTexture(l_OffsetTexture, 100f, UnityEngine.Vector2.one * 16f);
            m_SongOffsetText = UnityEngine.GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongOffsetText.transform.parent.SetAsFirstSibling();
            m_SongOffsetText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_OffsetSprite;

            var l_NJSTexture = BeatSaberMarkupLanguage.Utilities.FindTextureInAssembly("BeatSaberPlus.SDK.UI.Resources.NJS.png");
            var l_NJSSprite = SDK.Unity.Sprite.CreateFromTexture(l_NJSTexture, 100f, UnityEngine.Vector2.one * 16f);
            m_SongNJSText = UnityEngine.GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongNJSText.transform.parent.SetAsFirstSibling();
            m_SongNJSText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_NJSSprite;

            m_SongNPSText.transform.parent.SetAsFirstSibling();

            m_SongBPMText = UnityEngine.GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongBPMText.transform.parent.SetAsFirstSibling();
            m_SongBPMText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Sprite>().First(x => x.name == "MetronomeIcon");

            m_SongTimeText = UnityEngine.GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongTimeText.transform.parent.SetAsFirstSibling();
            m_SongTimeText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Sprite>().First(x => x.name == "ClockIcon");

            /// Bind events
            m_SongCharacteristicSegmentedControl.didSelectCellEvent += OnCharacteristicChanged;
            m_SongDiffSegmentedControl.didSelectCellEvent           += OnDifficultyChanged;
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
        public bool FromSongCore(CustomPreviewBeatmapLevel p_BeatMap, UnityEngine.Sprite p_Cover, string p_Characteristic, string p_DifficultyRaw, out BeatmapCharacteristicSO p_CharacteristicSO)
        {
            p_CharacteristicSO = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromSongCore] Null Beatmap provided!");
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
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromSongCore] No preview beatmap found!");
                return false;
            }

            /// Display mode
            Characteristic = new HMUI.IconSegmentedControl.DataItem(l_PreviewBeatmap.beatmapCharacteristic.icon, Polyglot.Localization.Get(l_PreviewBeatmap.beatmapCharacteristic.descriptionLocalizationKey));

            /// Select difficulty
            BeatmapData l_SelectedDifficulty = null;
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
                        var l_Info = l_Loader.GetBeatmapDataFromJson(l_JSON, l_StandartLevelInformation.beatsPerMinute, l_StandartLevelInformation.shuffle, l_StandartLevelInformation.shufflePeriod);

                        if (l_Info != null)
                        {
                            l_SelectedDifficulty = l_Info;
                            break;
                        }
                    }
                    catch (System.Exception p_Exception)
                    {
                        Logger.Instance?.Error(p_Exception);
                    }
                }
            }

            if (l_SelectedDifficulty == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromSongCore] No valid difficulty found!");
                return false;
            }

            /// Display difficulty
            Difficulty = Game.Level.SerializedToDifficultyName(p_DifficultyRaw);

            Name            = p_BeatMap.songName;
            AuthorNameText  = "Mapped by <b><u><i>" + p_BeatMap.levelAuthorName + "</b></u></i>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            Time            = p_BeatMap.songDuration;
            BPM             = p_BeatMap.standardLevelInfoSaveData.beatsPerMinute;
            NPS             = ((float)l_SelectedDifficulty.cuttableNotesType / (float)p_BeatMap.standardLevelInfoSaveData.beatsPerMinute);
            NJS             = (int)l_DifficultyBeatMap.noteJumpMovementSpeed;
            Offset          = l_DifficultyBeatMap.noteJumpStartBeatOffset;
            Notes           = l_SelectedDifficulty.cuttableNotesType;
            Obstacles       = l_SelectedDifficulty.obstaclesCount;
            Bombs           = l_SelectedDifficulty.bombsCount;

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
        public bool FromBeatSaver(Beatmap p_BeatMap, UnityEngine.Sprite p_Cover, string p_Characteristic, string p_DifficultyRaw, out BeatmapCharacteristicSO p_CharacteristicSO)
        {
            m_BeatMap = null;
            p_CharacteristicSO = null;

            if (p_BeatMap == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver1] Null Beatmap provided!");
                return false;
            }

            /// Select characteristic
            BeatmapCharacteristic l_SelectedCharacteristic = default;
            bool l_CharacteristicFound = false;

            foreach (var l_Current in p_BeatMap.Metadata.Characteristics)
            {
                if (l_Current.Name.ToLower() != Game.BeatSaver.SanitizeGameMode(p_Characteristic).ToLower())
                    continue;

                l_SelectedCharacteristic    = l_Current;
                l_CharacteristicFound       = true;
                break;
            }

            if (l_CharacteristicFound == default)
                return false;

            /// Display mode
            BeatmapCharacteristicSO l_CharacteristicDetails = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_SelectedCharacteristic.Name);

            if (l_CharacteristicDetails == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver1] Characteristic \"{l_SelectedCharacteristic.Name}\" not found in song core");
                return false;
            }

            Characteristic      = new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey));
            p_CharacteristicSO  = l_CharacteristicDetails;

            /// Select difficulty
            BeatmapCharacteristicDifficulty l_SelectedDifficulty = default;
            bool l_DifficultyFound = false;

            foreach (var l_Current in l_SelectedCharacteristic.Difficulties.Where(x => l_SelectedCharacteristic.Difficulties[x.Key].HasValue).ToList())
            {
                if (!l_Current.Value.HasValue || l_Current.Key.ToLower() != p_DifficultyRaw.ToLower())
                    continue;

                l_SelectedDifficulty    = l_Current.Value.Value;
                l_DifficultyFound       = true;
                break;
            }

            if (l_DifficultyFound == default)
                return false;

            /// Display difficulty
            Difficulty = Game.Level.SerializedToDifficultyName(p_DifficultyRaw);

            /// Display informations
            Name            = p_BeatMap.Name;
            AuthorNameText  = "Mapped by <b><u><i>" + p_BeatMap.Metadata.LevelAuthorName + "</b></u></i>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            Time            = (double)l_SelectedDifficulty.Length;
            BPM             = p_BeatMap.Metadata.BPM;
            NPS             = (float)l_SelectedDifficulty.Notes / (float)l_SelectedDifficulty.Length;
            NJS             = (int)l_SelectedDifficulty.NoteJumpSpeed;
            Offset          = l_SelectedDifficulty.NoteJumpSpeedOffset;
            Notes           = l_SelectedDifficulty.Notes;
            Obstacles       = l_SelectedDifficulty.Obstacles;
            Bombs           = l_SelectedDifficulty.Bombs;

            return true;
        }
        /// <summary>
        /// Set from BeatSaver
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <returns></returns>
        public bool FromBeatSaver(Beatmap p_BeatMap, UnityEngine.Sprite p_Cover)
        {
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Null Beatmap provided!");
                return false;
            }

            if (p_BeatMap.Metadata.Characteristics == null)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Invalid characteristics!");
                return false;
            }

            /// Display modes
            var l_Characteristics = new List<HMUI.IconSegmentedControl.DataItem>();
            foreach (var l_Current in p_BeatMap.Metadata.Characteristics)
            {
                var l_SerializedName = Game.BeatSaver.SanitizeGameMode(l_Current.Name);

                if (string.IsNullOrEmpty(l_SerializedName))
                {
                    Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] Invalid characteristic \"{l_Current.Name}\"!");
                    return false;
                }

                BeatmapCharacteristicSO l_CharacteristicDetails = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_SerializedName);

                if (l_CharacteristicDetails != null)
                    l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey)));
            }

            if (l_Characteristics.Count == 0)
            {
                Logger.Instance.Error($"[SDK.UI][LevelDetail.FromBeatSaver2] No valid characteristics found for map \"{p_BeatMap.Hash}\"!");
                return false;
            }

            /// Store beatmap
            m_BeatMap = p_BeatMap;

            m_SongCharacteristicSegmentedControl.SetData(l_Characteristics.ToArray());
            m_SongCharacteristicSegmentedControl.SelectCellWithNumber(0);
            OnCharacteristicChanged(null, 0);

            /// Display informations
            Name            = p_BeatMap.Name;
            AuthorNameText  = "Mapped by <b><u><i>" + p_BeatMap.Metadata.LevelAuthorName + "</b></u></i>";
            Cover           = p_Cover ?? SongCore.Loader.defaultCoverImage;
            BPM             = p_BeatMap.Metadata.BPM;

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
        public void SetFavoriteToggleImage(string p_Default, string p_Enabled)
        {
            var l_IVDefault = m_FavoriteToggle.transform.GetChild(0).GetComponent<HMUI.ImageView>();
            var l_IVMarked  = m_FavoriteToggle.transform.GetChild(1).GetComponent<HMUI.ImageView>();

            BeatSaberMarkupLanguage.Utilities.GetData(p_Default, (p_Bytes) =>
            {
                var l_Texture = new UnityEngine.Texture2D(2, 2);
                if (l_Texture.LoadImage(p_Bytes))
                    l_IVDefault.sprite = UnityEngine.Sprite.Create(l_Texture, new UnityEngine.Rect(0, 0, l_Texture.width, l_Texture.height), UnityEngine.Vector2.one * 0.5f, 100);
            });
            BeatSaberMarkupLanguage.Utilities.GetData(p_Enabled, (p_Bytes) =>
            {
                var l_Texture = new UnityEngine.Texture2D(2, 2);
                if (l_Texture.LoadImage(p_Bytes))
                    l_IVMarked.sprite = UnityEngine.Sprite.Create(l_Texture, new UnityEngine.Rect(0, 0, l_Texture.width, l_Texture.height), UnityEngine.Vector2.one * 0.5f, 100);
            });
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
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
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
            m_PracticeButton.transform.SetAsLastSibling();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonEnabled(bool p_Value)
        {
            m_PracticeButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPlayButtonEnabled(bool p_Value)
        {
            m_PlayButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonInteractable(bool p_Value)
        {
            m_PracticeButton.interactable = p_Value;
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPlayButtonInteractable(bool p_Value)
        {
            m_PlayButton.interactable = p_Value;
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonText(string p_Value)
        {
            m_PracticeButton.transform.Find("Content").GetComponentInChildren<HMUI.CurvedTextMeshPro>().text = p_Value;
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPlayButtonText(string p_Value)
        {
            m_PlayButton.transform.Find("Content").GetComponentInChildren<HMUI.CurvedTextMeshPro>().text = p_Value;
        }
        /// <summary>
        /// Set left button action
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonAction(UnityEngine.Events.UnityAction p_Value)
        {
            m_PracticeButton.onClick.RemoveAllListeners();
            m_PracticeButton.onClick.AddListener(p_Value);
        }
        /// <summary>
        /// Set right button action
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPlayButtonAction(UnityEngine.Events.UnityAction p_Value)
        {
            m_PlayButton.onClick.RemoveAllListeners();
            m_PlayButton.onClick.AddListener(p_Value);
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
            if (m_BeatMap == null || p_Index >= m_BeatMap.Metadata.Characteristics.Count)
                return;

            SelectedBeatmapCharacteristicSO = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(m_BeatMap.Metadata.Characteristics[p_Index].Name);

            List<string> l_Difficulties = new List<string>();
            foreach (var l_Current in GetBeatMapDifficultiesPerCharacteristicInOrder(m_BeatMap.Metadata.Characteristics[p_Index]))
                l_Difficulties.Add(Game.Level.SerializedToDifficultyName(l_Current.Key.ToString()));

            m_SongDiffSegmentedControl.SetTexts(l_Difficulties.ToArray());
            m_SongDiffSegmentedControl.SelectCellWithNumber(l_Difficulties.Count - 1);
            OnDifficultyChanged(null, l_Difficulties.Count - 1);
        }
        /// <summary>
        /// When the difficulty is changed
        /// </summary>
        /// <param name="p_SegmentControl">Control instance</param>
        /// <param name="p_Index">New selected index</param>
        private void OnDifficultyChanged(HMUI.SegmentedControl p_SegmentControl, int p_Index)
        {
            if (m_BeatMap == null)
                return;

            var l_CharacIndex = m_SongCharacteristicSegmentedControl.selectedCellNumber;
            if (l_CharacIndex >= m_BeatMap.Metadata.Characteristics.Count)
                return;

            var l_Difficulties = GetBeatMapDifficultiesPerCharacteristicInOrder(m_BeatMap.Metadata.Characteristics[l_CharacIndex]);
            if (p_Index >= l_Difficulties.Count)
                return;

            SelectedBeatmapCharacteristicDifficulty = l_Difficulties.ElementAt(p_Index).Value;
            SelecteBeatmapDifficulty                = l_Difficulties.ElementAt(p_Index).Key;

            /// Display informations
            Time        = (double)SelectedBeatmapCharacteristicDifficulty.Length;
            NPS         = (float)SelectedBeatmapCharacteristicDifficulty.Notes / (float)SelectedBeatmapCharacteristicDifficulty.Length;
            NJS         = (int)SelectedBeatmapCharacteristicDifficulty.NoteJumpSpeed;
            Offset      = SelectedBeatmapCharacteristicDifficulty.NoteJumpSpeedOffset;
            Notes       = SelectedBeatmapCharacteristicDifficulty.Notes;
            Obstacles   = SelectedBeatmapCharacteristicDifficulty.Obstacles;
            Bombs       = SelectedBeatmapCharacteristicDifficulty.Bombs;

            if (OnActiveDifficultyChanged != null)
                OnActiveDifficultyChanged.Invoke(GetIDifficultyBeatMap());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get difficulties in order for a beatmap characteristic
        /// </summary>
        /// <param name="p_Chara">Beatmap characteristic</param>
        /// <returns></returns>
        private Dictionary<BeatmapDifficulty, BeatmapCharacteristicDifficulty> GetBeatMapDifficultiesPerCharacteristicInOrder(BeatmapCharacteristic p_Chara)
        {
            var l_Difficulties = new Dictionary<BeatmapDifficulty, BeatmapCharacteristicDifficulty>();
            foreach (var l_Current in p_Chara.Difficulties.Where(x => x.Value.HasValue).ToList())
            {
                if (!l_Current.Value.HasValue)
                    continue;

                l_Difficulties.Add(Game.Level.SerializedToDifficulty(l_Current.Key), l_Current.Value.Value);
            }

            return l_Difficulties.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
        }
        /// <summary>
        /// Get IDifficultyBeatmap
        /// </summary>
        /// <returns></returns>
        private IDifficultyBeatmap GetIDifficultyBeatMap()
        {
            if (m_BeatMap == null)
                return null;

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
            }
        }
    }
}
