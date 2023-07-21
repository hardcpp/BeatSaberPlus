using System.Collections.Generic;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Enums
{
    public static class Comparison
    {
        public enum E
        {
            Less,
            LessOrEqual,
            Equal,
            GreaterOrEqual,
            Greater
        }

        public static List<string> S = new List<string>()
        {
            "Less",
            "LessOrEqual",
            "Equal",
            "GreaterOrEqual",
            "Greater"
        };

        public static int ValueCount => S.Count;

        public static bool Evaluate(E p_Comparison, int p_Left, int p_Right)
        {
            switch (p_Comparison)
            {
                case E.Less:            return p_Left  < p_Right;
                case E.LessOrEqual:     return p_Left <= p_Right;
                case E.Equal:           return p_Left == p_Right;
                case E.GreaterOrEqual:  return p_Left >= p_Right;
                case E.Greater:         return p_Left  > p_Right;
            }
            return false;
        }
        public static bool Evaluate(E p_Comparison, uint p_Left, uint p_Right)
        {
            switch (p_Comparison)
            {
                case E.Less:            return p_Left  < p_Right;
                case E.LessOrEqual:     return p_Left <= p_Right;
                case E.Equal:           return p_Left == p_Right;
                case E.GreaterOrEqual:  return p_Left >= p_Right;
                case E.Greater:         return p_Left  > p_Right;
            }
            return false;
        }

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
