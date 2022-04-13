using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.SDK.Animation
{
    public class AnimationControllerData
    {
        public Sprite sprite;

        public int uvIndex = 0;
        public float lastSwitch;
        public Rect[] uvs;
        public float[] delays;
        public Sprite[] sprites;
        private bool _isDelayConsistent = true;

        public List<Image> activeImages = new List<Image>(50);

        public AnimationControllerData(Texture2D tex, Rect[] uvs, float[] delays)
        {
            sprites = new Sprite[uvs.Length];
            float firstDelay = -1;
            for (int i = 0; i < uvs.Length; i++)
            {
                sprites[i] = Sprite.Create(tex, new Rect(uvs[i].x * tex.width, uvs[i].y * tex.height, uvs[i].width * tex.width, uvs[i].height * tex.height), new Vector2(0, 0), 100f);
                if (i == 0)
                    firstDelay = delays[i];

                if (delays[i] != firstDelay)
                    _isDelayConsistent = false;
            }

            sprite = Unity.Sprite.CreateFromTexture(tex, 100f, Vector2.zero);
            this.uvs = uvs;
            this.delays = delays;

            lastSwitch = Time.realtimeSinceStartup;
        }

        internal void CheckFrame(float now)
        {
            if (activeImages.Count == 0)
                return;


            double differenceMs = (now - lastSwitch) * 1000f;
            if (differenceMs < delays[uvIndex])
                return;

            // Bump animations with consistently 10ms or lower frame timings to 100ms
            if (_isDelayConsistent && delays[uvIndex] <= 10 && differenceMs < 100)
                return;

            lastSwitch = now;
            do
            {
                uvIndex++;
                if (uvIndex >= uvs.Length)
                    uvIndex = 0;
            }
            while (!_isDelayConsistent && delays[uvIndex] == 0);

            for (int l_I = 0; l_I < activeImages.Count; ++l_I)
                activeImages[l_I].sprite = sprites[uvIndex];
        }
    }
}
