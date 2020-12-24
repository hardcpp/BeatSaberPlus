using BeatSaberMarkupLanguage.Parser;
using System.Linq;
using TMPro;

using BSMLSliderSetting = BeatSaberMarkupLanguage.Components.Settings.SliderSetting;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Slider setting helper
    /// </summary>
    public class SliderSetting
    {
        /// <summary>
        /// Setup a toggle setting
        /// </summary>
        /// <param name="p_Setting">Setting to setûp</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_Formatter">Value formatter</param>
        /// <param name="p_Value">New value</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        /// <param name="p_AddControls">Add Inc/dec buttons</param>
        /// <param name="p_NewRectMin">New rect min</param>
        /// <param name="p_NewRectMax">New rect max</param>
        public static void Setup(BSMLSliderSetting p_Setting,
                                BSMLAction p_Action,
                                BSMLAction p_Formatter,
                                float p_Value,
                                bool p_RemoveLabel,
                                bool p_AddControls = false,
                                UnityEngine.Vector2 p_NewRectMin = default,
                                UnityEngine.Vector2 p_NewRectMax = default)
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
                UnityEngine.GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                UnityEngine.RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as UnityEngine.RectTransform;
                l_RectTransform.anchorMin = UnityEngine.Vector2.zero;
                l_RectTransform.anchorMax = UnityEngine.Vector2.one;
                l_RectTransform.sizeDelta = UnityEngine.Vector2.one;

                p_Setting.gameObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = -1f;

                if (p_AddControls)
                {
                    l_RectTransform = p_Setting.gameObject.transform.Find("BSMLSlider") as UnityEngine.RectTransform;
                    l_RectTransform.anchorMin = p_NewRectMin;
                    l_RectTransform.anchorMax = p_NewRectMax;

                    FormattedFloatListSettingsValueController l_BaseSettings = UnityEngine.MonoBehaviour.Instantiate(UnityEngine.Resources.FindObjectsOfTypeAll<FormattedFloatListSettingsValueController>().First(x => (x.name == "VRRenderingScale")), p_Setting.gameObject.transform, false);
                    var l_DecButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<UnityEngine.UI.Button>().First();
                    var l_IncButton = l_BaseSettings.transform.GetChild(1).GetComponentsInChildren<UnityEngine.UI.Button>().Last();

                    l_DecButton.transform.SetParent(p_Setting.gameObject.transform, false);
                    l_DecButton.name = "BSP_DecButton";
                    l_IncButton.transform.SetParent(p_Setting.gameObject.transform, false);
                    l_IncButton.name = "BSP_IncButton";

                    l_IncButton.transform.SetAsFirstSibling();
                    l_DecButton.transform.SetAsFirstSibling();

                    foreach (UnityEngine.Transform l_Child in l_BaseSettings.transform)
                        UnityEngine.GameObject.Destroy(l_Child.gameObject);

                    UnityEngine.GameObject.Destroy(l_BaseSettings);

                    p_Setting.slider.valueDidChangeEvent += (_, p_NewValue) =>
                    {
                        l_DecButton.interactable = p_NewValue > p_Setting.slider.minValue;
                        l_IncButton.interactable = p_NewValue < p_Setting.slider.maxValue;
                        p_Setting.ApplyValue();
                        p_Setting.ReceiveValue();
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
                        p_Setting.ReceiveValue();
                    });
                    l_IncButton.onClick.RemoveAllListeners();
                    l_IncButton.onClick.AddListener(() =>
                    {
                        p_Setting.slider.value += p_Setting.increments;
                        l_DecButton.interactable = p_Setting.slider.value > p_Setting.slider.minValue;
                        l_IncButton.interactable = p_Setting.slider.value < p_Setting.slider.maxValue;
                        p_Setting.ApplyValue();
                        p_Setting.ReceiveValue();
                    });
                }
            }

            p_Setting.gameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set slider interactable
        /// </summary>
        /// <param name="p_Setting">Instance</param>
        /// <param name="p_Interactable">New state</param>
        public static void SetInteractable(BSMLSliderSetting p_Setting, bool p_Interactable)
        {
            if (p_Setting.slider.interactable == p_Interactable)
                return;

            p_Setting.gameObject.SetActive(false);
            p_Setting.slider.interactable = p_Interactable;

            if (p_Setting.gameObject.transform.GetChild(2).Find("BG"))
                p_Setting.gameObject.transform.GetChild(2).Find("BG").gameObject.SetActive(p_Interactable);

            var l_DecButton = p_Setting.gameObject.transform.Find("BSP_DecButton")?.GetComponent<UnityEngine.UI.Button>();
            var l_IncButton = p_Setting.gameObject.transform.Find("BSP_IncButton")?.GetComponent<UnityEngine.UI.Button>();

            if (l_DecButton != null) l_DecButton.interactable = p_Interactable && p_Setting.slider.value > p_Setting.slider.minValue;
            if (l_IncButton != null) l_IncButton.interactable = p_Interactable && p_Setting.slider.value < p_Setting.slider.maxValue;
            p_Setting.gameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Setting">Instance</param>
        /// <param name="p_Value">New value</param>
        public static void SetValue(BSMLSliderSetting p_Setting, float p_Value)
        {
            p_Setting.slider.value = p_Value;
            p_Setting.ApplyValue();
            p_Setting.ReceiveValue();
        }
    }
}
