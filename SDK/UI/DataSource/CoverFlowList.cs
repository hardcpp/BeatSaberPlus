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
    public class CoverFlowList : MonoBehaviour, TableView.IDataSource
    {
        /// <summary>
        /// Cell template
        /// </summary>
        private AnnotatedBeatmapLevelCollectionTableCell m_AnnotatedBeatmapLevelCollectionTableCell;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        public TableView TableViewInstance;
        /// <summary>
        /// Data (Cover, tag, hover hint)
        /// </summary>
        public List<(Sprite, string, string)> Data = new List<(Sprite, string, string)>();

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
            AnnotatedBeatmapLevelCollectionTableCell l_Cell = GetTableCell();
            var l_Image = l_Cell.GetField<UnityEngine.UI.Image, AnnotatedBeatmapLevelCollectionTableCell>("_coverImage");
            l_Image.sprite = Data[p_Index].Item1 == null ? SongCore.Loader.defaultCoverImage : Data[p_Index].Item1;

            if (!string.IsNullOrEmpty(Data[p_Index].Item2))
            {
                l_Cell.showNewRibbon = !string.IsNullOrEmpty(Data[p_Index].Item2);
                l_Cell.transform.Find("Wrapper").Find("Artwork").Find("InfoText").GetComponent<TextMeshProUGUI>().text = Data[p_Index].Item2;
            }
            else
                l_Cell.showNewRibbon = false;

            var l_HoverHint = l_Cell.gameObject.GetComponent<HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            if (!string.IsNullOrEmpty(Data[p_Index].Item3))
            {
                l_HoverHint.enabled = true;
                l_HoverHint.text = Data[p_Index].Item3;
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
            return 10f;
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
        public AnnotatedBeatmapLevelCollectionTableCell GetTableCell()
        {
            AnnotatedBeatmapLevelCollectionTableCell l_Cell = (AnnotatedBeatmapLevelCollectionTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_CoverFlow_Cell");
            if (!l_Cell)
            {
                if (m_AnnotatedBeatmapLevelCollectionTableCell == null)
                    m_AnnotatedBeatmapLevelCollectionTableCell = Resources.FindObjectsOfTypeAll<AnnotatedBeatmapLevelCollectionTableCell>().First(x => x.name == "AnnotatedBeatmapLevelCollectionTableCell");

                l_Cell = Instantiate(m_AnnotatedBeatmapLevelCollectionTableCell);
            }

            l_Cell.reuseIdentifier = "BSP_CoverFlow_Cell";
            return l_Cell;
        }
    }
}
