using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Actions
{
    public class GamePlay_ChangeBombColor
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeBombColor, Models.Actions.GamePlay_ChangeBombColor>
    {
        private XUIDropdown     m_ValueSource = null;
        private XUIColorInput   m_Color       = null;
        private XUIToggle       m_SendMessage = null;

        private Color? m_ColorCache;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change bomb color";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Choices = new List<string>(Enums.ValueSource.S);
            if (   Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatCommand)
                && Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatPointsReward))
                l_Choices.Remove(Enums.ValueSource.ToStr(Enums.ValueSource.E.Event));

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("User color",
                    XUIColorInput.Make()
                        .SetValue(ColorU.ToUnityColor(Model.Color)).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Color)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();

            if (!ModulePresence.NoteTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: NoteTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            m_ColorCache = m_Color.Element.GetValue();

            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.Color             = ColorU.ToHexRGB(m_ColorCache.Value);
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_Color.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                || Model.ValueSource == Enums.ValueSource.E.Config)
            {
                if (Model.ValueSource == Enums.ValueSource.E.Config)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PBombNoteController.SetBombColorOverride(false, Color.black);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} bomb color is back to default!");

                    l_Failed = false;
                }
                else
                {
                    var l_Hex = Model.Color;

                    if (Model.ValueSource == Enums.ValueSource.E.Random)
                    {
                        m_ColorCache = UnityEngine.Random.ColorHSV();
                        l_Hex = ColorU.ToHexRGB(m_ColorCache.Value);
                    }
                    else if (Model.ValueSource == Enums.ValueSource.E.Event && (p_Context.Message != null || p_Context.PointsEvent != null)) /// Event user input
                    {
                        var l_Src   = (p_Context.Message?.Message ?? p_Context.PointsEvent?.UserInput) ?? "";
                        var l_Parts = l_Src.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (l_Parts.Length >= 1
                            && ColorU.TryToUnityColor(l_Parts[l_Parts.Length - 1], out var l_LeftColor))
                        {
                            m_ColorCache        = l_LeftColor;
                            l_Hex               = l_Parts[l_Parts.Length - 2];
                            l_Failed            = false;
                        }
                        else if (p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        {
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} the syntax is: #HEXCOLOR");
                        }
                    }
                    else if (Model.ValueSource == Enums.ValueSource.E.User)
                    {
                        l_Failed = false;
                        EnsureColorCache();
                    }

                    if (!l_Failed)
                    {
                        BeatSaberPlus_NoteTweaker.Patches.PBombNoteController.SetBombColorOverride(true, m_ColorCache.Value);

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} bomb color is changed to {l_Hex}");
                    }
                }
            }

            if (l_Failed)
                p_Context.HasActionFailed = true;

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void EnsureColorCache()
        {
            if (m_ColorCache.HasValue)
                return;

            m_ColorCache = ColorU.ToUnityColor(Model.Color);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeBombScale
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeBombScale, Models.Actions.GamePlay_ChangeBombScale>
    {
        private XUIDropdown m_ValueSource = null;
        private XUISlider   m_UserValue   = null;
        private XUISlider   m_Min         = null;
        private XUISlider   m_Max         = null;
        private XUIToggle   m_SendMessage = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Choose a random bomb scale";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(Enums.ValueSource.S).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("User value",
                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.UserValue).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_UserValue)
                ),

                Templates.SettingsHGroup("Random/Event value min/max",
                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Min).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Min),

                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Max).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Max)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.UserValue         = m_UserValue.Element.GetValue();
            Model.Min               = m_Min.Element.GetValue();
            Model.Max               = m_Max.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_UserValue.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_Min.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
            m_Max.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                || Model.ValueSource == Enums.ValueSource.E.Config)
            {
                if (Model.ValueSource == Enums.ValueSource.E.Config)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PBombNoteController.SetTemp(false, 0f);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} note scale was set to default");
                }
                else
                {
                    var l_NewValue = 0f;

                    /// Random
                    if (Model.ValueSource == Enums.ValueSource.E.Random)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueSource == Enums.ValueSource.E.User)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueSource == Enums.ValueSource.E.Event)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(ChatPlexMod_ChatIntegrations.Interfaces.EValueType.Integer);
                        var l_EventInput    = 1f;

                        if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                        {
                            l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                            l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                            l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                        }

                        l_NewValue = l_EventInput;
                    }

                    BeatSaberPlus_NoteTweaker.Patches.PBombNoteController.SetTemp(true, l_NewValue);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} bomb scale was set to {Mathf.RoundToInt(l_NewValue * 100f)}%");
                }

                l_Failed = false;
            }

            if (l_Failed)
            {
                if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} no map is currently played!");

                p_Context.HasActionFailed = true;
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeDebris
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeDebris, Models.Actions.GamePlay_ChangeDebris>
    {
        private XUIToggle m_Debris = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Turn on or off debris";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Enable debris",
                    XUIToggle.Make()
                        .SetValue(Model.Debris).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Debris)
                )
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.GameTweaker)
                View.ShowMessageModal("GameTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.Debris = m_Debris.Element.GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.GameTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, GameTweaker module is missing!");
                yield break;
            }

            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
                BeatSaberPlus_GameTweaker.Patches.PNoteDebrisSpawner.SetTemp(!Model.Debris);
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeLightIntensity
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeLightIntensity, Models.Actions.GamePlay_ChangeLightIntensity>
    {
        private XUIDropdown m_ValueSource = null;
        private XUISlider   m_UserValue   = null;
        private XUISlider   m_Min         = null;
        private XUISlider   m_Max         = null;
        private XUIToggle   m_SendMessage = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Choose a random light intensity";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(Enums.ValueSource.S).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("User value",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(20.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.UserValue).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_UserValue)
                ),

                Templates.SettingsHGroup("Random/Event value min/max",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(20.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Min).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Min),

                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(20.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Max).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Max)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.UserValue         = m_UserValue.Element.GetValue();
            Model.Min               = m_Min.Element.GetValue();
            Model.Max               = m_Max.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_UserValue.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_Min.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
            m_Max.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.GameTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, GameTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_Level     = CP_SDK_BS.Game.Logic.LevelData?.Data?.difficultyBeatmap?.difficulty;
                var l_Effects   = l_Level == BeatmapDifficulty.ExpertPlus
                    ? CP_SDK_BS.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterExpertPlusPreset
                    : CP_SDK_BS.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterDefaultPreset;

                if (l_Effects != EnvironmentEffectsFilterPreset.NoEffects)
                {
                    if (Model.ValueSource == Enums.ValueSource.E.Config)
                    {
                        BeatSaberPlus_GameTweaker.Patches.Lights.PLightsPatches.SetFromConfig();

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} lights was set to default");
                    }
                    else
                    {
                        var l_NewValue = 0f;

                        /// Random
                        if (Model.ValueSource == Enums.ValueSource.E.Random)
                            l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                        /// User input
                        else if (Model.ValueSource == Enums.ValueSource.E.User)
                            l_NewValue = Model.UserValue;
                        /// Event input
                        else if (Model.ValueSource == Enums.ValueSource.E.Event)
                        {
                            var l_FirstInteger  = p_Context.GetFirstValueOfType(ChatPlexMod_ChatIntegrations.Interfaces.EValueType.Integer);
                            var l_EventInput    = 1f;

                            if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                            {
                                l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                                l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                                l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                            }

                            l_NewValue = l_EventInput;
                        }

                        BeatSaberPlus_GameTweaker.Patches.Lights.PLightsPatches.SetTempLightIntensity(l_NewValue);

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} lights was set to {Mathf.RoundToInt(l_NewValue * 100f)}%");
                    }

                    l_Failed = false;
                }
            }

            if (l_Failed)
            {
                if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} no map is currently played!");

                p_Context.HasActionFailed = true;
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeMusicVolume
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeMusicVolume, Models.Actions.GamePlay_ChangeMusicVolume>
    {
        private XUIDropdown m_ValueSource = null;
        private XUISlider   m_UserValue   = null;
        private XUISlider   m_Min         = null;
        private XUISlider   m_Max         = null;
        private XUIToggle   m_SendMessage = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Choose a random volume music";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(Enums.ValueSource.S).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("User value",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.UserValue).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_UserValue)
                ),

                Templates.SettingsHGroup("Random/Event value min/max",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Min).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Min),

                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Max).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Max)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.UserValue         = m_UserValue.Element.GetValue();
            Model.Min               = m_Min.Element.GetValue();
            Model.Max               = m_Max.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_UserValue.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_Min.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
            m_Max.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_NewValue                  = 1f;
                var l_AudioTimeSyncController   = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();

                if (l_AudioTimeSyncController != null && l_AudioTimeSyncController)
                {
                    /// Random
                    if (Model.ValueSource == Enums.ValueSource.E.Random)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueSource == Enums.ValueSource.E.User)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueSource == Enums.ValueSource.E.Event)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(ChatPlexMod_ChatIntegrations.Interfaces.EValueType.Integer);
                        var l_EventInput    = 1f;

                        if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                        {
                            l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                            l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                            l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                        }

                        l_NewValue = l_EventInput;
                    }

                    l_AudioTimeSyncController._audioSource.volume = l_NewValue;

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} music volume was set to {Mathf.RoundToInt(l_NewValue * 100f)}]%");
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeNoteColors
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeNoteColors, Models.Actions.GamePlay_ChangeNoteColors>
    {
        private XUIDropdown     m_ValueSource   = null;
        private XUIColorInput   m_LeftColor     = null;
        private XUIColorInput   m_RightColor    = null;
        private XUIToggle       m_SendMessage   = null;

        private Color? m_LeftColorCache;
        private Color? m_RightColorCache;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change notes colors";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Choices = new List<string>(Enums.ValueSource.S);
            if (   Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatCommand)
                && Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatPointsReward))
                l_Choices.Remove(Enums.ValueSource.ToStr(Enums.ValueSource.E.Event));

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("Left/Right user color",
                    XUIColorInput.Make()
                        .SetValue(ColorU.ToUnityColor(Model.Left)).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_LeftColor),

                    XUIColorInput.Make()
                        .SetValue(ColorU.ToUnityColor(Model.Right)).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_RightColor)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();

            if (!ModulePresence.NoteTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: NoteTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            m_LeftColorCache        = m_LeftColor.Element.GetValue();
            m_RightColorCache       = m_RightColor.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            Model.Left  = ColorU.ToHexRGB(m_LeftColorCache.Value);
            Model.Right = ColorU.ToHexRGB(m_RightColorCache.Value);

            m_LeftColor.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_RightColor.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                || Model.ValueSource == Enums.ValueSource.E.Config)
            {
                if (Model.ValueSource == Enums.ValueSource.E.Config)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
                    PatchSabers(true);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} colors are back to default!");

                    l_Failed = false;
                }
                else
                {
                    string l_LeftHex    = Model.Left;
                    string l_RightHex   = Model.Right;

                    if (Model.ValueSource == Enums.ValueSource.E.Random)
                    {
                        m_LeftColorCache    = UnityEngine.Random.ColorHSV();
                        m_RightColorCache   = UnityEngine.Random.ColorHSV();

                        l_LeftHex  = ColorU.ToHexRGB(m_LeftColorCache.Value);
                        l_RightHex = ColorU.ToHexRGB(m_RightColorCache.Value);
                    }
                    else if (Model.ValueSource == Enums.ValueSource.E.Event && (p_Context.Message != null || p_Context.PointsEvent != null)) /// Event user input
                    {
                        var l_Src = (p_Context.Message?.Message ?? p_Context.PointsEvent?.UserInput) ?? "";
                        var l_Parts = l_Src.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (l_Parts.Length >= 2
                            && ColorU.TryToUnityColor(l_Parts[l_Parts.Length - 2], out var l_LeftColor)
                            && ColorU.TryToUnityColor(l_Parts[l_Parts.Length - 1], out var l_RightColor))
                        {
                            m_LeftColorCache    = l_LeftColor;
                            m_RightColorCache   = l_RightColor;
                            l_LeftHex           = l_Parts[l_Parts.Length - 2];
                            l_RightHex          = l_Parts[l_Parts.Length - 1];
                            l_Failed            = false;
                        }
                        else if (p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        {
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} the syntax is: #LEFTHEX #RIGHTHEX");
                        }
                    }
                    else if (Model.ValueSource == Enums.ValueSource.E.User)
                    {
                        l_Failed = false;
                        EnsureColorCache();
                    }

                    if (!l_Failed)
                    {
                        BeatSaberPlus_NoteTweaker.Patches.PColorNoteVisuals.SetBlockColorOverride(true, m_LeftColorCache.Value, m_RightColorCache.Value);
                        PatchSabers(false);

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} colors are changed to {l_LeftHex} {l_RightHex}");
                    }
                }
            }

            if (l_Failed)
                p_Context.HasActionFailed = true;

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void EnsureColorCache()
        {
            if (!m_LeftColorCache.HasValue)
            {
                if (ColorU.TryToUnityColor(Model.Left, out var l_LeftColor))
                    m_LeftColorCache = l_LeftColor;
                else
                    m_LeftColorCache = CP_SDK_BS.Game.Logic.LevelData?.Data?.colorScheme?.saberAColor ?? Color.red;
            }
            if (!m_RightColorCache.HasValue)
            {
                if (ColorU.TryToUnityColor(Model.Right, out var l_RightColor))
                    m_RightColorCache = l_RightColor;
                else
                    m_RightColorCache = CP_SDK_BS.Game.Logic.LevelData?.Data?.colorScheme?.saberBColor ?? Color.blue;
            }
        }
        private void PatchSabers(bool p_UseDefault)
        {
            /// todo
            return;

            var l_Sabers            = Resources.FindObjectsOfTypeAll<SaberModelController>();
            var l_ColorManager      = null as ColorManager;
            var l_ColorSchemeBackup = null as ColorScheme;

            for (int l_I = 0; l_I < l_Sabers.Length; ++l_I)
            {
                if (l_I == 0)
                {
                    l_ColorManager = l_Sabers[l_I].GetField<ColorManager, SaberModelController>("_colorManager");

                    if (l_ColorManager != null)
                    {
                        l_ColorSchemeBackup = l_ColorManager.GetProperty<ColorScheme, ColorManager>("_colorScheme");
                        if (l_ColorSchemeBackup != null && !p_UseDefault)
                        {
                            var l_ColorScheme = new ColorScheme("", "", false, "", false,
                                m_LeftColorCache.Value, m_RightColorCache.Value, l_ColorSchemeBackup.environmentColor0,
                                l_ColorSchemeBackup.environmentColor1, l_ColorSchemeBackup.supportsEnvironmentColorBoost,
                                l_ColorSchemeBackup.environmentColor0Boost, l_ColorSchemeBackup.environmentColor1Boost,
                                l_ColorSchemeBackup.obstaclesColor);

                            l_ColorManager.SetProperty<ColorManager, ColorScheme>("_colorScheme", l_ColorScheme);
                        }
                    }
                }

                if (l_ColorSchemeBackup == null)
                    break;

                var l_SaberTrail                = l_Sabers[l_I].GetField<SaberTrail, SaberModelController>("_saberTrail");
                var l_SetSaberGlowColors        = l_Sabers[l_I].GetField<SetSaberGlowColor[], SaberModelController>("_setSaberGlowColors");
                var l_SetSaberFakeGlowColors    = l_Sabers[l_I].GetField<SetSaberFakeGlowColor[], SaberModelController>("_setSaberFakeGlowColors");
                var l_SaberLight                = l_Sabers[l_I].GetField<TubeBloomPrePassLight, SaberModelController>("_saberLight");

                if (l_SaberTrail == null || l_SetSaberGlowColors == null || l_SetSaberFakeGlowColors == null || l_SaberLight == null)
                    continue;

                var l_SaberType = l_SaberLight.color == l_ColorSchemeBackup.saberAColor ? SaberType.SaberA : SaberType.SaberB;
                var l_Color     = l_SaberType == SaberType.SaberA ? m_LeftColorCache.Value : m_RightColorCache.Value;

                //l_SaberTrail.Setup((l_Color * this._initData.trailTintColor).linear, (IBladeMovementData)saber.movementData);

                foreach (var l_SetSaberGlowColor in l_SetSaberGlowColors)
                    l_SetSaberGlowColor.SetColors();
                foreach (var l_SaberFakeGlowColor in l_SetSaberFakeGlowColors)
                    l_SaberFakeGlowColor.SetColors();

                l_SaberLight.color = l_Color;

                if (!p_UseDefault && l_I == (l_Sabers.Length - 1))
                    l_ColorManager.SetProperty<ColorManager, ColorScheme>("_colorScheme", l_ColorSchemeBackup);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeNoteScale
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ChangeNoteScale, Models.Actions.GamePlay_ChangeNoteScale>
    {
        private XUIDropdown m_ValueSource = null;
        private XUISlider   m_UserValue   = null;
        private XUISlider   m_Min         = null;
        private XUISlider   m_Max         = null;
        private XUIToggle   m_SendMessage = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Choose a random note scale";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(Enums.ValueSource.S).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsHGroup("User value",
                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.UserValue).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_UserValue)
                ),

                Templates.SettingsHGroup("Random/Event value min/max",
                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Min).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Min),

                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Max).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Max)
                ),

                Templates.SettingsHGroup("Send chat message?",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessage)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.UserValue         = m_UserValue.Element.GetValue();
            Model.Min               = m_Min.Element.GetValue();
            Model.Max               = m_Max.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_UserValue.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_Min.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
            m_Max.SetInteractable(Model.ValueSource == Enums.ValueSource.E.Random || Model.ValueSource == Enums.ValueSource.E.Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                || Model.ValueSource == Enums.ValueSource.E.Config)
            {
                if (Model.ValueSource == Enums.ValueSource.E.Config)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PGameNoteController.SetTemp(false, 0f);
                    BeatSaberPlus_NoteTweaker.Patches.PBurstSliderGameNoteController.SetTemp(false, 0f);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} note scale was set to default");
                }
                else
                {
                    var l_NewValue = 0f;

                    /// Random
                    if (Model.ValueSource == Enums.ValueSource.E.Random)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueSource == Enums.ValueSource.E.User)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueSource == Enums.ValueSource.E.Event)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(ChatPlexMod_ChatIntegrations.Interfaces.EValueType.Integer);
                        var l_EventInput    = 1f;

                        if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                        {
                            l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                            l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                            l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                        }

                        l_NewValue = l_EventInput;
                    }

                    BeatSaberPlus_NoteTweaker.Patches.PGameNoteController.SetTemp(true, l_NewValue);
                    BeatSaberPlus_NoteTweaker.Patches.PBurstSliderGameNoteController.SetTemp(true, l_NewValue);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} note scale was set to {Mathf.RoundToInt(l_NewValue * 100f)}%");
                }

                l_Failed = false;
            }

            if (l_Failed)
            {
                if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} no map is currently played!");

                p_Context.HasActionFailed = true;
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_Pause
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_Pause, Models.Actions.GamePlay_Pause>
    {
        private XUIToggle m_HideUI = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Trigger a pause during a song";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Hide UI",
                    XUIToggle.Make()
                        .SetValue(Model.HideUI).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_HideUI)
                )
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.GameTweaker)
                View.ShowMessageModal("GameTweaker module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.HideUI = m_HideUI.Element.GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_BSP_MP_BigRank = GameObject.Find("BSP_MP_BigRank");
                if (l_BSP_MP_BigRank && l_BSP_MP_BigRank.activeSelf)
                    p_Context.HasActionFailed = true;
                else
                {
                    var l_PauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();
                    var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                    if (l_PauseController && l_PauseMenuManager)
                    {
                        l_PauseController.Pause();
                        if (Model.HideUI)
                        {
                            l_PauseController.didResumeEvent += PauseController_didResumeEvent;
                            l_PauseMenuManager.transform.Find("Wrapper/MenuWrapper/Canvas")?.gameObject?.SetActive(false);
                        }
                    }
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void PauseController_didResumeEvent()
        {
            var l_PauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();
            if (l_PauseController)
                l_PauseController.didResumeEvent -= PauseController_didResumeEvent;

            var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
            if (l_PauseMenuManager)
                l_PauseMenuManager.transform.Find("Wrapper/MenuWrapper/Canvas")?.gameObject?.SetActive(true);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_Quit
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_Quit, ChatPlexMod_ChatIntegrations.Models.Action>
    {
        public override string Description => "Exit current song";
        public override string UIPlaceHolder => "Will exit the current map";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_BSP_MP_BigRank = GameObject.Find("BSP_MP_BigRank");
                if (l_BSP_MP_BigRank && l_BSP_MP_BigRank.activeSelf)
                    p_Context.HasActionFailed = true;
                else
                {
                    var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                    if (l_PauseMenuManager != null && l_PauseMenuManager)
                        l_PauseMenuManager.MenuButtonPressed();
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_Restart
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_Restart, ChatPlexMod_ChatIntegrations.Models.Action>
    {
        public override string Description => "Restart current song";
        public override string UIPlaceHolder => "Will restart the map";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_BSP_MP_BigRank = GameObject.Find("BSP_MP_BigRank");
                if (l_BSP_MP_BigRank && l_BSP_MP_BigRank.activeSelf)
                    p_Context.HasActionFailed = true;
                else
                {
                    var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                    if (l_PauseMenuManager != null && l_PauseMenuManager)
                        l_PauseMenuManager.RestartButtonPressed();
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_Resume
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_Resume, ChatPlexMod_ChatIntegrations.Models.Action>
    {
        public override string Description   => "Resume current song";
        public override string UIPlaceHolder => "Will resume the map";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_BSP_MP_BigRank = GameObject.Find("BSP_MP_BigRank");
                if (l_BSP_MP_BigRank && l_BSP_MP_BigRank.activeSelf)
                    p_Context.HasActionFailed = true;
                else
                {
                    var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                    if (l_PauseMenuManager != null && l_PauseMenuManager)
                        l_PauseMenuManager.ContinueButtonPressed();
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_SpawnBombPatterns
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_SpawnBombPatterns, Models.Actions.GamePlay_SpawnBombPatterns>
    {

        private XUISlider m_Interval    = null;
        private XUISlider m_Count       = null;

        protected XUIToggle m_L0R2 = null;
        protected XUIToggle m_L1R2 = null;
        protected XUIToggle m_L2R2 = null;
        protected XUIToggle m_L3R2 = null;

        protected XUIToggle m_L0R1 = null;
        protected XUIToggle m_L1R1 = null;
        protected XUIToggle m_L2R1 = null;
        protected XUIToggle m_L3R1 = null;

        protected XUIToggle m_L0R0 = null;
        protected XUIToggle m_L1R0 = null;
        protected XUIToggle m_L2R0 = null;
        protected XUIToggle m_L3R0 = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Spawn bomb patterns in a map";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Interval between bombs in seconds",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(20.0f).SetIncrements(1.0f).SetInteger(true).SetFormatter(CP_SDK.UI.ValueFormatters.TimeShortBaseSeconds)
                        .SetValue(Model.Interval).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Interval)
                ),

                Templates.SettingsHGroup("Number of bombs pattern",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(20.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.Count).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Count)
                ),

                Templates.SettingsHGroup("Top",
                    XUIToggle.Make().SetValue((Model.L0 & (1 << 2)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L0R2),
                    XUIToggle.Make().SetValue((Model.L1 & (1 << 2)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L1R2),
                    XUIToggle.Make().SetValue((Model.L2 & (1 << 2)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L2R2),
                    XUIToggle.Make().SetValue((Model.L3 & (1 << 2)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L3R2)
                ),
                Templates.SettingsHGroup("Middle",
                    XUIToggle.Make().SetValue((Model.L0 & (1 << 1)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L0R1),
                    XUIToggle.Make().SetValue((Model.L1 & (1 << 1)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L1R1),
                    XUIToggle.Make().SetValue((Model.L2 & (1 << 1)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L2R1),
                    XUIToggle.Make().SetValue((Model.L3 & (1 << 1)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L3R1)
                ),
                Templates.SettingsHGroup("Bottom",
                    XUIToggle.Make().SetValue((Model.L0 & (1 << 0)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L0R0),
                    XUIToggle.Make().SetValue((Model.L1 & (1 << 0)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L1R0),
                    XUIToggle.Make().SetValue((Model.L2 & (1 << 0)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L2R0),
                    XUIToggle.Make().SetValue((Model.L3 & (1 << 0)) != 0).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_L3R0)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Interval  = (int)m_Interval.Element.GetValue();
            Model.Count     = (int)m_Count.Element.GetValue();
            Model.L0        = (byte)((m_L0R0.Element.GetValue() ? (1 << 0) : 0) | (m_L0R1.Element.GetValue() ? (1 << 1) : 0) | (m_L0R2.Element.GetValue() ? (1 << 2) : 0));
            Model.L1        = (byte)((m_L1R0.Element.GetValue() ? (1 << 0) : 0) | (m_L1R1.Element.GetValue() ? (1 << 1) : 0) | (m_L1R2.Element.GetValue() ? (1 << 2) : 0));
            Model.L2        = (byte)((m_L2R0.Element.GetValue() ? (1 << 0) : 0) | (m_L2R1.Element.GetValue() ? (1 << 1) : 0) | (m_L2R2.Element.GetValue() ? (1 << 2) : 0));
            Model.L3        = (byte)((m_L3R0.Element.GetValue() ? (1 << 0) : 0) | (m_L3R1.Element.GetValue() ? (1 << 1) : 0) | (m_L3R2.Element.GetValue() ? (1 << 2) : 0));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                && CP_SDK_BS.Game.Logic.LevelData != null
                && !CP_SDK_BS.Game.Logic.LevelData.IsNoodle
                && !CP_SDK_BS.Game.Scoring.IsInReplay)
            {
                var l_AudioTimeSyncController       = UnityEngine.Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
                var l_BeatmapObjectSpawnController  = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();

                if (l_AudioTimeSyncController != null && l_AudioTimeSyncController
                    && l_BeatmapObjectSpawnController != null && l_BeatmapObjectSpawnController)
                {
                    List<(int, int)> l_SpawnList = new List<(int, int)>();
                    for (int l_B = 0; l_B < 3; ++l_B)
                    {
                        var l_Mask = 1 << l_B;

                        if ((Model.L0 & l_Mask) != 0)
                            l_SpawnList.Add((0, l_B));
                        if ((Model.L1 & l_Mask) != 0)
                            l_SpawnList.Add((1, l_B));
                        if ((Model.L2 & l_Mask) != 0)
                            l_SpawnList.Add((2, l_B));
                        if ((Model.L3 & l_Mask) != 0)
                            l_SpawnList.Add((3, l_B));
                    }

                    float l_Time = 2f;
                    for (int l_I = 0; l_I < Model.Count; ++l_I)
                    {
                        for (int l_S = 0; l_S < l_SpawnList.Count; ++l_S)
                        {
                            var l_Current = l_SpawnList[l_S];
                            l_BeatmapObjectSpawnController.HandleNoteDataCallback(NoteData.CreateBombNoteData(
                                l_AudioTimeSyncController.songTime + l_Time,
                                l_Current.Item1,
                                (NoteLineLayer)l_Current.Item2
                                )
                            );
                        }

                        l_Time += Model.Interval;
                    }
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_SpawnSquatWalls
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_SpawnSquatWalls, Models.Actions.GamePlay_SpawnSquatWalls>
    {
        private XUISlider m_Interval    = null;
        private XUISlider m_Count       = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Spawn fake squat walls in a map";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Interval between squat walls in seconds",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(20.0f).SetIncrements(1.0f).SetInteger(true).SetFormatter(CP_SDK.UI.ValueFormatters.TimeShortBaseSeconds)
                        .SetValue(Model.Interval).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Interval)
                ),

                Templates.SettingsHGroup("Number of squat wall",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(20.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.Count).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Count)
                )
            };

            BuildUIAuto(p_Parent);

            OnSettingChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Interval  = (int)m_Interval.Element.GetValue();
            Model.Count     = (int)m_Count.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing
                && CP_SDK_BS.Game.Logic.LevelData != null
                && !CP_SDK_BS.Game.Logic.LevelData.IsNoodle
                && !CP_SDK_BS.Game.Scoring.IsInReplay)
            {
                var l_AudioTimeSyncController       = UnityEngine.Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
                var l_BeatmapObjectSpawnController  = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();

                if (   l_AudioTimeSyncController      != null && l_AudioTimeSyncController
                    && l_BeatmapObjectSpawnController != null && l_BeatmapObjectSpawnController)
                {
                    float l_Time = 2f;
                    for (int l_I = 0; l_I < Model.Count; ++l_I)
                    {
                        l_BeatmapObjectSpawnController.HandleObstacleDataCallback(new ObstacleData(
                            l_AudioTimeSyncController.songTime + l_Time, 4, NoteLineLayer.Top, 0.3f, -4, 3
                        ));
                        l_Time += Model.Interval;
                    }
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ToggleHUD
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<GamePlay_ToggleHUD, Models.Actions.GamePlay_ToggleHUD>
    {
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Toggle HUD visibility";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(ChatPlexMod_ChatIntegrations.Enums.Toggle.S).SetValue(ChatPlexMod_ChatIntegrations.Enums.Toggle.ToStr(Model.ChangeType)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                var l_CoreGameHUDController = Resources.FindObjectsOfTypeAll<CoreGameHUDController>().FirstOrDefault();
                if (l_CoreGameHUDController != null && l_CoreGameHUDController)
                {
                    switch (Model.ChangeType)
                    {
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                            l_CoreGameHUDController.gameObject.SetActive(!l_CoreGameHUDController.gameObject.activeSelf);
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                            l_CoreGameHUDController.gameObject.SetActive(true);
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                            l_CoreGameHUDController.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_NoteCutScoreSpawner = Resources.FindObjectsOfTypeAll<NoteCutScoreSpawner>().FirstOrDefault();
                if (l_NoteCutScoreSpawner != null && l_NoteCutScoreSpawner)
                {
                    switch (Model.ChangeType)
                    {
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                            if (l_NoteCutScoreSpawner.gameObject.activeSelf)
                            {
                                l_NoteCutScoreSpawner.OnDestroy();
                                l_NoteCutScoreSpawner.gameObject.SetActive(false);
                            }
                            else
                            {
                                l_NoteCutScoreSpawner.gameObject.SetActive(true);
                                l_NoteCutScoreSpawner.Start();
                            }
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                            l_NoteCutScoreSpawner.gameObject.SetActive(true);
                            l_NoteCutScoreSpawner.Start();
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                            l_NoteCutScoreSpawner.OnDestroy();
                            l_NoteCutScoreSpawner.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_BadNoteCutEffectSpawner = Resources.FindObjectsOfTypeAll<BadNoteCutEffectSpawner>().FirstOrDefault();
                if (l_BadNoteCutEffectSpawner != null && l_BadNoteCutEffectSpawner)
                {
                    switch (Model.ChangeType)
                    {
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                            if (l_BadNoteCutEffectSpawner.gameObject.activeSelf)
                            {
                                l_BadNoteCutEffectSpawner.OnDestroy();
                                l_BadNoteCutEffectSpawner.gameObject.SetActive(false);
                            }
                            else
                            {
                                l_BadNoteCutEffectSpawner.gameObject.SetActive(true);
                                l_BadNoteCutEffectSpawner.Start();
                            }
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                            l_BadNoteCutEffectSpawner.gameObject.SetActive(true);
                            l_BadNoteCutEffectSpawner.Start();
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                            l_BadNoteCutEffectSpawner.OnDestroy();
                            l_BadNoteCutEffectSpawner.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_MissedNoteEffectSpawner = Resources.FindObjectsOfTypeAll<MissedNoteEffectSpawner>().FirstOrDefault();
                if (l_MissedNoteEffectSpawner != null && l_BadNoteCutEffectSpawner)
                {
                    switch (Model.ChangeType)
                    {
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                            if (l_MissedNoteEffectSpawner.gameObject.activeSelf)
                            {
                                l_MissedNoteEffectSpawner.OnDestroy();
                                l_MissedNoteEffectSpawner.gameObject.SetActive(false);
                            }
                            else
                            {
                                l_MissedNoteEffectSpawner.gameObject.SetActive(true);
                                l_MissedNoteEffectSpawner.Start();
                            }
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                            l_MissedNoteEffectSpawner.gameObject.SetActive(true);
                            l_MissedNoteEffectSpawner.Start();
                            break;
                        case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                            l_MissedNoteEffectSpawner.OnDestroy();
                            l_MissedNoteEffectSpawner.gameObject.SetActive(false);
                            break;
                    }
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }
}
