using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Loading progress bar
    /// </summary>
    public class LoadingProgressBar : PersistentSingleton<LoadingProgressBar>
    {
        private static readonly Vector3 POSITION            = new Vector3(0, 2.5f, 4f);
        private static readonly Vector3 ROTATION            = new Vector3(0, 0, 0);
        private static readonly Vector3 SCALE               = new Vector3(0.01f, 0.01f, 0.01f);
        private static readonly Vector2 CANVAS_SIZE         = new Vector2(100, 50);
        private static readonly Vector2 LOADING_BAR_SIZE    = new Vector2(100, 10);
        private static readonly Vector2 HEADER_POSITION     = new Vector2(0, 15);
        private static readonly Vector2 HEADER_SIZE         = new Vector2(100, 20);
        private static readonly Color   BACKGROUND_COLOR    = new Color(0, 0, 0, 0.2f);
        private const           float   HEADER_FONT_SIZE    = 14f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Root canvas
        /// </summary>
        private Canvas m_Canvas;
        /// <summary>
        /// Top text instance
        /// </summary>
        private TMP_Text m_HeaderText;
        /// <summary>
        /// Image faking a loading bar background
        /// </summary>
        private Image m_LoadingBackground;
        /// <summary>
        /// Image faking the loading bar filling
        /// </summary>
        private Image m_LoadingBar;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        private void Awake()
        {
            gameObject.name                     = "BSP_LoadingProgressBar";
            gameObject.transform.position       = POSITION;
            gameObject.transform.eulerAngles    = ROTATION;
            gameObject.transform.localScale     = SCALE;

            m_Canvas = gameObject.AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.WorldSpace;
            m_Canvas.enabled = false;
            var l_RectTransform = m_Canvas.transform as RectTransform;
            l_RectTransform.sizeDelta = CANVAS_SIZE;

            m_HeaderText = BeatSaberMarkupLanguage.BeatSaberUI.CreateText(m_Canvas.transform as RectTransform, "", HEADER_POSITION);
            l_RectTransform = m_HeaderText.transform as RectTransform;
            l_RectTransform.SetParent(m_Canvas.transform, false);
            l_RectTransform.anchoredPosition    = HEADER_POSITION;
            l_RectTransform.sizeDelta           = HEADER_SIZE;
            m_HeaderText.fontSize               = HEADER_FONT_SIZE;
            m_HeaderText.alignment              = TextAlignmentOptions.Midline;

            m_LoadingBackground = new GameObject("Background").AddComponent<Image>();
            l_RectTransform = m_LoadingBackground.transform as RectTransform;
            l_RectTransform.SetParent(m_Canvas.transform, false);
            l_RectTransform.sizeDelta   = LOADING_BAR_SIZE;
            m_LoadingBackground.color   = BACKGROUND_COLOR;

            m_LoadingBar = new GameObject("Loading Bar").AddComponent<Image>();
            l_RectTransform = m_LoadingBar.transform as RectTransform;
            l_RectTransform.SetParent(m_Canvas.transform, false);
            l_RectTransform.sizeDelta   = LOADING_BAR_SIZE;
            m_LoadingBar.sprite         = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f, 100, 1);
            m_LoadingBar.type           = Image.Type.Filled;
            m_LoadingBar.fillMethod     = Image.FillMethod.Horizontal;
            m_LoadingBar.color          = new Color(0.1f, 1, 0.1f, 0.5f);

            DontDestroyOnLoad(gameObject);

            SDK.Game.Logic.OnSceneChange += Logic_OnSceneChange;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show a message with a hide timer
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Time">Time before disapearing</param>
        public void ShowTimedMessage(string p_Message, float p_Time)
        {
            Unity.MainThreadInvoker.Enqueue(() =>
            {
                StopAllCoroutines();

                m_HeaderText.text           = p_Message;
                m_LoadingBar.enabled        = false;
                m_LoadingBackground.enabled = false;
                m_Canvas.enabled            = true;

                StartCoroutine(Coroutine_DisableCanvas(p_Time));
            });
        }
        /// <summary>
        /// Show loading progress bar with a message
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Progress">Current progress</param>
        public void ShowLoadingProgressBar(string p_Message, float p_Progress)
        {
            Unity.MainThreadInvoker.Enqueue(() =>
            {
                StopAllCoroutines();

                m_HeaderText.text           = p_Message;
                m_LoadingBar.enabled        = true;
                m_LoadingBar.fillAmount     = p_Progress;
                m_LoadingBackground.enabled = true;
                m_Canvas.enabled            = true;
            });
        }
        /// <summary>
        /// Set current progress and displayed message
        /// </summary>
        /// <param name="p_Message">Displayed message</param>
        /// <param name="p_Progress">Loading progress</param>
        public void SetProgress(string p_Message, float p_Progress)
        {
            Unity.MainThreadInvoker.Enqueue(() =>
            {
                StopAllCoroutines();

                m_HeaderText.text       = p_Message;
                m_LoadingBar.fillAmount = p_Progress;
            });
        }
        /// <summary>
        /// Set hide timer
        /// </summary>
        /// <param name="p_Time">Time in seconds</param>
        public void HideTimed(float p_Time)
        {
            Unity.MainThreadInvoker.Enqueue(() =>
            {
                StopAllCoroutines();
                StartCoroutine(Coroutine_DisableCanvas(p_Time));
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On scene changed
        /// </summary>
        /// <param name="p_NewScene">New scene type</param>
        private void Logic_OnSceneChange(Game.Logic.SceneType p_NewScene)
        {
            if (p_NewScene != Game.Logic.SceneType.Menu)
            {
                StopAllCoroutines();
                m_Canvas.enabled = false;
            }
        }
        /// <summary>
        /// Timed canvas disabler
        /// </summary>
        /// <param name="p_Time">Time in seconds</param>
        /// <returns></returns>
        private IEnumerator Coroutine_DisableCanvas(float p_Time)
        {
            yield return new WaitForSecondsRealtime(p_Time);
            m_Canvas.enabled = false;
        }
    }
}
