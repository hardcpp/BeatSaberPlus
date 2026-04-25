using CP_SDK.XUI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Hype train floating panel view
    /// </summary>
    internal sealed class HypeTrainFloatingPanelView : CP_SDK.UI.ViewController<HypeTrainFloatingPanelView>
    {
        public static readonly float HEIGHT = 7f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIHLayout  m_Filler    = null;
        private XUIText     m_Label     = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK.Chat.Services.Twitch.TwitchService    m_TwitchService         = null;
        private CP_SDK.Chat.Models.Twitch.EventSub_HypeTrain m_LastHypeTrain         = null;
        private float                                        m_CurrentProgression    = 0f;
        private float                                        m_CurrentExpire         = 0f;
        private int                                          m_CurrentLevel          = 0;
        private float                                        m_DisplayedProgression  = 0f;
        private int                                          m_DisplayedRemaining    = 0;
        private int                                          m_DisplayedLevel        = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            var whiteSprite = CP_SDK.Unity.SpriteU.CreateFromTexture(Texture2D.whiteTexture);

            XUIHLayout.Make(
                XUIHLayout.Make(
                    XUIText.Make("<line-height=1%><align=\"left\"><b>LVL 1</b> - Hype Train!\n<line-height=100%><align=\"right\">33% 1:33")
                        .SetFontSize(5f)
                        .SetAlign(TMPro.TextAlignmentOptions.MidlineLeft)
                        .Bind(ref m_Label)
                )
                .SetPadding(0, 2, 0, 2).SetSpacing(0)
                .SetBackground(true, new Color32(120, 44, 232, 255))
                .SetBackgroundSprite(whiteSprite, Image.Type.Filled)
                .SetBackgroundFillMethod(Image.FillMethod.Horizontal)
                .SetBackgroundFillAmount(0.33f)
                .OnReady(x =>
                {
                    x.HLayoutGroup.childForceExpandWidth    = true;
                    x.HLayoutGroup.childForceExpandHeight   = true;
                    x.CSizeFitter.enabled                   = false;
                    x.LElement.ignoreLayout                 = true;
                    x.RTransform.anchorMin                  = Vector2.zero;
                    x.RTransform.anchorMax                  = Vector2.one;
                })
                .Bind(ref m_Filler)
            )
            .SetPadding(0).SetSpacing(0)
            .SetBackground(true, new Color32(24, 24, 26, 255))
            .SetBackgroundSprite(whiteSprite, Image.Type.Simple)
            .OnReady(x =>
            {
                x.HLayoutGroup.childForceExpandWidth    = true;
                x.HLayoutGroup.childForceExpandHeight   = true;
                x.CSizeFitter.enabled                   = false;
                x.LElement.ignoreLayout                 = true;
                x.RTransform.anchorMin                  = Vector2.zero;
                x.RTransform.anchorMax                  = Vector2.one;
            })
            .BuildUI(transform);

            CP_SDK.Chat.Service.Acquire();
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_TwitchService = l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService;
                m_TwitchService.EventSub.OnActiveHypeTrainChanged += HelixAPI_OnActiveHypeTrainChanged;
            }
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Hide by default
            CurrentScreen?.gameObject?.SetActive(false);
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected override void OnViewDestruction()
        {
            if (m_TwitchService != null)
                m_TwitchService.EventSub.OnActiveHypeTrainChanged -= HelixAPI_OnActiveHypeTrainChanged;

            CP_SDK.Chat.Service.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_LastHypeTrain != null)
            {
                var hasExpired = (m_CurrentExpire + 60) < Time.realtimeSinceStartup;
                if (hasExpired)
                    CurrentScreen?.gameObject?.SetActive(false);
                else
                {
                    m_Filler.Element.SetBackgroundFillAmount(
                        Mathf.Lerp(
                            m_Filler.Element.GetBackgroundFillAmount(),
                            Mathf.Min(1f, m_CurrentProgression),
                            Time.smoothDeltaTime * 2.5f
                        )
                    );

                    var newDisplayProgression = Mathf.Lerp(m_DisplayedProgression,    m_CurrentProgression, Time.smoothDeltaTime * 2.5f);
                    var remainingSeconds      = (int)Mathf.Max(0f, m_CurrentExpire - Time.realtimeSinceStartup);

                    if (m_CurrentLevel != m_DisplayedLevel || Mathf.Abs(newDisplayProgression - m_DisplayedProgression) >= 0.0001 || remainingSeconds != m_DisplayedRemaining)
                    {
                        m_DisplayedLevel        = m_CurrentLevel;
                        m_DisplayedProgression  = newDisplayProgression;
                        m_DisplayedRemaining    = remainingSeconds;

                        var minutes = remainingSeconds / 60;
                        var seconds = remainingSeconds - (minutes * 60);

                        m_Label.SetText(
                            $"<line-height=1%><align=\"left\"><b>LVL {m_DisplayedLevel}</b> - Hype Train!\n<line-height=100%><align=\"right\">" +
                            $"{Mathf.RoundToInt(m_DisplayedProgression * 100.0f)}% {minutes}:{seconds.ToString().PadLeft(2, '0')}"
                        );
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active hype train changed
        /// </summary>
        /// <param name="hypeTrainData">Current hype train</param>
        private void HelixAPI_OnActiveHypeTrainChanged(CP_SDK.Chat.Models.Twitch.EventSub_HypeTrain hypeTrainData)
        {
            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                if (hypeTrainData != null)
                {
                    var hasExpired = hypeTrainData.expires_at.AddSeconds(60) < DateTime.UtcNow;
                    if (hasExpired && CurrentScreen && CurrentScreen.gameObject.activeSelf)
                        CurrentScreen.gameObject.SetActive(false);
                    else if (!hasExpired)
                    {
                        if (CurrentScreen && !CurrentScreen.gameObject.activeSelf)
                            CurrentScreen.gameObject.SetActive(true);

                        var progress = 1.0f;
                        if (hypeTrainData.goal != null)
                            progress = (float)hypeTrainData.total / (float)hypeTrainData.goal;

                        m_CurrentExpire         = Time.realtimeSinceStartup + (float)((hypeTrainData.expires_at - DateTime.UtcNow).TotalSeconds);
                        m_CurrentProgression    = progress;
                        m_CurrentLevel          = hypeTrainData.level;
                    }
                }

                m_LastHypeTrain = hypeTrainData;
            });
        }
    }
}
