using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Level selection filter
    /// </summary>
    public class LevelSelection
    {
        /// <summary>
        /// Pending filter song
        /// </summary>
        static private CustomPreviewBeatmapLevel m_PendingFilterSong = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Filter to specific song
        /// </summary>
        /// <param name="p_SongToFilter">Song to filter</param>
        /// <returns></returns>
        public static bool FilterToSpecificSong(CustomPreviewBeatmapLevel p_SongToFilter)
        {
            m_PendingFilterSong = p_SongToFilter;

            try
            {
                var l_LevelFilteringNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().FirstOrDefault();

                if (l_LevelFilteringNavigationController != null)
                {
                    if (l_LevelFilteringNavigationController.gameObject.activeInHierarchy)
                        LevelSelectionNavigationController_didActivateEvent(false, false, false);
                    else
                        l_LevelFilteringNavigationController.didActivateEvent += LevelSelectionNavigationController_didActivateEvent;
                    return true;
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance?.Critical(p_Exception);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change current song view to all songs view
        /// </summary>
        /// <param name="p_FirstActivation"></param>
        /// <param name="p_AddedToHierarchy"></param>
        /// <param name="p_ScreenSystemEnabling"></param>
        private static void LevelSelectionNavigationController_didActivateEvent(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            var l_LevelSelectionNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().FirstOrDefault();
            if (l_LevelSelectionNavigationController == null)
                return;

            l_LevelSelectionNavigationController.didActivateEvent -= LevelSelectionNavigationController_didActivateEvent;

            SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_SelectLevelCategory(l_LevelSelectionNavigationController));
        }
        /// <summary>
        /// Level selection, select level category
        /// </summary>
        /// <param name="p_LevelSelectionNavigationController">LevelSelectionNavigationController instance</param>
        /// <returns></returns>
        private static IEnumerator LevelSelection_SelectLevelCategory(LevelSelectionNavigationController p_LevelSelectionNavigationController)
        {
            yield return new WaitUntil(() => !p_LevelSelectionNavigationController || !p_LevelSelectionNavigationController.isInTransition);

            if (Logic.ActiveScene != Logic.SceneType.Menu)
                yield break;

            if (!p_LevelSelectionNavigationController || !p_LevelSelectionNavigationController.isInViewControllerHierarchy || !p_LevelSelectionNavigationController.isActiveAndEnabled)
                yield break;

            var l_LevelFilteringNavigationController = p_LevelSelectionNavigationController.GetField<LevelFilteringNavigationController, LevelSelectionNavigationController>("_levelFilteringNavigationController");
            if (!l_LevelFilteringNavigationController)
                yield break;

            if (l_LevelFilteringNavigationController.selectedLevelCategory != SelectLevelCategoryViewController.LevelCategory.All)
            {
                var l_Selector = l_LevelFilteringNavigationController.GetField<SelectLevelCategoryViewController, LevelFilteringNavigationController>("_selectLevelCategoryViewController");
                if (l_Selector != null && l_Selector)
                {
                    var l_SegmentControl    = l_Selector.GetField<HMUI.IconSegmentedControl, SelectLevelCategoryViewController>("_levelFilterCategoryIconSegmentedControl");
                    var l_Tags              = l_Selector.GetField<SelectLevelCategoryViewController.LevelCategoryInfo[], SelectLevelCategoryViewController>("_levelCategoryInfos");
                    var l_IndexToSelect     = l_Tags.Select((x => x.levelCategory)).ToList().IndexOf(SelectLevelCategoryViewController.LevelCategory.All);

                    /// Multiplayer : missing extension
                    if (l_IndexToSelect == -1)
                        yield break;

                    l_SegmentControl.SelectCellWithNumber(l_IndexToSelect);
                    l_Selector.LevelFilterCategoryIconSegmentedControlDidSelectCell(l_SegmentControl, l_IndexToSelect);

                    SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_FilterLevel(
                        l_LevelFilteringNavigationController.GetField<LevelSearchViewController, LevelFilteringNavigationController>("_levelSearchViewController"),
                        true
                    ));
                }
            }
            else
            {
                SharedCoroutineStarter.instance.StartCoroutine(LevelSelection_FilterLevel(
                    l_LevelFilteringNavigationController.GetField<LevelSearchViewController, LevelFilteringNavigationController>("_levelSearchViewController"),
                    false
                ));
            }
        }
        /// <summary>
        /// Level selection, filter
        /// </summary>
        /// <param name="p_LevelSearchViewController">LevelSearchViewController instance</param>
        /// <param name="p_Wait">Should wait for any transition</param>
        /// <returns></returns>
        private static IEnumerator LevelSelection_FilterLevel(LevelSearchViewController p_LevelSearchViewController, bool p_Wait)
        {
            if (Logic.ActiveScene != Logic.SceneType.Menu)
                yield break;

            if (p_LevelSearchViewController == null || !p_LevelSearchViewController || m_PendingFilterSong == null)
                yield break;

            if (p_Wait)
            {
                yield return new WaitUntil(() => !p_LevelSearchViewController || !p_LevelSearchViewController.isInTransition);

                if (!p_LevelSearchViewController || !p_LevelSearchViewController.isInViewControllerHierarchy || !p_LevelSearchViewController.isActiveAndEnabled)
                    yield break;

                if (Logic.ActiveScene != Logic.SceneType.Menu)
                    yield break;
            }

            try
            {
                p_LevelSearchViewController.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;
                p_LevelSearchViewController.ResetCurrentFilterParams();
                var l_InputFieldView = p_LevelSearchViewController.GetField<HMUI.InputFieldView, LevelSearchViewController>("_searchTextInputFieldView");
                if (l_InputFieldView != null && l_InputFieldView)
                {
                    //l_InputFieldView.SetText(m_PendingFilterSong.songName);
                    //l_InputFieldView.UpdateClearButton();
                    //l_InputFieldView.UpdatePlaceholder();
                }

                p_LevelSearchViewController.UpdateSearchLevelFilterParams(LevelFilterParams.ByBeatmapLevelIds(new HashSet<string>() { m_PendingFilterSong.levelID }));
                p_LevelSearchViewController.didStartLoadingEvent += LevelSearchViewController_didStartLoadingEvent;
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[SDK.Game][LevelSelection.LevelSelection_FilterLevel] coroutine failed : ");
                Logger.Instance.Error(p_Exception);

                LevelSearchViewController_didStartLoadingEvent(p_LevelSearchViewController);
            }
        }
        /// <summary>
        /// LevelSearchViewController didStartLoadingEvent
        /// </summary>
        /// <param name="p_LevelSearchViewController">LevelSearchViewController instance</param>
        private static void LevelSearchViewController_didStartLoadingEvent(LevelSearchViewController p_LevelSearchViewController)
        {
            if (!p_LevelSearchViewController)
                return;

            p_LevelSearchViewController.didStartLoadingEvent -= LevelSearchViewController_didStartLoadingEvent;

            try
            {
                var l_Filter = p_LevelSearchViewController.GetField<LevelFilterParams, LevelSearchViewController>("_currentFilterParams");
                if (l_Filter != null && l_Filter.filterByLevelIds)
                {
                    p_LevelSearchViewController.ResetCurrentFilterParams();

                    var l_InputFieldView = p_LevelSearchViewController.GetField<HMUI.InputFieldView, LevelSearchViewController>("_searchTextInputFieldView");
                    if (l_InputFieldView != null && l_InputFieldView)
                    {
                        l_InputFieldView.UpdateClearButton();
                        l_InputFieldView.UpdatePlaceholder();
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[SDK.Game][LevelSelection.LevelSearchViewController_didStartLoadingEvent] failed : ");
                Logger.Instance.Error(p_Exception);
            }
        }
    }
}
