using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus_SongChartVisualizer.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal class FloatingWindow : BeatSaberPlus.SDK.UI.ResourceViewController<FloatingWindow>
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
                ColorUtility.TryParseHtmlString(value ? "#FFFFFFFF" : "#FFFFFF80", out var l_ColH);
                ColorUtility.TryParseHtmlString(value ? "#FFFFFF80" : "#FFFFFFFF", out var l_ColD);
                m_LockIcon.HighlightColor   = l_ColH;
                m_LockIcon.DefaultColor     = l_ColD;

                var l_FloatingScreen = transform.parent.GetComponent<FloatingScreen>();
                l_FloatingScreen.ShowHandle = value;

                if (l_FloatingScreen.handle)
                {
                    l_FloatingScreen.handle.transform.localScale    = new Vector2(105, 65);
                    l_FloatingScreen.handle.transform.localPosition = Vector3.zero;
                    l_FloatingScreen.handle.transform.localRotation = Quaternion.identity;

                    /// Update handle material
                    var l_ChartFloatingScreenHandleMaterial = GameObject.Instantiate(BeatSaberPlus.SDK.Unity.MaterialU.UINoGlowMaterial);
                    l_ChartFloatingScreenHandleMaterial.color = Color.clear;
                    l_FloatingScreen.handle.gameObject.GetComponent<Renderer>().material = l_ChartFloatingScreenHandleMaterial;
                }

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
                        Logger.Instance.Warning("Failed to get VRPointer!");
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
            if (!SCVConfig.Instance.ShowLockIcon)
                m_LockIcon.gameObject.SetActive(false);

            CP_SDK.Unity.GameObjectU.ChangerLayerRecursive(gameObject, LayerMask.NameToLayer("UI"));

            /// Make icons easier to click
            m_LockIcon.gameObject.AddComponent<SphereCollider>().radius = 10f;
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
