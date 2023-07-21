using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Floating Panel component
    /// </summary>
    public abstract class CFloatingPanel : IScreen
    {
        private IViewController                                                 m_CurrentViewController = null;
        private bool                                                            m_AllowMovement         = false;
        private bool                                                            m_AutoLockOnSceneSwitch = true;
        private bool                                                            m_AlignWithFloor        = true;
        private Dictionary<ChatPlexSDK.EGenericScene, (Vector3, Vector3)>       m_SceneTransforms       = new Dictionary<ChatPlexSDK.EGenericScene, (Vector3, Vector3)>();
        private Dictionary<ChatPlexSDK.EGenericScene, Action<Vector3, Vector3>> m_OnSceneRelease        = new Dictionary<ChatPlexSDK.EGenericScene, Action<Vector3, Vector3>>();
        private Image                                                           m_Background            = null;
        private CIconButton                                                     m_LockIcon              = null;
        private CIconButton                                                     m_GearIcon              = null;

        private event Action<CFloatingPanel>            m_OnSceneRelocated;
        private event Action<CFloatingPanel, Vector2>   m_OnSizeChanged;
        private event Action<CFloatingPanel>            m_OnGearIcon;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public enum ECorner
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IViewController CurrentViewController => m_CurrentViewController;
        public abstract RectTransform   RTransform { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Replace active view controller
        /// </summary>
        /// <param name="p_ViewController">New view controller</param>
        public override void SetViewController(IViewController p_ViewController)
        {
            if (p_ViewController && p_ViewController.CurrentScreen == this)
                return;

            if (m_CurrentViewController)
                m_CurrentViewController.__Deactivate();

            if (p_ViewController && p_ViewController.CurrentScreen)
                p_ViewController.CurrentScreen.SetViewController(null);

            m_CurrentViewController = p_ViewController;
            if (p_ViewController) p_ViewController.__Activate(this);

            if (m_Background)   m_Background.transform.SetAsFirstSibling();
            if (m_LockIcon)     m_LockIcon.transform.SetAsLastSibling();
            if (m_GearIcon)     m_GearIcon.transform.SetAsLastSibling();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On grab event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CFloatingPanel OnGrab(Action<CFloatingPanel> p_Functor, bool p_Add = true);
        /// <summary>
        /// On release event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CFloatingPanel OnRelease(Action<CFloatingPanel> p_Functor, bool p_Add = true);
        /// <summary>
        /// On scene relocated icon event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public CFloatingPanel OnSceneRelocated(Action<CFloatingPanel> p_Functor, bool p_Add = true)
        {
            if (p_Add) m_OnSceneRelocated += p_Functor;
            else       m_OnSceneRelocated -= p_Functor;

            return this;
        }
        /// <summary>
        /// On scene relocated icon event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public CFloatingPanel OnSizeChanged(Action<CFloatingPanel, Vector2> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnSizeChanged += p_Functor;
            else        m_OnSizeChanged -= p_Functor;

            return this;
        }
        /// <summary>
        /// On gear icon event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public CFloatingPanel OnGearIcon(Action<CFloatingPanel> p_Functor, bool p_Add = true)
        {
            if (p_Add) m_OnGearIcon += p_Functor;
            else       m_OnGearIcon -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get allow movement
        /// </summary>
        /// <returns></returns>
        public virtual bool GetAllowMovement() => m_AllowMovement;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set align with floor
        /// </summary>
        /// <param name="p_Align">Align</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetAlignWithFloor(bool p_Align)
        {
            m_AlignWithFloor = p_Align;
            return this;
        }
        /// <summary>
        /// Set allow movements
        /// </summary>
        /// <param name="p_Allow">Is allowed?</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetAllowMovement(bool p_Allow)
        {
            m_AllowMovement = p_Allow;
            if (m_LockIcon)
            {
                m_LockIcon.SetSprite(p_Allow ? UISystem.GetUIIconUnlockedSprite() : UISystem.GetUIIconLockedSprite());
                m_LockIcon.SetColor(GetAllowMovement() ? ColorU.ToUnityColor("#D0FCB3") : Color.white);
            }
            return this;
        }
        /// <summary>
        /// Set background state
        /// </summary>
        /// <param name="p_Enabled">Is enabled?</param>
        /// <param name="p_Color">Optional color</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetBackground(bool p_Enabled, Color? p_Color = null)
        {
            if (p_Enabled)
            {
                if (!m_Background)
                {
                    m_Background = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
                    m_Background.gameObject.layer = UISystem.UILayer;
                    m_Background.rectTransform.SetParent(transform, false);
                    m_Background.rectTransform.SetAsFirstSibling();
                    m_Background.rectTransform.localPosition    = Vector3.zero;
                    m_Background.rectTransform.localRotation    = Quaternion.identity;
                    m_Background.rectTransform.localScale       = Vector3.one;
                    m_Background.rectTransform.anchorMin        = Vector2.zero;
                    m_Background.rectTransform.anchorMax        = Vector2.one;
                    m_Background.rectTransform.sizeDelta        = Vector2.zero;
                    m_Background.raycastTarget                  = true;
                    m_Background.material                       = UISystem.Override_GetUIMaterial();
                }

                m_Background.pixelsPerUnitMultiplier    = 1;
                m_Background.type                       = Image.Type.Sliced;
                m_Background.sprite                     = UISystem.GetUIRoundBGSprite();
                m_Background.color                      = (p_Color.HasValue ? p_Color.Value : UISystem.DefaultBGColor);
                m_Background.enabled                    = true;
            }
            else if (m_Background)
            {
                Destroy(m_Background.gameObject);
                m_Background = null;
            }

            return this;
        }
        /// <summary>
        /// Set background color
        /// </summary>
        /// <param name="p_Color">New background color</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetBackgroundColor(Color p_Color)
        {
            if (!m_Background)
                return this;

            m_Background.color = p_Color;

            return this;
        }
        /// <summary>
        /// Set background sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <param name="p_Type">Image type</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetBackgroundSprite(Sprite p_Sprite, Image.Type p_Type = Image.Type.Simple)
        {
            if (!m_Background)
                return this;

            m_Background.type    = p_Type;
            m_Background.sprite  = p_Sprite;

            return this;
        }
        /// <summary>
        /// Set lock icon mode
        /// </summary>
        /// <param name="p_Corner">Corner or none</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetLockIcon(ECorner p_Corner)
        {
            if (p_Corner == ECorner.None)
            {
                if (m_LockIcon)
                {
                    GameObject.Destroy(m_LockIcon.gameObject);
                    m_LockIcon = null;
                }
                return this;
            }

            float l_Width   = 5.0f;
            float l_Height  = 5.0f;

            if (!m_LockIcon)
            {
                m_LockIcon = UISystem.IconButtonFactory.Create("LockIcon", transform);
                m_LockIcon.RTransform.SetAsLastSibling();
                m_LockIcon.LElement.enabled = false;
                m_LockIcon.SetWidth(l_Width).SetHeight(l_Height);
                m_LockIcon.SetSprite(UISystem.GetUIIconLockedSprite());
                m_LockIcon.SetColor(GetAllowMovement() ? ColorU.ToUnityColor("#D0FCB3") : Color.white);
                m_LockIcon.OnClick(() => SetAllowMovement(!GetAllowMovement()));
            }

            if (p_Corner == ECorner.TopLeft)
            {
                m_LockIcon.RTransform.anchorMin         = new Vector2(0.0f, 1.0f);
                m_LockIcon.RTransform.anchorMax         = new Vector2(0.0f, 1.0f);
                m_LockIcon.RTransform.anchoredPosition  = new Vector2(l_Width, -l_Height);
            }
            else if (p_Corner == ECorner.TopRight)
            {
                m_LockIcon.RTransform.anchorMin         = new Vector2(1.0f, 1.0f);
                m_LockIcon.RTransform.anchorMax         = new Vector2(1.0f, 1.0f);
                m_LockIcon.RTransform.anchoredPosition  = new Vector2(-l_Width, -l_Height);
            }
            else if (p_Corner == ECorner.BottomLeft)
            {
                m_LockIcon.RTransform.anchorMin         = new Vector2(0.0f, 0.0f);
                m_LockIcon.RTransform.anchorMax         = new Vector2(0.0f, 0.0f);
                m_LockIcon.RTransform.anchoredPosition  = new Vector2(l_Width, l_Height);
            }
            else if (p_Corner == ECorner.BottomRight)
            {
                m_LockIcon.RTransform.anchorMin         = new Vector2(1.0f, 0.0f);
                m_LockIcon.RTransform.anchorMax         = new Vector2(1.0f, 0.0f);
                m_LockIcon.RTransform.anchoredPosition  = new Vector2(-l_Width, l_Height);
            }

            return this;
        }
        /// <summary>
        /// Set gear icon mode
        /// </summary>
        /// <param name="p_Corner">Corner or none</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetGearIcon(ECorner p_Corner)
        {
            if (p_Corner == ECorner.None)
            {
                if (m_GearIcon)
                {
                    GameObject.Destroy(m_GearIcon.gameObject);
                    m_GearIcon = null;
                }
                return this;
            }

            float l_Width   = 5.0f;
            float l_Height  = 5.0f;

            if (!m_GearIcon)
            {
                m_GearIcon = UISystem.IconButtonFactory.Create("GearIcon", transform);
                m_GearIcon.RTransform.SetAsLastSibling();
                m_GearIcon.LElement.enabled = false;
                m_GearIcon.SetWidth(l_Width).SetHeight(l_Height);
                m_GearIcon.SetSprite(UISystem.GetUIIconGearSprite());
                m_GearIcon.OnClick(() => {
                    try { m_OnGearIcon?.Invoke(this); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Components][CFloatingPanel.SetGearIcon] Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }
                });
            }

            if (p_Corner == ECorner.TopLeft)
            {
                m_GearIcon.RTransform.anchorMin         = new Vector2(0.0f, 1.0f);
                m_GearIcon.RTransform.anchorMax         = new Vector2(0.0f, 1.0f);
                m_GearIcon.RTransform.anchoredPosition  = new Vector2(l_Width, -l_Height);
            }
            else if (p_Corner == ECorner.TopRight)
            {
                m_GearIcon.RTransform.anchorMin         = new Vector2(1.0f, 1.0f);
                m_GearIcon.RTransform.anchorMax         = new Vector2(1.0f, 1.0f);
                m_GearIcon.RTransform.anchoredPosition  = new Vector2(-l_Width, -l_Height);
            }
            else if (p_Corner == ECorner.BottomLeft)
            {
                m_GearIcon.RTransform.anchorMin         = new Vector2(0.0f, 0.0f);
                m_GearIcon.RTransform.anchorMax         = new Vector2(0.0f, 0.0f);
                m_GearIcon.RTransform.anchoredPosition  = new Vector2(l_Width, l_Height);
            }
            else if (p_Corner == ECorner.BottomRight)
            {
                m_GearIcon.RTransform.anchorMin         = new Vector2(1.0f, 0.0f);
                m_GearIcon.RTransform.anchorMax         = new Vector2(1.0f, 0.0f);
                m_GearIcon.RTransform.anchoredPosition  = new Vector2(-l_Width, l_Height);
            }

            return this;
        }
        /// <summary>
        /// Set radius on supported games
        /// </summary>
        /// <param name="p_Radius">Canvas radius</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetRadius(float p_Radius)
        {
            return this;
        }
        /// <summary>
        /// Set on scene release
        /// </summary>
        /// <param name="p_Scene">Target scene</param>
        /// <param name="p_Callback">Callback</param>
        /// <returns></returns>
        public virtual CFloatingPanel OnSceneRelease(ChatPlexSDK.EGenericScene p_Scene, Action<Vector3, Vector3> p_Callback)
        {
            if (m_OnSceneRelease.ContainsKey(p_Scene))
                m_OnSceneRelease[p_Scene] = p_Callback;
            else
                m_OnSceneRelease.Add(p_Scene, p_Callback);

            return this;
        }
        /// <summary>
        /// Set scene transform
        /// </summary>
        /// <param name="p_Scene">Target scene</param>
        /// <param name="p_LocalPosition">Local position</param>
        /// <param name="p_LocalEulerAngles">Local euler angles</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetSceneTransform(ChatPlexSDK.EGenericScene p_Scene, Vector3 p_LocalPosition, Vector3 p_LocalEulerAngles)
        {
            if (m_SceneTransforms.ContainsKey(p_Scene))
                m_SceneTransforms[p_Scene] = (p_LocalPosition, p_LocalEulerAngles);
            else
                m_SceneTransforms.Add(p_Scene, (p_LocalPosition, p_LocalEulerAngles));

            if (p_Scene == ChatPlexSDK.ActiveGenericScene)
            {
                RTransform.localPosition    = p_LocalPosition;
                RTransform.localEulerAngles = p_LocalEulerAngles;

                try { m_OnSceneRelocated?.Invoke(this); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.CFloatingPanel][CFloatingPanel.SetSceneTransform] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }

            return this;
        }
        /// <summary>
        /// Set size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetSize(Vector2 p_Size)
        {
            RTransform.sizeDelta = p_Size;

            try { m_OnSizeChanged?.Invoke(this, p_Size); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.CFloatingPanel][CFloatingPanel.SetSize] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return this;
        }
        /// <summary>
        /// Set transform direct
        /// </summary>
        /// <param name="p_LocalPosition">Local position</param>
        /// <param name="p_LocalEulerAngles">Local euler angles</param>
        /// <returns></returns>
        public virtual CFloatingPanel SetTransformDirect(Vector3 p_LocalPosition, Vector3 p_LocalEulerAngles)
        {
            RTransform.localPosition    = p_LocalPosition;
            RTransform.localEulerAngles = p_LocalEulerAngles;

            try { m_OnSceneRelocated?.Invoke(this); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.CFloatingPanel][CFloatingPanel.SetTransformDirect] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        protected virtual void Awake()
        {
            /// Bind event
            ChatPlexSDK.OnGenericSceneChange -= ChatPlexSDK_OnGenericSceneChange;
            ChatPlexSDK.OnGenericSceneChange += ChatPlexSDK_OnGenericSceneChange;

            OnRelease((_) =>
            {
                if (m_AlignWithFloor)
                    RTransform.localEulerAngles = new Vector3(RTransform.localEulerAngles.x, RTransform.localEulerAngles.y, 0);

                if (m_SceneTransforms.ContainsKey(ChatPlexSDK.ActiveGenericScene))
                    SetSceneTransform(ChatPlexSDK.ActiveGenericScene, RTransform.localPosition, RTransform.localEulerAngles);

                if (m_OnSceneRelease.TryGetValue(ChatPlexSDK.ActiveGenericScene, out var l_OnSceneReleaseCallback) && l_OnSceneReleaseCallback != null)
                {
                    try { l_OnSceneReleaseCallback?.Invoke(RTransform.localPosition, RTransform.localEulerAngles); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.UI.CFloatingPanel][CFloatingPanel.OnRelease] Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }
                }
            });
        }
        /// <summary>
        /// On component destruction
        /// </summary>
        protected virtual void OnDestroy()
        {
            /// Unbind event
            ChatPlexSDK.OnGenericSceneChange -= ChatPlexSDK_OnGenericSceneChange;

            /// Discard any view controller
            SetViewController(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On generic scene change
        /// </summary>
        /// <param name="p_ActiveScene">New active scene</param>
        private void ChatPlexSDK_OnGenericSceneChange(ChatPlexSDK.EGenericScene p_ActiveScene)
        {
            if (m_AutoLockOnSceneSwitch)
                SetAllowMovement(false);

            if (m_SceneTransforms.TryGetValue(p_ActiveScene, out var l_SceneTransform))
            {
                RTransform.localPosition    = l_SceneTransform.Item1;
                RTransform.localEulerAngles = l_SceneTransform.Item2;

                try { m_OnSceneRelocated?.Invoke(this); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.CFloatingPanel][CFloatingPanel.ChatPlexSDK_OnGenericSceneChange] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
        }
    }
}
