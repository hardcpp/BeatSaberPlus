using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BS_Utils.Utilities;
using HMUI;
using IPA.Utilities;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// User interface utils
    /// </summary>
    public class GameUI
    {
        /// <summary>
        /// Create text segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static TextSegmentedControl CreateTextSegmentedControl(RectTransform p_Parent, bool p_HideCellBackground)
        {
            TextSegmentedControl l_Prefab = Resources.FindObjectsOfTypeAll<TextSegmentedControl>().First(x => x.name == "BeatmapDifficultySegmentedControl" && x.GetField<DiContainer, TextSegmentedControl>("_container") != null);
            TextSegmentedControl l_Control = MonoBehaviour.Instantiate(l_Prefab, p_Parent, false);
            l_Control.name = "BSMLTextSegmentedControl";
            l_Control.SetField("_container", l_Prefab.GetField<DiContainer, TextSegmentedControl>("_container"));
            l_Control.SetField("_hideCellBackground", p_HideCellBackground);

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            MonoBehaviour.Destroy(l_Control.GetComponent<BeatmapDifficultySegmentedControlController>());
            return l_Control;
        }
        /// <summary>
        /// Create icon segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static IconSegmentedControl CreateHorizontalIconSegmentedControl(RectTransform p_Parent, bool p_HideCellBackground)
        {
            IconSegmentedControl l_Prefab = Resources.FindObjectsOfTypeAll<IconSegmentedControl>().First(x => x.name == "BeatmapCharacteristicSegmentedControl" && x.GetField<DiContainer, IconSegmentedControl>("_container") != null);
            IconSegmentedControl l_Control = MonoBehaviour.Instantiate(l_Prefab, p_Parent, false);
            l_Control.name = "BSMLIconSegmentedControl";
            l_Control.SetField("_container", l_Prefab.GetField<DiContainer, IconSegmentedControl>("_container"));
            l_Control.SetField("_hideCellBackground", p_HideCellBackground);

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            MonoBehaviour.Destroy(l_Control.GetComponent<BeatmapCharacteristicSegmentedControlController>());

            return l_Control;
        }
        /// <summary>
        /// Create icon segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static IconSegmentedControl CreateVerticalIconSegmentedControl(RectTransform p_Parent, bool p_HideCellBackground)
        {
            PlatformLeaderboardViewController l_PlatformLeaderboardViewController = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().First();
            var l_Prefab = l_PlatformLeaderboardViewController.GetField<IconSegmentedControl, PlatformLeaderboardViewController>("_scopeSegmentedControl");

            IconSegmentedControl l_Control = MonoBehaviour.Instantiate(l_Prefab, p_Parent, false);
            l_Control.name = "BSMLVerticalIconSegmentedControl";
            l_Control.SetField("_container", l_Prefab.GetField<DiContainer, IconSegmentedControl>("_container"));
            l_Control.SetField("_hideCellBackground", p_HideCellBackground);

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            return l_Control;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sanitize user input of TextMeshPro tags.
        /// </summary>
        /// <param name="p_String">A <see cref="string"/> containing user input.</param>
        /// <returns>Sanitized <see cref="string"/>.</returns>
        public static string EscapeTextMeshProTags(string p_String)
        {
            return new StringBuilder(p_String).Replace("<", "<\u200B").Replace(">", "\u200B>").ToString();
        }


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static void PrepareToggleSetting(ToggleSetting p_Setting, BSMLAction p_Action, bool p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            p_Setting.Value = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            if (p_RemoveLabel)
            {
                GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as RectTransform;
                l_RectTransform.anchorMin = Vector2.zero;
                l_RectTransform.anchorMax = Vector2.one;
                l_RectTransform.sizeDelta = Vector2.one;

                p_Setting.gameObject.GetComponent<LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);
        }
        public static void PrepareIncrementSetting(IncrementSetting p_Setting, BSMLAction p_Action, BSMLAction p_Formatter, float p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            if (p_Formatter != null)
                p_Setting.formatter = p_Formatter;

            p_Setting.Value = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            if (p_RemoveLabel)
            {
                GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as RectTransform;
                l_RectTransform.anchorMin = Vector2.zero;
                l_RectTransform.anchorMax = Vector2.one;
                l_RectTransform.sizeDelta = Vector2.one;

                p_Setting.gameObject.GetComponent<LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);
        }

        public static void PrepareColorSetting(ColorSetting p_Setting, BSMLAction p_Action, Color p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            p_Value.a = 1.0f;
            p_Setting.CurrentColor = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            p_Setting.updateOnChange = true;

            if (p_RemoveLabel)
            {
                GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as RectTransform;
                l_RectTransform.anchorMin = Vector2.zero;
                l_RectTransform.anchorMax = Vector2.one;
                l_RectTransform.sizeDelta = Vector2.one;

                p_Setting.gameObject.GetComponent<LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);
        }

        public static void PrepareSliderSetting(SliderSetting p_Setting,
                                                BSMLAction p_Action,
                                                BSMLAction p_Formatter,
                                                float p_Value,
                                                bool p_RemoveLabel,
                                                bool p_AddControls = false,
                                                Vector2 p_NewRectMin = default,
                                                Vector2 p_NewRectMax = default)
        {
            p_Setting.gameObject.SetActive(false);

            if (p_Formatter != null)
                p_Setting.formatter = p_Formatter;

            p_Setting.slider.value = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            p_Setting.updateOnChange = true;

            if (p_RemoveLabel)
            {
                GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as RectTransform;
                l_RectTransform.anchorMin = Vector2.zero;
                l_RectTransform.anchorMax = Vector2.one;
                l_RectTransform.sizeDelta = Vector2.one;

                p_Setting.gameObject.GetComponent<LayoutElement>().preferredWidth = -1f;

                if (p_AddControls)
                {
                    l_RectTransform = p_Setting.gameObject.transform.Find("BSMLSlider") as RectTransform;
                    l_RectTransform.anchorMin = p_NewRectMin;
                    l_RectTransform.anchorMax = p_NewRectMax;

                    FormattedFloatListSettingsValueController l_BaseSettings = MonoBehaviour.Instantiate(Resources.FindObjectsOfTypeAll<FormattedFloatListSettingsValueController>().First(x => (x.name == "VRRenderingScale")), p_Setting.gameObject.transform, false);
                    var l_DecButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<Button>().First();
                    var l_IncButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<Button>().Last();

                    l_DecButton.transform.SetParent(p_Setting.gameObject.transform, false);
                    l_DecButton.name = "BSP_DecButton";
                    l_IncButton.transform.SetParent(p_Setting.gameObject.transform, false);
                    l_IncButton.name = "BSP_IncButton";

                    l_IncButton.transform.SetAsFirstSibling();
                    l_DecButton.transform.SetAsFirstSibling();

                    foreach (Transform l_Child in l_BaseSettings.transform)
                        GameObject.Destroy(l_Child.gameObject);

                    GameObject.Destroy(l_BaseSettings);

                    p_Setting.slider.valueDidChangeEvent += (_, p_NewValue) =>
                    {
                        l_DecButton.interactable = p_NewValue > p_Setting.slider.minValue;
                        l_IncButton.interactable = p_NewValue < p_Setting.slider.maxValue;
                        p_Setting.ApplyValue();
                    };

                    l_DecButton.interactable = p_Setting.slider.value > p_Setting.slider.minValue;
                    l_IncButton.interactable = p_Setting.slider.value < p_Setting.slider.maxValue;

                    l_DecButton.onClick.RemoveAllListeners();
                    l_DecButton.onClick.AddListener(() =>
                    {
                        p_Setting.slider.value -= p_Setting.increments;
                        l_DecButton.interactable = p_Setting.slider.value > p_Setting.slider.minValue;
                        l_IncButton.interactable = p_Setting.slider.value < p_Setting.slider.maxValue;
                        p_Setting.ApplyValue();
                    });
                    l_IncButton.onClick.RemoveAllListeners();
                    l_IncButton.onClick.AddListener(() =>
                    {
                        p_Setting.slider.value += p_Setting.increments;
                        l_DecButton.interactable = p_Setting.slider.value > p_Setting.slider.minValue;
                        l_IncButton.interactable = p_Setting.slider.value < p_Setting.slider.maxValue;
                        p_Setting.ApplyValue();
                    });
                }
            }

            p_Setting.gameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static void SetSliderInteractable(SliderSetting p_Setting, bool p_Interactable)
        {
            p_Setting.gameObject.SetActive(false);
            p_Setting.slider.interactable = p_Interactable;

            if (p_Setting.gameObject.transform.GetChild(2).Find("BG"))
                p_Setting.gameObject.transform.GetChild(2).Find("BG").gameObject.SetActive(p_Interactable);

            var l_DecButton = p_Setting.gameObject.transform.Find("BSP_DecButton")?.GetComponent<Button>();
            var l_IncButton = p_Setting.gameObject.transform.Find("BSP_IncButton")?.GetComponent<Button>();

            if (l_DecButton != null) l_DecButton.interactable = p_Interactable && p_Setting.slider.value > p_Setting.slider.minValue;
            if (l_IncButton != null) l_IncButton.interactable = p_Interactable && p_Setting.slider.value < p_Setting.slider.maxValue;
            p_Setting.gameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static BSMLAction m_Formatter_DateMonthFrom2018;
        internal static BSMLAction Formatter_DateMonthFrom2018
        {
            get
            {
                if (m_Formatter_DateMonthFrom2018 == null)
                    m_Formatter_DateMonthFrom2018 = new BSMLAction(null, typeof(GameUI).GetMethod(nameof(Formatter_FNDateMonthFrom2018), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                return m_Formatter_DateMonthFrom2018;
            }
        }
        /// <summary>
        /// On date setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string Formatter_FNDateMonthFrom2018(int p_Value)
        {
            string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            int l_Year = 2018 + (p_Value / 12);

            return s_Months[p_Value % 12] + " " + l_Year;
        }

        private static BSMLAction m_Formatter_Time;
        internal static BSMLAction Formatter_Time
        {
            get
            {
                if (m_Formatter_Time == null)
                    m_Formatter_Time = new BSMLAction(null, typeof(GameUI).GetMethod(nameof(Formatter_FNTime), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                return m_Formatter_Time;
            }
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string Formatter_FNTime(int p_Value)
        {
            int l_Seconds = p_Value / 60;
            int l_Minutes = (p_Value > 60) ? (p_Value - l_Seconds) / 60 : 0;

            string l_Result = (l_Minutes != 0 ? l_Minutes : l_Seconds).ToString();
            if (l_Minutes != 0)
                l_Result += "m " + l_Seconds + "s";

            return l_Result;
        }

        private static BSMLAction m_Formatter_Seconds;
        internal static BSMLAction Formatter_Seconds
        {
            get
            {
                if (m_Formatter_Seconds == null)
                    m_Formatter_Seconds = new BSMLAction(null, typeof(GameUI).GetMethod(nameof(Formatter_FNSeconds), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                return m_Formatter_Seconds;
            }
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string Formatter_FNSeconds(int p_Value)
        {
            return p_Value + " Second" + (p_Value > 1 ? "s" : "");
        }

        private static BSMLAction m_Formatter_Minutes;
        internal static BSMLAction Formatter_Minutes
        {
            get
            {
                if (m_Formatter_Minutes == null)
                    m_Formatter_Minutes = new BSMLAction(null, typeof(GameUI).GetMethod(nameof(Formatter_FNMinutes), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                return m_Formatter_Minutes;
            }
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string Formatter_FNMinutes(int p_Value)
        {
            return p_Value + " Minute" + (p_Value > 1 ? "s" : "");
        }

        private static BSMLAction m_Formatter_Percentage;
        internal static BSMLAction Formatter_Percentage
        {
            get
            {
                if (m_Formatter_Percentage == null)
                    m_Formatter_Percentage = new BSMLAction(null, typeof(GameUI).GetMethod(nameof(Formatter_FNPercentage), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                return m_Formatter_Percentage;
            }
        }
        /// <summary>
        /// On percentage setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string Formatter_FNPercentage(float p_Value)
        {
            return System.Math.Round(p_Value * 100f, 2) + " %";
        }
    }
}
