using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    public class AnimationController : PersistentSingleton<AnimationController>
    {
        private Dictionary<string, AnimationControllerData> registeredAnimations = new Dictionary<string, AnimationControllerData>();
        public ReadOnlyDictionary<string, AnimationControllerData> RegisteredAnimations;

        private void Awake()
        {
            RegisteredAnimations = new ReadOnlyDictionary<string, AnimationControllerData>(registeredAnimations);
        }

        public AnimationControllerData Register(string identifier, Texture2D tex, Rect[] uvs, float[] delays)
        {
            if(!registeredAnimations.TryGetValue(identifier, out AnimationControllerData animationData))
            {
                animationData = new AnimationControllerData(tex, uvs, delays);
                registeredAnimations.Add(identifier, animationData);
            }
            else
            {
                GameObject.Destroy(tex);//if the identifier exists then this texture is a duplicate so might as well destroy it and free some memory (this can happen if you try to load a gif twice before the first one finishes processing)
            }
            return animationData;
        }


        public void Update()
        {
            DateTime now = DateTime.UtcNow;
            foreach (AnimationControllerData animation in registeredAnimations.Values)
                if (animation.IsPlaying == true)
                    animation.CheckFrame(now);
        }
    }
}
