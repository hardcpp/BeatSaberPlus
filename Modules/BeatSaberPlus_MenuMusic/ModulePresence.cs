using BeatSaberPlus_ChatRequest;
using System.Linq;

namespace BeatSaberPlus_MenuMusic
{
    internal class ModulePresence
    {
        private static CP_SDK.IModuleBase m_ChatRequest;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static bool ChatRequest
        {
            get
            {
                if (m_ChatRequest == null)
                    m_ChatRequest = CP_SDK.ChatPlexSDK.GetModules().FirstOrDefault(x => x.Name == "Chat Request");

                return m_ChatRequest?.IsEnabled ?? false;
            }
        }
    }
}
