using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Modules.GameTweaker.Components
{
    /// <summary>
    /// Music band logo remover
    /// </summary>
    public class MusicBandLogoRemover : MonoBehaviour
    {
        /// <summary>
        /// Audio time sync controller
        /// </summary>
        private AudioTimeSyncController m_AudioTimeSyncController;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component first frame
        /// </summary>
        private void Start()
        {
            m_AudioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frames
        /// </summary>
        public void Update()
        {
            if (m_AudioTimeSyncController == null || !m_AudioTimeSyncController)
                m_AudioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();

            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Playing
                || m_AudioTimeSyncController == null
                || !m_AudioTimeSyncController)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            if (m_AudioTimeSyncController.songTime == 0f)
                return;

            GameObject l_Object = null;

            /// BTS
            l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "MagicDoorSprite");
            if (l_Object != null)
            {
                l_Object.GetComponent<SpriteRenderer>().enabled = false;
                GameObject.Destroy(gameObject);
                return;
            }

            /// LinkinPark
            l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "LinkinParkTextLogoL");
            if (l_Object != null)
            {
                l_Object.GetComponent<SpriteRenderer>().enabled = false;

                l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "LinkinParkTextLogoR");
                if (l_Object != null)
                    l_Object.GetComponent<SpriteRenderer>().enabled = false;

                l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "Logo" && x.transform.parent.name == "Environment");
                if (l_Object != null)
                    l_Object.GetComponent<SpriteRenderer>().enabled = false;

                GameObject.Destroy(gameObject);
                return;
            }

            GameObject.Destroy(gameObject);
        }
    }
}
