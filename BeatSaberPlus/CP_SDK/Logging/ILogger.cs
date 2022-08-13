using System;

namespace CP_SDK.Logging
{
    /// <summary>
    /// Base logger
    /// </summary>
    public abstract class ILogger
    {
        /// <summary>
        /// Log type
        /// </summary>
        public enum ELogType
        {
            Error,
            Warning,
            Info,
            Debug
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ILogger()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void Error(string p_Data)      => InternalLog(ELogType.Error, p_Data);
        public void Error(Exception p_Data)   => InternalLog(ELogType.Error, p_Data);
        public void Warning(string p_Data)    => InternalLog(ELogType.Warning, p_Data);
        public void Warning(Exception p_Data) => InternalLog(ELogType.Warning, p_Data);
        public void Info(string p_Data)       => InternalLog(ELogType.Info, p_Data);
        public void Info(Exception p_Data)    => InternalLog(ELogType.Info, p_Data);
        public void Debug(string p_Data)      => InternalLog(ELogType.Debug, p_Data);
        public void Debug(Exception p_Data)   => InternalLog(ELogType.Debug, p_Data);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Internal log method
        /// </summary>
        /// <param name="p_Type">Type</param>
        /// <param name="p_Data">Data</param>
        private void InternalLog(ELogType p_Type, string p_Data)
        {
            LogImplementation(p_Type, p_Data);
        }
        /// <summary>
        /// Internal log method
        /// </summary>
        /// <param name="p_Type">Type</param>
        /// <param name="p_Data">Data</param>
        private void InternalLog(ELogType p_Type, Exception p_Data)
        {
            LogImplementation(p_Type, p_Data);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log implementation
        /// </summary>
        /// <param name="p_Type">Kind</param>
        /// <param name="p_Data">Data</param>
        protected abstract void LogImplementation(ELogType p_Type, string p_Data);
        /// <summary>
        /// Log implementation
        /// </summary>
        /// <param name="p_Type">Kind</param>
        /// <param name="p_Data">Data</param>
        protected abstract void LogImplementation(ELogType p_Type, Exception p_Data);
    }
}
