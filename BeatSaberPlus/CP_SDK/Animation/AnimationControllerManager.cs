using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.Animation
{
    /// <summary>
    /// Animation controller manager
    /// </summary>
    public class AnimationControllerManager : PersistentSingleton<AnimationControllerManager>
    {
        /// <summary>
        /// Registered dictionary
        /// </summary>
        private Dictionary<string, AnimationControllerInstance> m_RegisteredDict = new Dictionary<string, AnimationControllerInstance>(100);
        /// <summary>
        /// Registered
        /// </summary>
        private AnimationControllerInstance[] m_Registered = new AnimationControllerInstance[2000];
        /// <summary>
        /// Registered count
        /// </summary>
        private int m_QuickUpdateListCount = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register
        /// </summary>
        /// <param name="p_ID">Identifier</param>
        /// <param name="p_Atlas">Texture atlas</param>
        /// <param name="p_UVs">UVs rects</param>
        /// <param name="p_Delays">Delays</param>
        /// <returns></returns>
        public AnimationControllerInstance Register(string p_ID, Texture2D p_Atlas, Rect[] p_UVs, ushort[] p_Delays)
        {
            if (!m_RegisteredDict.TryGetValue(p_ID, out AnimationControllerInstance l_ControllerInstance))
            {
                if ((m_QuickUpdateListCount + 1) >= m_Registered.Length)
                    return null;

                try
                {
                    l_ControllerInstance = new AnimationControllerInstance(p_Atlas, p_UVs, p_Delays);
                    m_RegisteredDict.Add(p_ID, l_ControllerInstance);

                    m_Registered[m_QuickUpdateListCount++] = l_ControllerInstance;
                }
                catch
                {

                }
            }
            else
            {
                GameObject.Destroy(p_Atlas);
            }

            return l_ControllerInstance;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            var l_Now = (long)(Time.realtimeSinceStartup * 1000.0f);

            for (int l_I = 0; l_I < m_QuickUpdateListCount; ++l_I)
                m_Registered[l_I].CheckForNextFrame(l_Now);
        }
    }
}
