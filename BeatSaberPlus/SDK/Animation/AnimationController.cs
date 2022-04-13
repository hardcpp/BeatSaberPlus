using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    public class AnimationController : PersistentSingleton<AnimationController>
    {
        private Dictionary<string, AnimationControllerData> registeredAnimations = new Dictionary<string, AnimationControllerData>(100);
        private List<AnimationControllerData> m_QuickUpdateList = new List<AnimationControllerData>(100);

        public AnimationControllerData Register(string identifier, Texture2D tex, Rect[] uvs, float[] delays)
        {
            if(!registeredAnimations.TryGetValue(identifier, out AnimationControllerData animationData))
            {
                animationData = new AnimationControllerData(tex, uvs, delays);
                registeredAnimations.Add(identifier, animationData);
                m_QuickUpdateList.Add(animationData);
            }
            else
            {
                GameObject.Destroy(tex);//if the identifier exists then this texture is a duplicate so might as well destroy it and free some memory (this can happen if you try to load a gif twice before the first one finishes processing)
            }
            return animationData;
        }


        public void Update()
        {
            var l_Now = Time.realtimeSinceStartup;
            for (int l_I = 0; l_I < m_QuickUpdateList.Count; ++l_I)
                m_QuickUpdateList[l_I].CheckFrame(l_Now);
        }
    }
}
