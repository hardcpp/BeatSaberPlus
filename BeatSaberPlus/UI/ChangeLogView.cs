using BeatSaberMarkupLanguage.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// ChangeLog view
    /// </summary>
    internal class ChangeLogView : SDK.UI.ResourceViewController<ChangeLogView>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("ChangeLog")]
        private HMUI.TextPageScrollView m_ChangeLog = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Change opacity
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);

            new CP_SDK.Network.APIClient("https://plugin.beatsaberplus.com/", System.TimeSpan.FromSeconds(20), false)
            .GetAsync("?/API/ChangeLog", System.Threading.CancellationToken.None, false).ContinueWith(p_Result =>
            {
                if (p_Result.Status != System.Threading.Tasks.TaskStatus.RanToCompletion
                 || p_Result.Result == null || !p_Result.Result.IsSuccessStatusCode)
                {
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                    {
                        if (CanBeUpdated)
                        {
                            SetMessageModal_PendingMessage("Failed to contact the server!");
                            HideLoadingModal();
                        }
                    });
                    return;
                }

                var l_ResultRaw = p_Result.Result.BodyString;
                var l_Result    = JObject.Parse(l_ResultRaw);
                var l_ChangeLog = l_Result.ContainsKey("ChangeLog") ? l_Result["ChangeLog"] : "Error";
                bool l_IsUpdated = true;

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    if (CanBeUpdated)
                    {
                        m_ChangeLog.SetText("<line-height=125%>" + l_ChangeLog);
                        HideLoadingModal();

                        if (!l_IsUpdated)
                            ShowConfirmationModal("An update is available, do you want to download it?", OnDownloadPressed);
                    }
                });
            }).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the download button is pressed
        /// </summary>
        private void OnDownloadPressed()
        {

        }
    }
}
