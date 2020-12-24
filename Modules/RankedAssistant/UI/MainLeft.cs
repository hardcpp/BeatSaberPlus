using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.RankedAssistant.UI
{
    internal class MainLeft : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
#pragma warning disable CS0169
#pragma warning disable CS0414
        [UIValue("req-accu")]
        private float m_RequiredAccuracy = 75;
        [UIValue("req-diff")]
        private float m_RequiredDifficulty = 5;
        [UIValue("playlist-size")]
        private int m_PlaylistSize = 120;
        [UIValue("type-options")]
        private List<object> m_TypeOptions;
        [UIValue("type-choice")]
        private string m_TypeChoice;
        [UIValue("accuracy-options")]
        private List<object> m_AccuracyOptions;
        [UIValue("accuracy-choice")]
        private string m_AccuracyChoice;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIValue("reset-button-text")]
        public string ResetButtonText = "Set acc & diff from profile avg";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIValue("playlist-type-hint")]
        public string PlaylistTypeHint = "Playlist types detailed on the left panel";
        [UIValue("accuracy-type-hint")]
        public string AccuracyTypeHint = "Max : Use target accuracy\nEstimate : Adapt target accuracy according to song difficulty";

        [UIValue("target-accuracy-hint")]
        public string TargetAccuracyHint = "Songs you scored above this accuracy will be ignored";
        [UIValue("difficulty-hint")]
        public string DifficultyHint = "Songs above this star rating will be ignored";
        [UIValue("playlist-size-hint")]
        public string PlaylistSizeHint = "Maximum playlist size";

        [UIValue("generate-button-hint")]
        public string GenerateButtonHint = "Will generate a playlist";
        [UIValue("rescan-button-hint")]
        public string RescanButtonHint = "This will force a profile re-scan on the sever (This is automatically done every hour)";
        [UIValue("reset-button-hint")]
        public string ResetButtonHint = "This will set for you the target accuracy & difficulty\naccording to your score saber profile average";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("playlist-type-slider")]
        private GameObject m_PlaylistTypeSlider;
        [UIObject("accuracy-type-slider")]
        private GameObject m_AccuracyTypeSlider;
        [UIObject("accuracy-slider")]
        private GameObject m_AccuracySlider;
        [UIObject("difficulty-slider")]
        private GameObject m_DifficultySlider;
        [UIObject("playlist-size-slider")]
        private GameObject m_PlaylistSizeSlider;
#pragma warning restore CS0414
#pragma warning restore CS0169
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// If first activation, bind event
            if (p_FirstActivation)
            {
                m_TypeOptions   = new object[] { "Grind", "Improve", "Discover" }.ToList();
                m_TypeChoice    = "Grind";

                m_AccuracyOptions   = new object[] { "Max", "Estimate" }.ToList();
                m_AccuracyChoice    = "Estimate";
            }

            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if (p_FirstActivation)
            {
                PrepareSlider(m_PlaylistTypeSlider);
                PrepareSlider(m_AccuracyTypeSlider);

                PrepareSlider(m_AccuracySlider, true);
                PrepareSlider(m_DifficultySlider, true);
                PrepareSlider(m_PlaylistSizeSlider, true);

            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);


        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prepare slider widget
        /// </summary>
        /// <param name="p_Object">Slider instance</param>
        /// <param name="p_AddControls">Should add left & right controls</param>
        void PrepareSlider(GameObject p_Object, bool p_AddControls = false)
        {
            GameObject.Destroy(p_Object.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

            RectTransform l_RectTransform = p_Object.transform.GetChild(1) as RectTransform;
            l_RectTransform.anchorMin = new Vector2(0f, 0f);
            l_RectTransform.anchorMax = new Vector2(1f, 1f);
            l_RectTransform.sizeDelta = new Vector2(1, 1);

            p_Object.GetComponent<LayoutElement>().preferredWidth = -1f;

            if (p_AddControls)
            {
                l_RectTransform = p_Object.transform.Find("BSMLSlider") as RectTransform;
                l_RectTransform.anchorMin = new Vector2(1.00f, -0.05f);
                l_RectTransform.anchorMax = new Vector2(0.90f, 1.05f);

                FormattedFloatListSettingsValueController l_BaseSettings = MonoBehaviour.Instantiate(Resources.FindObjectsOfTypeAll<FormattedFloatListSettingsValueController>().First(x => (x.name == "VRRenderingScale")), p_Object.transform, false);
                var l_DecButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<Button>().First();
                var l_IncButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<Button>().Last();

                l_DecButton.transform.SetParent(p_Object.transform, false);
                l_IncButton.transform.SetParent(p_Object.transform, false);

                l_DecButton.transform.SetAsFirstSibling();
                l_IncButton.transform.SetAsFirstSibling();

                foreach (Transform l_Child in l_BaseSettings.transform)
                    GameObject.Destroy(l_Child.gameObject);

                GameObject.Destroy(l_BaseSettings);

                SliderSetting l_SliderSetting = p_Object.GetComponent<SliderSetting>();

                l_SliderSetting.slider.valueDidChangeEvent += (_, p_Value) =>
                {
                    l_DecButton.interactable = p_Value > l_SliderSetting.slider.minValue;
                    l_IncButton.interactable = p_Value < l_SliderSetting.slider.maxValue;
                };

                l_DecButton.interactable = l_SliderSetting.slider.value > l_SliderSetting.slider.minValue;
                l_IncButton.interactable = l_SliderSetting.slider.value < l_SliderSetting.slider.maxValue;

                l_DecButton.onClick.RemoveAllListeners();
                l_DecButton.onClick.AddListener(() =>
                {
                    l_SliderSetting.slider.value -= l_SliderSetting.increments;
                    l_SliderSetting.InvokeMethod("OnChange", new object[] { l_SliderSetting.slider, l_SliderSetting.slider.value });
                });
                l_IncButton.onClick.RemoveAllListeners();
                l_IncButton.onClick.AddListener(() =>
                {
                    l_SliderSetting.slider.value += l_SliderSetting.increments;
                    l_SliderSetting.InvokeMethod("OnChange", new object[] { l_SliderSetting.slider, l_SliderSetting.slider.value });
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Download play list
        /// </summary>
        [UIAction("click-btn-generate")]
        public void Generate()
        {

        }
        /// <summary>
        /// Rescan profile
        /// </summary>
        [UIAction("click-btn-rescan")]
        public void RescanProfile()
        {

        }
        /// <summary>
        /// Rescan profile
        /// </summary>
        [UIAction("click-btn-reset")]
        public void Reset()
        {

        }
    }
}
