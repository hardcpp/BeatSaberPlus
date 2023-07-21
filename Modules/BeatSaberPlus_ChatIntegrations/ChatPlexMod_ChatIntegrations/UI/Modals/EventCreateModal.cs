using CP_SDK;
using CP_SDK.XUI;
using System;
using System.Linq;

namespace ChatPlexMod_ChatIntegrations.UI.Modals
{
    /// <summary>
    /// Event create modal
    /// </summary>
    internal sealed class EventCreateModal : CP_SDK.UI.IModal
    {
        private XUIDropdown  m_Type = null;
        private XUITextInput m_Name = null;

        private Action<Interfaces.IEventBase>   m_Callback = null;
        private string                          m_Selected = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_Type != null)
                return;

            Templates.ModalRectLayout(
                XUIText.Make("What kind of event do you want to create?"),

                XUIDropdown.Make()
                    .SetOptions(ChatIntegrations.RegisteredEventTypes.ToList())
                    .OnValueChanged((_, p_Selected) => m_Selected = p_Selected)
                    .Bind(ref m_Type),

                XUITextInput.Make("", "Name...")
                    .Bind(ref m_Name),

                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).SetWidth(30f),
                    XUIPrimaryButton.Make("Create", OnCreateButton).SetWidth(30f)
                )
                .SetPadding(0)
            )
            .SetWidth(90.0f)
            .BuildUI(transform);
        }
        /// <summary>
        /// On modal close
        /// </summary>
        public override void OnClose()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="p_Callback">Callback</param>
        public void Init(Action<Interfaces.IEventBase> p_Callback)
        {
            m_Callback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel button
        /// </summary>
        private void OnCancelButton()
        {
            VController.CloseModal(this);
        }
        /// <summary>
        /// On create button
        /// </summary>
        private void OnCreateButton()
        {
            try
            {
                var l_NewEvent = ChatIntegrations.CreateEvent(m_Selected);
                if (l_NewEvent != null)
                {
                    var l_Name = m_Name.Element.GetValue();
                    if (string.IsNullOrEmpty(l_Name))
                        l_Name = "New " + m_Selected + " " + CP_SDK.Misc.Time.UnixTimeNow();

                    l_NewEvent.GenericModel.Name = l_Name;

                    ChatIntegrations.Instance.AddEvent(l_NewEvent);

                    VController.CloseModal(this);

                    try { m_Callback?.Invoke(l_NewEvent); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[ChatPlexMod_ChatIntegrations.UI][EventCreateModal.OnCreateButton] Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }
                }
                else
                {
                    VController.CloseModal(this);
                    VController.ShowMessageModal("No type selected!");
                }
            }
            catch (System.Exception l_Exception)
            {
                VController.CloseModal(this);
                VController.ShowMessageModal("Unknown error!");

                ChatPlexSDK.Logger.Error($"[ChatPlexMod_ChatIntegrations.UI][EventCreateModal.OnCreateButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
