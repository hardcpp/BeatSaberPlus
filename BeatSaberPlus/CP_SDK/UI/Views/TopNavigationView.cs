using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using UnityEngine;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Top navigation view
    /// </summary>
    public sealed class TopNavigationView : ViewController<TopNavigationView>
    {
        private XUIText m_Title = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public event Action OnBackButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            XUIHLayout.Make(
                XUIPrimaryButton.Make("", OnBackButtonPressed)
                    .SetWidth(12.0f).SetHeight(8.0f)
                    .SetBackgroundSprite(UISystem.GetUIRoundRectLeftBGSprite())
                    .SetIconSprite(UISystem.GetUIDownArrowSprite())
                    .OnReady(x =>
                    {
                        x.IconImageC.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                        x.IconImageC.rectTransform.localScale       = new Vector3(0.4f, 0.4f, 0.6f);
                    }),

                XUIHLayout.Make(
                    XUIText.Make("super test title!")
                        .SetStyle(TMPro.FontStyles.UpperCase | TMPro.FontStyles.Bold)
                        .SetFontSize(4.5f)
                        .Bind(ref m_Title)
                )
                .SetPadding(0, 0, 0, -12).SetSpacing(0)
                .SetBackground(true, UISystem.NavigationBarBGColor, true)
                .SetBackgroundSprite(UISystem.GetUIRoundRectRightBGSprite(), UnityEngine.UI.Image.Type.Sliced)
                .OnReady(x => x.LElement.flexibleWidth = 5000.0f)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            )
            .SetPadding(0).SetSpacing(0)
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set title
        /// </summary>
        /// <param name="p_Title">New title</param>
        public void SetTitle(string p_Title)
        {
            m_Title.SetText(p_Title);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On back button pressed
        /// </summary>
        private void OnBackButtonPressed()
        {
            try { OnBackButton?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Views][TopNavigationView.OnBackButtonPressed] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
