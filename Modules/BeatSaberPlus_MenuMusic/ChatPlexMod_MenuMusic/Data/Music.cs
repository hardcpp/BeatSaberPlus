using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace ChatPlexMod_MenuMusic.Data
{
    /// <summary>
    /// Generic music entry
    /// </summary>
    public class Music
    {
        private IMusicProvider  m_MusicProvider;
        private string          m_SongPath;
        private string          m_SongCoverPath;
        private string          m_SongName;
        private string          m_SongArtist;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public IMusicProvider MusicProvider => m_MusicProvider;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_MusicProvider">Music provider instance</param>
        /// <param name="p_SongPath">Path to the music file</param>
        /// <param name="p_SongCoverPath">Path to the music cover file</param>
        /// <param name="p_SongName">Name of the song</param>
        /// <param name="p_SongArtist">Artist of the song</param>
        public Music(IMusicProvider p_MusicProvider, string p_SongPath, string p_SongCoverPath, string p_SongName, string p_SongArtist)
        {
            m_MusicProvider = p_MusicProvider;
            m_SongPath      = p_SongPath.Replace('\\', '/');
            m_SongCoverPath = p_SongCoverPath?.Replace('\\', '/');
            m_SongName      = p_SongName.Trim();
            m_SongArtist    = p_SongArtist.Trim();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get song path
        /// </summary>
        /// <returns></returns>
        public string GetSongPath()
            => m_SongPath;
        /// <summary>
        /// Get cover path
        /// </summary>
        /// <returns></returns>
        public string GetCoverPath()
            => m_SongCoverPath;
        /// <summary>
        /// Get song name
        /// </summary>
        /// <returns></returns>
        public string GetSongName()
            => m_SongName;
        /// <summary>
        /// Get song name
        /// </summary>
        /// <returns></returns>
        public string GetSongArtist()
            => m_SongArtist;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get audio clip async
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_OnSuccess">On success callback</param>
        /// <param name="p_OnError">On error callback</param>
        public void GetAudioAsync(CP_SDK.Misc.FastCancellationToken p_Token, Action<AudioClip> p_OnSuccess, Action p_OnError)
            => CP_SDK.Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_GetAudioAsync(p_Token, p_OnSuccess, p_OnError));
        /// <summary>
        /// Get cover async
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_OnSuccess">On success callback</param>
        /// <param name="p_OnError">On error callback</param>
        public void GetCoverBytesAsync(CP_SDK.Misc.FastCancellationToken p_Token, Action<byte[]> p_OnSuccess, Action p_OnError)
        {
            var l_StartSerial = p_Token.Serial;
            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                if (p_Token.IsCancelled(l_StartSerial))
                    return;

                try
                {
                    var l_Bytes = null as byte[];
                    if (File.Exists(m_SongCoverPath))
                        l_Bytes = File.ReadAllBytes(m_SongCoverPath);
                    else
                        l_Bytes = CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "ChatPlexMod_MenuMusic.Resources.DefaultCover.png");

                    if (p_Token.IsCancelled(l_StartSerial))
                        return;

                    p_OnSuccess?.Invoke(l_Bytes);
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[ChatPlexMod_MenuMusic.Data][Music.GetCoverAsync] Error:");
                    Logger.Instance.Error(l_Exception);
                    p_OnError?.Invoke();
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get audio clip async
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_OnSuccess">On success callback</param>
        /// <param name="p_OnError">On error callback</param>
        private IEnumerator Coroutine_GetAudioAsync(CP_SDK.Misc.FastCancellationToken p_Token, Action<AudioClip> p_OnSuccess, Action p_OnError)
        {
            var l_StartSerial = p_Token.Serial;

            yield return new WaitForEndOfFrame();

            if (p_Token.IsCancelled(l_StartSerial))
                yield break;

            var l_PathParts = m_SongPath.Split('/');
            var l_SafePath  = string.Join("/", l_PathParts.Select(x => x == l_PathParts[0] ? x : System.Uri.EscapeUriString(x)).ToArray());
            var l_FinalURL  = "file://" + l_SafePath.Replace("#", "%23");

            yield return new WaitForEndOfFrame();

            UnityWebRequest l_Loader = UnityWebRequestMultimedia.GetAudioClip(l_FinalURL, AudioType.OGGVORBIS);
            yield return l_Loader.SendWebRequest();

            /// Skip if it's not the menu
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            if (p_Token.IsCancelled(l_StartSerial))
                yield break;

            if (l_Loader.isNetworkError
                || l_Loader.isHttpError
                || !string.IsNullOrEmpty(l_Loader.error))
            {
                Logger.Instance.Error($"[ChatPlexMod_MenuMusic.Data][Music.GetAudioAsync] Can't load audio! {(!string.IsNullOrEmpty(l_Loader.error) ? l_Loader.error : string.Empty)}");
                p_OnError?.Invoke();
                yield break;
            }

            var l_AudioClip = null as AudioClip;
            try
            {
                ((DownloadHandlerAudioClip)l_Loader.downloadHandler).streamAudio = true;

                l_AudioClip = DownloadHandlerAudioClip.GetContent(l_Loader);
                if (l_AudioClip != null)
                    l_AudioClip.name = m_SongName;
                else
                {
                    Logger.Instance.Error("[ChatPlexMod_MenuMusic.Data][Music.GetAudioAsync] No audio found");
                    p_OnError?.Invoke();
                    yield break;
                }
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_MenuMusic.Data][Music.GetAudioAsync] Can't load audio! Exception:");
                Logger.Instance.Error(p_Exception);

                p_OnError?.Invoke();
                yield break;
            }

            var l_RemainingTry  = 15;
            var l_Waiter        = new WaitForSecondsRealtime(0.1f);

            while (    l_AudioClip.loadState != AudioDataLoadState.Loaded
                    && l_AudioClip.loadState != AudioDataLoadState.Failed)
            {
                yield return l_Waiter;
                l_RemainingTry--;

                if (l_RemainingTry < 0)
                {
                    p_OnError?.Invoke();
                    yield break;
                }

                if (p_Token.IsCancelled(l_StartSerial))
                    yield break;
            }

            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            if (p_Token.IsCancelled(l_StartSerial))
                yield break;

            if (l_AudioClip.loadState != AudioDataLoadState.Loaded)
            {
                p_OnError?.Invoke();
                yield break;
            }

            p_OnSuccess(l_AudioClip);
        }
    }
}
