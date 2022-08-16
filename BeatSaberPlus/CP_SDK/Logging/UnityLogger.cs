#if CP_SDK_UNITY
using System;
using UnityEngine;

namespace CP_SDK.Logging
{
    /// <summary>
    /// IPA logger implementation
    /// </summary>
    public class UnityLogger : ILogger
    {
        /// <summary>
        /// Log implementation
        /// </summary>
        /// <param name="p_Type">Kind</param>
        /// <param name="p_Data">Data</param>
        protected override void LogImplementation(ELogType p_Type, string p_Data)
        {
            switch (p_Type)
            {
                case ELogType.Error:    UnityEngine.Debug.LogError(p_Data);     break;
                case ELogType.Warning:  UnityEngine.Debug.LogWarning(p_Data);   break;
                case ELogType.Info:     UnityEngine.Debug.Log(p_Data);          break;
                case ELogType.Debug:    UnityEngine.Debug.Log(p_Data);          break;
            }
        }
        /// <summary>
        /// Log implementation
        /// </summary>
        /// <param name="p_Type">Kind</param>
        /// <param name="p_Data">Data</param>
        protected override void LogImplementation(ELogType p_Type, Exception p_Data)
        {
            switch (p_Type)
            {
                case ELogType.Error:    UnityEngine.Debug.LogError(p_Data);     break;
                case ELogType.Warning:  UnityEngine.Debug.LogWarning(p_Data);   break;
                case ELogType.Info:     UnityEngine.Debug.Log(p_Data);          break;
                case ELogType.Debug:    UnityEngine.Debug.Log(p_Data);          break;
            }
        }
    }
}
#endif