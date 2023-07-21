using CP_SDK.UI;
using CP_SDK.Unity.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CP_SDK.XUI
{
    /// <summary>
    /// XUI templates
    /// </summary>
    public static class Templates
    {
        /// <summary>
        /// Modal rect layout
        /// </summary>
        /// <param name="p_Childs">Childs</param>
        /// <returns></returns>
        public static XUIVLayout ModalRectLayout(params IXUIElement[] p_Childs)
            =>  XUIVLayout.Make("ModalRectLayout",
                    new IXUIElement[]
                    {
                        XUIImage.Make(UISystem.GetUIRoundSmoothFrameSprite())
                        .SetType(Image.Type.Sliced)
                        .SetColor(ColorU.WithAlpha(Color.white, 0.80f))
                        .OnReady(x =>
                        {
                            x.LElement.ignoreLayout = true;
                            x.RTransform.anchorMin         = Vector2.zero;
                            x.RTransform.anchorMax         = Vector2.one;
                            x.RTransform.anchoredPosition  = Vector2.zero;
                            x.RTransform.sizeDelta         = Vector2.zero;
                        })
                    }.Union(p_Childs).ToArray()
                )
                .SetBackground(true, UISystem.ModalBGColor, true);
        /// <summary>
        /// Full rect layout for screens template
        /// </summary>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static XUIVLayout FullRectLayout(params IXUIElement[] p_Childs)
            =>  XUIVLayout.Make("FullRectLayout",
                    p_Childs
                )
                .OnReady(x => {
                    x.CSizeFitter.enabled = false;
                });
        /// <summary>
        /// Full rect layout for screens template
        /// </summary>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static XUIVLayout FullRectLayoutMainView(params IXUIElement[] p_Childs)
            =>  XUIVLayout.Make("FullRectLayoutMainView",
                    p_Childs
                )
                .SetWidth(150)
                .SetHeight(80);
        /// <summary>
        /// Title bar template
        /// </summary>
        /// <param name="p_Text"></param>
        /// <returns></returns>
        public static XUIHLayout TitleBar(string p_Text)
            =>  XUIHLayout.Make("TitleBar",
                    XUIText.Make(p_Text)
                        .SetAlign(TMPro.TextAlignmentOptions.CaplineGeoAligned)
                        .SetFontSize(4.0f)
                )
                .SetSpacing(0)
                .SetPadding(0, 8, 0, 8)
                .SetBackground(true, UISystem.TitleBlockBGColor)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize);
        /// <summary>
        /// Scollable infos widget template
        /// </summary>
        /// <param name="p_Height"></param>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static XUIHLayout ScrollableInfos(float p_Height, params IXUIElement[] p_Childs)
            =>  XUIHLayout.Make("ScrollableInfos",
                    XUIVScrollView.Make("ScrollView",
                        p_Childs
                    )
                )
                .SetHeight(p_Height)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true);
        /// <summary>
        /// Expanded buttons line
        /// </summary>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static XUIHLayout ExpandedButtonsLine(params IXUIElement[] p_Childs)
            =>  XUIHLayout.Make("ExpandedButtonsLine",
                    p_Childs
                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .ForEachDirect<XUIPrimaryButton>  (y => {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
                .ForEachDirect<XUISecondaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                });
        /// <summary>
        /// Settings group
        /// </summary>
        /// <param name="p_Title"></param>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static IXUIElement SettingsHGroup(string p_Title, params IXUIElement[] p_Childs)
            =>  XUIVLayout.Make(
                    XUIText.Make(p_Title)
                        .SetAlign(TMPro.TextAlignmentOptions.CaplineGeoAligned)
                        .SetColor(Color.yellow),

                    XUIHLayout.Make(
                        p_Childs
                    )
                    .SetPadding(0)
                    .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                )
                .SetBackground(true)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true);
        /// <summary>
        /// Settings group
        /// </summary>
        /// <param name="p_Title"></param>
        /// <param name="p_Childs"></param>
        /// <returns></returns>
        public static IXUIElement SettingsVGroup(string p_Title, params IXUIElement[] p_Childs)
            =>  XUIVLayout.Make(
                    XUIText.Make(p_Title)
                        .SetAlign(TMPro.TextAlignmentOptions.CaplineGeoAligned)
                        .SetColor(Color.yellow),

                    XUIVLayout.Make(
                        p_Childs
                    )
                    .SetPadding(0)
                    .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                )
                .SetBackground(true)
                .OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true);
    }
}
