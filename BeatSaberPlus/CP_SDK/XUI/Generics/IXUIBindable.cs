namespace CP_SDK.XUI
{
    /// <summary>
    /// IXUIElement Bind interface
    /// </summary>
    /// <typeparam name="t_Base">Return type for daisy chaining</typeparam>
    public interface IXUIBindable<t_Base>
        where t_Base : IXUIElement
    {
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        t_Base Bind(ref t_Base p_Target);
    }
}
