#if CP_SDK_MELON_LOADER
using System;

namespace CP_SDK.Logging
{
    /// <summary>
    /// MelonLoader logger implementation
    /// </summary>
    public class MelonLoaderLogger : ILogger
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
                case ELogType.Error:    MelonLoader.MelonLogger.Error(p_Data);      break;
                case ELogType.Warning:  MelonLoader.MelonLogger.Warning(p_Data);    break;
                case ELogType.Info:     MelonLoader.MelonLogger.Msg(p_Data);        break;
                case ELogType.Debug:    MelonLoader.MelonLogger.Msg(p_Data);        break;
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
                case ELogType.Error:    MelonLoader.MelonLogger.Error(p_Data);      break;
                case ELogType.Warning:  MelonLoader.MelonLogger.Warning(p_Data);    break;
                case ELogType.Info:     MelonLoader.MelonLogger.Msg(p_Data);        break;
                case ELogType.Debug:    MelonLoader.MelonLogger.Msg(p_Data);        break;
            }
        }
    }
}
#endif