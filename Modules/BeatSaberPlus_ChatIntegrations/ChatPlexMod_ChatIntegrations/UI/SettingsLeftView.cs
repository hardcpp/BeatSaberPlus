using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.UI
{
    /// <summary>
    /// Settings left view controller
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr =
                     "<b>Special thanks to HypersonicSharkz#3301 for help on TwitchAPI and some Actions code!"
            + "\n" + "This module allow you execute actions on your game when triggered by events."
            + "\n" + ""
            + "\n" + "<b>Events</b>"
            + "\n" + "- <color=yellow><b>ChatBits</b></color>\n<i><color=#CCCCCCFF>When someone spends bits your channel!</color></i>"
            + "\n" + "- <color=yellow><b>ChatCommand</b></color>\n<i><color=#CCCCCCFF>Allow you to create chat commands and execute actions with them</color></i>"
            + "\n" + "- <color=yellow><b>ChatFollow</b></color>\n<i><color=#CCCCCCFF>When someone follows your channel</color></i>"
            + "\n" + "- <color=yellow><b>ChatPointsReward</b></color>\n<i><color=#CCCCCCFF>Allow you to create channel points rewards and fully configure them and bind some actions to them</color></i>"
            + "\n" + "- <color=yellow><b>ChatSubscription</b></color>\n<i><color=#CCCCCCFF>When someone subscribes or subgifts</color></i>"
            + "\n" + "- <color=yellow><b>Dummy</b></color>\n<i><color=#CCCCCCFF>Dummy event that can get triggered by other events</color></i>"
            + "\n" + "- <color=yellow><b>LevelEnded</b></color>\n<i><color=#CCCCCCFF>When you exit a map</color></i>"
            + "\n" + "- <color=yellow><b>LevelPaused</b></color>\n<i><color=#CCCCCCFF>When you pause a map</color></i>"
            + "\n" + "- <color=yellow><b>LevelResumed</b></color>\n<i><color=#CCCCCCFF>When you resume a map</color></i>"
            + "\n" + "- <color=yellow><b>LevelStarted</b></color>\n<i><color=#CCCCCCFF>When you enter a map</color></i>"
            + "\n" + "- <color=yellow><b>VoiceAttackCommand</b></color>\n<i><color=#CCCCCCFF>Bind VoiceAttack commands to BS+</color></i>"
            + "\n" + ""
            + "\n" + "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information"),

                Templates.ScrollableInfos(50,
                    XUIText.Make(s_InformationStr)
                        .SetAlign(TMPro.TextAlignmentOptions.Left)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Web Configuration", OnWebConfigurationButton)
                ),
                Templates.ExpandedButtonsLine(
                    XUISecondaryButton.Make("Documentation", OnDocumentationButton)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Open web configuration button
        /// </summary>
        private void OnWebConfigurationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            CP_SDK.Chat.Service.OpenWebConfiguration();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL(ChatIntegrations.Instance.DocumentationURL);
        }
    }
}
