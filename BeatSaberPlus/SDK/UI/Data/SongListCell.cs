using CP_SDK.UI;
using CP_SDK.UI.Components;
using CP_SDK.UI.Data;
using CP_SDK.Unity.Extensions;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI.Data
{
    /// <summary>
    /// Song list cell
    /// </summary>
    public class SongListCell : IListCell
    {
        public CImage CoverMask;
        public CImage Cover;
        public CImage CoverFrame;
        public CText Title;
        public CText SubTitle;
        public CText Duration;
        public CText BPM;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        public override void Build()
        {
            if (RTransform)
                return;

            base.Build();

            CoverMask = UISystem.ImageFactory.Create("CoverMask", RTransform);
            CoverMask.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            CoverMask.SetSprite(UISystem.GetUIRoundBGSprite());
            CoverMask.SetType(UnityEngine.UI.Image.Type.Sliced);
            CoverMask.SetPixelsPerUnitMultiplier(4f);
            CoverMask.RTransform.anchorMin          = new Vector2(  0.0f,  0.0f);
            CoverMask.RTransform.anchorMax          = new Vector2(  0.0f,  0.0f);
            CoverMask.RTransform.pivot              = new Vector2(  0.0f,  0.0f);
            CoverMask.RTransform.sizeDelta          = new Vector2(  9.0f,  9.0f);
            CoverMask.RTransform.anchoredPosition   = new Vector2(  0.5f,  0.5f);

            Cover = UISystem.ImageFactory.Create("Cover", CoverMask.RTransform);
            Cover.RTransform.anchorMin          = new Vector2(  0.0f,  0.0f);
            Cover.RTransform.anchorMax          = new Vector2(  1.0f,  1.0f);
            Cover.RTransform.pivot              = new Vector2(  0.5f,  0.5f);
            Cover.RTransform.sizeDelta          = new Vector2( -0.4f, -0.4f);
            Cover.RTransform.anchoredPosition   = new Vector2(  0.0f,  0.0f);

            CoverFrame = UISystem.ImageFactory.Create("CoverFrame", RTransform);
            CoverFrame.SetSprite(UISystem.GetUIRoundSmoothFrameSprite());
            CoverFrame.SetType(UnityEngine.UI.Image.Type.Sliced);
            CoverFrame.SetPixelsPerUnitMultiplier(20f);
            CoverFrame.SetColor(ColorU.ToUnityColor("#CCCCCC"));
            CoverFrame.RTransform.anchorMin          = new Vector2(  0.0f,  0.0f);
            CoverFrame.RTransform.anchorMax          = new Vector2(  0.0f,  0.0f);
            CoverFrame.RTransform.pivot              = new Vector2(  0.0f,  0.0f);
            CoverFrame.RTransform.sizeDelta          = new Vector2(  9.0f,  9.0f);
            CoverFrame.RTransform.anchoredPosition   = new Vector2(  0.5f,  0.5f);

            Title = UISystem.TextFactory.Create("Title", RTransform);
            Title.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft);
            Title.SetOverflowMode(TMPro.TextOverflowModes.Ellipsis);
            Title.SetMargins(1.0f, 0.0f, 1.0f, 0.0f);
            Title.SetStyle(TMPro.FontStyles.Bold);
            Title.SetFontSizes(4.00f, 2.75f);
            Title.RTransform.anchorMin          = new Vector2(  0.0f, 0.45f);
            Title.RTransform.anchorMax          = new Vector2(  1.0f, 1.0f);
            Title.RTransform.pivot              = new Vector2(  0.5f, 0.5f);
            Title.RTransform.sizeDelta          = new Vector2(-20.0f, 0.0f);
            Title.RTransform.anchoredPosition   = new Vector2(  0.0f, 0.0f);

            SubTitle = UISystem.TextFactory.Create("SubTitle", RTransform);
            SubTitle.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft);
            SubTitle.SetOverflowMode(TMPro.TextOverflowModes.Ellipsis);
            SubTitle.SetMargins(1.0f, 0.0f, 1.0f, 0.0f);
            SubTitle.SetFontSizes(2.8f, 2.00f);
            SubTitle.SetColor(ColorU.ToUnityColor("#BBBBBB"));
            SubTitle.RTransform.anchorMin           = new Vector2(  0.0f, 0.0f);
            SubTitle.RTransform.anchorMax           = new Vector2(  1.0f, 0.55f);
            SubTitle.RTransform.pivot               = new Vector2(  0.5f, 0.5f);
            SubTitle.RTransform.sizeDelta           = new Vector2(-18.0f, 0.0f);
            SubTitle.RTransform.anchoredPosition    = new Vector2(  1.0f, 0.0f);

            Duration = UISystem.TextFactory.Create("Duration", RTransform);
            Duration.SetAlign(TMPro.TextAlignmentOptions.CaplineRight);
            Duration.SetOverflowMode(TMPro.TextOverflowModes.Truncate);
            Duration.SetMargins(0.0f, 0.0f, 1.0f, 0.0f);
            Duration.SetStyle(TMPro.FontStyles.Bold);
            Duration.SetFontSizes(3.75f, 2.75f);
            Duration.RTransform.anchorMin          = new Vector2(  1.0f, 0.45f);
            Duration.RTransform.anchorMax          = new Vector2(  1.0f, 1.0f);
            Duration.RTransform.pivot              = new Vector2(  1.0f, 0.5f);
            Duration.RTransform.sizeDelta          = new Vector2( 10.0f, 0.0f);
            Duration.RTransform.anchoredPosition   = new Vector2(  0.0f, 0.0f);

            BPM = UISystem.TextFactory.Create("BPM", RTransform);
            BPM.SetAlign(TMPro.TextAlignmentOptions.CaplineRight);
            BPM.SetOverflowMode(TMPro.TextOverflowModes.Truncate);
            BPM.SetMargins(0.0f, 0.0f, 1.0f, 0.0f);
            BPM.SetFontSizes(2.8f, 2.00f);
            BPM.SetColor(ColorU.ToUnityColor("#BBBBBB"));
            BPM.RTransform.anchorMin           = new Vector2(  1.0f, 0.0f);
            BPM.RTransform.anchorMax           = new Vector2(  1.0f, 0.55f);
            BPM.RTransform.pivot               = new Vector2(  1.0f, 0.5f);
            BPM.RTransform.sizeDelta           = new Vector2(  8.0f, 0.0f);
            BPM.RTransform.anchoredPosition    = new Vector2(  0.0f, 0.0f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get cell height
        /// </summary>
        /// <returns></returns>
        public override float GetCellHeight()
            => 10.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add self component
        /// </summary>
        /// <param name="p_Target">Target gameobject</param>
        /// <returns></returns>
        protected override IListCell AddSelfComponent(GameObject p_Target)
            => p_Target.GetComponent<SongListCell>() ?? p_Target.AddComponent<SongListCell>();
    }
}
