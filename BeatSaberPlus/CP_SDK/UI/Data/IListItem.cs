namespace CP_SDK.UI.Data
{
    /// <summary>
    /// Abstract List Item
    /// </summary>
    public abstract class IListItem
    {
        private IListCell m_Cell;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public IListCell Cell => m_Cell;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set displayed Cell
        /// </summary>
        /// <param name="p_Cell">New display cell</param>
        public void SetCell(IListCell p_Cell)
        {
            m_Cell = p_Cell;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public abstract void OnShow();
        /// <summary>
        /// On hide
        /// </summary>
        public virtual void OnHide()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On select
        /// </summary>
        public virtual void OnSelect()
        {

        }
        /// <summary>
        /// On Unselect
        /// </summary>
        public virtual void OnUnselect()
        {

        }
    }
}
