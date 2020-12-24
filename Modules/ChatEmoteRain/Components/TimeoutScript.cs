using System.Collections;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatEmoteRain.Components
{
    internal class TimeoutScript : MonoBehaviour
    {
        [SerializeField]
        private float timeLimit = 15.0f;
        internal string key;
        internal SDK.Game.Logic.SceneType mode;
        private byte queue;
        private bool timingOut;
        private Coroutine coroutine;
        internal ParticleSystem PS
        {
            get
            {
                if (_PS == null)
                {
                    _PS = gameObject.GetComponent<ParticleSystem>();
                }
                return _PS;
            }
        }
        private ParticleSystem _PS;
        internal ParticleSystemRenderer PSR
        {
            get
            {
                if (_PSR == null)
                {
                    _PSR = gameObject.GetComponent<ParticleSystemRenderer>();
                }
                return _PSR;
            }
        }
        private ParticleSystemRenderer _PSR;
        internal void PerformTest()
        {
            var l_System = GetComponent<ParticleSystem>();
            if (l_System)
                l_System.Emit(1);
        }
        internal void Emit(byte amount)
        {
            if (amount > 0)
            {
                queue += amount;
                if (coroutine == null)
                    coroutine = StartCoroutine(Init());
                else if (timingOut)
                {
                    StopCoroutine(coroutine);
                    timingOut = false;
                    coroutine = StartCoroutine(Emit());
                }
            }
        }
        private IEnumerator Init()
        {
            yield return new WaitForEndOfFrame();
            coroutine = StartCoroutine(Emit());
            yield break;
        }
        private IEnumerator Emit()
        {
            byte frameCount = (byte)Config.ChatEmoteRain.EmoteDelay;
            while (queue > 0)
            {
                if (frameCount >= Config.ChatEmoteRain.EmoteDelay)
                {
                    frameCount = 0;
                    PS.Emit(1);
                    queue--;
                }
                frameCount++;
                yield return new WaitForFixedUpdate();
            }
            coroutine = StartCoroutine(TimeOut());
            yield break;
        }
        private IEnumerator TimeOut()
        {
            timingOut = true;
            yield return new WaitForSeconds(timeLimit);
            coroutine = null;
            ChatEmoteRain.Instance.UnregisterParticleSystem(key, mode);
            yield break;
        }
    }
}
