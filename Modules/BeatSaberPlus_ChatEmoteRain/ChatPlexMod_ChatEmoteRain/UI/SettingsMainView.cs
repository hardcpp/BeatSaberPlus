using CP_SDK.UI.Data;
using CP_SDK.XUI;
using System.Linq;
using UnityEngine.UI;

using EmitterConfig = CP_SDK.Unity.Components.EnhancedImageParticleEmitter.EmitterConfig;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        internal class EmitterConfigListItem : IListItem
        {
            internal EmitterConfig EConfig;

            internal EmitterConfigListItem(EmitterConfig p_EmitterConfig)
                => EConfig = p_EmitterConfig;

            public override void OnShow() => RefreshVisual();
            public override void OnHide() { }

            internal void RefreshVisual()
            {
                if (!(Cell is TextListCell l_TextListCell))
                    return;

                l_TextListCell.Text.SetText((EConfig.Enabled ? "" : "<alpha=#70>") + EConfig.Name);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITabControl           m_TabControl = null;

        private XUIToggle               m_GeneralTab_MenuRain           = null;
        private XUISlider               m_GeneralTab_MenuRainSize       = null;
        private XUISlider               m_GeneralTab_MenuFallSpeed      = null;
        private XUIToggle               m_GeneralTab_PlayingRain        = null;
        private XUISlider               m_GeneralTab_PlayingRainSize    = null;
        private XUISlider               m_GeneralTab_PlayingFallSpeed   = null;

        private XUIVVList               m_MenuEmittersTab_List          = null;
        private Widgets.EmitterWidget   m_MenuEmittersTab_EmitterWidget = null;

        private XUIVVList               m_PlayingEmittersTab_List           = null;
        private Widgets.EmitterWidget   m_PlayingEmittersTab_EmitterWidget  = null;

        private XUIToggle               m_CommandsTab_ModeratorPowerToggle     = null;
        private XUIToggle               m_CommandsTab_VIPPowerToggle           = null;
        private XUIToggle               m_CommandsTab_SubscriberPowerToggle    = null;
        private XUIToggle               m_CommandsTab_UserPowerToggle          = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool                    m_PreventChanges        = false;
        private EmitterConfigListItem   m_SelectedItemMenu      = null;
        private EmitterConfigListItem   m_SelectedItemPlaying   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat Emote Rain | Settings"),

                XUITabControl.Make(
                    ("General",          BuildGeneralTab()),
                    ("Menu Emitters",    BuildMenuEmittersTab()),
                    ("Playing Emitters", BuildPlayingEmittersTab()),
                    ("Chat Commands",    BuildChatCommandsTab())
                 )
                .OnActiveChanged(OnTabSelected)
                .Bind(ref m_TabControl)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            RefreshSettings();
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            m_MenuEmittersTab_List.SetListItems(CERConfig.Instance.MenuEmitters.Select(x => new EmitterConfigListItem(x)).ToList());
            m_PlayingEmittersTab_List.SetListItems(CERConfig.Instance.SongEmitters.Select(x => new EmitterConfigListItem(x)).ToList());
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            if (ChatEmoteRain.Instance != null)
            {
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu, false, null);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, false, null);
            }

            CERConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build general tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildGeneralTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIText.Make("Rain in menu"),
                    XUIToggle.Make()
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_MenuRain),

                    XUIText.Make("Emote size in menu"),
                    XUISlider.Make()
                        .SetMinValue(0.1f).SetMaxValue(5.0f).SetIncrements(0.1f)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_MenuRainSize),

                    XUIText.Make("Emote fall speed in menu"),
                    XUISlider.Make()
                        .SetMinValue(1.1f).SetMaxValue(10.0f).SetIncrements(0.1f)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_MenuFallSpeed)
                )
                .SetWidth(40.0f),

                XUIVLayout.Make(
                    XUIText.Make("Rain while playing"),
                    XUIToggle.Make()
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_PlayingRain),

                    XUIText.Make("Emote size while playing"),
                    XUISlider.Make()
                        .SetMinValue(0.1f).SetMaxValue(5.0f).SetIncrements(0.1f)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_PlayingRainSize),

                    XUIText.Make("Emote fall speed while playing"),
                    XUISlider.Make()
                        .SetMinValue(1.1f).SetMaxValue(10.0f).SetIncrements(0.1f)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_GeneralTab_PlayingFallSpeed)
                )
                .SetWidth(40.0f)
            );
        }
        /// <summary>
        /// Build menu emitters tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildMenuEmittersTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                            .OnListItemSelected((x) => OnEmitterSelected(m_MenuEmittersTab_List, x))
                            .Bind(ref m_MenuEmittersTab_List)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetHeight(50)
                    .SetBackground(true)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUIPrimaryButton  .Make("+",      OnEmitterAdd)   .SetWidth(10.0f),
                        XUISecondaryButton.Make("Toggle", OnEmitterToggle).SetWidth(15.0f),
                        XUISecondaryButton.Make("-",      OnEmitterDelete).SetWidth(10.0f)
                    )
                    .SetPadding(0)
                )
                .SetPadding(0)
                .SetWidth(41.0f),

                XUIVLayout.Make(

                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                .OnReady(x => m_MenuEmittersTab_EmitterWidget = x.gameObject.AddComponent<Widgets.EmitterWidget>())
            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true);
        }
        /// <summary>
        /// Build playing emitters tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildPlayingEmittersTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                            .OnListItemSelected((x) => OnEmitterSelected(m_PlayingEmittersTab_List, x))
                            .Bind(ref m_PlayingEmittersTab_List)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetHeight(50)
                    .SetBackground(true)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUIPrimaryButton.Make("+", OnEmitterAdd).SetWidth(10.0f),
                        XUISecondaryButton.Make("Toggle", OnEmitterToggle).SetWidth(15.0f),
                        XUISecondaryButton.Make("-", OnEmitterDelete).SetWidth(10.0f)
                    )
                    .SetPadding(0)
                )
                .SetPadding(0)
                .SetWidth(41.0f),

                XUIVLayout.Make(

                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                .OnReady(x => m_PlayingEmittersTab_EmitterWidget = x.gameObject.AddComponent<Widgets.EmitterWidget>())
            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true);
        }
        /// <summary>
        /// Build chat commands tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildChatCommandsTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Give moderators power"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_CommandsTab_ModeratorPowerToggle),

                XUIText.Make("Give VIP power"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_CommandsTab_VIPPowerToggle),

                XUIText.Make("Give subscriber power"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_CommandsTab_SubscriberPowerToggle),

                XUIText.Make("Give user power"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_CommandsTab_UserPowerToggle)
            );
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a tab is selected
        /// </summary>
        /// <param name="p_TabIndex">Tab index</param>
        private void OnTabSelected(int p_TabIndex)
        {
            ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu,    p_TabIndex == 1, null);
            ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, p_TabIndex == 2, null);
        }
        /// <summary>
        /// When settings are changed
        /// </summary>
        private void OnSettingChanged()
        {
            if (m_PreventChanges)
                return;

            #region General Tab
            var l_Config = CERConfig.Instance;
            l_Config.EnableMenu   = m_GeneralTab_MenuRain.Element.GetValue();
            l_Config.MenuSize     = m_GeneralTab_MenuRainSize.Element.GetValue();
            l_Config.MenuSpeed    = m_GeneralTab_MenuFallSpeed.Element.GetValue();

            l_Config.EnableSong   = m_GeneralTab_PlayingRain.Element.GetValue();
            l_Config.SongSize     = m_GeneralTab_PlayingRainSize.Element.GetValue();
            l_Config.SongSpeed    = m_GeneralTab_PlayingFallSpeed.Element.GetValue();

            m_GeneralTab_MenuRainSize.SetInteractable(l_Config.EnableMenu);
            m_GeneralTab_MenuFallSpeed.SetInteractable(l_Config.EnableMenu);

            m_GeneralTab_PlayingRainSize.SetInteractable(l_Config.EnableSong);
            m_GeneralTab_PlayingFallSpeed.SetInteractable(l_Config.EnableSong);
            #endregion

            #region ChatCommands Tab
            var l_ChatCommands = CERConfig.Instance.ChatCommands;
            l_ChatCommands.ModeratorPower     = m_CommandsTab_ModeratorPowerToggle.Element.GetValue();
            l_ChatCommands.VIPPower           = m_CommandsTab_VIPPowerToggle.Element.GetValue();
            l_ChatCommands.SubscriberPower    = m_CommandsTab_SubscriberPowerToggle.Element.GetValue();
            l_ChatCommands.UserPower          = m_CommandsTab_UserPowerToggle.Element.GetValue();
            #endregion

            ChatEmoteRain.Instance.OnSettingsChanged();
        }
        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            #region General Tab
            var l_Config = CERConfig.Instance;
            m_GeneralTab_MenuRain     .SetValue(l_Config.EnableMenu);
            m_GeneralTab_MenuRainSize .SetValue(l_Config.MenuSize);
            m_GeneralTab_MenuFallSpeed.SetValue(l_Config.MenuSpeed);

            m_GeneralTab_PlayingRain     .SetValue(l_Config.EnableSong);
            m_GeneralTab_PlayingRainSize .SetValue(l_Config.SongSize);
            m_GeneralTab_PlayingFallSpeed.SetValue(l_Config.SongSpeed);

            m_GeneralTab_MenuRainSize.SetInteractable(l_Config.EnableMenu);
            m_GeneralTab_MenuFallSpeed.SetInteractable(l_Config.EnableMenu);

            m_GeneralTab_PlayingRainSize.SetInteractable(l_Config.EnableSong);
            m_GeneralTab_PlayingFallSpeed.SetInteractable(l_Config.EnableSong);
            #endregion

            #region ChatCommands Tab
            var l_ChatCommands = CERConfig.Instance.ChatCommands;
            m_CommandsTab_ModeratorPowerToggle  .SetValue(l_ChatCommands.ModeratorPower);
            m_CommandsTab_VIPPowerToggle        .SetValue(l_ChatCommands.VIPPower);
            m_CommandsTab_SubscriberPowerToggle .SetValue(l_ChatCommands.SubscriberPower);
            m_CommandsTab_UserPowerToggle       .SetValue(l_ChatCommands.UserPower);
            #endregion

            m_PreventChanges = false;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When an emitter is selected
        /// </summary>
        /// <param name="p_List">Source list</param>
        /// <param name="p_Item">Selected item</param>
        private void OnEmitterSelected(XUIVVList p_List, IListItem p_Item)
        {
            var l_IsMenu = m_TabControl.Element.GetActiveTab() == 1;
            if (p_List == m_MenuEmittersTab_List)
            {
                m_SelectedItemMenu = (EmitterConfigListItem)p_Item;
                m_MenuEmittersTab_EmitterWidget.SetCurrent(m_SelectedItemMenu);

                ChatEmoteRain.Instance.SetTemplatesPreview(
                    CP_SDK.EGenericScene.Menu,
                    m_TabControl.Element.GetActiveTab() == 1,
                    m_SelectedItemMenu?.EConfig
                );
            }
            else
            {
                m_SelectedItemPlaying = (EmitterConfigListItem)p_Item;
                m_PlayingEmittersTab_EmitterWidget.SetCurrent(m_SelectedItemPlaying);

                ChatEmoteRain.Instance.SetTemplatesPreview(
                    CP_SDK.EGenericScene.Playing,
                    m_TabControl.Element.GetActiveTab() == 2,
                    m_SelectedItemPlaying?.EConfig
                );
            }
        }
        /// <summary>
        /// On add emitter button
        /// </summary>
        private void OnEmitterAdd()
        {
            var l_New = new EmitterConfigListItem(new EmitterConfig());
            if (m_TabControl.Element.GetActiveTab() == 1)
            {
                CERConfig.Instance.MenuEmitters.Add(l_New.EConfig);
                m_MenuEmittersTab_List.AddListItem(l_New);
                m_MenuEmittersTab_List.SetSelectedListItem(l_New);
                ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Menu);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu, true, l_New.EConfig);
            }
            else
            {
                CERConfig.Instance.SongEmitters.Add(l_New.EConfig);
                m_PlayingEmittersTab_List.AddListItem(l_New);
                m_PlayingEmittersTab_List.SetSelectedListItem(l_New);
                ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Playing);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, true, l_New.EConfig);
            }
        }
        /// <summary>
        /// On toggle emitter button
        /// </summary>
        private void OnEmitterToggle()
        {
            var l_IsMenu        = m_TabControl.Element.GetActiveTab() == 1;
            var l_DataSource    = l_IsMenu ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_Selected      = l_IsMenu ? m_SelectedItemMenu              : m_SelectedItemPlaying;

            if (l_Selected == null)
            {
                ShowMessageModal("Please select an emitter first!");
                return;
            }

            if (l_Selected.EConfig.Enabled)
            {
                ShowConfirmationModal($"Do you want to disable emitter\n\"{l_Selected.EConfig.Name}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_Selected.EConfig.Enabled = false;
                    l_Selected.RefreshVisual();

                    if (l_IsMenu)
                    {
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Menu);
                        ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu, true, l_Selected?.EConfig);
                    }
                    else
                    {
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Playing);
                        ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, true, l_Selected?.EConfig);
                    }
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable emitter\n\"{l_Selected.EConfig.Name}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_Selected.EConfig.Enabled = true;
                    l_Selected.RefreshVisual();

                    if (l_IsMenu)
                    {
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Menu);
                        ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu, true, l_Selected?.EConfig);
                    }
                    else
                    {
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Playing);
                        ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, true, l_Selected?.EConfig);
                    }
                });
            }
        }
        /// <summary>
        /// On delete emitter button
        /// </summary>
        private void OnEmitterDelete()
        {
            var l_IsMenu        = m_TabControl.Element.GetActiveTab() == 1;
            var l_DataSource    = l_IsMenu ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_Selected      = l_IsMenu ? m_SelectedItemMenu              : m_SelectedItemPlaying;

            if (l_Selected == null)
            {
                ShowMessageModal("Please select an emitter first!");
                return;
            }

            ShowConfirmationModal($"<color=red>Do you want to delete emitter</color>\n\"{l_Selected.EConfig.Name}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                if (l_IsMenu)
                {
                    m_MenuEmittersTab_List.RemoveListItem(l_Selected);
                    CERConfig.Instance.MenuEmitters.Remove(l_Selected.EConfig);
                    ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Menu);
                    ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Menu, true, l_Selected?.EConfig);
                }
                else
                {
                    m_PlayingEmittersTab_List.RemoveListItem(l_Selected);
                    CERConfig.Instance.SongEmitters.Remove(l_Selected.EConfig);
                    ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.EGenericScene.Playing);
                    ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.EGenericScene.Playing, true, l_Selected?.EConfig);
                }
            });
        }
    }
}
