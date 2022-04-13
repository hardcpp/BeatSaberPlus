using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Hype train floating window
    /// </summary>
    internal class FloatingWindow_HypeTrain : BeatSaberPlus.SDK.UI.ResourceViewController<FloatingWindow_HypeTrain>
    {
        public static float HEIGHT = 7f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TopFrame")] private GameObject m_TopFrame = null;
        [UIComponent("Label")] private TMPro.TextMeshProUGUI m_Label = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch service instance
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService m_Service = null;
        /// <summary>
        /// Latest hype train data
        /// </summary>
        private BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_HypeTrain m_LastHypeTrain = null;
        /// <summary>
        /// Filler progress bar
        /// </summary>
        private UnityEngine.UI.Image m_Filler = null;
        /// <summary>
        /// Current hype train progression
        /// </summary>
        private float m_CurrentProgression = 0f;
        /// <summary>
        /// Current hype train expire time
        /// </summary>
        private float m_CurrentExpire = 0f;
        /// <summary>
        /// Current hype train level
        /// </summary>
        private int m_CurrentLevel = 0;
        /// <summary>
        /// Displayed hype train progression
        /// </summary>
        private float m_DisplayedProgression = 0f;
        /// <summary>
        /// Displayed hype train remaining time
        /// </summary>
        private int m_DisplayedRemaining = 0;
        /// <summary>
        /// Displayed hype train level
        /// </summary>
        private int m_DisplayedLevel = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Background        = m_TopFrame.AddComponent<UnityEngine.UI.Image>();
            l_Background.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            l_Background.type       = UnityEngine.UI.Image.Type.Filled;
            l_Background.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            l_Background.fillAmount = 1f;
            l_Background.color      = new Color32(24, 24, 26, 255);
            l_Background.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            m_Filler            = m_TopFrame.transform.GetChild(0).gameObject.AddComponent<UnityEngine.UI.Image>();
            m_Filler.sprite     = BeatSaberPlus.SDK.Unity.Sprite.CreateFromTexture(Texture2D.whiteTexture);
            m_Filler.type       = UnityEngine.UI.Image.Type.Filled;
            m_Filler.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            m_Filler.fillAmount = 0.33f;
            m_Filler.color      = new Color32(120, 44, 232, 255);
            m_Filler.material   = BeatSaberPlus.SDK.Unity.Material.UINoGlowMaterial;

            BeatSaberPlus.SDK.Chat.Service.Acquire();
            var l_TwitchService = BeatSaberPlus.SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                m_Service = l_TwitchService as BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService;
                m_Service.HelixAPI.OnActiveHypeTrainChanged += HelixAPI_OnActiveHypeTrainChanged;
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
                m_Service.HelixAPI.OnActiveHypeTrainChanged -= HelixAPI_OnActiveHypeTrainChanged;

            BeatSaberPlus.SDK.Chat.Service.Release();
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
                var l_HasExpired = (m_CurrentExpire + 60) < Time.realtimeSinceStartup;
                if (l_HasExpired)
                    gameObject.SetActive(false);
                else
                {
                    m_Filler.fillAmount         = Mathf.Lerp(m_Filler.fillAmount,       Mathf.Min(1f, m_CurrentProgression),    Time.smoothDeltaTime * 2.5f);
                    var l_NewDisplayProgression = Mathf.Lerp(m_DisplayedProgression,    m_CurrentProgression,                   Time.smoothDeltaTime * 2.5f);
                    var l_RemainingSeconds      = (int)Mathf.Max(0f, m_CurrentExpire - Time.realtimeSinceStartup);

                    if (m_CurrentLevel != m_DisplayedLevel || Mathf.Abs(l_NewDisplayProgression - m_DisplayedProgression) >= 0.0001 || l_RemainingSeconds != m_DisplayedRemaining)
                    {
                        m_DisplayedLevel        = m_CurrentLevel;
                        m_DisplayedProgression  = l_NewDisplayProgression;
                        m_DisplayedRemaining    = l_RemainingSeconds;

                        var l_Minutes = l_RemainingSeconds / 60;
                        var l_Seconds = l_RemainingSeconds - (l_Minutes * 60);

                        m_Label.text = $"<line-height=1%><align=\"left\"><b>LVL {m_DisplayedLevel}</b> - Hype Train!\n<line-height=100%><align=\"right\">{Mathf.RoundToInt(m_DisplayedProgression * 100.0f)}% {l_Minutes}:{l_Seconds.ToString().PadLeft(2, '0')}";
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active hype train changed
        /// </summary>
        /// <param name="p_HypeTrain">Current hype train</param>
        private void HelixAPI_OnActiveHypeTrainChanged(BeatSaberPlus.SDK.Chat.Models.Twitch.Helix_HypeTrain p_HypeTrain)
        {
            BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                if (p_HypeTrain != null)
                {
                    var l_HasExpired = p_HypeTrain.event_data.expires_at.AddSeconds(60) < DateTime.UtcNow;
                    if (l_HasExpired && gameObject.activeSelf)
                        gameObject.SetActive(false);
                    else if (!l_HasExpired)
                    {
                        if (!gameObject.activeSelf)
                            gameObject.SetActive(true);

                        var l_Progress = p_HypeTrain.event_data.goal == 0 ? 0f :(float)p_HypeTrain.event_data.total / (float)p_HypeTrain.event_data.goal;

                        m_CurrentExpire         = Time.realtimeSinceStartup + (float)((p_HypeTrain.event_data.expires_at - DateTime.UtcNow).TotalSeconds);
                        m_CurrentProgression    = l_Progress;
                        m_CurrentLevel          = p_HypeTrain.event_data.level;
                    }
                }

                m_LastHypeTrain = p_HypeTrain;
            });
        }
    }
}
