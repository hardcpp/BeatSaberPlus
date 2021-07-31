﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberPlus.Modules.ChatIntegrations.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Actions
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
                new GamePlay_Pause(),
                new GamePlay_Quit(),
                new GamePlay_ChangeDebris(),
                new GamePlay_ChangeLightIntensity(),
                new GamePlay_ChangeMusicVolume(),
                new GamePlay_Restart(),
                new GamePlay_SpawnBombPatterns(),
                new GamePlay_SpawnSquatWalls(),
                new GamePlay_ToggleHUD(),
                //new GamePlay_ToggleLights()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeDebris : Interfaces.IAction<GamePlay_ChangeDebris, Models.Actions.GamePlay_ChangeDebris>
    {
        public override string Description => "Turn on or off debris";

#pragma warning disable CS0414
        [UIComponent("DebrisToggle")]
        private ToggleSetting m_DebrisToggle = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ToggleSetting.Setup(m_DebrisToggle, l_Event, Model.Debris, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Debris = m_DebrisToggle.Value;
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
                GameTweaker.Patches.PNoteDebrisSpawner.SetTemp(!Model.Debris);
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

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                    false);
            SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Model.UserValue,        false);
            SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Model.Min,              false);
            SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Model.Max,              false);
            SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                            Model.SendChatMessage,  false);

            OnSettingChanged(null);
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
            bool l_Failed = true;
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                var l_Level     = SDK.Game.Logic.LevelData?.Data?.difficultyBeatmap?.difficulty;
                var l_Effects   = l_Level == BeatmapDifficulty.ExpertPlus
                    ? SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterExpertPlusPreset
                    : SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterDefaultPreset;

                if (l_Effects != EnvironmentEffectsFilterPreset.NoEffects)
                {
                    if (Model.ValueType == 3)
                    {
                        GameTweaker.Patches.Lights.PLightSwitchEventEffect.SetFromConfig();

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

                        GameTweaker.Patches.Lights.PLightSwitchEventEffect.SetTempLightIntensity(l_NewValue);

                        if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} lights was set to {Mathf.RoundToInt(l_NewValue * 100f)}]%");
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

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ListSetting.Setup(m_TypeList,            l_Event,                                                                     false);
            SDK.UI.IncrementSetting.Setup(m_UserIncrement,  l_Event, SDK.UI.BSMLSettingFormartter.Percentage,    Model.UserValue,        false);
            SDK.UI.IncrementSetting.Setup(m_MinIncrement,   l_Event, SDK.UI.BSMLSettingFormartter.Percentage,    Model.Min,              false);
            SDK.UI.IncrementSetting.Setup(m_MaxIncrement,   l_Event, SDK.UI.BSMLSettingFormartter.Percentage,    Model.Max,              false);
            SDK.UI.ToggleSetting.Setup(m_SendMessageToggle, l_Event,                                             Model.SendChatMessage,  false);

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
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
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

                    l_AudioTimeSyncController.audioSource.volume = l_NewValue;

                    if (Model.SendChatMessage && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} music volume was set to {Mathf.RoundToInt(l_NewValue * 100f)}]%");
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_Pause : Interfaces.IAction<GamePlay_Pause, Models.Action>
    {
        public override string Description => "Trigger a pause during a song";

        public GamePlay_Pause() => UIPlaceHolder = "Will pause the current map";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                var l_PauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();
                if (l_PauseController != null && l_PauseController)
                    l_PauseController.Pause();
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_Quit : Interfaces.IAction<GamePlay_Quit, Models.Action>
    {
        public override string Description => "Exit current song";

        public GamePlay_Quit() => UIPlaceHolder = "Will exit the current map";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                if (l_PauseMenuManager != null && l_PauseMenuManager)
                    l_PauseMenuManager.MenuButtonPressed();
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
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                var l_PauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                if (l_PauseMenuManager)
                    l_PauseMenuManager.RestartButtonPressed();
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
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.IncrementSetting.Setup(m_IntervalIncrement, l_Event, null, Model.Interval, false);
            SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.Count, false);

            SDK.UI.ToggleSetting.Setup(m_L0R2, l_Event, (Model.L0 & (1 << 0)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L1R2, l_Event, (Model.L1 & (1 << 0)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L2R2, l_Event, (Model.L2 & (1 << 0)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L3R2, l_Event, (Model.L3 & (1 << 0)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L0R1, l_Event, (Model.L0 & (1 << 1)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L1R1, l_Event, (Model.L1 & (1 << 1)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L2R1, l_Event, (Model.L2 & (1 << 1)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L3R1, l_Event, (Model.L3 & (1 << 1)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L0R0, l_Event, (Model.L0 & (1 << 2)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L1R0, l_Event, (Model.L1 & (1 << 2)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L2R0, l_Event, (Model.L2 & (1 << 2)) != 0, true);
            SDK.UI.ToggleSetting.Setup(m_L3R0, l_Event, (Model.L3 & (1 << 2)) != 0, true);

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
            Model.L0        = (byte)((m_L0R0.Value ? 1 : 0) | (m_L0R1.Value ? (1 << 1) : 0) | (m_L0R2.Value ? (1 << 2) : 0));
            Model.L1        = (byte)((m_L1R0.Value ? 1 : 0) | (m_L1R1.Value ? (1 << 1) : 0) | (m_L1R2.Value ? (1 << 2) : 0));
            Model.L2        = (byte)((m_L2R0.Value ? 1 : 0) | (m_L2R1.Value ? (1 << 1) : 0) | (m_L2R2.Value ? (1 << 2) : 0));
            Model.L3        = (byte)((m_L3R0.Value ? 1 : 0) | (m_L3R1.Value ? (1 << 1) : 0) | (m_L3R2.Value ? (1 << 2) : 0));
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing
                && SDK.Game.Logic.LevelData != null
                && !SDK.Game.Logic.LevelData.IsNoodle
                && !SDK.Game.Logic.IsInReplay)
            {
                var l_AudioTimeSyncController = UnityEngine.Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
                var l_BeatmapObjectSpawnController = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();

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
                        foreach (var l_Current in l_SpawnList)
                        {
                            l_BeatmapObjectSpawnController.SpawnBombNote(new NoteData(l_AudioTimeSyncController.songTime + l_Time,
                                l_Current.Item1,
                                (NoteLineLayer)l_Current.Item2,
                                (NoteLineLayer)l_Current.Item2,
                                ColorType.None,
                                NoteCutDirection.None,
                                0f,
                                0f,
                                0,
                                0,
                                1f));
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
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.IncrementSetting.Setup(m_IntervalIncrement,  l_Event, null, Model.Interval,  false);
            SDK.UI.IncrementSetting.Setup(m_CountIncrement,     l_Event, null, Model.Count,     false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Interval  = m_IntervalIncrement.Value;
            Model.Count     = (int)m_CountIncrement.Value;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing
                && SDK.Game.Logic.LevelData != null
                && !SDK.Game.Logic.LevelData.IsNoodle
                && !SDK.Game.Logic.IsInReplay)
            {
                var l_AudioTimeSyncController       = UnityEngine.Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
                var l_BeatmapObjectSpawnController  = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();

                if (   l_AudioTimeSyncController      != null && l_AudioTimeSyncController
                    && l_BeatmapObjectSpawnController != null && l_BeatmapObjectSpawnController)
                {
                    float l_Time = 2f;
                    for (int l_I = 0; l_I < Model.Count; ++l_I)
                    {
                        l_BeatmapObjectSpawnController.SpawnObstacle(new ObstacleData(l_AudioTimeSyncController.songTime + l_Time, 4, ObstacleType.Top, 0.3f, -4));
                        l_Time += Model.Interval;
                    }
                }
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_ToggleHUD : Interfaces.IAction<GamePlay_ToggleHUD, Models.Action>
    {
        public override string Description => "Toggle HUD visibility";

        public GamePlay_ToggleHUD() => UIPlaceHolder = "Will show or hide HUD";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                var l_CoreGameHUDController = Resources.FindObjectsOfTypeAll<CoreGameHUDController>().FirstOrDefault();
                if (l_CoreGameHUDController != null && l_CoreGameHUDController)
                    l_CoreGameHUDController.gameObject.SetActive(!l_CoreGameHUDController.gameObject.activeSelf);
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class GamePlay_ToggleLights : Interfaces.IAction<GamePlay_ToggleLights, Models.Actions.GamePlay_ToggleLights>
    {
        public override string Description => "Toggle lights";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "Enable", "Disable" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ChangeType % m_TypeListList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ListSetting.Setup(m_TypeList, l_Event, false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ChangeType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing && Config.GameTweaker.Enabled)
            {
                if (Model.ChangeType == 0)
                    GameTweaker.Patches.Lights.PLightSwitchEventEffect.SetTempFauxStaticLight(!GameTweaker.Patches.Lights.PLightSwitchEventEffect.FauxStaticLight);
                else if (Model.ChangeType == 1)
                    GameTweaker.Patches.Lights.PLightSwitchEventEffect.SetTempFauxStaticLight(true);
                else
                    GameTweaker.Patches.Lights.PLightSwitchEventEffect.SetTempFauxStaticLight(false);
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }
}