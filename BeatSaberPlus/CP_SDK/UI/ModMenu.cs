using System;
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Mod menu
    /// </summary>
    public class ModMenu : MonoBehaviour
    {
        private static ModMenu                  m_Instance      = null;
        private static List<ModButton>          m_ModButtons    = new List<ModButton>();
        private static IReadOnlyList<ModButton> m_ModButtonsRO  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Transform                   m_ScreenContainer   = null;
        private Components.CFloatingPanel   m_Screen            = null;
        private Views.ModMenuView           m_View              = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static ModMenu                   Instance => m_Instance;
        public static IReadOnlyList<ModButton>  ModButtons { get { if (m_ModButtonsRO == null) m_ModButtonsRO = m_ModButtons.AsReadOnly(); return m_ModButtonsRO; } }

        public static event Action              OnCreated;
        public static event Action<ModButton>   OnModButtonRegistered;
        public static event Action<ModButton>   OnModButtonChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public Transform                    ScreenContainer => m_ScreenContainer;
        public Components.CFloatingPanel    Screen          => m_Screen;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create the mod menu
        /// </summary>
        internal static void Create()
        {
            if (m_Instance)
                return;

            m_Instance = new GameObject("[CP_SDK.UI.ModMenu]", typeof(ModMenu)).GetComponent<ModMenu>();
            GameObject.DontDestroyOnLoad(m_Instance.gameObject);

            ChatPlexSDK.OnGenericSceneChange += (p_Scene) => {
                if (p_Scene == ChatPlexSDK.EGenericScene.Menu)
                    return;

                m_Instance?.Dismiss();
            };

            m_Instance.Dismiss();
        }
        /// <summary>
        /// Destroy
        /// </summary>
        internal static void Destroy()
        {
            if (!m_Instance)
                return;

            GameObject.Destroy(m_Instance.gameObject);
            m_Instance = null;
        }
        /// <summary>
        /// Register a mod button
        /// </summary>
        /// <param name="p_Button">Button to register</param>
        public static void Register(ModButton p_Button)
        {
            if (m_ModButtons.Contains(p_Button))
                return;

            m_ModButtons.Add(p_Button);
            m_ModButtons.Sort((x, y) => x.Text.CompareTo(y.Text));

            if (m_Instance && m_Instance.m_View)
                m_Instance.m_View.Refresh();

            try { OnModButtonRegistered?.Invoke(p_Button); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ModMenu.Register] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// Fire on changed
        /// </summary>
        /// <param name="p_Button">On button changed</param>
        internal static void FireOnModButtonChanged(ModButton p_Button)
        {
            if (m_Instance && m_Instance.m_View && m_Instance.m_View.gameObject.activeInHierarchy)
                m_Instance.m_View.Refresh();

            try { OnModButtonChanged?.Invoke(p_Button); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ModMenu.FireOnModButtonChanged] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Present the mod menu
        /// </summary>
        public void Present()
        {
            if (!m_ScreenContainer)
                Init();

            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
        }
        /// <summary>
        /// Dismiss the mod menu
        /// </summary>
        public void Dismiss()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init the mod menu
        /// </summary>
        private void Init()
        {
            transform.position   = Vector3.zero;
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            m_ScreenContainer = new GameObject("[CP_SDK.UI.ModMenu.ScreenContainer]").transform;
            m_ScreenContainer.SetParent(transform, false);
            m_ScreenContainer.localPosition = new Vector3(0.00f, 0.85f, 2.90f);

            m_Screen    = UISystem.FloatingPanelFactory.Create("[CP_SDK.UI.ModMenu.ScreenContainer.Screen]", m_ScreenContainer);
            m_View      = UISystem.CreateViewController<Views.ModMenuView>();
            m_Screen.SetTransformDirect(new Vector3(-2.60f, 0.00f, -0.815f), new Vector3(0.00f, -40.00f, 0.00f));
            m_Screen.SetSize(new Vector2(120.0f, 80.0f));
            m_Screen.SetRadius(0.0f);
            m_Screen.SetBackground(true);
            m_Screen.SetViewController(m_View);

            try { OnCreated?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ModMenu.Init] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            gameObject.SetActive(false);
        }
    }
}
