using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberPlus_ChatIntegrations.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Conditions
{
    public class GamePlay_InMenu : Interfaces.ICondition<GamePlay_InMenu, Models.Condition>
    {
        public override string Description => "Are we currently in the menu?";

        public GamePlay_InMenu() => UIPlaceHolder = "<b><i>Ensure that you are currently in the menu</i></b>";

        public override bool Eval(Models.EventContext p_Context)
        {
            return BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu;
        }
    }

    public class GamePlay_LevelEndType : Interfaces.ICondition<GamePlay_LevelEndType, Models.Conditions.GamePlay_LevelEndType>
    {
        public override string Description => "Kind of level end!";

#pragma warning disable CS0414
        [UIComponent("QuitToggle")]
        private ToggleSetting m_QuitToggle = null;
        [UIComponent("RestartToggle")]
        private ToggleSetting m_RestartToggle = null;
        [UIComponent("PassToggle")]
        private ToggleSetting m_PassToggle = null;
        [UIComponent("FailToggle")]
        private ToggleSetting m_FailToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_QuitToggle,    l_Event, Model.Quit,    false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RestartToggle, l_Event, Model.Restart, false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PassToggle,    l_Event, Model.Pass,    false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_FailToggle,    l_Event, Model.Fail,    false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Quit      = m_QuitToggle.Value;
            Model.Restart   = m_RestartToggle.Value;
            Model.Pass      = m_PassToggle.Value;
            Model.Fail      = m_FailToggle.Value;
        }

        public override bool Eval(EventContext p_Context)
        {
            bool l_IsQuit       = BeatSaberPlus.SDK.Game.Logic.LevelCompletionData.Results.levelEndAction     == LevelCompletionResults.LevelEndAction.Quit;
            bool l_IsRestart    = BeatSaberPlus.SDK.Game.Logic.LevelCompletionData.Results.levelEndAction     == LevelCompletionResults.LevelEndAction.Restart;
            bool l_IsPass       = BeatSaberPlus.SDK.Game.Logic.LevelCompletionData.Results.levelEndStateType  == LevelCompletionResults.LevelEndStateType.Cleared;
            bool l_IsFail       = BeatSaberPlus.SDK.Game.Logic.LevelCompletionData.Results.levelEndStateType  == LevelCompletionResults.LevelEndStateType.Failed;

            return (Model.Quit && l_IsQuit) || (Model.Restart && l_IsRestart) || (Model.Pass && l_IsPass) || (Model.Fail && l_IsFail);
        }
    }

    public class GamePlay_PlayingMap : Interfaces.ICondition<GamePlay_PlayingMap, Models.Conditions.GamePlay_PlayingMap>
    {
        public override string Description => "Are we currently playing a map?";

#pragma warning disable CS0414
        [UIComponent("LevelTypeList")]
        private ListSetting m_LevelTypeList = null;
        [UIValue("LevelTypeList_Choices")]
        private List<object> m_LevelTypeList_Choices = new List<object>() { };
        [UIValue("LevelTypeList_Value")]
        private string m_LevelTypeList_Value;

        [UIComponent("BeatmapList")]
        private ListSetting m_BeatmapTypeList = null;
        [UIValue("BeatmapList_Choices")]
        private List<object> m_BeatmapTypeList_Choices = new List<object>() {  };
        [UIValue("BeatmapList_Value")]
        private string m_BeatmapTypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            if (m_LevelTypeList_Choices.Count == 0)
            {
                foreach (var l_Current in System.Enum.GetValues(typeof(Models.Conditions.GamePlay_PlayingMap.ELevelType)))
                    m_LevelTypeList_Choices.Add((object)l_Current.ToString());
            }
            if (m_BeatmapTypeList_Choices.Count == 0)
            {
                foreach (var l_Current in System.Enum.GetValues(typeof(Models.Conditions.GamePlay_PlayingMap.EBeatmapModType)))
                    m_BeatmapTypeList_Choices.Add((object)l_Current.ToString());
            }

            m_LevelTypeList_Value   = (string)m_LevelTypeList_Choices.ElementAt((int)Model.LevelType % m_LevelTypeList_Choices.Count);
            m_BeatmapTypeList_Value = (string)m_BeatmapTypeList_Choices.ElementAt((int)Model.BeatmapModType % m_BeatmapTypeList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_LevelTypeList,     l_Event,    false);
            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_BeatmapTypeList,   l_Event,    false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.LevelType         = (Models.Conditions.GamePlay_PlayingMap.ELevelType)m_LevelTypeList_Choices.IndexOf(m_LevelTypeList.Value);
            Model.BeatmapModType    = (Models.Conditions.GamePlay_PlayingMap.EBeatmapModType)m_BeatmapTypeList_Choices.IndexOf(m_BeatmapTypeList.Value);
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                return false;

            var l_LevelData = BeatSaberPlus.SDK.Game.Logic.LevelData;

            if (l_LevelData == null)
                return false;

            var l_IsInReplay        = BeatSaberPlus.SDK.Game.Logic.IsInReplay;
            var l_LevelTypeCond     = false;
            var l_BeatMapTypeCond   = false;

            switch (Model.LevelType)
            {
                case Models.Conditions.GamePlay_PlayingMap.ELevelType.Solo:
                    l_LevelTypeCond = !l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo;
                    break;
                case Models.Conditions.GamePlay_PlayingMap.ELevelType.Multiplayer:
                    l_LevelTypeCond = !l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.ELevelType.Replay:
                    l_LevelTypeCond = l_IsInReplay && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.ELevelType.SoloAndMultiplayer:
                    l_LevelTypeCond = !l_IsInReplay && (l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo || l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer);
                    break;

                case Models.Conditions.GamePlay_PlayingMap.ELevelType.Any:
                default:
                    l_LevelTypeCond = true;
                    break;
            }

            switch (Model.BeatmapModType)
            {
                case Models.Conditions.GamePlay_PlayingMap.EBeatmapModType.NonNoodle:
                    l_BeatMapTypeCond = !l_LevelData.IsNoodle;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.EBeatmapModType.Noodle:
                    l_BeatMapTypeCond = l_LevelData.IsNoodle;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.EBeatmapModType.Chroma:
                    l_BeatMapTypeCond = l_LevelData.IsChroma;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.EBeatmapModType.NoodleOrChroma:
                    l_BeatMapTypeCond = l_LevelData.IsNoodle || l_LevelData.IsChroma;
                    break;

                case Models.Conditions.GamePlay_PlayingMap.EBeatmapModType.All:
                default:
                    l_BeatMapTypeCond = true;
                    break;
            }

            return l_LevelTypeCond && l_BeatMapTypeCond;
        }
    }
}
