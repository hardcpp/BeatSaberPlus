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
        [UIComponent("SoloToggle")]
        private ToggleSetting m_SoloToggle = null;
        [UIComponent("MultiToggle")]
        private ToggleSetting m_MultiToggle = null;
        [UIComponent("ReplayToggle")]
        private ToggleSetting m_ReplayToggle = null;

        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "All", "Non noodle extensions", "Noodle extensions", "Chroma extensions", "Noodle & Chroma extensions" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.BeatmapType % m_TypeListList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SoloToggle,    l_Event, Model.Solo,    false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_MultiToggle,   l_Event, Model.Multi,   false);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ReplayToggle,  l_Event, Model.Replay,  false);
            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList,        l_Event,                false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Solo          = m_SoloToggle.Value;
            Model.Multi         = m_MultiToggle.Value;
            Model.Replay        = m_ReplayToggle.Value;
            Model.BeatmapType   = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                return false;

            var l_LevelData         = BeatSaberPlus.SDK.Game.Logic.LevelData;
            var l_ReplayCond        = Model.Replay == BeatSaberPlus.SDK.Game.Logic.IsInReplay;
            var l_BeatMapTypeCond   = Model.BeatmapType == 0; /* ALL */

            if (l_LevelData == null)
                return false;

            /// Noodle excluded
            if (Model.BeatmapType == 1 && !l_LevelData.IsNoodle)
                l_BeatMapTypeCond = true;
            /// Noodle required
            else if (Model.BeatmapType == 2 && l_LevelData.IsNoodle)
                l_BeatMapTypeCond = true;
            /// Chroma required
            else if (Model.BeatmapType == 3 && l_LevelData.IsChroma)
                l_BeatMapTypeCond = true;
            /// Noodle & Chroma required
            else if (Model.BeatmapType == 4 && l_LevelData.IsNoodle && l_LevelData.IsChroma)
                l_BeatMapTypeCond = true;

            if (Model.Solo && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Solo)
                return l_ReplayCond && l_BeatMapTypeCond;
            else if (Model.Multi && l_LevelData.Type == BeatSaberPlus.SDK.Game.LevelType.Multiplayer)
                return l_ReplayCond && l_BeatMapTypeCond;

            return false;
        }
    }
}
