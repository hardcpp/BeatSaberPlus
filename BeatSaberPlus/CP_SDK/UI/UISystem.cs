using CP_SDK.Unity.Extensions;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI
{
    /// <summary>
    /// UI system main class
    /// </summary>
    public static class UISystem
    {
        private static Unity.EnhancedImage m_LoadingAnimation = null;

        private static Sprite m_UIButtonSprite              = null;
        private static Sprite m_UIColorPickerFBGSprite      = null;
        private static Sprite m_UIColorPickerHBGSprite      = null;
        private static Sprite m_UIColorPickerSBGSprite      = null;
        private static Sprite m_UIColorPickerVBGSprite      = null;
        private static Sprite m_UIDownArrowSprite           = null;
        private static Sprite m_UIIconGear                  = null;
        private static Sprite m_UIIconLocked                = null;
        private static Sprite m_UIIconUnlocked              = null;
        private static Sprite m_UIRectBGSprite              = null;
        private static Sprite m_UIRoundBGSprite             = null;
        private static Sprite m_UIRoundRectLeftBGSprite     = null;
        private static Sprite m_UIRoundRectRightBGSprite    = null;
        private static Sprite m_UIRoundSmoothFrameSprite    = null;
        private static Sprite m_UISliderBGSprite            = null;
        private static Sprite m_UISliderHandleSprite        = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static FactoryInterfaces.IColorInput                     ColorInputFactory           = new DefaultFactories.DefaultColorInput();
        public static FactoryInterfaces.IFLayoutFactory                 FLayoutFactory              = new DefaultFactories.DefaultFLayoutFactory();
        public static FactoryInterfaces.IFloatingPanelFactory           FloatingPanelFactory        = new DefaultFactories.DefaultFloatingPanelFactory();
        public static FactoryInterfaces.IDropdownFactory                DropdownFactory             = new DefaultFactories.DefaultDropdownFactory();
        public static FactoryInterfaces.IGLayoutFactory                 GLayoutFactory              = new DefaultFactories.DefaultGLayoutFactory();
        public static FactoryInterfaces.IHLayoutFactory                 HLayoutFactory              = new DefaultFactories.DefaultHLayoutFactory();
        public static FactoryInterfaces.IIconButtonFactory              IconButtonFactory           = new DefaultFactories.DefaultIconButtonFactory();
        public static FactoryInterfaces.IImageFactory                   ImageFactory                = new DefaultFactories.DefaultImageFactory();
        public static FactoryInterfaces.IPrimaryButtonFactory           PrimaryButtonFactory        = new DefaultFactories.DefaultPrimaryButtonFactory();
        public static FactoryInterfaces.ISecondaryButtonFactory         SecondaryButtonFactory      = new DefaultFactories.DefaultSecondaryButtonFactory();
        public static FactoryInterfaces.ISliderFactory                  SliderFactory               = new DefaultFactories.DefaultSliderFactory();
        public static FactoryInterfaces.ITabControlFactory              TabControl                  = new DefaultFactories.DefaultTabControlFactory();
        public static FactoryInterfaces.ITextFactory                    TextFactory                 = new DefaultFactories.DefaultTextFactory();
        public static FactoryInterfaces.ITextInputFactory               TextInputFactory            = new DefaultFactories.DefaultTextInputFactory();
        public static FactoryInterfaces.ITextSegmentedControlFactory    TextSegmentedControlFactory = new DefaultFactories.DefaultTextSegmentedControlFactory();
        public static FactoryInterfaces.IToggleFactory                  ToggleFactory               = new DefaultFactories.DefaultToggleFactory();
        public static FactoryInterfaces.IVLayoutFactory                 VLayoutFactory              = new DefaultFactories.DefaultVLayoutFactory();
        public static FactoryInterfaces.IVScrollViewFactory             VScrollViewFactory          = new DefaultFactories.DefaultVScrollViewFactory();
        public static FactoryInterfaces.IVVListFactory                  VVListFactory               = new DefaultFactories.DefaultVVListFactory();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static int   UILayer     = 5;
        public static float FontScale   = 0.875f;

        public static Color DefaultBGColor          = ColorU.WithAlpha("#000000", 0.5f);
        public static Color NavigationBarBGColor    = ColorU.WithAlpha("#727272", 0.65f);
        public static Color PrimaryColor            = new Color32( 37, 140, 255, 255);
        public static Color SecondaryColor          = new Color32(133, 133, 135, 255);
        public static Color TitleBlockBGColor       = ColorU.WithAlpha("#727272", 0.5f);
        public static Color ModalBGColor            = ColorU.WithAlpha("#202021", 0.975f);
        public static Color ListBGColor             = ColorU.WithAlpha("#000000", 0.75f);
        public static Color KeyboardTextBGColor     = ColorU.WithAlpha(Color.gray, 0.50f);
        public static Color TooltipBGColor          = ColorU.WithAlpha("#3A3A3B", 0.9875f);

        public static Color TextColor               = Color.white;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Type Override_UnityComponent_Image            = typeof(Image);
        public static Type Override_UnityComponent_TextMeshProUGUI  = typeof(TextMeshProUGUI);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Func<TMP_FontAsset>   Override_GetUIFont                  = ()  => Unity.FontManager.GetMainFont();
        public static Func<Material>        Override_GetUIFontSharedMaterial    = ()  => null;
        public static Func<Material>        Override_GetUIMaterial              = ()  => null;
        public static Action<MonoBehaviour> Override_OnClick                    = (x) => { };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Action<IScreen>       OnScreenCreated = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static void Init()
        {
            var l_Bytes = Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.ChatPlexLogoLoading.webp");
            Unity.EnhancedImage.FromRawAnimated(
                "CP_SDK._Resources.ChatPlexLogoLoading.webp",
                Animation.EAnimationType.WEBP,
                l_Bytes,
                (x) => {
                    m_LoadingAnimation = x;
                }
            );

            ScreenSystem.Create();
            ModMenu.Create();
        }
        internal static void Destroy()
        {
            ModMenu.Destroy();
            ScreenSystem.Destroy();

            FlowCoordinators.MainFlowCoordinator.Destroy();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Unity.EnhancedImage GetLoadingAnimation() => m_LoadingAnimation;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Func<Sprite> GetUIButtonSprite            = () => GetXSprite(ref m_UIButtonSprite,            "UIButton",             new Vector4(10, 10, 10, 10));
        public static Func<Sprite> GetUIColorPickerFBGSprite    = () => GetXSprite(ref m_UIColorPickerFBGSprite,    "UIColorPickerFBG",     Vector4.zero);
        public static Func<Sprite> GetUIColorPickerHBGSprite    = () => GetXSprite(ref m_UIColorPickerHBGSprite,    "UIColorPickerHBG",     Vector4.zero);
        public static Func<Sprite> GetUIColorPickerSBGSprite    = () => GetXSprite(ref m_UIColorPickerSBGSprite,    "UIColorPickerSBG",     Vector4.zero);
        public static Func<Sprite> GetUIColorPickerVBGSprite    = () => GetXSprite(ref m_UIColorPickerVBGSprite,    "UIColorPickerVBG",     Vector4.zero);
        public static Func<Sprite> GetUIDownArrowSprite         = () => GetXSprite(ref m_UIDownArrowSprite,         "UIDownArrow",          Vector4.zero);
        public static Func<Sprite> GetUIIconGearSprite          = () => GetXSprite(ref m_UIIconGear,                "UIIconGear",           Vector4.zero);
        public static Func<Sprite> GetUIIconLockedSprite        = () => GetXSprite(ref m_UIIconLocked,              "UIIconLocked",         Vector4.zero);
        public static Func<Sprite> GetUIIconUnlockedSprite      = () => GetXSprite(ref m_UIIconUnlocked,            "UIIconUnlocked",       Vector4.zero);
        public static Func<Sprite> GetUIRectBGSprite            = () => GetXSprite(ref m_UIRectBGSprite,            "UIRectBG",             new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUIRoundBGSprite           = () => GetXSprite(ref m_UIRoundBGSprite,           "UIRoundBG",            new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUIRoundRectLeftBGSprite   = () => GetXSprite(ref m_UIRoundRectLeftBGSprite,   "UIRoundRectLeftBG",    new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUIRoundRectRightBGSprite  = () => GetXSprite(ref m_UIRoundRectRightBGSprite,  "UIRoundRectRightBG",   new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUIRoundSmoothFrameSprite  = () => GetXSprite(ref m_UIRoundSmoothFrameSprite,  "UIRoundSmoothFrame",   new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUISliderBGSprite          = () => GetXSprite(ref m_UISliderBGSprite,          "UISliderBG",           new Vector4(15, 15, 15, 15));
        public static Func<Sprite> GetUISliderHandleSprite      = () => GetXSprite(ref m_UISliderHandleSprite,      "UISliderHandle",       Vector4.zero);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static t_ViewController CreateViewController<t_ViewController>()
            where t_ViewController : ViewController<t_ViewController>
        {
            if (ViewController<t_ViewController>.Instance)
                return ViewController<t_ViewController>.Instance;

            var l_GameObject = new GameObject(typeof(t_ViewController).Name, typeof(RectTransform), typeof(CanvasGroup));
            GameObject.DontDestroyOnLoad(l_GameObject);

            var l_ViewController = l_GameObject.AddComponent<t_ViewController>();
            l_ViewController.RTransform.anchorMin           = Vector2.zero;
            l_ViewController.RTransform.anchorMax           = Vector2.one;
            l_ViewController.RTransform.sizeDelta           = Vector2.zero;
            l_ViewController.RTransform.anchoredPosition    = Vector2.zero;
            l_ViewController.gameObject.SetActive(false);

            return l_ViewController;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static void DestroyUI<t_ViewController>(ref t_ViewController p_ViewController)
            where t_ViewController : ViewController<t_ViewController>
        {
            if (p_ViewController)
            {
                GameObject.Destroy(p_ViewController.gameObject);
                p_ViewController = null;
            }
        }
        public static void DestroyUI(ref Components.CFloatingPanel p_Panel)
        {
            if (p_Panel)
            {
                GameObject.DestroyImmediate(p_Panel.gameObject);
                p_Panel = null;
            }
        }
        public static void DestroyUI<t_ViewController>(ref Components.CFloatingPanel p_Panel, ref t_ViewController p_ViewController)
            where t_ViewController : ViewController<t_ViewController>
        {
            if (p_Panel)
            {
                GameObject.DestroyImmediate(p_Panel.gameObject);
                p_Panel = null;
            }

            if (p_ViewController)
            {
                GameObject.Destroy(p_ViewController.gameObject);
                p_ViewController = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Sprite GetXSprite(ref Sprite p_Sprite, string p_File, Vector4 p_Borders)
        {
            if (p_Sprite)
                return p_Sprite;

            var l_Bytes     = Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources." + p_File + ".png");
            var l_Texture   = Unity.Texture2DU.CreateFromRaw(l_Bytes);
            l_Texture.filterMode = FilterMode.Trilinear;

            p_Sprite = Unity.SpriteU.CreateFromTextureWithBorders(l_Texture, 100.0f, new Vector2(0.5f, 0.5f), 0, SpriteMeshType.FullRect, p_Borders);

            return p_Sprite;
        }
    }
}
