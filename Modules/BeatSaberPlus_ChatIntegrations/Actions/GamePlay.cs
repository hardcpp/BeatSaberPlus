using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberPlus_ChatIntegrations.Models;
using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class GamePlayBuilder
    {
        internal static List<Interfaces.IActionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            switch (p_Event)
            {
                case Events.LevelEnded _:
                    return new List<Interfaces.IActionBase>()
                    {

                    };
            }

            return new List<Interfaces.IActionBase>()
            {
                new GamePlay_ChangeBombColor(),
                new GamePlay_ChangeBombScale(),
                new GamePlay_ChangeDebris(),
                new GamePlay_ChangeLightIntensity(),
                new GamePlay_ChangeMusicVolume(),
                new GamePlay_ChangeNoteColors(),
                new GamePlay_ChangeNoteScale(),
                new GamePlay_Pause(),
                new GamePlay_Quit(),
                new GamePlay_Restart(),
                new GamePlay_Resume(),
                new GamePlay_SpawnBombPatterns(),
                new GamePlay_SpawnSquatWalls(),
                new GamePlay_ToggleHUD()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////


    public class GamePlay_ChangeBombColor : Interfaces.IAction<GamePlay_ChangeBombColor, Models.Actions.GamePlay_ChangeBombColor>
    {
        public override string Description => "Change bomb color";

#pragma warning disable CS0414
        [UIComponent("TypeList")]           private     ListSetting     m_TypeList = null;
        [UIValue("TypeList_Choices")]       private     List<object>    m_TypeListList_Choices = new List<object>() { "Default", "Input", "EventInput" };
        [UIValue("TypeList_Value")]         private     string          m_TypeList_Value;
        [UIComponent("Color")]              protected   ColorSetting    m_Color = null;
        [UIComponent("SendMessageToggle")]  private     ToggleSetting   m_SendMessageToggle = null;
#pragma warning restore CS0414

        private Color? m_ColorCache;

        public override sealed void BuildUI(Transform p_Parent)
        {
            if (Event.GetType() != typeof(Events.ChatCommand)
                && Event.GetType() != typeof(Events.ChatPointsReward))
            {
                m_TypeListList_Choices.Remove("EventInput");
            }

            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            EnsureColorCache();

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,              l_Event,                            false);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_Color,                l_Event,    m_ColorCache.Value,     false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle,   l_Event,    Model.SendChatMessage,  false);

            OnSettingChanged(null);

            if (!ModulePresence.NoteTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: NoteTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            m_ColorCache            = m_Color.CurrentColor;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            Model.Color  = "#" + ColorUtility.ToHtmlStringRGB(m_ColorCache.Value);

            m_Color.interactable    = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                || Model.ValueType == 0)
            {
                if (Model.ValueType == 0)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PBombController.SetBombColorOverride(false, Color.black);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} bomb color is back to default!");

                    l_Failed = false;
                }
                else
                {
                    var l_Hex = "#" + Model.Color;

                    if (Model.ValueType == 2 && (p_Context.Message != null || p_Context.PointsEvent != null)) /// Event user input
                    {
                        var l_Src   = (p_Context.Message?.Message ?? p_Context.PointsEvent?.UserInput) ?? "";
                        var l_Parts = l_Src.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (l_Parts.Length >= 1
                            && ColorUtility.TryParseHtmlString(l_Parts[l_Parts.Length - 1], out var l_LeftColor))
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
                    else
                    {
                        l_Failed = false;
                        EnsureColorCache();
                    }

                    if (!l_Failed)
                    {
                        BeatSaberPlus_NoteTweaker.Patches.PBombController.SetBombColorOverride(true, m_ColorCache.Value);

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} bomb color is changed to {l_Hex}");
                    }
                }
            }

            if (l_Failed)
                p_Context.HasActionFailed = true;

            yield return null;
        }

        private void EnsureColorCache()
        {
            if (!m_ColorCache.HasValue)
            {
                if (ColorUtility.TryParseHtmlString(Model.Color, out var l_Color))
                    m_ColorCache = l_Color;
                else
                    m_ColorCache = Color.black;
            }
        }
    }

    public class GamePlay_ChangeBombScale : Interfaces.IAction<GamePlay_ChangeBombScale, Models.Actions.GamePlay_ChangeBombScale>
    {
        public override string Description => "Choose a random bomb scale";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Random", "Input", "EventInput", "Config" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIComponent("UserIncrement")]
        protected IncrementSetting m_UserIncrement = null;
        [UIComponent("MinIncrement")]
        protected IncrementSetting m_MinIncrement = null;
        [UIComponent("MaxIncrement")]
        protected IncrementSetting m_MaxIncrement = null;
        [UIComponent("SendMessageToggle")]
        private ToggleSetting m_SendMessageToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                                  false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.UserValue,        false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Min,              false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Max,              false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                                          Model.SendChatMessage,  false);

            OnSettingChanged(null);

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.UserValue         = m_UserIncrement.Value;
            Model.Min               = m_MinIncrement.Value;
            Model.Max               = m_MaxIncrement.Value;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            m_MinIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_MaxIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_UserIncrement.interactable    = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                || Model.ValueType == 3)
            {
                if (Model.ValueType == 3)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PBombController.SetTemp(false, 0f);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} note scale was set to default");
                }
                else
                {
                    var l_NewValue = 0f;

                    /// Random
                    if (Model.ValueType == 0)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueType == 1)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueType == 2)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(Interfaces.IValueType.Integer);
                        var l_EventInput    = 1f;

                        if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                        {
                            l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                            l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                            l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                        }

                        l_NewValue = l_EventInput;
                    }

                    BeatSaberPlus_NoteTweaker.Patches.PBombController.SetTemp(true, l_NewValue);

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

    public class GamePlay_ChangeDebris : Interfaces.IAction<GamePlay_ChangeDebris, Models.Actions.GamePlay_ChangeDebris>
    {
        public override string Description => "Turn on or off debris";

#pragma warning disable CS0414
        [UIComponent("DebrisToggle")]
        private ToggleSetting m_DebrisToggle = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DebrisToggle, l_Event, Model.Debris, false);

            OnSettingChanged(null);

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Debris = m_DebrisToggle.Value;
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (!ModulePresence.GameTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, GameTweaker module is missing!");
                yield break;
            }

            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                BeatSaberPlus_GameTweaker.Patches.PNoteDebrisSpawner.SetTemp(!Model.Debris);
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_ChangeLightIntensity : Interfaces.IAction<GamePlay_ChangeLightIntensity, Models.Actions.GamePlay_ChangeLightIntensity>
    {
        public override string Description => "Choose a random light intensity";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Random", "Input", "EventInput", "Config" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIComponent("UserIncrement")]
        protected IncrementSetting m_UserIncrement = null;
        [UIComponent("MinIncrement")]
        protected IncrementSetting m_MinIncrement = null;
        [UIComponent("MaxIncrement")]
        protected IncrementSetting m_MaxIncrement = null;
        [UIComponent("SendMessageToggle")]
        private ToggleSetting m_SendMessageToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                                  false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.UserValue,        false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Min,              false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Max,              false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                                          Model.SendChatMessage,  false);

            OnSettingChanged(null);

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.UserValue         = m_UserIncrement.Value;
            Model.Min               = m_MinIncrement.Value;
            Model.Max               = m_MaxIncrement.Value;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            m_MinIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_MaxIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_UserIncrement.interactable    = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.GameTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, GameTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
            {
                var l_Level     = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.difficultyBeatmap?.difficulty;
                var l_Effects   = l_Level == BeatmapDifficulty.ExpertPlus
                    ? BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterExpertPlusPreset
                    : BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterDefaultPreset;

                if (l_Effects != EnvironmentEffectsFilterPreset.NoEffects)
                {
                    if (Model.ValueType == 3)
                    {
                        BeatSaberPlus_GameTweaker.Patches.Lights.PLightsPatches.SetFromConfig();

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} lights was set to default");
                    }
                    else
                    {
                        var l_NewValue = 0f;

                        /// Random
                        if (Model.ValueType == 0)
                            l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                        /// User input
                        else if (Model.ValueType == 1)
                            l_NewValue = Model.UserValue;
                        /// Event input
                        else if (Model.ValueType == 2)
                        {
                            var l_FirstInteger  = p_Context.GetFirstValueOfType(Interfaces.IValueType.Integer);
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

    public class GamePlay_ChangeMusicVolume : Interfaces.IAction<GamePlay_ChangeMusicVolume, Models.Actions.GamePlay_ChangeMusicVolume>
    {
        public override string Description => "Choose a random volume music";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Random", "Input", "EventInput" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIComponent("UserIncrement")]
        protected IncrementSetting m_UserIncrement = null;
        [UIComponent("MinIncrement")]
        protected IncrementSetting m_MinIncrement = null;
        [UIComponent("MaxIncrement")]
        protected IncrementSetting m_MaxIncrement = null;
        [UIComponent("SendMessageToggle")]
        private ToggleSetting m_SendMessageToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                                   false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,    Model.UserValue,        false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,    Model.Min,              false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,    Model.Max,              false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                                           Model.SendChatMessage,  false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.UserValue         = m_UserIncrement.Value;
            Model.Min               = m_MinIncrement.Value;
            Model.Max               = m_MaxIncrement.Value;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            m_MinIncrement.interactable     = Model.ValueType != 1;
            m_MaxIncrement.interactable     = Model.ValueType != 1;
            m_UserIncrement.interactable    = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
            {
                var l_NewValue                  = 0f;
                var l_AudioTimeSyncController   = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();

                if (l_AudioTimeSyncController != null && l_AudioTimeSyncController)
                {
                    /// Random
                    if (Model.ValueType == 0)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueType == 1)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueType == 2)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(Interfaces.IValueType.Integer);
                        var l_EventInput    = 1f;

                        if (l_FirstInteger != default && p_Context.GetIntegerValue(l_FirstInteger.Item2, out var l_ContextVar))
                        {
                            l_EventInput = (((float)l_ContextVar.Value) / 100.0f);
                            l_EventInput = Mathf.Max(Model.Min, l_EventInput);
                            l_EventInput = Mathf.Min(Model.Max, l_EventInput);
                        }

                        l_NewValue = l_EventInput;
                    }

                    l_AudioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource").volume = l_NewValue;

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} music volume was set to {Mathf.RoundToInt(l_NewValue * 100f)}]%");
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_ChangeNoteScale : Interfaces.IAction<GamePlay_ChangeNoteScale, Models.Actions.GamePlay_ChangeNoteScale>
    {
        public override string Description => "Choose a random note scale";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Random", "Input", "EventInput", "Config" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIComponent("UserIncrement")]
        protected IncrementSetting m_UserIncrement = null;
        [UIComponent("MinIncrement")]
        protected IncrementSetting m_MinIncrement = null;
        [UIComponent("MaxIncrement")]
        protected IncrementSetting m_MaxIncrement = null;
        [UIComponent("SendMessageToggle")]
        private ToggleSetting m_SendMessageToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                                  false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.UserValue,        false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Min,              false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   Model.Max,              false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                                          Model.SendChatMessage,  false);

            OnSettingChanged(null);

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.UserValue         = m_UserIncrement.Value;
            Model.Min               = m_MinIncrement.Value;
            Model.Max               = m_MaxIncrement.Value;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            m_MinIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_MaxIncrement.interactable     = Model.ValueType == 0 || Model.ValueType == 2;
            m_UserIncrement.interactable    = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                || Model.ValueType == 3)
            {
                if (Model.ValueType == 3)
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
                    if (Model.ValueType == 0)
                        l_NewValue = UnityEngine.Random.Range(Model.Min, Model.Max);
                    /// User input
                    else if (Model.ValueType == 1)
                        l_NewValue = Model.UserValue;
                    /// Event input
                    else if (Model.ValueType == 2)
                    {
                        var l_FirstInteger  = p_Context.GetFirstValueOfType(Interfaces.IValueType.Integer);
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

    public class GamePlay_ChangeNoteColors : Interfaces.IAction<GamePlay_ChangeNoteColors, Models.Actions.GamePlay_ChangeNoteColors>
    {
        public override string Description => "Change notes colors";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Default", "Input", "EventInput" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIComponent("LeftColor")]
        protected ColorSetting m_LeftColor = null;
        [UIComponent("RightColor")]
        protected ColorSetting m_RightColor = null;
        [UIComponent("SendMessageToggle")]
        private ToggleSetting m_SendMessageToggle = null;
#pragma warning restore CS0414

        private Color? m_LeftColorCache;
        private Color? m_RightColorCache;

        public override sealed void BuildUI(Transform p_Parent)
        {
            if (Event.GetType() != typeof(Events.ChatCommand)
                && Event.GetType() != typeof(Events.ChatPointsReward))
            {
                m_TypeListList_Choices.Remove("EventInput");
            }

            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ValueType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            EnsureColorCache();

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                false);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_LeftColor,          l_Event,    m_LeftColorCache.Value,     false);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_RightColor,         l_Event,    m_RightColorCache.Value,    false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,    Model.SendChatMessage,      false);

            OnSettingChanged(null);

            if (!ModulePresence.NoteTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: NoteTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ValueType         = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            m_LeftColorCache        = m_LeftColor.CurrentColor;
            m_RightColorCache       = m_RightColor.CurrentColor;
            Model.SendChatMessage   = m_SendMessageToggle.Value;

            Model.Left  = "#" + ColorUtility.ToHtmlStringRGB(m_LeftColorCache.Value);
            Model.Right = "#" + ColorUtility.ToHtmlStringRGB(m_RightColorCache.Value);

            m_LeftColor.interactable    = Model.ValueType == 1;
            m_RightColor.interactable   = Model.ValueType == 1;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            bool l_Failed = true;
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                || Model.ValueType == 0)
            {
                if (Model.ValueType == 0)
                {
                    BeatSaberPlus_NoteTweaker.Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
                    PatchSabers(true);

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} colors are back to default!");

                    l_Failed = false;
                }
                else
                {
                    string l_LeftHex    = "#" + Model.Left;
                    string l_RightHex   = "#" + Model.Right;

                    if (Model.ValueType == 2 && (p_Context.Message != null || p_Context.PointsEvent != null)) /// Event user input
                    {
                        var l_Src = (p_Context.Message?.Message ?? p_Context.PointsEvent?.UserInput) ?? "";
                        var l_Parts = l_Src.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (l_Parts.Length >= 2
                            && ColorUtility.TryParseHtmlString(l_Parts[l_Parts.Length - 2], out var l_LeftColor)
                            && ColorUtility.TryParseHtmlString(l_Parts[l_Parts.Length - 1], out var l_RightColor))
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
                    else
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

        private void EnsureColorCache()
        {
            if (!m_LeftColorCache.HasValue)
            {
                if (ColorUtility.TryParseHtmlString(Model.Left, out var l_LeftColor))
                    m_LeftColorCache = l_LeftColor;
                else
                    m_LeftColorCache = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.colorScheme?.saberAColor ?? Color.red;
            }
            if (!m_RightColorCache.HasValue)
            {
                if (ColorUtility.TryParseHtmlString(Model.Right, out var l_RightColor))
                    m_RightColorCache = l_RightColor;
                else
                    m_RightColorCache = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.colorScheme?.saberBColor ?? Color.blue;
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

    public class GamePlay_Pause : Interfaces.IAction<GamePlay_Pause, Models.Actions.GamePlay_Pause>
    {
        public override string Description => "Trigger a pause during a song";

#pragma warning disable CS0414
        [UIComponent("HideUI")]
        private ToggleSetting m_HideUI = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_HideUI, l_Event, Model.HideUI, false);

            OnSettingChanged(null);

            if (!ModulePresence.GameTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: GameTweaker module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.HideUI = m_HideUI.Value;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
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

    public class GamePlay_Quit : Interfaces.IAction<GamePlay_Quit, Models.Action>
    {
        public override string Description => "Exit current song";

        public GamePlay_Quit() => UIPlaceHolder = "Will exit the current map";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
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

    public class GamePlay_Restart : Interfaces.IAction<GamePlay_Restart, Models.Action>
    {
        public override string Description => "Restart current song";

        public GamePlay_Restart() => UIPlaceHolder = "Will restart the map";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
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

    public class GamePlay_Resume : Interfaces.IAction<GamePlay_Resume, Models.Action>
    {
        public override string Description => "Resume current song";

        public GamePlay_Resume() => UIPlaceHolder = "Will resume the map";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
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

    public class GamePlay_SpawnBombPatterns : Interfaces.IAction<GamePlay_SpawnBombPatterns, Models.Actions.GamePlay_SpawnBombPatterns>
    {
        public override string Description => "Spawn bomb patterns in a map";

#pragma warning disable CS0414
        [UIComponent("IntervalIncrement")]
        protected IncrementSetting m_IntervalIncrement = null;
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;

        [UIComponent("L0R2")]
        protected ToggleSetting m_L0R2 = null;
        [UIComponent("L1R2")]
        protected ToggleSetting m_L1R2 = null;
        [UIComponent("L2R2")]
        protected ToggleSetting m_L2R2 = null;
        [UIComponent("L3R2")]
        protected ToggleSetting m_L3R2 = null;

        [UIComponent("L0R1")]
        protected ToggleSetting m_L0R1 = null;
        [UIComponent("L1R1")]
        protected ToggleSetting m_L1R1 = null;
        [UIComponent("L2R1")]
        protected ToggleSetting m_L2R1 = null;
        [UIComponent("L3R1")]
        protected ToggleSetting m_L3R1 = null;

        [UIComponent("L0R0")]
        protected ToggleSetting m_L0R0 = null;
        [UIComponent("L1R0")]
        protected ToggleSetting m_L1R0 = null;
        [UIComponent("L2R0")]
        protected ToggleSetting m_L2R0 = null;
        [UIComponent("L3R0")]
        protected ToggleSetting m_L3R0 = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_IntervalIncrement, l_Event, null, Model.Interval, false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.Count, false);

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L0R2, l_Event, (Model.L0 & (1 << 2)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L1R2, l_Event, (Model.L1 & (1 << 2)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L2R2, l_Event, (Model.L2 & (1 << 2)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L3R2, l_Event, (Model.L3 & (1 << 2)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L0R1, l_Event, (Model.L0 & (1 << 1)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L1R1, l_Event, (Model.L1 & (1 << 1)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L2R1, l_Event, (Model.L2 & (1 << 1)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L3R1, l_Event, (Model.L3 & (1 << 1)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L0R0, l_Event, (Model.L0 & (1 << 0)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L1R0, l_Event, (Model.L1 & (1 << 0)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L2R0, l_Event, (Model.L2 & (1 << 0)) != 0, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_L3R0, l_Event, (Model.L3 & (1 << 0)) != 0, true);

            m_L0R2.transform.localScale = 0.8f * Vector3.one;
            m_L1R2.transform.localScale = 0.8f * Vector3.one;
            m_L2R2.transform.localScale = 0.8f * Vector3.one;
            m_L3R2.transform.localScale = 0.8f * Vector3.one;
            m_L0R1.transform.localScale = 0.8f * Vector3.one;
            m_L1R1.transform.localScale = 0.8f * Vector3.one;
            m_L2R1.transform.localScale = 0.8f * Vector3.one;
            m_L3R1.transform.localScale = 0.8f * Vector3.one;
            m_L0R0.transform.localScale = 0.8f * Vector3.one;
            m_L1R0.transform.localScale = 0.8f * Vector3.one;
            m_L2R0.transform.localScale = 0.8f * Vector3.one;
            m_L3R0.transform.localScale = 0.8f * Vector3.one;

        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Interval  = m_IntervalIncrement.Value;
            Model.Count     = (int)m_CountIncrement.Value;
            Model.L0        = (byte)((m_L0R0.Value ? (1 << 0) : 0) | (m_L0R1.Value ? (1 << 1) : 0) | (m_L0R2.Value ? (1 << 2) : 0));
            Model.L1        = (byte)((m_L1R0.Value ? (1 << 0) : 0) | (m_L1R1.Value ? (1 << 1) : 0) | (m_L1R2.Value ? (1 << 2) : 0));
            Model.L2        = (byte)((m_L2R0.Value ? (1 << 0) : 0) | (m_L2R1.Value ? (1 << 1) : 0) | (m_L2R2.Value ? (1 << 2) : 0));
            Model.L3        = (byte)((m_L3R0.Value ? (1 << 0) : 0) | (m_L3R1.Value ? (1 << 1) : 0) | (m_L3R2.Value ? (1 << 2) : 0));
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                && BeatSaberPlus.SDK.Game.Logic.LevelData != null
                && !BeatSaberPlus.SDK.Game.Logic.LevelData.IsNoodle
                && !BeatSaberPlus.SDK.Game.Scoring.IsInReplay)
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

    public class GamePlay_SpawnSquatWalls : Interfaces.IAction<GamePlay_SpawnSquatWalls, Models.Actions.GamePlay_SpawnSquatWalls>
    {
        public override string Description => "Spawn fake squat walls in a map";

#pragma warning disable CS0414
        [UIComponent("IntervalIncrement")]
        protected IncrementSetting m_IntervalIncrement = null;
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_IntervalIncrement,  l_Event, null, Model.Interval,  false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement,     l_Event, null, Model.Count,     false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Interval  = m_IntervalIncrement.Value;
            Model.Count     = (int)m_CountIncrement.Value;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing
                && BeatSaberPlus.SDK.Game.Logic.LevelData != null
                && !BeatSaberPlus.SDK.Game.Logic.LevelData.IsNoodle
                && !BeatSaberPlus.SDK.Game.Scoring.IsInReplay)
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

    public class GamePlay_ToggleHUD : Interfaces.IAction<GamePlay_ToggleHUD, Models.Actions.GamePlay_ToggleHUD>
    {
        public override string Description => "Toggle HUD visibility";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList, l_Event, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ToggleType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
            {
                var l_CoreGameHUDController = Resources.FindObjectsOfTypeAll<CoreGameHUDController>().FirstOrDefault();
                if (l_CoreGameHUDController != null && l_CoreGameHUDController)
                {
                    switch(Model.ToggleType)
                    {
                        case 0:
                            l_CoreGameHUDController.gameObject.SetActive(!l_CoreGameHUDController.gameObject.activeSelf);
                            break;
                        case 1:
                            l_CoreGameHUDController.gameObject.SetActive(true);
                            break;
                        case 2:
                            l_CoreGameHUDController.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_NoteCutScoreSpawner = Resources.FindObjectsOfTypeAll<NoteCutScoreSpawner>().FirstOrDefault();
                if (l_NoteCutScoreSpawner != null && l_NoteCutScoreSpawner)
                {
                    switch (Model.ToggleType)
                    {
                        case 0:
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
                        case 1:
                            l_NoteCutScoreSpawner.gameObject.SetActive(true);
                            l_NoteCutScoreSpawner.Start();
                            break;
                        case 2:
                            l_NoteCutScoreSpawner.OnDestroy();
                            l_NoteCutScoreSpawner.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_BadNoteCutEffectSpawner = Resources.FindObjectsOfTypeAll<BadNoteCutEffectSpawner>().FirstOrDefault();
                if (l_BadNoteCutEffectSpawner != null && l_BadNoteCutEffectSpawner)
                {
                    switch (Model.ToggleType)
                    {
                        case 0:
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
                        case 1:
                            l_BadNoteCutEffectSpawner.gameObject.SetActive(true);
                            l_BadNoteCutEffectSpawner.Start();
                            break;
                        case 2:
                            l_BadNoteCutEffectSpawner.OnDestroy();
                            l_BadNoteCutEffectSpawner.gameObject.SetActive(false);
                            break;
                    }
                }

                var l_MissedNoteEffectSpawner = Resources.FindObjectsOfTypeAll<MissedNoteEffectSpawner>().FirstOrDefault();
                if (l_MissedNoteEffectSpawner != null && l_BadNoteCutEffectSpawner)
                {
                    switch (Model.ToggleType)
                    {
                        case 0:
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
                        case 1:
                            l_MissedNoteEffectSpawner.gameObject.SetActive(true);
                            l_MissedNoteEffectSpawner.Start();
                            break;
                        case 2:
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
