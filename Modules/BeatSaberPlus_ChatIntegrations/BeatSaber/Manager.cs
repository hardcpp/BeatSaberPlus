using System.Collections;
using UnityEngine;

using CI = ChatPlexMod_ChatIntegrations.ChatIntegrations;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber
{
    /// <summary>
    /// BeatSaber chat integration manager
    /// </summary>
    internal static class Manager
    {
        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            CI.OnModuleEnable -= Register;
            CI.OnModuleEnable += Register;

            CI.OnModuleDisable -= UnRegister;
            CI.OnModuleDisable += UnRegister;

            CI.RegisterEventType("LevelEnded",   () => new Events.LevelEnded());
            CI.RegisterEventType("LevelPaused",  () => new Events.LevelPaused());
            CI.RegisterEventType("LevelResumed", () => new Events.LevelResumed());
            CI.RegisterEventType("LevelStarted", () => new Events.LevelStarted());

            ////////////////////////////////////////////////////////////////////////////

            CI.RegisterConditionType("ChatRequest_QueueDuration", () => new Conditions.ChatRequest_QueueDuration());
            CI.RegisterConditionType("ChatRequest_QueueSize",     () => new Conditions.ChatRequest_QueueSize());
            CI.RegisterConditionType("ChatRequest_QueueStatus",   () => new Conditions.ChatRequest_QueueStatus());

            CI.RegisterConditionType("GamePlay_InMenu",       () => new Conditions.GamePlay_InMenu());
            CI.RegisterConditionType("GamePlay_PlayingMap",   () => new Conditions.GamePlay_PlayingMap());

            ////////////////////////////////////////////////////////////////////////////

            CI.RegisterActionType("Camera2_SwitchToDefaultScene",     () => new Actions.Camera2_SwitchToDefaultScene());
            CI.RegisterActionType("Camera2_SwitchToScene",            () => new Actions.Camera2_SwitchToScene());
            CI.RegisterActionType("Camera2_ToggleCamera",             () => new Actions.Camera2_ToggleCamera());

            CI.RegisterActionType("GamePlay_ChangeBombColor",         () => new Actions.GamePlay_ChangeBombColor());
            CI.RegisterActionType("GamePlay_ChangeBombScale",         () => new Actions.GamePlay_ChangeBombScale());
            CI.RegisterActionType("GamePlay_ChangeDebris",            () => new Actions.GamePlay_ChangeDebris());
            CI.RegisterActionType("GamePlay_ChangeLightIntensity",    () => new Actions.GamePlay_ChangeLightIntensity());
            CI.RegisterActionType("GamePlay_ChangeMusicVolume",       () => new Actions.GamePlay_ChangeMusicVolume());
            CI.RegisterActionType("GamePlay_ChangeNoteColors",        () => new Actions.GamePlay_ChangeNoteColors());
            CI.RegisterActionType("GamePlay_ChangeNoteScale",         () => new Actions.GamePlay_ChangeNoteScale());
            CI.RegisterActionType("GamePlay_Pause",                   () => new Actions.GamePlay_Pause());
            CI.RegisterActionType("GamePlay_Quit",                    () => new Actions.GamePlay_Quit());
            CI.RegisterActionType("GamePlay_Restart",                 () => new Actions.GamePlay_Restart());
            CI.RegisterActionType("GamePlay_Resume",                  () => new Actions.GamePlay_Resume());
            CI.RegisterActionType("GamePlay_SpawnBombPatterns",       () => new Actions.GamePlay_SpawnBombPatterns());
            CI.RegisterActionType("GamePlay_SpawnSquatWalls",         () => new Actions.GamePlay_SpawnSquatWalls());
            CI.RegisterActionType("GamePlay_ToggleHUD",               () => new Actions.GamePlay_ToggleHUD());

            CI.RegisterActionType("NoteTweaker_SwitchProfile",        () => new Actions.NoteTweaker_SwitchProfile());

            CI.RegisterActionType("SongChartVisualizer_ToggleVisibility", () => new Actions.SongChartVisualizer_ToggleVisibility());

            ////////////////////////////////////////////////////////////////////////////

            CI.RegisterTemplate("ChatPointReward : 5 Squats", () =>
            {
                var l_Event = new ChatPlexMod_ChatIntegrations.Events.ChatPointsReward();
                l_Event.Model.Cooldown  = 60;
                l_Event.Model.Cost      = 100;
                l_Event.Model.Name      = "5 Squats (Template)";
                l_Event.Model.Title     = "5 Squats (Template)";

                l_Event.AddCondition(new Conditions.GamePlay_PlayingMap() { Event = l_Event, IsEnabled = true });

                var l_SquatAction = new Actions.GamePlay_SpawnSquatWalls() { Event = l_Event, IsEnabled = true };
                l_SquatAction.Model.Count       = 5;
                l_SquatAction.Model.Interval    = 5;
                l_Event.AddOnSuccessAction(l_SquatAction);

                var l_MessageAction = new ChatPlexMod_ChatIntegrations.Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "5 squats from $SenderName, let's gooo!";
                l_Event.AddOnSuccessAction(l_MessageAction);

                return l_Event;
            });
            CI.RegisterTemplate("ChatCommand : 250% lights for 10 seconds with cooldown", () =>
            {
                var l_Event = new ChatPlexMod_ChatIntegrations.Events.ChatCommand();
                l_Event.Model.Name      = "10 seconds of 250% lights with cooldown (Template)";
                l_Event.Model.Command   = "!lights";

                l_Event.AddCondition(new Conditions.GamePlay_PlayingMap() { Event = l_Event, IsEnabled = true });

                var l_CooldownCondition = new ChatPlexMod_ChatIntegrations.Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = true;
                l_CooldownCondition.Model.NotifyUser    = true;
                l_CooldownCondition.Model.CooldownTime  = 60;
                l_Event.Conditions.Add(l_CooldownCondition);

                l_CooldownCondition = new ChatPlexMod_ChatIntegrations.Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = false;
                l_CooldownCondition.Model.NotifyUser    = true;
                l_CooldownCondition.Model.CooldownTime  = 20;
                l_Event.Conditions.Add(l_CooldownCondition);

                var l_MessageAction = new ChatPlexMod_ChatIntegrations.Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Lights go brrrrr";
                l_Event.AddOnSuccessAction(l_MessageAction);

                var l_LightAction = new Actions.GamePlay_ChangeLightIntensity() { Event = l_Event, IsEnabled = true };
                l_LightAction.Model.UserValue       = 2.5f;
                l_LightAction.Model.SendChatMessage = false;
                l_LightAction.Model.ValueSource     = Enums.ValueSource.E.User;
                l_Event.AddOnSuccessAction(l_LightAction);

                var l_DelayAction = new ChatPlexMod_ChatIntegrations.Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                l_DelayAction.Model.Delay                       = 10;
                l_DelayAction.Model.PreventNextActionFailure    = true;
                l_Event.AddOnSuccessAction(l_DelayAction);

                l_LightAction = new Actions.GamePlay_ChangeLightIntensity() { Event = l_Event, IsEnabled = true };
                l_LightAction.Model.ValueSource     = Enums.ValueSource.E.Config;
                l_LightAction.Model.SendChatMessage = false;
                l_Event.AddOnSuccessAction(l_LightAction);

                return l_Event;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On enable
        /// </summary>
        private static void Register()
        {
            CP_SDK_BS.Game.Logic.OnLevelStarted -= Game_OnLevelStarted;
            CP_SDK_BS.Game.Logic.OnLevelStarted += Game_OnLevelStarted;

            CP_SDK_BS.Game.Logic.OnLevelEnded   -= Game_OnLevelEnded;
            CP_SDK_BS.Game.Logic.OnLevelEnded   += Game_OnLevelEnded;
        }
        /// <summary>
        /// On disable
        /// </summary>
        private static void UnRegister()
        {
            CP_SDK_BS.Game.Logic.OnLevelEnded   -= Game_OnLevelEnded;
            CP_SDK_BS.Game.Logic.OnLevelStarted -= Game_OnLevelStarted;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On level started
        /// </summary>
        /// <param name="p_Data">Level data</param>
        private static void Game_OnLevelStarted(CP_SDK_BS.Game.LevelData p_Data)
        {
            var l_Instance = CI.Instance;
            if (l_Instance == null || !l_Instance.IsEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => {
                l_Instance.HandleEvents(new ChatPlexMod_ChatIntegrations.Models.EventContext()
                {
                    Type        = ChatPlexMod_ChatIntegrations.Interfaces.ETriggerType.LevelStarted,
                    CustomData  = p_Data
                });
            });
            CP_SDK.Unity.MTCoroutineStarter.Start(Game_FindPauseManager(p_Data));
        }
        private static IEnumerator Game_FindPauseManager(CP_SDK_BS.Game.LevelData p_Data)
        {
            if (p_Data.Type == CP_SDK_BS.Game.LevelType.Multiplayer)
                yield break;

            var l_PauseController   = null as PauseController;
            yield return new WaitUntil(() => (l_PauseController = GameObject.FindObjectOfType<PauseController>()));

            var l_Instance = CI.Instance;
            if (l_Instance == null || !l_Instance.IsEnabled)
                yield break;

            if (l_PauseController)
            {
                l_PauseController.didPauseEvent += () =>
                {
                    CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => {
                        l_Instance?.HandleEvents(new ChatPlexMod_ChatIntegrations.Models.EventContext()
                        {
                            Type        = ChatPlexMod_ChatIntegrations.Interfaces.ETriggerType.LevelPaused,
                            CustomData  = p_Data
                        });
                    });
                };
                l_PauseController.didResumeEvent += () =>
                {
                    CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => {
                        l_Instance?.HandleEvents(new ChatPlexMod_ChatIntegrations.Models.EventContext()
                        {
                            Type        = ChatPlexMod_ChatIntegrations.Interfaces.ETriggerType.LevelResumed,
                            CustomData  = p_Data
                        });
                    });
                };
            }
        }
        /// <summary>
        /// On level ended
        /// </summary>
        /// <param name="p_Data">Completion data</param>
        private static void Game_OnLevelEnded(CP_SDK_BS.Game.LevelCompletionData p_Data)
        {
            var l_Instance = CI.Instance;
            if (l_Instance == null || !l_Instance.IsEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => {
                l_Instance.HandleEvents(new ChatPlexMod_ChatIntegrations.Models.EventContext()
                {
                    Type        = ChatPlexMod_ChatIntegrations.Interfaces.ETriggerType.LevelEnded,
                    CustomData  = p_Data
                });
            });
        }
    }
}
