using CP_SDK.XUI;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Conditions
{
    public class GamePlay_InMenu
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<GamePlay_InMenu, ChatPlexMod_ChatIntegrations.Models.Condition>
    {
        public override string Description      => "Are we currently in the menu?";
        public override string UIPlaceHolder    => "<b><i>Ensure that you are currently in the menu</i></b>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            return BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.ESceneType.Menu;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_LevelEndType
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<GamePlay_LevelEndType, Models.Conditions.GamePlay_LevelEndType>
    {
        private XUIToggle m_Quit    = null;
        private XUIToggle m_Restart = null;
        private XUIToggle m_Pass    = null;
        private XUIToggle m_Fail    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Kind of level end!";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Valid types:"),

                Templates.SettingsHGroup("On quit",
                    XUIToggle.Make()
                        .SetValue(Model.Quit).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Quit)
                ),

                Templates.SettingsHGroup("On restart",
                    XUIToggle.Make()
                        .SetValue(Model.Restart).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Restart)
                ),

                Templates.SettingsHGroup("On pass",
                    XUIToggle.Make()
                        .SetValue(Model.Pass).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Pass)
                ),

                Templates.SettingsHGroup("On fail",
                    XUIToggle.Make()
                        .SetValue(Model.Fail).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Fail)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Quit      = m_Quit.Element.GetValue();
            Model.Restart   = m_Restart.Element.GetValue();
            Model.Pass      = m_Pass.Element.GetValue();
            Model.Fail      = m_Fail.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            var l_LevelCompletionDataResults = BeatSaberPlus.SDK.Game.Logic.LevelCompletionData.Results;
            var l_LevelEndAction             = l_LevelCompletionDataResults.levelEndAction;
            var l_LevelEndStateType          = l_LevelCompletionDataResults.levelEndStateType;

            var l_IsQuit       = l_LevelEndAction      == LevelCompletionResults.LevelEndAction.Quit;
            var l_IsRestart    = l_LevelEndAction      == LevelCompletionResults.LevelEndAction.Restart;
            var l_IsPass       = l_LevelEndStateType   == LevelCompletionResults.LevelEndStateType.Cleared;
            var l_IsFail       = l_LevelEndStateType   == LevelCompletionResults.LevelEndStateType.Failed;

            return (Model.Quit && l_IsQuit) || (Model.Restart && l_IsRestart) || (Model.Pass && l_IsPass) || (Model.Fail && l_IsFail);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_PlayingMap
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<GamePlay_PlayingMap, Models.Conditions.GamePlay_PlayingMap>
    {
        private XUIDropdown m_LevelType     = null;
        private XUIDropdown m_BeatmapType   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Are we currently playing a map?";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Level type"),
                XUIDropdown.Make()
                    .SetOptions(Enums.LevelType.S).SetValue(Enums.LevelType.ToStr(Model.LevelType))
                    .OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_LevelType),

                XUIText.Make("Beatmap Mod type"),
                XUIDropdown.Make()
                    .SetOptions(Enums.BeatmapModType.S).SetValue(Enums.BeatmapModType.ToStr(Model.BeatmapModType))
                    .OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_BeatmapType),
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.LevelType         = Enums.LevelType.ToEnum(m_LevelType.Element.GetValue());
            Model.BeatmapModType    = Enums.BeatmapModType.ToEnum(m_BeatmapType.Element.GetValue());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                return false;

            var l_LevelData = BeatSaberPlus.SDK.Game.Logic.LevelData;

            if (l_LevelData == null)
                return false;

            var l_IsInReplay        = BeatSaberPlus.SDK.Game.Scoring.IsInReplay;
            var l_LevelTypeCond     = false;
            var l_BeatMapTypeCond   = false;

            switch (Model.LevelType)
            {
                case Enums.LevelType.E.Solo:
                    l_LevelTypeCond = !l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo;
                    break;
                case Enums.LevelType.E.Multiplayer:
                    l_LevelTypeCond = !l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer;
                    break;
                case Enums.LevelType.E.Replay:
                    l_LevelTypeCond = l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo;
                    break;
                case Enums.LevelType.E.SoloAndMultiplayer:
                    l_LevelTypeCond = !l_IsInReplay && (l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo || l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer);
                    break;
                case Enums.LevelType.E.Any:
                default:
                    l_LevelTypeCond = true;
                    break;
            }

            switch (Model.BeatmapModType)
            {
                case Enums.BeatmapModType.E.NonNoodle:
                    l_BeatMapTypeCond = !l_LevelData.IsNoodle;
                    break;
                case Enums.BeatmapModType.E.Noodle:
                    l_BeatMapTypeCond = l_LevelData.IsNoodle;
                    break;
                case Enums.BeatmapModType.E.Chroma:
                    l_BeatMapTypeCond = l_LevelData.IsChroma;
                    break;
                case Enums.BeatmapModType.E.NoodleOrChroma:
                    l_BeatMapTypeCond = l_LevelData.IsNoodle || l_LevelData.IsChroma;
                    break;
                case Enums.BeatmapModType.E.All:
                default:
                    l_BeatMapTypeCond = true;
                    break;
            }

            return l_LevelTypeCond && l_BeatMapTypeCond;
        }
    }
}
