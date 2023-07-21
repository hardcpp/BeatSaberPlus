using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Vertical layout component
    /// </summary>
    public abstract class CVLayout : CHOrVLayout
    {
        /// <summary>
        /// Unity VerticalLayoutGroup accessor
        /// </summary>
        public abstract VerticalLayoutGroup VLayoutGroup { get; }
    }
}
