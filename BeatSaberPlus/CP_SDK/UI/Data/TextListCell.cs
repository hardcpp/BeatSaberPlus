using UnityEngine;

namespace CP_SDK.UI.Data
{
    /// <summary>
    /// Text list cell
    /// </summary>
    public class TextListCell : IListCell
    {
        public Components.CText Text;

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

            Text = UISystem.TextFactory.Create("Text", RTransform);
            Text.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft);
            Text.SetMargins(2.0f, 0.0f, 2.0f, 0.0f);
            Text.RTransform.anchorMin = Vector2.zero;
            Text.RTransform.anchorMax = Vector2.one;
            Text.RTransform.sizeDelta = Vector2.zero;
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
        /// Add self component
        /// </summary>
        /// <param name="p_Target">Target gameobject</param>
        /// <returns></returns>
        protected override IListCell AddSelfComponent(GameObject p_Target)
            => p_Target.GetComponent<TextListCell>() ?? p_Target.AddComponent<TextListCell>();
    }
}
