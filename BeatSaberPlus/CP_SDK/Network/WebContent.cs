using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CP_SDK.Network
{
    /// <summary>
    /// WebContent
    /// </summary>
    public class WebContent
    {
        /// <summary>
        /// Content
        /// </summary>
        public byte[] Bytes;
        /// <summary>
        /// Content type
        /// </summary>
        public string Type;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Bytes">Bytes</param>
        /// <param name="p_Type">Content</param>
        private WebContent(byte[] p_Bytes, string p_Type)
        {
            Bytes   = p_Bytes;
            Type    = p_Type;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor from Json
        /// </summary>
        /// <param name="p_Content">Json content</param>
        /// <returns></returns>
        public static WebContent FromJson(string p_Content)
            => new WebContent(Encoding.UTF8.GetBytes(p_Content), $"application/json; charset=utf-8");
        /// <summary>
        /// Constructor from Json
        /// </summary>
        /// <param name="p_Content">Json content</param>
        /// <param name="p_Indent">Should indent?</param>
        /// <returns></returns>
        public static WebContent FromJson(JObject p_Content, bool p_Indent = false)
            => new WebContent(Encoding.UTF8.GetBytes(p_Content.ToString(p_Indent ? Formatting.Indented : Formatting.None)), $"application/json; charset=utf-8");
        /// <summary>
        /// Constructor from Json
        /// </summary>
        /// <param name="p_Content">Json content</param>
        /// <param name="p_Indent">Should indent?</param>
        /// <returns></returns>
        public static WebContent FromJson(object p_Content, bool p_Indent = false)
            => new WebContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(p_Content, p_Indent ? Formatting.Indented : Formatting.None)), $"application/json; charset=utf-8");
    }
}
