using HMUI;
using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI.DataSource
{
    /// <summary>
    /// Simple text list
    /// </summary>
    public class SimpleTextList : MonoBehaviour, TableView.IDataSource
    {
         /// <summary>
        /// Cell template
        /// </summary>
        private SimpleTextTableCell m_SongListTableCellInstance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        public TableView TableViewInstance;
        /// <summary>
        /// Data (text, hover hint)
        /// </summary>
        public List<(string, string)> Data = new List<(string, string)>();
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSizeValue = 5.2f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Index">Cell index</param>
        /// <returns></returns>
        public TableCell CellForIdx(TableView p_TableView, int p_Index)
        {
            SimpleTextTableCell l_Cell = GetTableCell();

            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").richText             = true;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").enableWordWrapping   = true;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").fontStyle            = FontStyles.Normal;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").enableAutoSizing     = true;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").fontSizeMin          = 2f;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").fontSizeMax          = 3.5f;
            l_Cell.GetField<TextMeshProUGUI, SimpleTextTableCell>("_text").text                 = Data[p_Index].Item1;
            l_Cell.text = Data[p_Index].Item1;

            var l_HoverHint = l_Cell.gameObject.GetComponent<HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            if (!string.IsNullOrEmpty(Data[p_Index].Item2))
            {
                l_HoverHint.enabled = true;
                l_HoverHint.text    = Data[p_Index].Item2;
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
            return CellSizeValue;
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
        public SimpleTextTableCell GetTableCell()
        {
            SimpleTextTableCell l_Cell = (SimpleTextTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_SimpleTextList_Cell");
            if (!l_Cell)
            {
                if (m_SongListTableCellInstance == null)
                    m_SongListTableCellInstance = Resources.FindObjectsOfTypeAll<SimpleTextTableCell>().First(x => x.name == "SimpleTextTableCell");

                l_Cell = Instantiate(m_SongListTableCellInstance);
            }

            l_Cell.reuseIdentifier = "BSP_SimpleTextList_Cell";
            return l_Cell;
        }
    }
}
