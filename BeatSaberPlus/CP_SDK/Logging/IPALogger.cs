#if CP_SDK_IPA
using System;

namespace CP_SDK.Logging
{
    /// <summary>
    /// IPA logger implementation
    /// </summary>
    public class IPALogger : ILogger
    {
        /// <summary>
        /// IPA logger instance
        /// </summary>
        private IPA.Logging.Logger m_BaseLogger;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_BaseLogger">IPA Logger instance</param>
        public IPALogger(IPA.Logging.Logger p_BaseLogger)
            : base()
            => m_BaseLogger = p_BaseLogger;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log implementation
        /// </summary>
        /// <param name="p_Type">Kind</param>
        /// <param name="p_Data">Data</param>
        protected override void LogImplementation(ELogType p_Type, string p_Data)
        {
            switch (p_Type)
            {
                case ELogType.Error:    m_BaseLogger.Error(p_Data);   break;
                case ELogType.Warning:  m_BaseLogger.Warn(p_Data);    break;
                case ELogType.Info:     m_BaseLogger.Info(p_Data);    break;
                case ELogType.Debug:    m_BaseLogger.Debug(p_Data);   break;
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
                case ELogType.Error:    m_BaseLogger.Error(p_Data);   break;
                case ELogType.Warning:  m_BaseLogger.Warn(p_Data);    break;
                case ELogType.Info:     m_BaseLogger.Info(p_Data);    break;
                case ELogType.Debug:    m_BaseLogger.Debug(p_Data);   break;
            }
        }
    }
}
#endif