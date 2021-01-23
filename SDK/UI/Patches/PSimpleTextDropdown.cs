using HarmonyLib;
using IPA.Utilities;
using System;
using TMPro;

namespace BeatSaberPlus.SDK.UI.Patches
{
    [HarmonyPatch(typeof(HMUI.SimpleTextDropdown))]
    [HarmonyPatch(nameof(HMUI.SimpleTextDropdown.CellForIdx), new Type[] { typeof(HMUI.TableView), typeof(int) })]
    public class PSimpleTextDropdown
    {
        /// <summary>
        /// Postfix
        /// </summary>
        /// <param name="__result">Result</param>
        internal static void Postfix(ref HMUI.TableCell __result)
        {
            if (!(__result is SimpleTextTableCell))
                return;

            var l_Cell = __result as SimpleTextTableCell;
            var l_Text = l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text");
            if (l_Text != null && l_Text)
                l_Text.richText = true;
        }
    }
}
