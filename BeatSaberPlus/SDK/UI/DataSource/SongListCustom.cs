using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

namespace BeatSaberPlus.SDK.UI.DataSource
{
    /// <summary>
    /// Song entry list source
    /// </summary>
    public class SongListCustom : UnityEngine.MonoBehaviour, HMUI.TableView.IDataSource
    {
        /// <summary>
        /// Cell template
        /// </summary>
        private LevelListTableCell m_SongListTableCellInstance;
        /// <summary>
        /// Default cover image
        /// </summary>
        private UnityEngine.Sprite m_DefaultCover = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        public HMUI.TableView TableViewInstance;
        /// <summary>
        /// Data
        /// </summary>
        public List<(string, string, string)> Data = new List<(string, string, string)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Index">Cell index</param>
        /// <returns></returns>
        public HMUI.TableCell CellForIdx(HMUI.TableView p_TableView, int p_Index)
        {
            LevelListTableCell l_Cell = GetTableCell();

            TextMeshProUGUI l_Text      = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText");
            TextMeshProUGUI l_SubText   = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songAuthorText");
            l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_favoritesBadgeImage").gameObject.SetActive(false);

            var l_HoverHint = l_Cell.gameObject.GetComponent<HMUI.HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HMUI.HoverHint>();
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                UnityEngine.GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            var l_Entry = Data[p_Index];
            string l_Title          = l_Entry.Item1;
            string l_SubTitle       = l_Entry.Item2;

            //if (Regex.Replace(l_Title, "<.*?>", String.Empty).Length > 28)
            //    l_Title = l_Title.Substring(0, 28) + "...";
            //if (Regex.Replace(l_SubTitle, "<.*?>", String.Empty).Length > 35)
            //    l_SubTitle = l_SubTitle.Substring(0, 35) + "...";

            /// Enable rich text support for the lower row text
            l_SubText.richText = true;

            l_Text.text     = l_Title;
            l_SubText.text  = l_SubTitle;

            var l_BPMText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText");
            l_BPMText.gameObject.SetActive(false);

            var l_DurationText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText");
            l_DurationText.gameObject.SetActive(false);

            l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(l_Entry.Item3))
            {
                l_HoverHint.enabled = true;
                l_HoverHint.text    = l_Entry.Item3;
            }
            else
                l_HoverHint.enabled = false;

            return l_Cell;
        }
        /// <summary>
        /// Get cell size
        /// </summary>
        /// <returns></returns>
        public float CellSize()
        {
            return 8.5f;
        }
        /// <summary>
        /// Get number of cell
        /// </summary>
        /// <returns></returns>
        public int NumberOfCells()
        {
            return Data.Count();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get new table cell or reuse old one
        /// </summary>
        /// <returns></returns>
        private LevelListTableCell GetTableCell()
        {
            LevelListTableCell l_Cell = (LevelListTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_SongListCustom_Cell");
            if (!l_Cell)
            {
                if (m_SongListTableCellInstance == null)
                    m_SongListTableCellInstance = UnityEngine.Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                l_Cell = Instantiate(m_SongListTableCellInstance);
            }

            l_Cell.SetField("_notOwned", false);
            l_Cell.reuseIdentifier = "BSP_SongListCustom_Cell";

            if (m_DefaultCover == null)
                m_DefaultCover = l_Cell.GetField<UnityEngine.UI.Image, LevelListTableCell>("_coverImage").sprite;

            return l_Cell;
        }
    }
}
