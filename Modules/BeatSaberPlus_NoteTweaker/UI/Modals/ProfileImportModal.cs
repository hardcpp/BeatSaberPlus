using CP_SDK;
using CP_SDK.XUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BeatSaberPlus_NoteTweaker.UI.Modals
{
    /// <summary>
    /// Profile import modal
    /// </summary>
    internal sealed class ProfileImportModal : CP_SDK.UI.IModal
    {
        private XUIDropdown m_DropDown = null;

        private Action m_Callback = null;
        private string m_Selected = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_DropDown != null)
                return;

            Templates.ModalRectLayout(
                XUIText.Make("What profile do you want to import?"),

                XUIDropdown.Make()
                    .OnValueChanged((_, p_Selected) => m_Selected = p_Selected)
                    .Bind(ref m_DropDown),

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
        public void Init(Action p_Callback)
        {
            m_Selected = string.Empty;

            var l_Files = new List<string>();
            foreach (var l_File in System.IO.Directory.GetFiles(NoteTweaker.IMPORT_FOLDER, "*.bspnt"))
                l_Files.Add(System.IO.Path.GetFileNameWithoutExtension(l_File));

            m_DropDown.SetOptions(l_Files);

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
            var l_FileName = NoteTweaker.IMPORT_FOLDER + m_Selected + ".bspnt";

            if (System.IO.File.Exists(l_FileName))
            {
                var l_Raw = System.IO.File.ReadAllText(l_FileName, System.Text.Encoding.Unicode);

                try
                {
                    var l_NewProfile = JsonConvert.DeserializeObject<NTConfig._Profile>(l_Raw, new JsonConverter[]
                    {
                        new CP_SDK.Config.JsonConverters.ColorConverter()
                    });

                    l_NewProfile.Name += " (Imported)";

                    if (l_NewProfile != null)
                    {
                        NTConfig.Instance.Profiles.Add(l_NewProfile);

                        VController.CloseModal(this);

                        try { m_Callback?.Invoke(); }
                        catch (System.Exception l_Exception)
                        {
                            ChatPlexSDK.Logger.Error($"[BeatSaberPlus_NoteTweaker.UI][ProfileImportModal.OnImportButton] Error:");
                            ChatPlexSDK.Logger.Error(l_Exception);
                        }
                    }
                    else
                    {
                        VController.CloseModal(this);
                        VController.ShowMessageModal("Error importing profile!");
                    }
                }
                catch
                {
                    VController.CloseModal(this);
                    VController.ShowMessageModal("Invalid file!");
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
