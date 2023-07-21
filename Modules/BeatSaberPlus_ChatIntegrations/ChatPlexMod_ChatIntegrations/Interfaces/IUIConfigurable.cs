using CP_SDK.XUI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.Interfaces
{
    /// <summary>
    /// UI Configurable interface
    /// </summary>
    public abstract class IUIConfigurable
    {
        /// <summary>
        /// Fields
        /// </summary>
        public CP_SDK.XUI.IXUIElement[] XUIElements;
        /// <summary>
        /// View instance
        /// </summary>
        public UI.SettingsMainView View => UI.SettingsMainView.Instance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public void BuildUIAuto(Transform p_Parent)
        {
            var l_Title = string.Empty;

                 if (this is IEventBase     l_EventBase)        l_Title = l_EventBase.GetTypeName() + " | " + l_EventBase.GenericModel.Name;
            else if (this is IActionBase    l_ActionBase)       l_Title = l_ActionBase.GetTypeName();
            else if (this is IConditionBase l_ConditionBase)    l_Title = l_ConditionBase.GetTypeName();

            l_Title = "<color=yellow>" + l_Title.Replace("_", "::</color><b>");

            var l_FinalList = new List<IXUIElement>()
            {
                XUIText.Make(l_Title).SetStyle(FontStyles.Bold).SetAlign(TextAlignmentOptions.Center)
            };

            if (XUIElements != null)
                l_FinalList.AddRange(XUIElements);

            try
            {
                XUIVScrollView.Make(
                    XUIVLayout.Make(l_FinalList.ToArray())
                        .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)
                )
                .OnReady(x => x.Container.GetComponent<HorizontalOrVerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0))
                .BuildUI(p_Parent);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error(l_Exception);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(p_Parent.parent as RectTransform);

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                if (!p_Parent || !p_Parent.parent)
                    return;

                LayoutRebuilder.ForceRebuildLayoutImmediate(p_Parent.parent as RectTransform);
            });
        }
    }
}
