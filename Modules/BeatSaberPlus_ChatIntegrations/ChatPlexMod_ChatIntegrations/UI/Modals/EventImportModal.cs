using CP_SDK;
using CP_SDK.XUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ChatPlexMod_ChatIntegrations.UI.Modals
{
    /// <summary>
    /// Event import modal
    /// </summary>
    internal sealed class EventImportModal : CP_SDK.UI.IModal
    {
        private XUIDropdown m_File = null;

        private Action<Interfaces.IEventBase>   m_Callback = null;
        private string                          m_Selected = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_File != null)
                return;

            Templates.ModalRectLayout(
                XUIText.Make("Which event do you want to import?"),

                XUIDropdown.Make()
                    .OnValueChanged((_, p_Selected) => m_Selected = p_Selected)
                    .Bind(ref m_File),

                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).SetWidth(30f),
                    XUIPrimaryButton.Make("Import", OnImportButton).SetWidth(30f)
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
            m_Selected = string.Empty;

            var l_Files = new List<string>();
            foreach (var l_File in System.IO.Directory.GetFiles(ChatIntegrations.s_IMPORT_PATH, "*.bspci"))
                l_Files.Add(System.IO.Path.GetFileNameWithoutExtension(l_File));

            m_File.SetOptions(l_Files);

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
        /// On import button
        /// </summary>
        private void OnImportButton()
        {
            var l_FileName = ChatIntegrations.s_IMPORT_PATH + m_Selected + ".bspci";

            if (System.IO.File.Exists(l_FileName))
            {
                var l_Raw = System.IO.File.ReadAllText(l_FileName, System.Text.Encoding.Unicode);

                try
                {
                    var l_JObject  = JObject.Parse(l_Raw);
                    var l_NewEvent = ChatIntegrations.Instance.AddEventFromSerialized(l_JObject, true, false, out var l_Error);

                    if (l_NewEvent != null)
                    {
                        VController.CloseModal(this);

                        try { m_Callback?.Invoke(l_NewEvent); }
                        catch (System.Exception l_Exception)
                        {
                            ChatPlexSDK.Logger.Error($"[ChatPlexMod_ChatIntegrations.UI][ProfileImportModal.EventImportModal] Error:");
                            ChatPlexSDK.Logger.Error(l_Exception);
                        }
                    }
                    else
                    {
                        VController.CloseModal(this);
                        VController.ShowMessageModal("Error importing profile!");
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
            else
            {
                VController.CloseModal(this);
                VController.ShowMessageModal("File not found!");
            }
        }
    }
}
