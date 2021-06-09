using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus.Modules.SongChartVisualizer.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal class FloatingWindow : SDK.UI.ResourceViewController<FloatingWindow>
    {
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m__AllowMovement = false;
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m_AllowMovement
        {
            get => m__AllowMovement;
            set {
                m__AllowMovement = value;
                ColorUtility.TryParseHtmlString(value ? "#FFFFFFFF" : "#FFFFFF11", out var l_ColH);
                ColorUtility.TryParseHtmlString(value ? "#FFFFFF11" : "#FFFFFFFF", out var l_ColD);
                m_LockIcon.HighlightColor   = l_ColH;
                m_LockIcon.DefaultColor     = l_ColD;

                var l_FloatingScreen = transform.parent.GetComponent<FloatingScreen>();
                l_FloatingScreen.ShowHandle = value;

                if (value)
                {
                    /// Refresh VR pointer due to bug
                    var l_Pointers = Resources.FindObjectsOfTypeAll<VRPointer>();
                    var l_Pointer  = BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing ? l_Pointers.LastOrDefault() : l_Pointers.FirstOrDefault();

                    if (l_Pointer != null)
                    {
                        if (l_FloatingScreen.screenMover)
                            Destroy(l_FloatingScreen.screenMover);

                        l_FloatingScreen.screenMover = l_Pointer.gameObject.AddComponent<FloatingScreenMoverPointer>();
                        l_FloatingScreen.screenMover.Init(l_FloatingScreen);
                    }
                    else
                    {
                        Logger.Instance.Warn("Failed to get VRPointer!");
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lock icon
        /// </summary>
        [UIComponent("LockIcon")]
        private ClickableImage m_LockIcon = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = Color.white.ColorWithAlpha(0f);

            /// Update lock state
            m_AllowMovement = false;

            /// Hide the lock icon
            if (!Config.SongChartVisualizer.ShowLockIcon)
                m_LockIcon.gameObject.SetActive(false);

            SDK.Unity.GameObject.ChangerLayerRecursive(gameObject, LayerMask.NameToLayer("UI"));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIAction("lock-pressed")]
        internal void OnLockPressed()
        {
            m_AllowMovement = !m_AllowMovement;
        }
    }
}
