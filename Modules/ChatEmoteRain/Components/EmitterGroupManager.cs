using System.Collections.Generic;

namespace BeatSaberPlus.Modules.ChatEmoteRain.Components
{
    /// <summary>
    /// Emitter instance manager
    /// </summary>
    internal class EmitterGroupManager : PersistentSingleton<EmitterGroupManager>
    {
        /// <summary>
        /// Update queue
        /// </summary>
        private List<EmitterGroup> m_UpdateQueue = new List<EmitterGroup>(100);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register an emitter instance
        /// </summary>
        /// <param name="p_Instance">Emitter instance</param>
        internal void Register(EmitterGroup p_Instance)
        {
            if (p_Instance.Registered)
                return;

            p_Instance.Registered   = true;
            p_Instance.Roll         = CERConfig.Instance.EmoteDelay;
            m_UpdateQueue.Add(p_Instance);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Physics frame update
        /// </summary>
        internal void FixedUpdate()
        {
            for (int l_I = 0; l_I < m_UpdateQueue.Count; ++l_I)
            {
                var l_Item          = m_UpdateQueue[l_I];
                var l_ShouldDelete  = !l_Item || false;

                if (!l_ShouldDelete && --l_Item.Roll == 0)
                {
                    if (l_Item.OnPhysicFrame())
                        l_Item.Roll = CERConfig.Instance.EmoteDelay;
                    else
                        l_ShouldDelete = true;
                }

                if (l_ShouldDelete)
                {
                    if (l_Item) l_Item.Registered = false;
                    m_UpdateQueue.RemoveAt(l_I);
                    l_I--;
                    continue;
                }
            }
        }
    }
}
