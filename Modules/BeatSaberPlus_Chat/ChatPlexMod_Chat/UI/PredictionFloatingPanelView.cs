using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Prediction floating panel view
    /// </summary>
    internal sealed class PredictionFloatingPanelView : CP_SDK.UI.ViewController<PredictionFloatingPanelView>
    {
        public static Vector2 SIZE = new Vector2(80, 75);

        private static Color TIME_PROGRESSBAR_BACKGROUND    = new Color32( 70,  70,  73, 255);
        private static Color TIME_PROGRESSBAR_FILLER        = new Color32(164, 115, 251, 255);

        private static string TAG_BLUE = "<" + ColorU.ToHexRGB(new Color32( 56, 122, 255, 255)) + ">";
        private static string TAG_PINK = "<" + ColorU.ToHexRGB(new Color32(245,   0, 155, 255)) + ">";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIText             m_Subject           = null;

        private XUIText             m_Labels            = null;
        private XUIText             m_Percentages       = null;
        private XUIText             m_Points            = null;
        private XUIText             m_Ratios            = null;
        private XUIText             m_Votees            = null;

        private XUIHLayout          m_LockButtonFrame   = null;
        private XUIPrimaryButton    m_LockButton        = null;

        private XUIHLayout          m_PickButtonFrame   = null;
        private XUIPrimaryButton    m_PickButtonBlue    = null;
        private XUIPrimaryButton    m_PickButtonPink    = null;

        private XUIHLayout          m_CancelButtonFrame = null;
        private XUISecondaryButton  m_CancelButton      = null;

        private XUIHLayout          m_TimeProgressBar   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK.Chat.Services.Twitch.TwitchService   m_TwitchService     = null;
        private CP_SDK.Chat.Models.Twitch.Helix_Prediction  m_LastPrediction    = null;
        private float                                       m_BluePctTarget     = 0f;
        private float                                       m_BluePctDisplayed  = 0f;
        private float                                       m_PinkPctTarget     = 0f;
        private float                                       m_PinkPctDisplayed  = 0f;
        private float                                       m_WindowStart       = 0f;
        private float                                       m_WindowEnd         = 1f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_WhiteSprite = CP_SDK.Unity.SpriteU.CreateFromTexture(Texture2D.whiteTexture);

            Templates.FullRectLayout(
                Templates.TitleBar("Prediction"),

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
                .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained),

                XUIText.Make("Option 1").SetFontSize(5.5f).Bind(ref m_Labels),
                XUIText.Make("Option 1").SetFontSize(6.0f).SetStyle(TMPro.FontStyles.Bold).Bind(ref m_Percentages),
                XUIText.Make("Option 1").SetFontSize(4.0f).Bind(ref m_Points),
                XUIText.Make("Option 1").SetFontSize(4.0f).Bind(ref m_Ratios),
                XUIText.Make("Option 1").SetFontSize(4.0f).Bind(ref m_Votees),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Lock votes", OnLockButton).Bind(ref m_LockButton)
                )
                .SetPadding(0)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .ForEachDirect<XUIPrimaryButton>((y) => y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained))
                .Bind(ref m_LockButtonFrame),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Pick as winner", OnPickBlueButton).Bind(ref m_PickButtonBlue),
                    XUIPrimaryButton.Make("Pick as winner", OnPickPinkButton).Bind(ref m_PickButtonPink)
                )
                .SetPadding(0)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .ForEachDirect<XUIPrimaryButton>((y) => y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained))
                .Bind(ref m_PickButtonFrame),

                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).Bind(ref m_CancelButton)
                )
                .SetPadding(0)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .ForEachDirect<XUISecondaryButton>((y) => y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained))
                .Bind(ref m_CancelButtonFrame),

                XUIHLayout.Make(
                    XUIHLayout.Make()
                        .SetSpacing(0).SetPadding(0, 2, 0, 2)
                        .SetHeight(1)
                        .SetBackground(true, TIME_PROGRESSBAR_FILLER)
                        .SetBackgroundSprite(l_WhiteSprite, Image.Type.Filled)
                        .SetBackgroundFillMethod(Image.FillMethod.Horizontal)
                        .SetBackgroundFillAmount(1.0f)
                        .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                        .Bind(ref m_TimeProgressBar)
                )
                .SetSpacing(0).SetPadding(0)
                .SetBackground(true, TIME_PROGRESSBAR_BACKGROUND)
                .SetBackgroundSprite(l_WhiteSprite, Image.Type.Filled)
                .SetBackgroundFillMethod(Image.FillMethod.Horizontal)
                .SetBackgroundFillAmount(1.0f)
                .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childAlignment = TextAnchor.MiddleLeft)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            )
            .SetSpacing(0)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .BuildUI(transform);

            CP_SDK.Chat.Service.Acquire();
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_TwitchService = l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService;
                m_TwitchService.HelixAPI.OnActivePredictionChanged += HelixAPI_OnActivePredictionChanged;
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
                m_TwitchService.HelixAPI.OnActivePredictionChanged -= HelixAPI_OnActivePredictionChanged;

            CP_SDK.Chat.Service.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_LastPrediction != null)
            {
                var l_HasExpired =  (m_LastPrediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED
                                        && m_WindowEnd < Time.realtimeSinceStartup)
                                    || m_LastPrediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.CANCELED;

                if (l_HasExpired)
                    CurrentScreen?.gameObject?.SetActive(false);
                else
                {
                    if (m_LastPrediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.ACTIVE
                        || m_LastPrediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED)
                    {
                        if (m_WindowStart < 0)
                        {
                            var l_Offset    = -m_WindowStart;
                            var l_NewStart  = m_WindowStart + l_Offset;
                            var l_NewEnd    = m_WindowEnd + l_Offset;

                            m_TimeProgressBar.SetBackgroundFillAmount(Mathf.Max(0, 1f - (((Time.realtimeSinceStartup + l_Offset) - l_NewStart) / (l_NewEnd - l_NewStart))));
                        }
                        else
                            m_TimeProgressBar.SetBackgroundFillAmount(Mathf.Max(0, 1f - ((Time.realtimeSinceStartup - m_WindowStart) / (m_WindowEnd - m_WindowStart))));
                    }

                    var l_NewBluePct = Mathf.Lerp(m_BluePctDisplayed, m_BluePctTarget, Time.smoothDeltaTime * 2.5f);
                    var l_NewPinkPct = Mathf.Lerp(m_PinkPctDisplayed, m_PinkPctTarget, Time.smoothDeltaTime * 2.5f);

                    if (Mathf.Abs(l_NewBluePct - m_BluePctDisplayed) >= 0.0001 || Mathf.Abs(l_NewPinkPct - m_PinkPctDisplayed) >= 0.0001)
                    {
                        m_BluePctDisplayed = l_NewBluePct;
                        m_PinkPctDisplayed = l_NewPinkPct;

                        m_Percentages.SetText($"<line-height=1%><align=\"left\">{TAG_BLUE}{Mathf.RoundToInt(l_NewBluePct * 100f)}%\n <line-height=100%><align=\"right\">{TAG_PINK}{Mathf.RoundToInt(m_PinkPctDisplayed * 100f)}%");
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lock votes
        /// </summary>
        private void OnLockButton()
        {
            ShowConfirmationModal("Lock the votes?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_LockButton.SetInteractable(false);

                m_TwitchService.HelixAPI.EndPrediction(
                    new CP_SDK.Chat.Models.Twitch.Helix_EndPrediction_Query(m_LastPrediction.id, CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.LOCKED, null),
                    (p_Status, x, p_Error) =>
                    {
                        Logger.Instance.Error(p_Status.ToString());
                        Logger.Instance.Error(p_Error);
                        HelixAPI_OnActivePredictionChanged(x);
                    }
                );
            });
        }
        /// <summary>
        /// Pick blue
        /// </summary>
        private void OnPickBlueButton()
        {
            ShowConfirmationModal("Make blue win?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_PickButtonBlue.SetInteractable(false);
                m_PickButtonPink.SetInteractable(false);

                var l_Winner = m_LastPrediction.outcomes.FirstOrDefault(x => x.color == CP_SDK.Chat.Models.Twitch.EHelix_PredictionColor.BLUE)?.id ?? null;
                m_TwitchService.HelixAPI.EndPrediction(
                    new CP_SDK.Chat.Models.Twitch.Helix_EndPrediction_Query(m_LastPrediction.id, CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED, l_Winner), 
                    (_, x, __) =>
                    {
                        HelixAPI_OnActivePredictionChanged(x);
                    }
                );
            });
        }
        /// <summary>
        /// Pick pink
        /// </summary>
        private void OnPickPinkButton()
        {
            ShowConfirmationModal("Make pink win?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_PickButtonBlue.SetInteractable(false);
                m_PickButtonPink.SetInteractable(false);

                var l_Winner = m_LastPrediction.outcomes.FirstOrDefault(x => x.color == CP_SDK.Chat.Models.Twitch.EHelix_PredictionColor.PINK)?.id ?? null;
                m_TwitchService.HelixAPI.EndPrediction(
                    new CP_SDK.Chat.Models.Twitch.Helix_EndPrediction_Query(m_LastPrediction.id, CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED, l_Winner),
                    (_, x, __) =>
                    {
                        HelixAPI_OnActivePredictionChanged(x);
                    }
                );
            });
        }
        /// <summary>
        /// Cancel prediction
        /// </summary>
        private void OnCancelButton()
        {
            ShowConfirmationModal("Cancel prediction?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_CancelButton.SetInteractable(false);

                m_TwitchService.HelixAPI.EndPrediction(
                    new CP_SDK.Chat.Models.Twitch.Helix_EndPrediction_Query(m_LastPrediction.id, CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.CANCELED, null),
                    (_, x, __) =>
                    {
                        HelixAPI_OnActivePredictionChanged(x);
                    }
                );
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active prediction changed
        /// </summary>
        /// <param name="p_Prediction">Active prediction</param>
        private void HelixAPI_OnActivePredictionChanged(CP_SDK.Chat.Models.Twitch.Helix_Prediction p_Prediction)
        {
            if (p_Prediction != null)
            {
                var l_HasExpired = (p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED
                                        && (p_Prediction.ended_at?.AddSeconds(60) < DateTime.UtcNow))
                                    || p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.CANCELED;

                if (l_HasExpired)
                {
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                    {
                        if (CurrentScreen?.gameObject?.activeSelf ?? false)
                            CurrentScreen?.gameObject?.SetActive(false);

                        m_WindowEnd         = Time.realtimeSinceStartup;
                        m_LastPrediction    = p_Prediction;
                    });
                }
                else
                {
                    var l_TotalPoints = p_Prediction.outcomes[0].channel_points + p_Prediction.outcomes[1].channel_points;

                    m_BluePctTarget = l_TotalPoints == 0 ? 0 : (float)p_Prediction.outcomes[0].channel_points / (float)l_TotalPoints;
                    m_PinkPctTarget = l_TotalPoints == 0 ? 0 : (float)p_Prediction.outcomes[1].channel_points / (float)l_TotalPoints;

                    var l_BlueRatio = p_Prediction.outcomes[0].channel_points == 0 ? "-:-" : ("1:" + (1 + ((float)p_Prediction.outcomes[1].channel_points / (float)p_Prediction.outcomes[0].channel_points)).ToString("0.00"));
                    var l_PinkRatio = p_Prediction.outcomes[1].channel_points == 0 ? "-:-" : ("1:" + (1 + ((float)p_Prediction.outcomes[0].channel_points / (float)p_Prediction.outcomes[1].channel_points)).ToString("0.00"));

                    var l_Points = $"<line-height=1%><align=\"left\">{TAG_BLUE}⭕ {p_Prediction.outcomes[0].channel_points}\n<line-height=100%><align=\"right\">{TAG_PINK}{p_Prediction.outcomes[1].channel_points} ⭕";
                    var l_Ratios = $"<line-height=1%><align=\"left\">{TAG_BLUE}🏆 {l_BlueRatio}\n<line-height=100%><align=\"right\">{TAG_PINK}{l_PinkRatio} 🏆";
                    var l_Votees = $"<line-height=1%><align=\"left\">{TAG_BLUE}👥 {p_Prediction.outcomes[0].users}\n<line-height=100%><align=\"right\">{TAG_PINK}{p_Prediction.outcomes[1].users} 👥";

                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                    {
                        if (!CurrentScreen?.gameObject?.activeSelf ?? false)
                        {
                            CurrentScreen?.gameObject?.SetActive(true);

                            m_LockButton.SetInteractable(true);
                            m_PickButtonBlue.SetInteractable(true);
                            m_PickButtonPink.SetInteractable(true);
                        }

                        if (m_LastPrediction == null || m_LastPrediction.id != p_Prediction.id)
                        {
                            var l_LeftTitle     = p_Prediction.outcomes[0].title;
                            var l_RightTitle    = p_Prediction.outcomes[1].title;
                            var l_MaxTitle      = 20;

                            if (l_LeftTitle.Length > l_MaxTitle)    l_LeftTitle     = l_LeftTitle.Substring(0, l_MaxTitle - 3) + "...";
                            if (l_RightTitle.Length > l_MaxTitle)   l_RightTitle    = l_RightTitle.Substring(0, l_MaxTitle - 3) + "...";

                            m_Subject       .SetText(p_Prediction.title);
                            m_Labels        .SetText($"<line-height=1%><align=\"left\">{TAG_BLUE}{l_LeftTitle}\n<line-height=100%><align=\"right\">{TAG_PINK}{l_RightTitle}");
                            m_Percentages   .SetText($"<line-height=1%><align=\"left\">{TAG_BLUE}0%\n <line-height=100%><align=\"right\">{TAG_PINK}0%");

                            m_BluePctDisplayed = 0f;
                            m_PinkPctDisplayed = 0f;
                        }

                        if (p_Prediction.winning_outcome_id != null)
                        {
                            var l_Winner        = p_Prediction.outcomes[0].id == p_Prediction.winning_outcome_id ? 0 : 1;
                            var l_LeftPrefix    = l_Winner == 0 ? TAG_BLUE + "<alpha=#FF>" : TAG_BLUE + "<alpha=#AF>";
                            var l_RightPrefix   = l_Winner == 0 ? TAG_PINK + "<alpha=#AF>" : TAG_PINK + "<alpha=#FF>";

                            var l_LeftTitle     = p_Prediction.outcomes[0].title;
                            var l_RightTitle    = p_Prediction.outcomes[1].title;
                            var l_MaxTitle      = 20 - 1;

                            if (l_LeftTitle.Length > l_MaxTitle)    l_LeftTitle     = l_LeftTitle.Substring(0, l_MaxTitle - 3) + "...";
                            if (l_RightTitle.Length > l_MaxTitle)   l_RightTitle    = l_RightTitle.Substring(0, l_MaxTitle - 3) + "...";

                            l_LeftTitle     = (l_Winner == 0 ? "<color=green>✔</color>" : "<color=red>❌</color>") + l_LeftTitle;
                            l_RightTitle    = l_RightTitle + (l_Winner == 1 ? "<color=green>✔</color>" : "<color=red>❌</color>");

                            m_BluePctDisplayed = m_BluePctTarget;
                            m_PinkPctDisplayed = m_PinkPctTarget;
                            m_Labels     .SetText($"<line-height=1%><align=\"left\">{l_LeftPrefix}{l_LeftTitle}\n<line-height=100%><align=\"right\">{l_RightPrefix}{l_RightTitle}");
                            m_Percentages.SetText($"<line-height=1%><align=\"left\">{l_LeftPrefix}{Mathf.RoundToInt(m_BluePctTarget * 100f)}%\n <line-height=100%><align=\"right\">{l_RightPrefix}{Mathf.RoundToInt(m_PinkPctTarget * 100f)}%");
                            l_Points            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}⭕ {p_Prediction.outcomes[0].channel_points}\n<line-height=100%><align=\"right\">{l_RightPrefix}{p_Prediction.outcomes[1].channel_points} ⭕";
                            l_Ratios            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}🏆 {l_BlueRatio}\n<line-height=100%><align=\"right\">{l_RightPrefix}{l_PinkRatio} 🏆";
                            l_Votees            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}👥 {p_Prediction.outcomes[0].users}\n<line-height=100%><align=\"right\">{l_RightPrefix}{p_Prediction.outcomes[1].users} 👥";
                        }

                        m_Points.SetText(l_Points);
                        m_Ratios.SetText(l_Ratios);
                        m_Votees.SetText(l_Votees);

                        m_LockButtonFrame.SetActive(p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.ACTIVE);

                        m_PickButtonFrame.SetActive(p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.LOCKED
                                                 || p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED);

                        m_CancelButton.SetInteractable(p_Prediction.status != CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED);

                        m_WindowStart   = Time.realtimeSinceStartup - Mathf.Abs((float)(DateTime.UtcNow - p_Prediction.created_at).TotalSeconds);
                        m_WindowEnd     = m_WindowStart + p_Prediction.prediction_window;

                        if (p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.ACTIVE)
                        {
                            m_PickButtonBlue.SetInteractable(false);
                            m_PickButtonPink.SetInteractable(false);

                            m_LockButton.SetInteractable(true);
                        }
                        else if (p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.LOCKED)
                        {
                            m_PickButtonBlue.SetInteractable(true);
                            m_PickButtonPink.SetInteractable(true);

                            m_LockButton.SetInteractable(false);

                            m_TimeProgressBar.SetBackgroundFillAmount(0.0f);
                        }
                        else if (p_Prediction.status == CP_SDK.Chat.Models.Twitch.EHelix_PredictionStatus.RESOLVED)
                        {
                            m_PickButtonBlue.SetInteractable(false);
                            m_PickButtonPink.SetInteractable(false);

                            m_LockButton.SetInteractable(false);

                            m_WindowStart   = Time.realtimeSinceStartup;
                            m_WindowEnd     = Time.realtimeSinceStartup + 60f;
                        }
                        else
                        {
                            m_PickButtonBlue.SetInteractable(false);
                            m_PickButtonPink.SetInteractable(false);

                            m_LockButton.SetInteractable(false);
                        }

                        m_LastPrediction = p_Prediction;
                    });
                }
            }
            else
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    if (CurrentScreen?.gameObject?.activeSelf ?? false)
                        CurrentScreen?.gameObject?.SetActive(false);

                    m_LastPrediction    = p_Prediction;
                    m_WindowEnd         = Time.realtimeSinceStartup;
                });
            }
        }
    }
}
