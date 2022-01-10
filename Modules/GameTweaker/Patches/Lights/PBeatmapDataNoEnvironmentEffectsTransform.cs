using HarmonyLib;
using System.Collections.Generic;

namespace BeatSaberPlus.Modules.GameTweaker.Patches.Lights
{
    ///[HarmonyPatch(typeof(BeatmapDataNoEnvironmentEffectsTransform))]
    ///[HarmonyPatch(nameof(BeatmapDataNoEnvironmentEffectsTransform.CreateTransformedData))]
    public class PBeatmapDataNoEnvironmentEffectsTransform_CreateTransformedData
    {
        /// <summary>
        /// Prefix CreateTransformedData
        /// </summary>
        /// <param name="__result">Method result</param>
        /// <param name="beatmapData">Input beatmap data</param>
        /// <returns></returns>
        internal static bool Prefix(ref BeatmapData __result, IReadonlyBeatmapData beatmapData)
        {
            if (!Config.GameTweaker.Enabled)
                return true; ///< Fallback to original method

            BeatmapData l_Copy = beatmapData.GetCopyWithoutEvents();
            l_Copy.AddBeatmapEventData(new BeatmapEventData(0.0f, BeatmapEventType.Event0, 1, 1));
            l_Copy.AddBeatmapEventData(new BeatmapEventData(0.0f, BeatmapEventType.Event4, 1, 1));

            foreach (BeatmapEventData beatmapEventData in (IEnumerable<BeatmapEventData>)beatmapData.beatmapEventsData)
                l_Copy.AddBeatmapEventData(beatmapEventData);

            __result = l_Copy;

            /// Skip original method
            return false;
        }
    }
}
