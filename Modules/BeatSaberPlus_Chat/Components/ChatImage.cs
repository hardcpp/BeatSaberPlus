using UnityEngine.UI;

namespace ChatPlexMod_Chat.Components
{
    /// <summary>
    /// Enhanced image with state updater
    /// </summary>
    internal class ChatImage : Image
    {
        /// <summary>
        /// Animation state updater instance
        /// </summary>
        internal CP_SDK.Animation.AnimationStateUpdater AnimStateUpdater { get; set; } = null;
    }
}
