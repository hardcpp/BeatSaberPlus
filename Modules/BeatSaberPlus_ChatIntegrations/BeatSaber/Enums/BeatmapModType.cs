﻿using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Enums
{
    public static class BeatmapModType
    {
        public enum E
        {
            All,
            NonNoodle,
            Noodle,
            Chroma,
            NoodleOrChroma
        }

        public static List<string> S = new List<string>()
        {
            "All",
            "NonNoodle",
            "Noodle",
            "Chroma",
            "NoodleOrChroma"
        };

        public static int ValueCount => S.Count;

        public static int ToInt(string p_Str)
            => Mathf.Clamp(S.IndexOf(p_Str), 0, ValueCount - 1);
        public static int ToInt(E p_Enum)
            => Mathf.Clamp((int)p_Enum, 0, ValueCount - 1);

        public static E ToEnum(string p_Str)
            => (E)ToInt(p_Str);
        public static E ToEnum(int p_Int)
            => (E)Mathf.Clamp(p_Int, 0, ValueCount - 1);

        public static string ToStr(int p_Int)
            => S[Mathf.Clamp(p_Int, 0, ValueCount - 1)];
        public static string ToStr(E p_Enum)
            => S[Mathf.Clamp((int)p_Enum, 0, ValueCount - 1)];
    }
}
