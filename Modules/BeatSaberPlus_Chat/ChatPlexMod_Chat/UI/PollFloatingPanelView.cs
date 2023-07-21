using CP_SDK.XUI;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Poll floating panel view
    /// </summary>
    internal sealed class PollFloatingPanelView : CP_SDK.UI.ViewController<PollFloatingPanelView>
    {
        public static Vector2 SIZE = new Vector2(80, 60);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Color PROGRESSBAR_BACKGROUND     = new Color32(36,  36,  36, 255);
        private static Color PROGRESSBAR_BACKGROUND_WIN = new Color32(64, 254, 153, 255);
        private static Color PROGRESSBAR_FILLER         = new Color32(68,  68,  78, 255);
        private static Color PROGRESSBAR_FILLER_WIN     = new Color32(56, 219, 138, 255);

        private static Color TIME_PROGRESSBAR_BACKGROUND    = new Color32( 70,  70,  73, 255);
        private static Color TIME_PROGRESSBAR_FILLER        = new Color32(164, 115, 251, 255);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIText         m_Subject                   = null;
        private XUIHLayout[]    m_ProgressBarsBackground    = new XUIHLayout[5] { null, null, null, null, null };
        private XUIHLayout[]    m_ProgressBars              = new XUIHLayout[5] { null, null, null, null, null };
        private XUIText[]       m_ProgressBarsLabels        = new XUIText[5]    { null, null, null, null, null };
        private XUIHLayout      m_TimeProgressBar           = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK.Chat.Services.Twitch.TwitchService   m_TwitchService             = null;
        private CP_SDK.Chat.Models.Twitch.Helix_Poll        m_LastPoll                  = null;
        private float                                       m_CurrentPollStart          = 0f;
        private float                                       m_CurrentPollEnd            = 1f;
        private float[]                                     m_ProgressBarsLerp          = new float[5] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_WhiteSprite = CP_SDK.Unity.SpriteU.CreateFromTextureWithBorders(Texture2D.whiteTexture);

            Templates.FullRectLayout(
                Templates.TitleBar("Poll"),

                XUIHLayout.Make(
                    XUIText.Make("Subject")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                        .SetColor(Color.yellow)
                        .SetFontSize(4.5f)
                        .SetOverflowMode(TMPro.TextOverflowModes.Ellipsis)
                        .OnReady(x => x.TMProUGUI.lineSpacing = -50.0f)
                        .Bind(ref m_Subject)
                )
                .SetPadding(0)
                .SetHeight(15f)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained),

                BuildOption(l_WhiteSprite, 0),
                BuildOption(l_WhiteSprite, 1),
                BuildOption(l_WhiteSprite, 2),
                BuildOption(l_WhiteSprite, 3),
                BuildOption(l_WhiteSprite, 4),

                XUIHLayout.Make(
                    XUIHLayout.Make()
                        .SetSpacing(0).SetPadding(0, 2, 0, 2)
                        .SetHeight(1)
                        .SetBackground(true, TIME_PROGRESSBAR_FILLER)
                        .SetBackgroundSprite(l_WhiteSprite, UnityEngine.UI.Image.Type.Filled)
                        .SetBackgroundFillMethod(UnityEngine.UI.Image.FillMethod.Horizontal)
                        .SetBackgroundFillAmount(1.0f)
                        .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                        .Bind(ref m_TimeProgressBar)
                )
                .SetSpacing(0).SetPadding(0)
                .SetBackground(true, TIME_PROGRESSBAR_BACKGROUND)
                .SetBackgroundSprite(l_WhiteSprite, UnityEngine.UI.Image.Type.Filled)
                .SetBackgroundFillMethod(UnityEngine.UI.Image.FillMethod.Horizontal)
                .SetBackgroundFillAmount(1.0f)
                .OnReady(x => x.CSizeFitter.horizontalFit               = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childAlignment          = TextAnchor.MiddleLeft)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth   = true)
            )
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .BuildUI(transform);

            CP_SDK.Chat.Service.Acquire();
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_TwitchService = l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService;
                m_TwitchService.HelixAPI.OnActivePollChanged += HelixAPI_OnActivePollChanged;
            }
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected sealed override void OnViewActivation()
        {
            /// Hide by default
            CurrentScreen?.gameObject?.SetActive(false);
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected sealed override void OnViewDestruction()
        {
            if (m_TwitchService != null)
                m_TwitchService.HelixAPI.OnActivePollChanged -= HelixAPI_OnActivePollChanged;

            CP_SDK.Chat.Service.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build option group
        /// </summary>
        /// <param name="p_WhiteSprite">White sprite</param>
        /// <param name="p_Index">Index of the option block</param>
        /// <returns></returns>
        private IXUIElement BuildOption(Sprite p_WhiteSprite, int p_Index)
        {
            return XUIHLayout.Make(
                XUIHLayout.Make(
                    XUIText.Make("Option " + (p_Index + 1))
                        .SetAlign(TMPro.TextAlignmentOptions.MidlineLeft)
                        .SetFontSize(4.0f)
                        .Bind(ref m_ProgressBarsLabels[p_Index])
                )
                .SetSpacing(0).SetPadding(0, 2, 0, 2)
                .SetBackground(true, PROGRESSBAR_FILLER)
                .SetBackgroundSprite(p_WhiteSprite, UnityEngine.UI.Image.Type.Filled)
                .SetBackgroundFillMethod(UnityEngine.UI.Image.FillMethod.Horizontal)
                .SetBackgroundFillAmount(0.0f)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                .Bind(ref m_ProgressBars[p_Index])
            )
            .SetSpacing(0).SetPadding(0)
            .SetBackground(true, PROGRESSBAR_BACKGROUND)
            .SetBackgroundSprite(p_WhiteSprite, UnityEngine.UI.Image.Type.Filled)
            .SetBackgroundFillMethod(UnityEngine.UI.Image.FillMethod.Horizontal)
            .SetBackgroundFillAmount(1.0f)
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            .Bind(ref m_ProgressBarsBackground[p_Index]);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_LastPoll != null
                && m_LastPoll.status != CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                && m_LastPoll.status != CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID)
            {
                if (m_CurrentPollStart < 0)
                {
                    var l_Offset    = -m_CurrentPollStart;
                    var l_NewStart  = m_CurrentPollStart + l_Offset;
                    var l_NewEnd    = m_CurrentPollEnd + l_Offset;

                    m_TimeProgressBar.SetBackgroundFillAmount(Mathf.Max(0, 1f - (((Time.realtimeSinceStartup + l_Offset) - l_NewStart) / (l_NewEnd - l_NewStart))));
                }
                else
                    m_TimeProgressBar.SetBackgroundFillAmount(Mathf.Max(0, 1f - ((Time.realtimeSinceStartup - m_CurrentPollStart) / (m_CurrentPollEnd - m_CurrentPollStart))));

                for (int l_I = 0; l_I < m_ProgressBars.Length; ++l_I)
                    m_ProgressBars[l_I].SetBackgroundFillAmount(Mathf.Lerp(m_ProgressBars[l_I].Element.GetBackgroundFillAmount(), m_ProgressBarsLerp[l_I], Time.smoothDeltaTime * 5f));
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On poll data update
        /// </summary>
        /// <param name="p_Poll">New poll data</param>
        private void HelixAPI_OnActivePollChanged(CP_SDK.Chat.Models.Twitch.Helix_Poll p_Poll)
        {
            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                /// If poll change
                if ((p_Poll?.id ?? null) != (m_LastPoll?.id ?? null))
                {
                    if (p_Poll != null
                        && p_Poll.status != CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                        && p_Poll.status != CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID)
                    {
                        CurrentScreen?.gameObject?.SetActive(true);

                        m_Subject.SetText(p_Poll.title);

                        /// Update choices
                        if (p_Poll.choices != null)
                        {
                            int l_TotalVotes = 0;
                            for (int l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                                l_TotalVotes += p_Poll.choices[l_I].votes;

                            if (   p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                                || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
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

                        if (   p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                            || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                            m_CurrentPollEnd = Time.realtimeSinceStartup;
                    }
                    else
                        CurrentScreen?.gameObject?.SetActive(false);
                }
                else if (p_Poll != null && m_LastPoll != null && p_Poll.id == m_LastPoll.id)
                {
                    if (p_Poll.choices != null)
                    {
                        int l_TotalVotes = 0;
                        for (var l_I = 0; l_I < p_Poll.choices.Count && l_I < m_ProgressBars.Length; ++l_I)
                            l_TotalVotes += p_Poll.choices[l_I].votes;

                        if (   p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                            || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
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

                    if (   p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.TERMINATED
                        || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.COMPLETED)
                        m_CurrentPollEnd = Time.realtimeSinceStartup;

                    if (   p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.ARCHIVED
                        || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.INVALID
                        || p_Poll.status == CP_SDK.Chat.Models.Twitch.Helix_Poll.Status.MODERATED)
                        CurrentScreen?.gameObject?.SetActive(false);
                }

                m_LastPoll = p_Poll;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
                m_ProgressBarsBackground[p_Index].SetBackgroundFillAmount(0.0f);
                m_ProgressBars[p_Index].SetBackgroundFillAmount(0.0f);
                m_ProgressBarsLabels[p_Index].SetText(" ");
            }
            else
            {
                m_ProgressBarsBackground[p_Index].SetBackgroundFillAmount(1.0f);
            }
        }
        /// <summary>
        /// Update frame
        /// </summary>
        /// <param name="p_Index">Frame index</param>
        /// <param name="p_Winner">Is it the winner frame</param>
        /// <param name="p_Choice">Choice data</param>
        /// <param name="p_TotalVotes">Total vote</param>
        private void UpdateFrame(int p_Index, bool p_Winner, CP_SDK.Chat.Models.Twitch.Helix_Poll.Choice p_Choice, int p_TotalVotes)
        {
            if (p_Index < 0 || p_Index > m_ProgressBars.Length)
                return;

            var l_Label     = (p_Winner ? "🏆 " : "") + p_Choice.title;
            var l_VotePct   = p_TotalVotes == 0 ? 0f : (float)p_Choice.votes / (float)p_TotalVotes;

            if (p_Winner)
            {
                m_ProgressBarsBackground[p_Index].SetBackgroundColor(PROGRESSBAR_BACKGROUND_WIN);
                m_ProgressBars[p_Index].SetBackgroundColor(PROGRESSBAR_FILLER_WIN);
                m_ProgressBarsLabels[p_Index].SetColor(Color.black);
            }
            else
            {
                m_ProgressBarsBackground[p_Index].SetBackgroundColor(PROGRESSBAR_BACKGROUND);
                m_ProgressBars[p_Index].SetBackgroundColor(PROGRESSBAR_FILLER);
                m_ProgressBarsLabels[p_Index].SetColor(Color.white);
            }

            m_ProgressBarsLerp[p_Index] = l_VotePct;
            m_ProgressBarsLabels[p_Index].SetText($"<line-height=1%><align=\"left\">{l_Label}\n<line-height=100%><align=\"right\">{Mathf.RoundToInt(l_VotePct * 100.0f)} % ({p_Choice.votes})");
        }
    }
}
