namespace CP_SDK.Misc
{
    /// <summary>
    /// Fast cancellation token
    /// </summary>
    public class FastCancellationToken
    {
        /// <summary>
        /// Current serial
        /// </summary>
        public int Serial { get; private set; } = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Increment serial
        /// </summary>
        public void Cancel()
            => Serial++;
        /// <summary>
        /// Compare serial
        /// </summary>
        /// <param name="p_OldSerial">Old serial to compare to</param>
        /// <returns></returns>
        public bool IsCancelled(int p_OldSerial)
            => Serial > p_OldSerial;
    }
}
