using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Horizontal layout component
    /// </summary>
    public abstract class CHLayout : CHOrVLayout
    {
        /// <summary>
        /// Unity HorizontalLayoutGroup accessor
        /// </summary>
        public abstract HorizontalLayoutGroup HLayoutGroup { get; }
    }
}
