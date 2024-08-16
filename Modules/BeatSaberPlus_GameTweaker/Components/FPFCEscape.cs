#if BEATSABER_1_35_0_OR_NEWER
using BGLib.Polyglot;
#else
using Polyglot;
#endif
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.Components
{
    /// <summary>
    /// FPFC Escape component
    /// </summary>
    internal class FPFCEscape : MonoBehaviour
    {
        /// <summary>
        /// Pause menu manager instance
        /// </summary>
        private PauseMenuManager m_IPauseMenuManager;
        private PauseMenuManager m_PauseMenuManager
        {
            get
            {
                if (!m_IPauseMenuManager)
                    m_IPauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();

                return m_IPauseMenuManager;
            }
        }
        /// <summary>
        /// Pause controller instance
        /// </summary>
        private PauseController m_IPauseController;
        private PauseController m_PauseController
        {
            get
            {
                if (!m_IPauseController)
                    m_IPauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();

                return m_IPauseController;
            }
        }
        /// <summary>
        /// Is an FPFC pause requested
        /// </summary>
        private bool m_FPFCPause = false;
        /// <summary>
        /// Back button text
        /// </summary>
        private string m_BackButtonText = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {
            /// Don't activate in menu
            if (CP_SDK_BS.Game.Logic.ActiveScene != CP_SDK_BS.Game.Logic.ESceneType.Playing
                || CP_SDK_BS.Game.Logic.LevelData?.Type == CP_SDK_BS.Game.LevelType.Multiplayer)
                return;

            /// Wait for components
            if (m_PauseController == null || m_PauseMenuManager == null)
                return;

            try
            {
                /// Restore in case of not FPFC pause
                if (m_FPFCPause && !m_PauseMenuManager.enabled)
                {
                    /// Enable localized
                    m_PauseMenuManager._backButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;
                    m_PauseMenuManager._restartButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;
                    m_PauseMenuManager._continueButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;

                    m_FPFCPause = false;
                }

                /// Is visible
                if (m_FPFCPause && m_PauseMenuManager.enabled)
                {
                    bool l_ShouldRestore = false;

                    /// Should resume
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        m_PauseMenuManager.ContinueButtonPressed();
                        l_ShouldRestore = true;
                    }
                    /// Menu button
                    else if (Input.GetKeyDown(KeyCode.M))
                    {
                        m_PauseMenuManager.MenuButtonPressed();
                        l_ShouldRestore = true;
                    }
                    /// Restart button
                    else if (Input.GetKeyDown(KeyCode.R))
                    {
                        m_PauseMenuManager.RestartButtonPressed();
                        l_ShouldRestore = true;
                    }
                    /// Continue button
                    else if (Input.GetKeyDown(KeyCode.C))
                    {
                        m_PauseMenuManager.ContinueButtonPressed();
                        l_ShouldRestore = true;
                    }

                    if (l_ShouldRestore)
                    {
                        /// Enable localized
                        m_PauseMenuManager._backButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;
                        m_PauseMenuManager._restartButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;
                        m_PauseMenuManager._continueButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = true;

                        m_FPFCPause = false;
                    }
                    else
                        m_PauseMenuManager._backButton.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = m_BackButtonText;
                }
                /// Should pause
                else if (!m_PauseMenuManager.enabled && Input.GetKeyDown(KeyCode.Escape))
                {
                    /// Start pause
                    m_PauseController.Pause();
                    m_PauseMenuManager.ShowMenu();

                    /// Disable localized
                    m_PauseMenuManager._backButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;
                    m_PauseMenuManager._restartButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;
                    m_PauseMenuManager._continueButton.transform.GetChild(2).GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;

                    /// Get buttons
                    var l_BackButton     = m_PauseMenuManager._backButton.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
                    var l_RestartButton  = m_PauseMenuManager._restartButton.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
                    var l_ContinueButton = m_PauseMenuManager._continueButton.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>();

                    /// Enable rich text
                    l_BackButton.richText       = true;
                    l_RestartButton.richText    = true;
                    l_ContinueButton.richText   = true;

                    /// Update buttons
                    l_BackButton.text       = "<color=\"red\"><b>[Key M]</b> <color=#FFFFFFBF>" + l_BackButton.text;
                    l_RestartButton.text    = "<color=\"red\"><b>[Key R]</b> <color=#FFFFFFBF>" + l_RestartButton.text;
                    l_ContinueButton.text   = "<color=\"red\"><b>[Key C]</b> <color=#FFFFFFBF>" + l_ContinueButton.text;

                    m_BackButtonText = l_BackButton.text;
                    m_FPFCPause      = true;
                }
            }
            catch (System.Exception)
            {

            }
        }
    }
}
