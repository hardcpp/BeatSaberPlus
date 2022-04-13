using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using UnityEngine;
using System.Linq;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Poll floating window
    /// </summary>
    internal class FloatingWindow_Poll : BeatSaberPlus.SDK.UI.ResourceViewController<FloatingWindow_Poll>
    {
        public static Vector2 SIZE = new Vector2(80, 60);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Color PROGRESSBAR_BACKGROUND     = new Color32(36, 36, 36, 255);
        private static Color PROGRESSBAR_BACKGROUND_WIN = new Color32(64, 254, 153, 255);
        private static Color PROGRESSBAR_FILLER         = new Color32(68, 68, 78, 255);
        private static Color PROGRESSBAR_FILLER_WIN     = new Color32(56, 219, 138, 255);

        private static Color TIME_PROGRESSBAR_BACKGROUND    = new Color32(70, 70, 73, 255);
        private static Color TIME_PROGRESSBAR_FILLER        = new Color32(164, 115, 251, 255);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("Subject")]    private TMPro.TextMeshProUGUI m_Subject = null;
        [UIObject("Option1Frame")]  private GameObject m_Option1Frame = null;
        [UIObject("Option2Frame")]  private GameObject m_Option2Frame = null;
        [UIObject("Option3Frame")]  private GameObject m_Option3Frame = null;
        [UIObject("Option4Frame")]  private GameObject m_Option4Frame = null;
        [UIObject("Option5Frame")]  private GameObject m_Option5Frame = null;
        [UIObject("TimeFrame")]     private GameObject m_TimeFrame = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch service instance
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService m_Service = null;
        /// <summary>
        /// Latest poll data
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll m_LastPoll = null;
        /// <summary>
        /// Current poll start real time since startup
        /// </summary>
        private float m_CurrentPollStart = 0f;
        /// <summary>
        /// Current poll end real time since startup
        /// </summary>
        private float m_CurrentPollEnd = 1f;
        /// <summary>
        /// Progress bars background
        /// </summary>
        private UnityEngine.UI.Image[] m_ProgressBarsBackground = new UnityEngine.UI.Image[5] { null, null, null, null, null };
        /// <summary>
        /// Progress bars filler
        /// </summary>
        private UnityEngine.UI.Image[] m_ProgressBars = new UnityEngine.UI.Image[5] { null, null, null, null, null };
        /// <summary>
        /// Progress bars target value
        /// </summary>
        private float[] m_ProgressBarsLerp = new float[5] { 0f, 0f, 0f, 0f, 0f };
        /// <summary>
        /// Progress bars labels
        /// </summary>
        private TMPro.TextMeshProUGUI[] m_m_ProgressBarsLabels = new TMPro.TextMeshProUGUI[5] { null, null, null, null, null };
        /// <summary>
        /// Time progress bar filler
        /// </summary>
        private UnityEngine.UI.Image m_TimeProgressBar = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color       = new Color(0f, 0f, 0f, 0.9f);
            GetComponentInChildren<ImageView>().material    = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            m_Subject.transform.parent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childControlWidth = false;
            m_Subject.transform.parent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childControlHeight = false;
            m_Subject.rectTransform.sizeDelta = new Vector2(75, 12);
            m_Subject.lineSpacing = -50f;

            SetupFrame(0, m_Option1Frame);
            SetupFrame(1, m_Option2Frame);
            SetupFrame(2, m_Option3Frame);
            SetupFrame(3, m_Option4Frame);
            SetupFrame(4, m_Option5Frame);

            SetupFrame(-1, m_TimeFrame);

            BeatSaberPlus.SDK.Chat.Service.Acquire();
            var l_TwitchService = BeatSaberPlus.SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_Service = l_TwitchService as BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService;
                m_Service.HelixAPI.OnActivePollChanged += HelixAPI_OnActivePollChanged;
            }
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected sealed override void OnViewActivation()
        {
            /// Hide by default
            gameObject.SetActive(false);
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected sealed override void OnViewDestruction()
        {
            if (m_Service != null)
                m_Service.HelixAPI.OnActivePollChanged -= HelixAPI_OnActivePollChanged;

            BeatSaberPlus.SDK.Chat.Service.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_LastPoll != null
                && m_LastPoll.status != BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                && m_LastPoll.status != BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID)
            {
                if (m_CurrentPollStart < 0)
                {
                    var l_Offset    = -m_CurrentPollStart;
                    var l_NewStart  = m_CurrentPollStart + l_Offset;
                    var l_NewEnd    = m_CurrentPollEnd + l_Offset;

                    m_TimeProgressBar.fillAmount = Mathf.Max(0, 1f - (((Time.realtimeSinceStartup + l_Offset) - l_NewStart) / (l_NewEnd - l_NewStart)));
                }
                else
                    m_TimeProgressBar.fillAmount = Mathf.Max(0, 1f - ((Time.realtimeSinceStartup - m_CurrentPollStart) / (m_CurrentPollEnd - m_CurrentPollStart)));

                for (int l_I = 0; l_I < m_ProgressBars.Length; ++l_I)
                    m_ProgressBars[l_I].fillAmount = Mathf.Lerp(m_ProgressBars[l_I].fillAmount, m_ProgressBarsLerp[l_I], Time.smoothDeltaTime * 5f);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On poll data update
        /// </summary>
        /// <param name="p_Poll">New poll data</param>
        private void HelixAPI_OnActivePollChanged(BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll p_Poll)
        {
            BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                /// If poll change
                if ((p_Poll?.id ?? null) != (m_LastPoll?.id ?? null))
                {
                    if (p_Poll != null
                        && p_Poll.status != BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                        && p_Poll.status != BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID)
                    {
                        gameObject.SetActive(true);

                        m_Subject.text = p_Poll.title;

                        /// Update choices
                        if (p_Poll.choices != null)
                        {
                            int l_TotalVotes = 0;
                            for (int l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                                l_TotalVotes += p_Poll.choices[l_I].votes;

                            if (p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                                || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                            {
                                var l_Sorted = p_Poll.choices.OrderByDescending(x => x.votes).ToArray();

                                for (int l_I = 0; l_I < l_Sorted.Length && l_I < m_ProgressBars.Length; ++l_I)
                                {
                                    SetFrameVisibility(l_I, true);
                                    UpdateFrame(l_I, l_I == 0, l_Sorted[l_I], l_TotalVotes);
                                }
                            }
                            else
                            {
                                for (int l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                                {
                                    SetFrameVisibility(l_I, true);
                                    UpdateFrame(l_I, false, p_Poll.choices[l_I], l_TotalVotes);
                                }
                            }

                            for (int l_I = p_Poll.choices.Count; l_I < m_ProgressBars.Length; ++l_I)
                                SetFrameVisibility(l_I, false);
                        }

                        var l_UTCNow    = System.DateTime.UtcNow;
                        var l_UTCStart  = p_Poll.started_at;

                        m_CurrentPollStart  = Time.realtimeSinceStartup - Mathf.Abs((float)(l_UTCNow - l_UTCStart).TotalSeconds);
                        m_CurrentPollEnd    = m_CurrentPollStart + p_Poll.duration;

                        if (p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                            || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                            m_CurrentPollEnd = Time.realtimeSinceStartup;
                    }
                    else
                        gameObject.SetActive(false);
                }
                else if (p_Poll != null && m_LastPoll != null && p_Poll.id == m_LastPoll.id)
                {
                    if (p_Poll.choices != null)
                    {
                        int l_TotalVotes = 0;
                        for (int l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                            l_TotalVotes += p_Poll.choices[l_I].votes;

                        if (p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                            || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                        {
                            var l_Sorted = p_Poll.choices.OrderByDescending(x => x.votes).ToArray();

                            for (int l_I = 0; l_I < l_Sorted.Length && l_I < m_ProgressBars.Length; ++l_I)
                                UpdateFrame(l_I, l_I == 0, l_Sorted[l_I], l_TotalVotes);
                        }
                        else
                        {
                            for (int l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                                UpdateFrame(l_I, false, p_Poll.choices[l_I], l_TotalVotes);
                        }
                    }

                    if (p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                        || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                        m_CurrentPollEnd = Time.realtimeSinceStartup;

                    if (p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                        || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID
                        || p_Poll.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Status.MODERATED)
                        gameObject.SetActive(false);
                }

                m_LastPoll = p_Poll;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup frame for progress bar & text
        /// </summary>
        /// <param name="p_Index">Frame index, -1 for the time frame</param>
        /// <param name="p_Frame">Frame instance</param>
        private void SetupFrame(int p_Index, GameObject p_Frame)
        {
            var l_Background = p_Frame.AddComponent<UnityEngine.UI.Image>();
            l_Background.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            l_Background.type       = UnityEngine.UI.Image.Type.Filled;
            l_Background.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            l_Background.fillAmount = 1f;
            l_Background.color      = p_Index == -1 ? TIME_PROGRESSBAR_BACKGROUND : PROGRESSBAR_BACKGROUND;
            l_Background.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            if (p_Index != -1)
                m_ProgressBarsBackground[p_Index] = l_Background;

            var l_Filler = p_Frame.transform.GetChild(0).gameObject.AddComponent<UnityEngine.UI.Image>();
            l_Filler.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            l_Filler.type       = UnityEngine.UI.Image.Type.Filled;
            l_Filler.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            l_Filler.fillAmount = p_Index == -1 ? 1f : 0f;
            l_Filler.color      = p_Index == -1 ? TIME_PROGRESSBAR_FILLER : PROGRESSBAR_FILLER;
            l_Filler.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            if (p_Index != -1)
                m_ProgressBars[p_Index] = l_Filler;
            else
                m_TimeProgressBar = l_Filler;

            if (p_Index != -1)
                m_m_ProgressBarsLabels[p_Index] = p_Frame.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }
        /// <summary>
        /// Set frame visibility
        /// </summary>
        /// <param name="p_Index">Frame index</param>
        /// <param name="p_Visible">Visibility state</param>
        private void SetFrameVisibility(int p_Index, bool p_Visible)
        {
            if (p_Index < 0 || p_Index > m_ProgressBars.Length)
                return;

            if (!p_Visible)
            {
                m_ProgressBarsBackground[p_Index].fillAmount = 0f;
                m_ProgressBars[p_Index].fillAmount = 0f;
                m_m_ProgressBarsLabels[p_Index].text = " ";
            }
            else
            {
                m_ProgressBarsBackground[p_Index].fillAmount = 1f;
            }
        }
        /// <summary>
        /// Update frame
        /// </summary>
        /// <param name="p_Index">Frame index</param>
        /// <param name="p_Winner">Is it the winner frame</param>
        /// <param name="p_Choice">Choice data</param>
        /// <param name="p_TotalVotes">Total vote</param>
        private void UpdateFrame(int p_Index, bool p_Winner, BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Poll.Choice p_Choice, int p_TotalVotes)
        {
            if (p_Index < 0 || p_Index > m_ProgressBars.Length)
                return;

            var l_Label     = (p_Winner ? "🏆 " : "") + p_Choice.title;
            var l_VotePct   = p_TotalVotes == 0 ? 0f : (float)p_Choice.votes / (float)p_TotalVotes;

            if (p_Winner)
            {
                m_ProgressBarsBackground[p_Index].color = PROGRESSBAR_BACKGROUND_WIN;
                m_ProgressBars[p_Index].color           = PROGRESSBAR_FILLER_WIN;
                m_m_ProgressBarsLabels[p_Index].color   = Color.black;
            }
            else
            {
                m_ProgressBarsBackground[p_Index].color = PROGRESSBAR_BACKGROUND;
                m_ProgressBars[p_Index].color           = PROGRESSBAR_FILLER;
                m_m_ProgressBarsLabels[p_Index].color   = Color.white;
            }

            m_ProgressBarsLerp[p_Index] = l_VotePct;
            m_m_ProgressBarsLabels[p_Index].text = $"<line-height=1%><align=\"left\">{l_Label}\n<line-height=100%><align=\"right\">{Mathf.RoundToInt(l_VotePct * 100.0f)} % ({p_Choice.votes})";
        }
    }
}
