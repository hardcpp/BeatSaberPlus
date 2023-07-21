#if CP_SDK_UNITY

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType table
    /// </summary>
    public abstract class OpenTypeTable
    {
        /// <summary>
        /// Read from
        /// </summary>
        /// <param name="p_Reader">Reader</param>
        /// <param name="p_Length">Size to read</param>
        public abstract void ReadFrom(OpenTypeReader p_Reader, uint p_Length);
    }
}
#endif
