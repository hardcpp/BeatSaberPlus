using CP_SDK.XUI;
using UnityEngine;
using UnityEngine.UI;

using EPermissions = BeatSaberPlus_ChatRequest.CRConfig._Commands.EPermission;

namespace BeatSaberPlus_ChatRequest.UI.Data
{
    /// <summary>
    /// Command list cell
    /// </summary>
    public class CommandListCell : CP_SDK.UI.Data.IListCell
    {
        public XUIText   Text;
        public XUIToggle All;
        public XUIToggle Subs;
        public XUIToggle VIPs;
        public XUIToggle Mods;

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

            SetStateless(true);

            var l_SettingWidth = 25.0f;
            XUIHLayout.Make(
                XUIHLayout.Make(
                    XUIText.Make("Command")
                        .SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)
                        .SetMargins(2.0f, 0.0f, 2.0f, 0.0f)
                        .Bind(ref Text)
                )
                .SetWidth(35.0f)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .SetPadding(0).SetSpacing(0),

                XUIHLayout.Make(
                    XUIToggle.Make().OnValueChanged(OnToggleChanged).Bind(ref All),
                    XUIText.Make("All")
                )
                .SetPadding(0).SetSpacing(0)
                .SetWidth(l_SettingWidth),

                XUIHLayout.Make(
                    XUIToggle.Make().OnValueChanged(OnToggleChanged).Bind(ref Subs),
                    XUIText.Make("Subs")
                )
                .SetPadding(0).SetSpacing(0)
                .SetWidth(l_SettingWidth),

                XUIHLayout.Make(
                    XUIToggle.Make().OnValueChanged(OnToggleChanged).Bind(ref VIPs),
                    XUIText.Make("VIPs")
                )
                .SetPadding(0).SetSpacing(0)
                .SetWidth(l_SettingWidth),

                XUIHLayout.Make(
                    XUIToggle.Make().OnValueChanged(OnToggleChanged).Bind(ref Mods),
                    XUIText.Make("Mods")
                )
                .SetPadding(0).SetSpacing(0)
                .SetWidth(l_SettingWidth)
            )
            .SetPadding(0).SetSpacing(0)
            .BuildUI(RTransform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get cell height
        /// </summary>
        /// <returns></returns>
        public override float GetCellHeight()
            => 5.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update from permissions
        /// </summary>
        /// <param name="p_Permissions">New permissions</param>
        internal void UpdateFrom(EPermissions p_Permissions)
        {
            All. SetValue((p_Permissions & EPermissions.Viewers    ) != 0, false);
            Subs.SetValue((p_Permissions & EPermissions.Subscribers) != 0, false);
            VIPs.SetValue((p_Permissions & EPermissions.VIPs       ) != 0, false);
            Mods.SetValue((p_Permissions & EPermissions.Moderators ) != 0, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On toggle changed
        /// </summary>
        /// <param name="_"></param>
        private void OnToggleChanged(bool _)
        {
            if (ListItem == null || !(ListItem is CommandListItem l_CommandListItem))
                return;

            var l_NewPermissions = (EPermissions)0;
            if (All .Element.GetValue()) l_NewPermissions |= EPermissions.Viewers;
            if (Subs.Element.GetValue()) l_NewPermissions |= EPermissions.Subscribers;
            if (VIPs.Element.GetValue()) l_NewPermissions |= EPermissions.VIPs;
            if (Mods.Element.GetValue()) l_NewPermissions |= EPermissions.Moderators;

            l_CommandListItem.SetPermissions(l_NewPermissions);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add self component
        /// </summary>
        /// <param name="p_Target">Target gameobject</param>
        /// <returns></returns>
        protected override CP_SDK.UI.Data.IListCell AddSelfComponent(GameObject p_Target)
            => p_Target.GetComponent<CommandListCell>() ?? p_Target.AddComponent<CommandListCell>();
    }
}
