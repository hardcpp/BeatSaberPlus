#if CP_SDK_XR_INPUT
using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.XRInput
{
    /// <summary>
    /// XR generic buttons list
    /// </summary>
    public enum EXRInputButton
    {
        None,
        PrimaryButton,
        SecondaryButton,
        SystemButton,
        Axis0Button,
        Axis1Button,
        Axis2Button,
        GripButton,
        MAX
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// XR input system
    /// </summary>
    public class XRInputSystem : Unity.PersistentSingleton<XRInputSystem>
    {
        private const int s_FakePointerID = -1;
        private const float s_MinTriggerValue = 0.9f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button data for controller
        /// </summary>
        private static bool[,] m_ControllersButtonPressData = new bool[2, (int)EXRInputButton.MAX];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pointers data
        /// </summary>
        private Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();
        /// <summary>
        /// Component list cache for ui ray casting
        /// </summary>
        private List<Component> m_ComponentList = new List<Component>(20);
        /// <summary>
        /// Current mouse state
        /// </summary>
        private InputInternals.FakeMouseState m_MouseState = new InputInternals.FakeMouseState();
        /// <summary>
        /// Raycast result cache
        /// </summary>
        private List<RaycastResult> m_RaycastResultCache = new List<RaycastResult>(20);
        /// <summary>
        /// Base event data
        /// </summary>
        private BaseEventData m_BaseEventData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current ray casting controller
        /// </summary>
        public static XRController RaycastingController { get; private set; } = null;
        /// <summary>
        /// Should ray cast
        /// </summary>
        public static bool EnableRaycasting = true;
        /// <summary>
        /// Ray casting layer mask
        /// </summary>
        public static LayerMask RaycastingUILayerMask;
        /// <summary>
        /// Ray casting max distance
        /// </summary>
        public static float RaycastingUIMaxDistance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ray casting controller change (OldController, NewController)
        /// </summary>
        public static event Action<XRController, XRController> OnRaycastingControllerChange;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set activate ray casting controller
        /// </summary>
        /// <param name="p_Controller">Target controller</param>
        /// <returns>If changed</returns>
        public static bool SetRaycastingController(XRController p_Controller)
        {
            if (RaycastingController == p_Controller)
                return false;

            OnRaycastingControllerChange?.Invoke(RaycastingController, p_Controller);
            RaycastingController = p_Controller;

            /// Update laser pointer
            XRLaserPointer.Instance?.SetController(p_Controller);

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On controller button press data
        /// </summary>
        /// <param name="p_Controller">Source controller</param>
        /// <param name="p_Button">Target button</param>
        /// <param name="p_State">Is down?</param>
        public static void OnControllerButtonPressData(XRController p_Controller, EXRInputButton p_Button, bool p_State)
        {
            if (p_Button == EXRInputButton.None || p_Button > EXRInputButton.MAX)
                return;

            var l_PreviousState = m_ControllersButtonPressData[p_Controller.IsLeftHand ? 0 : 1, (int)p_Button];
            if (l_PreviousState == p_State)
                return;

            m_ControllersButtonPressData[p_Controller.IsLeftHand ? 0 : 1, (int)p_Button] = p_State;

            if (!l_PreviousState)
            {
                if (p_Button == EXRInputButton.Axis1Button)
                    SetRaycastingController(p_Controller);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On first frame
        /// </summary>
        protected void Start()
        {
            /// Grab config
            EnableRaycasting            = true;
            RaycastingUILayerMask       = 1 << UI.UISystem.UILayer;
            RaycastingUIMaxDistance     = 40.0f;
        }
        /// <summary>
        /// On component disable
        /// </summary>
        protected void OnDisable()
        {
            ClearSelection();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get base event data
        /// </summary>
        /// <returns></returns>
        protected virtual BaseEventData GetBaseEventData()
        {
            if (m_BaseEventData == null)
                m_BaseEventData = new BaseEventData(EventSystem.current);

            m_BaseEventData.Reset();
            return m_BaseEventData;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get or create pointer event data
        /// </summary>
        /// <param name="p_PointerID">ID of the pointer</param>
        /// <param name="p_Data">Result data</param>
        /// <param name="p_ShouldCreate">Should create it if missing</param>
        /// <returns>Return true if the PointerEventData was created</returns>
        private bool GetFakePointerEventData(int p_PointerID, out PointerEventData p_Data, bool p_ShouldCreate)
        {
            if (m_PointerData.TryGetValue(p_PointerID, out p_Data) || !p_ShouldCreate)
                return false;

            p_Data = new PointerEventData(EventSystem.current)
            {
                pointerId = p_PointerID
            };
            m_PointerData.Add(p_PointerID, p_Data);

            return true;
        }
        /// <summary>
        /// Get mouse state
        /// </summary>
        /// <returns></returns>
        private InputInternals.FakeMouseState GetFakeMouseState()
        {
            bool l_WasCreated = GetFakePointerEventData(s_FakePointerID, out var l_PointerData, true);

            /// Prepare pointer data
            l_PointerData.Reset();
            l_PointerData.button = PointerEventData.InputButton.Left;

            var l_RaycastingController = RaycastingController;
            if (l_RaycastingController && l_RaycastingController.isActiveAndEnabled)
            {
                l_PointerData.pointerCurrentRaycast = new RaycastResult()
                {
                    worldPosition   = l_RaycastingController.RawTransform?.position ?? Vector3.zero,
                    worldNormal     = l_RaycastingController.RawTransform?.forward  ?? Vector3.forward
                };

                var l_ScrollDelta = new Vector2(l_RaycastingController.Axis0Joystick.x * -1f, l_RaycastingController.Axis0Joystick.y);
                l_PointerData.scrollDelta = l_ScrollDelta;
            }

            m_RaycastResultCache.Clear();

            /// Ray cast all graphics
            if (EnableRaycasting)
            {
                for (int l_I = 0; l_I < XRGraphicRaycaster.s_GraphicRaycasters.Count; l_I++)
                {
                    var l_CurrentRaycaster = XRGraphicRaycaster.s_GraphicRaycasters[l_I];
                    if (l_CurrentRaycaster && l_CurrentRaycaster.IsActive())
                    {
                        l_CurrentRaycaster.Raycast(l_PointerData, m_RaycastResultCache);
                    }
                }
            }

            /// Sort, grab the first one and clear cache
            m_RaycastResultCache.Sort(Unity.RaycastResultU.Comparer);
            var l_FirstRaycast = default(RaycastResult);
            for (var l_I = 0; l_I < m_RaycastResultCache.Count; ++l_I)
            {
                if (m_RaycastResultCache[l_I].gameObject == null || m_RaycastResultCache[l_I].worldNormal != Vector3.zero)
                    continue;
                l_FirstRaycast = m_RaycastResultCache[l_I];
                break;
            }
            m_RaycastResultCache.Clear();

            l_PointerData.pointerCurrentRaycast = l_FirstRaycast;
            l_PointerData.delta                 = l_WasCreated ? Vector2.zero : l_FirstRaycast.screenPosition - l_PointerData.position;
            l_PointerData.position              = l_FirstRaycast.screenPosition;

            if (RaycastingController)
                XRLaserPointer.Instance.SetDistance(l_FirstRaycast.gameObject ? l_FirstRaycast.distance : 0f);

            var l_StateForMouseButton = PointerEventData.FramePressState.NotChanged;
            if (l_RaycastingController && l_RaycastingController.isActiveAndEnabled)
            {
                var l_TriggerValue  = l_RaycastingController.IsButtonPressed(EXRInputButton.Axis1Button) ? 1f : 0.0f;
                var l_ButtonState   = m_MouseState.GetFakeButtonState(PointerEventData.InputButton.Left);

                if ((double)l_ButtonState.PressedValue < s_MinTriggerValue && (double)l_TriggerValue >= s_MinTriggerValue)
                    l_StateForMouseButton = PointerEventData.FramePressState.Pressed;
                else if ((double)l_ButtonState.PressedValue >= s_MinTriggerValue && (double)l_TriggerValue < s_MinTriggerValue)
                    l_StateForMouseButton = PointerEventData.FramePressState.Released;

                l_ButtonState.PressedValue = l_TriggerValue;
            }

            m_MouseState.SetFakeButtonState(PointerEventData.InputButton.Left, l_StateForMouseButton, l_PointerData);

            return m_MouseState;
        }
        /// <summary>
        /// Get last pointer event data
        /// </summary>
        /// <param name="p_PointerID">Pointer ID</param>
        /// <returns></returns>
        private PointerEventData GetLastPointerEventData(int p_PointerID)
        {
            GetFakePointerEventData(p_PointerID, out var l_Data, false);
            return l_Data;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is pointer over a game object
        /// </summary>
        /// <param name="p_PointerID">Pointer ID</param>
        /// <returns></returns>
        public bool IsPointerOverGameObject(int p_PointerID)
        {
            var l_PointerEventData = GetLastPointerEventData(p_PointerID);
            return l_PointerEventData != null && l_PointerEventData.pointerEnter != null;
        }
        /// <summary>
        /// Clear all selected UI graphics
        /// </summary>
        private void ClearSelection()
        {
            var l_PointerEventData = GetBaseEventData();
            foreach (var l_CurrentPointerData in m_PointerData.Values)
                HandlePointerExitAndEnter(l_CurrentPointerData, null);

            m_PointerData.Clear();

            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(null, l_PointerEventData);
        }
        /// <summary>
        /// Handle selection change
        /// </summary>
        /// <param name="p_CurrentHover">Previous / current hover game object</param>
        /// <param name="p_PointerEventData">Last pointer event data</param>
        private void DeselectIfSelectionChanged(GameObject p_CurrentHover, BaseEventData p_PointerEventData)
        {
            if (!p_CurrentHover || !EventSystem.current)
                return;

            var l_EventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(p_CurrentHover);
            if (l_EventHandler == EventSystem.current.currentSelectedGameObject)
                return;

            EventSystem.current.SetSelectedGameObject(l_EventHandler, p_PointerEventData);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process mouse press event
        /// </summary>
        /// <param name="p_MouseButtonEvent">Event data</param>
        private void HandleFakeMousePress(PointerInputModule.MouseButtonEventData p_MouseButtonEvent)
        {
            var l_ButtonData        = p_MouseButtonEvent.buttonData;
            var l_CurrentGameObject = l_ButtonData.pointerCurrentRaycast.gameObject;

            bool l_HasHandler = false;
            if (p_MouseButtonEvent.PressedThisFrame())
            {
                l_ButtonData.eligibleForClick       = true;
                l_ButtonData.delta                  = Vector2.zero;
                l_ButtonData.dragging               = false;
                l_ButtonData.useDragThreshold       = true;
                l_ButtonData.pressPosition          = l_ButtonData.position;
                l_ButtonData.pointerPressRaycast    = l_ButtonData.pointerCurrentRaycast;

                DeselectIfSelectionChanged(l_CurrentGameObject, l_ButtonData);

                var l_PointerDownHandler = ExecuteEvents.ExecuteHierarchy(l_CurrentGameObject, l_ButtonData, ExecuteEvents.pointerDownHandler);

                if (l_PointerDownHandler == null)
                    l_PointerDownHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(l_CurrentGameObject);

                var l_UnscaledTime = Time.unscaledTime;
                if (l_PointerDownHandler == l_ButtonData.lastPress)
                {
                    if (l_UnscaledTime - l_ButtonData.clickTime < 0.3f)
                        l_ButtonData.clickCount++;
                    else
                        l_ButtonData.clickCount = 1;

                    l_ButtonData.clickTime = l_UnscaledTime;
                }
                else
                    l_ButtonData.clickCount = 1;

                l_ButtonData.pointerPress       = l_PointerDownHandler;
                l_ButtonData.rawPointerPress    = l_CurrentGameObject;
                l_ButtonData.clickTime          = l_UnscaledTime;
                l_ButtonData.pointerDrag        = ExecuteEvents.GetEventHandler<IDragHandler>(l_CurrentGameObject);

                if (l_ButtonData.pointerDrag != null)
                    ExecuteEvents.Execute(l_ButtonData.pointerDrag, l_ButtonData, ExecuteEvents.initializePotentialDrag);

                l_ButtonData.eligibleForClick = true;
                l_HasHandler = l_ButtonData.pointerPress != null && l_ButtonData.pointerPress.GetComponent<IPointerClickHandler>() != null;
            }

            if (!(p_MouseButtonEvent.ReleasedThisFrame() | l_HasHandler))
                return;

            ExecuteEvents.Execute(l_ButtonData.pointerPress, l_ButtonData, ExecuteEvents.pointerUpHandler);

            var l_EventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(l_CurrentGameObject);

            if (l_ButtonData.pointerPress == l_EventHandler && l_ButtonData.eligibleForClick)
                ExecuteEvents.Execute(l_ButtonData.pointerPress, l_ButtonData, ExecuteEvents.pointerClickHandler);
            else if (l_ButtonData.pointerDrag != null && l_ButtonData.dragging)
                ExecuteEvents.ExecuteHierarchy(l_CurrentGameObject, l_ButtonData, ExecuteEvents.dropHandler);

            EventSystem.current.SetSelectedGameObject(null, null);

            l_ButtonData.eligibleForClick   = false;
            l_ButtonData.pointerPress       = null;
            l_ButtonData.rawPointerPress    = null;

            if (l_ButtonData.pointerDrag != null && l_ButtonData.dragging)
                ExecuteEvents.Execute(l_ButtonData.pointerDrag, l_ButtonData, ExecuteEvents.endDragHandler);

            l_ButtonData.dragging       = false;
            l_ButtonData.pointerDrag    = null;

            if (l_CurrentGameObject == l_ButtonData.pointerEnter)
                return;

            HandlePointerExitAndEnter(l_ButtonData, null);
            HandlePointerExitAndEnter(l_ButtonData, l_CurrentGameObject);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process move event
        /// </summary>
        /// <param name="p_PointerEventData">Event data</param>
        private void HandleFakeMouseMove(PointerEventData p_PointerEventData)
            => HandlePointerExitAndEnter(p_PointerEventData, p_PointerEventData.pointerCurrentRaycast.gameObject);
        /// <summary>
        /// Handle pointer exit & enter event
        /// </summary>
        /// <param name="p_PointerEventData">Pointer event data</param>
        /// <param name="p_NewEnterTarget">New target</param>
        protected void HandlePointerExitAndEnter(PointerEventData p_PointerEventData, GameObject p_NewEnterTarget)
        {
            if (p_NewEnterTarget == null || p_PointerEventData.pointerEnter == null)
            {
                for (var l_I = 0; l_I < p_PointerEventData.hovered.Count; ++l_I)
                    ExecuteEvents.Execute(p_PointerEventData.hovered[l_I], p_PointerEventData, ExecuteEvents.pointerExitHandler);

                p_PointerEventData.hovered.Clear();

                if (p_NewEnterTarget == null)
                {
                    p_PointerEventData.pointerEnter = null;
                    return;
                }
            }

            if (p_PointerEventData.pointerEnter == p_NewEnterTarget && p_NewEnterTarget)
                return;

            var l_CommonRoot = p_PointerEventData.pointerEnter?.FindCommonRoot(p_NewEnterTarget) ?? null;
            if (p_PointerEventData.pointerEnter != null)
            {
                for (var l_Transform = p_PointerEventData.pointerEnter.transform
                    ; l_Transform != null && (!(l_CommonRoot != null) || !(l_CommonRoot.transform == l_Transform))
                    ; l_Transform = l_Transform.parent)
                {
                    ExecuteEvents.Execute(l_Transform.gameObject, p_PointerEventData, ExecuteEvents.pointerExitHandler);
                    p_PointerEventData.hovered.Remove(l_Transform.gameObject);
                }
            }

            if (!enabled)
                return;

            p_PointerEventData.pointerEnter = p_NewEnterTarget;

            if (p_NewEnterTarget == null)
                return;

            bool l_IsActiveAndInteractable = false;
            bool l_IsNotInteractable = false;

            var l_CurrentTransform = p_NewEnterTarget.transform;
            for (; l_CurrentTransform != null; l_CurrentTransform = l_CurrentTransform.parent)
            {
                m_ComponentList.Clear();
                l_CurrentTransform.gameObject.GetComponents(m_ComponentList);

                for (int l_ComponentI = 0; l_ComponentI < m_ComponentList.Count; ++l_ComponentI)
                {
                    var l_Selectable  = m_ComponentList[l_ComponentI] as Selectable;
                    var l_CanvasGroup = m_ComponentList[l_ComponentI] as CanvasGroup;

                    l_IsNotInteractable = l_IsNotInteractable
                                                    || (l_Selectable != null && !l_Selectable.interactable)
                                                    || (l_CanvasGroup != null && !l_CanvasGroup.interactable);

                    l_IsActiveAndInteractable = l_IsActiveAndInteractable
                                                    || (l_Selectable != null && l_Selectable.isActiveAndEnabled && l_Selectable.interactable);
                }

                if (l_CurrentTransform.gameObject == l_CommonRoot)
                    break;

                ExecuteEvents.Execute(l_CurrentTransform.gameObject, p_PointerEventData, ExecuteEvents.pointerEnterHandler);
                p_PointerEventData.hovered.Add(l_CurrentTransform.gameObject);
            }

            if (l_IsNotInteractable || !l_IsActiveAndInteractable)
                return;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process drag event
        /// </summary>
        /// <param name="p_PointerEventData">Event data</param>
        private void HandleFakeMouseDrag(PointerEventData p_PointerEventData)
        {
            bool l_IsPointerMoving = p_PointerEventData.IsPointerMoving();

            if (l_IsPointerMoving
                && p_PointerEventData.pointerDrag != null
                && (!p_PointerEventData.dragging
                    && ShouldStartDrag(p_PointerEventData.pressPosition, p_PointerEventData.position, (float)EventSystem.current.pixelDragThreshold, p_PointerEventData.useDragThreshold)))
            {
                ExecuteEvents.Execute(p_PointerEventData.pointerDrag, p_PointerEventData, ExecuteEvents.beginDragHandler);
                p_PointerEventData.dragging = true;
            }

            if (!(p_PointerEventData.dragging & l_IsPointerMoving) || p_PointerEventData.pointerDrag == null)
                return;

            if (p_PointerEventData.pointerPress != p_PointerEventData.pointerDrag)
            {
                ExecuteEvents.Execute(p_PointerEventData.pointerPress, p_PointerEventData, ExecuteEvents.pointerUpHandler);
                p_PointerEventData.eligibleForClick = false;
                p_PointerEventData.pointerPress     = null;
                p_PointerEventData.rawPointerPress  = null;
            }

            ExecuteEvents.Execute(p_PointerEventData.pointerDrag, p_PointerEventData, ExecuteEvents.dragHandler);
        }
        /// <summary>
        /// Should start drag event
        /// </summary>
        /// <param name="p_PressPosition">Start position</param>
        /// <param name="p_CurrentPosition">Current position</param>
        /// <param name="p_Threshold">Minimum threshold</param>
        /// <param name="p_UseDragThreshold">Should use drag threshold</param>
        /// <returns></returns>
        private bool ShouldStartDrag(Vector2 p_PressPosition, Vector2 p_CurrentPosition, float p_Threshold, bool p_UseDragThreshold)
        {
            return !p_UseDragThreshold || (double)(p_PressPosition - p_CurrentPosition).sqrMagnitude >= (double)p_Threshold * (double)p_Threshold;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process ray casting
        /// </summary>
        public void Update()
        {
            if (!EnableRaycasting)
                return;

            /// UI
            if (RaycastingUILayerMask != 0)
            {
                try
                {
                    var l_EventData = GetFakeMouseState().GetFakeButtonState(PointerEventData.InputButton.Left).EventData;

                    HandleFakeMousePress(l_EventData);

                    if (l_EventData.buttonData.position.x < float.MaxValue &&
                        l_EventData.buttonData.position.y < float.MaxValue)
                    {
                        HandleFakeMouseMove(l_EventData.buttonData);
                        HandleFakeMouseDrag(l_EventData.buttonData);

                        if (!Mathf.Approximately(l_EventData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
                            ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(l_EventData.buttonData.pointerCurrentRaycast.gameObject), l_EventData.buttonData, ExecuteEvents.scrollHandler);
                    }
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.XRInput][XRInputSystem.Update] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception.ToString());
                }
            }
        }
    }
}
#endif