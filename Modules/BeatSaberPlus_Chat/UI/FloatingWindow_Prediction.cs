using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Prediction floating window
    /// </summary>
    internal class FloatingWindow_Prediction : BeatSaberPlus.SDK.UI.ResourceViewController<FloatingWindow_Prediction>
    {
        public static Vector2 SIZE = new Vector2(80, 75);

        private static Color TIME_PROGRESSBAR_BACKGROUND = new Color32(70, 70, 73, 255);
        private static Color TIME_PROGRESSBAR_FILLER = new Color32(164, 115, 251, 255);

        private static string TAG_BLUE = "<#" + ColorUtility.ToHtmlStringRGB(new Color32( 56, 122, 255, 255)) + ">";
        private static string TAG_PINK = "<#" + ColorUtility.ToHtmlStringRGB(new Color32(245,   0, 155, 255)) + ">";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("Subject")]        private TMPro.TextMeshProUGUI m_Subject = null;
        [UIComponent("Labels")]         private TMPro.TextMeshProUGUI m_Labels = null;
        [UIComponent("Percentages")]    private TMPro.TextMeshProUGUI m_Percentages = null;
        [UIComponent("Points")]         private TMPro.TextMeshProUGUI m_Points = null;
        [UIComponent("Ratios")]         private TMPro.TextMeshProUGUI m_Ratios = null;
        [UIComponent("Votees")]         private TMPro.TextMeshProUGUI m_Votees = null;


        [UIObject("LockButtonFrame")]   private GameObject m_LockButtonFrame = null;
        [UIComponent("LockButton")]     private Button m_LockButton = null;

        [UIObject("PickButtons")]       private GameObject m_PickButtons = null;
        [UIComponent("PickBlueButton")] private Button m_PickBlueButton = null;
        [UIComponent("PickPinkButton")] private Button m_PickPinkButton = null;
        [UIComponent("CancelButton")]   private Button m_CancelButton = null;
        [UIObject("TimeFrame")]         private GameObject m_TimeFrame = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch service instance
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService m_Service = null;
        /// <summary>
        /// Latest prediction data
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction m_LastPrediction = null;
        /// <summary>
        /// Blue target %
        /// </summary>
        private float m_BluePctTarget = 0f;
        /// <summary>
        /// Blue displayed %
        /// </summary>
        private float m_BluePctDisplayed = 0f;
        /// <summary>
        /// Pink target %
        /// </summary>
        private float m_PinkPctTarget = 0f;
        /// <summary>
        /// Pink displayed %
        /// </summary>
        private float m_PinkPctDisplayed = 0f;
        /// <summary>
        /// Betting window start RealmTime
        /// </summary>
        private float m_WindowStart = 0f;
        /// <summary>
        /// Betting window end RealmTime
        /// </summary>
        private float m_WindowEnd = 1f;
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

            SetupTimeFrame();

            BeatSaberPlus.SDK.Chat.Service.Acquire();
            var l_TwitchService = BeatSaberPlus.SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_Service = l_TwitchService as BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService;
                m_Service.HelixAPI.OnActivePredictionChanged += HelixAPI_OnActivePredictionChanged;
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
                m_Service.HelixAPI.OnActivePredictionChanged -= HelixAPI_OnActivePredictionChanged;

            BeatSaberPlus.SDK.Chat.Service.Release();
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
                var l_HasExpired =  (m_LastPrediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED
                                        && m_WindowEnd < Time.realtimeSinceStartup)
                                    || m_LastPrediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.CANCELED;

                if (l_HasExpired)
                    gameObject.SetActive(false);
                else
                {
                    if (m_LastPrediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.ACTIVE
                        || m_LastPrediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED)
                    {
                        if (m_WindowStart < 0)
                        {
                            var l_Offset    = -m_WindowStart;
                            var l_NewStart  = m_WindowStart + l_Offset;
                            var l_NewEnd    = m_WindowEnd + l_Offset;

                            m_TimeProgressBar.fillAmount = Mathf.Max(0, 1f - (((Time.realtimeSinceStartup + l_Offset) - l_NewStart) / (l_NewEnd - l_NewStart)));
                        }
                        else
                            m_TimeProgressBar.fillAmount = Mathf.Max(0, 1f - ((Time.realtimeSinceStartup - m_WindowStart) / (m_WindowEnd - m_WindowStart)));
                    }

                    var l_NewBluePct = Mathf.Lerp(m_BluePctDisplayed, m_BluePctTarget, Time.smoothDeltaTime * 2.5f);
                    var l_NewPinkPct = Mathf.Lerp(m_PinkPctDisplayed, m_PinkPctTarget, Time.smoothDeltaTime * 2.5f);

                    if (Mathf.Abs(l_NewBluePct - m_BluePctDisplayed) >= 0.0001 || Mathf.Abs(l_NewPinkPct - m_PinkPctDisplayed) >= 0.0001)
                    {
                        m_BluePctDisplayed = l_NewBluePct;
                        m_PinkPctDisplayed = l_NewPinkPct;

                        m_Percentages.text = $"<line-height=1%><align=\"left\">{TAG_BLUE}{Mathf.RoundToInt(l_NewBluePct * 100f)}%\n <line-height=100%><align=\"right\">{TAG_PINK}{Mathf.RoundToInt(m_PinkPctDisplayed * 100f)}%";
                    }
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup frame for progress bar & text
        /// </summary>
        private void SetupTimeFrame()
        {
            var l_Background = m_TimeFrame.AddComponent<UnityEngine.UI.Image>();
            l_Background.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            l_Background.type       = UnityEngine.UI.Image.Type.Filled;
            l_Background.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            l_Background.fillAmount = 1f;
            l_Background.color      = TIME_PROGRESSBAR_BACKGROUND;
            l_Background.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            var l_Filler = m_TimeFrame.transform.GetChild(0).gameObject.AddComponent<UnityEngine.UI.Image>();
            l_Filler.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            l_Filler.type       = UnityEngine.UI.Image.Type.Filled;
            l_Filler.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            l_Filler.fillAmount = 1f;
            l_Filler.color      = TIME_PROGRESSBAR_FILLER;
            l_Filler.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;
            m_TimeProgressBar = l_Filler;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lock votes
        /// </summary>
        [UIAction("click-lock")]
        private void OnLockButton()
        {
            ShowConfirmationModal("Lock the votes?", () =>
            {
                m_LockButton.interactable = false;

                m_Service.HelixAPI.EndPrediction(m_LastPrediction.id, BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.LOCKED, null, (_, x) =>
                {
                    HelixAPI_OnActivePredictionChanged(x);
                });
            });
        }
        /// <summary>
        /// Pick blue
        /// </summary>
        [UIAction("click-pick-blue")]
        private void OnPickBlueButton()
        {
            ShowConfirmationModal("Make blue win?", () =>
            {
                m_PickBlueButton.interactable = false;
                m_PickPinkButton.interactable = false;

                var l_Winner = m_LastPrediction.outcomes.FirstOrDefault(x => x.color == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Color.BLUE)?.id ?? null;
                m_Service.HelixAPI.EndPrediction(m_LastPrediction.id, BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED, l_Winner, (_, x) =>
                {
                    HelixAPI_OnActivePredictionChanged(x);
                });
            });
        }
        /// <summary>
        /// Pick pink
        /// </summary>
        [UIAction("click-pick-pink")]
        private void OnPickPinkButton()
        {
            ShowConfirmationModal("Make pink win?", () =>
            {
                m_PickBlueButton.interactable = false;
                m_PickPinkButton.interactable = false;

                var l_Winner = m_LastPrediction.outcomes.FirstOrDefault(x => x.color == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Color.PINK)?.id ?? null;
                m_Service.HelixAPI.EndPrediction(m_LastPrediction.id, BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED, l_Winner, (_, x) =>
                {
                    HelixAPI_OnActivePredictionChanged(x);
                });
            });
        }
        /// <summary>
        /// Cancel prediction
        /// </summary>
        [UIAction("click-cancel")]
        private void OnCancelButton()
        {
            ShowConfirmationModal("Cancel prediction?", () =>
            {
                m_CancelButton.interactable = false;
                m_Service.HelixAPI.EndPrediction(m_LastPrediction.id, BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.CANCELED, null, (_, x) =>
                {
                    HelixAPI_OnActivePredictionChanged(x);
                });
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active prediction changed
        /// </summary>
        /// <param name="p_Prediction">Active prediction</param>
        private void HelixAPI_OnActivePredictionChanged(BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction p_Prediction)
        {
            if (p_Prediction != null)
            {
                var l_HasExpired = (p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED
                                        && (p_Prediction.ended_at?.AddSeconds(60) < DateTime.UtcNow))
                                    || p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.CANCELED;

                if (l_HasExpired)
                {
                    BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                    {
                        if (gameObject.activeSelf)
                            gameObject.SetActive(false);

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

                    BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                    {
                        if (!gameObject.activeSelf)
                        {
                            gameObject.SetActive(true);

                            m_LockButton.interactable = true;
                            m_PickBlueButton.interactable = true;
                            m_PickPinkButton.interactable = true;
                        }

                        if (m_LastPrediction == null || m_LastPrediction.id != p_Prediction.id)
                        {
                            var l_LeftTitle     = p_Prediction.outcomes[0].title;
                            var l_RightTitle    = p_Prediction.outcomes[1].title;
                            var l_MaxTitle      = 20;

                            if (l_LeftTitle.Length > l_MaxTitle)    l_LeftTitle     = l_LeftTitle.Substring(0, l_MaxTitle - 3) + "...";
                            if (l_RightTitle.Length > l_MaxTitle)   l_RightTitle    = l_RightTitle.Substring(0, l_MaxTitle - 3) + "...";

                            m_Subject.text      = p_Prediction.title;
                            m_Labels.text       = $"<line-height=1%><align=\"left\">{TAG_BLUE}{l_LeftTitle}\n<line-height=100%><align=\"right\">{TAG_PINK}{l_RightTitle}";
                            m_Percentages.text  = $"<line-height=1%><align=\"left\">{TAG_BLUE}0%\n <line-height=100%><align=\"right\">{TAG_PINK}0%";

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
                            m_Labels.text       = $"<line-height=1%><align=\"left\">{l_LeftPrefix}{l_LeftTitle}\n<line-height=100%><align=\"right\">{l_RightPrefix}{l_RightTitle}";
                            m_Percentages.text  = $"<line-height=1%><align=\"left\">{l_LeftPrefix}{Mathf.RoundToInt(m_BluePctTarget * 100f)}%\n <line-height=100%><align=\"right\">{l_RightPrefix}{Mathf.RoundToInt(m_PinkPctTarget * 100f)}%";
                            l_Points            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}⭕ {p_Prediction.outcomes[0].channel_points}\n<line-height=100%><align=\"right\">{l_RightPrefix}{p_Prediction.outcomes[1].channel_points} ⭕";
                            l_Ratios            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}🏆 {l_BlueRatio}\n<line-height=100%><align=\"right\">{l_RightPrefix}{l_PinkRatio} 🏆";
                            l_Votees            = $"<line-height=1%><align=\"left\">{l_LeftPrefix}👥 {p_Prediction.outcomes[0].users}\n<line-height=100%><align=\"right\">{l_RightPrefix}{p_Prediction.outcomes[1].users} 👥";
                        }

                        m_Points.text = l_Points;
                        m_Ratios.text = l_Ratios;
                        m_Votees.text = l_Votees;

                        m_LockButtonFrame.SetActive(p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.ACTIVE);

                        m_PickButtons.SetActive(p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.LOCKED
                                                || p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED);

                        m_CancelButton.interactable = p_Prediction.status != BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED;

                        m_WindowStart   = Time.realtimeSinceStartup - Mathf.Abs((float)(DateTime.UtcNow - p_Prediction.created_at).TotalSeconds);
                        m_WindowEnd     = m_WindowStart + p_Prediction.prediction_window;

                        if (p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.LOCKED)
                            m_TimeProgressBar.fillAmount = 0f;
                        else if (p_Prediction.status == BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_Prediction.Status.RESOLVED)
                        {
                            m_PickBlueButton.interactable = false;
                            m_PickPinkButton.interactable = false;

                            m_WindowStart   = Time.realtimeSinceStartup;
                            m_WindowEnd     = Time.realtimeSinceStartup + 60f;
                        }

                        if (m_LastPrediction == null || m_LastPrediction.status != p_Prediction.status)
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(m_PickButtons.transform.parent.transform as RectTransform);

                        m_LastPrediction = p_Prediction;
                    });
                }
            }
            else
            {
                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    if (gameObject.activeSelf)
                        gameObject.SetActive(false);

                    m_LastPrediction    = p_Prediction;
                    m_WindowEnd         = Time.realtimeSinceStartup;
                });
            }
        }
    }
}
