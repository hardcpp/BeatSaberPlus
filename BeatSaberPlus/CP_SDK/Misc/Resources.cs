using System;
using System.IO;
using System.Reflection;

namespace CP_SDK.Misc
{
    /// <summary>
    /// Resources
    /// </summary>
    public static class Resources
    {
        /// <summary>
        /// Get resources bytes
        /// </summary>
        /// <param name="p_Assembly">Executing assembly</param>
        /// <param name="p_Path">Resource path</param>
        /// <returns></returns>
        public static byte[] FromRelPath(Assembly p_Assembly, string p_Path)
        {
            var l_Stream    = p_Assembly.GetManifestResourceStream(p_Assembly.GetName().Name + "." + p_Path);
            var l_Data      = new byte[l_Stream.Length];

            l_Stream.Read(l_Data, 0, (int)l_Stream.Length);
            return l_Data;
        }
        /// <summary>
        /// Get resources bytes
        /// </summary>
        /// <param name="p_Assembly">Executing assembly</param>
        /// <param name="p_Path">Resource path</param>
        /// <returns></returns>
        public static string FromRelPathStr(Assembly p_Assembly, string p_Path)
        {
            var l_Stream = p_Assembly.GetManifestResourceStream(p_Assembly.GetName().Name + "." + p_Path);
            return new StreamReader(l_Stream).ReadToEnd();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get resources bytes
        /// </summary>
        /// <param name="p_Assembly">Executing assembly</param>
        /// <param name="p_Path">Resource path</param>
        /// <returns></returns>
        public static byte[] FromPath(Assembly p_Assembly, string p_Path)
        {
            var l_Stream = p_Assembly.GetManifestResourceStream(p_Path);
            var l_Data = new byte[l_Stream.Length];

            l_Stream.Read(l_Data, 0, (int)l_Stream.Length);
            return l_Data;
        }
        /// <summary>
        /// Get resources bytes
        /// </summary>
        /// <param name="p_Assembly">Executing assembly</param>
        /// <param name="p_Path">Resource path</param>
        /// <returns></returns>
        public static string FromPathStr(Assembly p_Assembly, string p_Path)
        {
            var l_Stream = p_Assembly.GetManifestResourceStream(p_Path);
            return new StreamReader(l_Stream).ReadToEnd();
        }
    }
}
