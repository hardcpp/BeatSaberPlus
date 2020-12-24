using HarmonyLib;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Game.Patches
{
    /// <summary>
    /// BeatMap FetchCoverImage patch to support absolute URL
    /// </summary>
    [HarmonyPatch(typeof(BeatSaverSharp.Beatmap))]
    [HarmonyPatch(nameof(BeatSaverSharp.Beatmap.FetchCoverImage), new Type[] { typeof(CancellationToken), typeof(IProgress<double>) })]
    public class BeatSaverSharp_Beatmap
    {
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static PropertyInfo m_Beatmap_Client = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static PropertyInfo m_BeatSaver_HttpInstance = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static MethodInfo m_Http_GetAsync = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static MethodInfo m_TaskHttpResponse_GetAwaiter = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static MethodInfo m_AwaiterHttpResponse_GetResult = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static MethodInfo m_HttpResponse_Bytes = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">BeatMap instance</param>
        /// <param name="__result">Output result</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="progress">Progress reporter</param>
        /// <returns></returns>
        internal static bool Prefix(ref BeatSaverSharp.Beatmap __instance, ref Task<byte[]> __result, ref CancellationToken token, ref IProgress<double> progress)
        {
            if (!__instance.CoverURL.ToLower().StartsWith("http"))
                return true;

            __result = FetchCoverImage(__instance, token, progress);
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fetch cover image patch
        /// </summary>
        /// <param name="p_BeatMap">BeatMap instance</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        private static async Task<byte[]> FetchCoverImage(BeatSaverSharp.Beatmap p_BeatMap, CancellationToken p_Token, IProgress<double> p_Progress = null)
        {
            if (m_Beatmap_Client == null)
                m_Beatmap_Client = typeof(BeatSaverSharp.Beatmap).GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic);
            if (m_BeatSaver_HttpInstance == null)
                m_BeatSaver_HttpInstance = typeof(BeatSaverSharp.BeatSaver).GetProperty("HttpInstance", BindingFlags.Instance | BindingFlags.NonPublic);
            if (m_Http_GetAsync == null)
                m_Http_GetAsync = m_BeatSaver_HttpInstance.PropertyType.GetMethod("GetAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            var l_Client = m_Beatmap_Client.GetValue(p_BeatMap);
            if (l_Client != null)
            {
                var l_HTTPInstance = m_BeatSaver_HttpInstance.GetValue(l_Client);
                if (l_HTTPInstance != null && m_Http_GetAsync != null)
                {
                    var l_TaskRaw = m_Http_GetAsync.Invoke(l_HTTPInstance, new object[] { p_BeatMap.CoverURL, p_Token, p_Progress });
                    await (l_TaskRaw as Task);

                    if (m_TaskHttpResponse_GetAwaiter == null)
                        m_TaskHttpResponse_GetAwaiter = m_Http_GetAsync.ReturnType.GetMethod("GetAwaiter", BindingFlags.Instance | BindingFlags.Public);

                    var l_Awaiter = m_TaskHttpResponse_GetAwaiter.Invoke(l_TaskRaw, new object[] { });
                    if (l_Awaiter != null)
                    {
                        if (m_AwaiterHttpResponse_GetResult == null)
                            m_AwaiterHttpResponse_GetResult = l_Awaiter.GetType().GetMethod("GetResult", BindingFlags.Instance | BindingFlags.Public);

                        var l_Result = m_AwaiterHttpResponse_GetResult.Invoke(l_Awaiter, new object[] { });

                        if (m_HttpResponse_Bytes == null)
                            m_HttpResponse_Bytes = l_Result.GetType().GetMethod("Bytes", BindingFlags.Instance | BindingFlags.Public);

                        return m_HttpResponse_Bytes.Invoke(l_Result, new object[] { }) as byte[];
                    }
                }
            }

            return await p_BeatMap.FetchCoverImage(p_Token, p_Progress).ConfigureAwait(false);
        }
    }
}
