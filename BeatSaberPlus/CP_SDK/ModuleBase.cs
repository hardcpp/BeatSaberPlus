namespace CP_SDK
{
    /// <summary>
    /// Module type
    /// </summary>
    public enum EIModuleBaseType
    {
        Integrated,
        External
    }
    /// <summary>
    /// Activation type kind
    /// </summary>
    public enum EIModuleBaseActivationType
    {
        Never,
        OnStart,
        OnMenuSceneLoaded
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Module base interface
    /// </summary>
    public interface IModuleBase
    {
        /// <summary>
        /// Module type
        /// </summary>
        EIModuleBaseType Type { get; }
        /// <summary>
        /// Name of the Module
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Fancy Name of the Module
        /// </summary>
        string FancyName { get; }
        /// <summary>
        /// Description of the Module
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Is the plugin using chat features
        /// </summary>
        bool UseChatFeatures { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        bool IsEnabled { get; set; }
        /// <summary>
        /// Activation type
        /// </summary>
        EIModuleBaseActivationType ActivationType { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set enabled
        /// </summary>
        /// <param name="p_Enabled"></param>
        void SetEnabled(bool p_Enabled);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for module activation
        /// </summary>
        /// <param name="p_Kind"></param>
        void CheckForActivation(EIModuleBaseActivationType p_Kind);
        /// <summary>
        /// On application exit
        /// </summary>
        void OnApplicationExit();
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Module base interface
    /// </summary>
    public abstract class ModuleBase<T> : IModuleBase
        where T : ModuleBase<T>, new()
    {
        /// <summary>
        /// Module type
        /// </summary>
        public abstract EIModuleBaseType Type { get; }
        /// <summary>
        /// Name of the Module
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Fancy Name of the Module
        /// </summary>
        public virtual string FancyName => Name;
        /// <summary>
        /// Description of the Module
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// Is the plugin using chat features
        /// </summary>
        public abstract bool UseChatFeatures { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        public abstract bool IsEnabled { get; set; }
        /// <summary>
        /// Activation type
        /// </summary>
        public abstract EIModuleBaseActivationType ActivationType { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        public static T Instance { get; private set; } = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is enabled
        /// </summary>
        private bool m_WasEnabled = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set enabled
        /// </summary>
        /// <param name="p_Enabled"></param>
        public void SetEnabled(bool p_Enabled)
        {
            IsEnabled = p_Enabled;

            if (IsEnabled && !m_WasEnabled) Enable();
            if (!IsEnabled && m_WasEnabled) Disable();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for module activation
        /// </summary>
        /// <param name="p_Kind"></param>
        public void CheckForActivation(EIModuleBaseActivationType p_Kind)
        {
            if (!m_WasEnabled && IsEnabled && ActivationType == p_Kind)
                Enable();
        }
        /// <summary>
        /// On application exit
        /// </summary>
        public void OnApplicationExit()
        {
            if (m_WasEnabled && IsEnabled)
                Disable();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        internal void Enable()
        {
            if (m_WasEnabled)
                return;

            m_WasEnabled = true;
            Instance = this as T;
            OnEnable();
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        internal void Disable()
        {
            if (!m_WasEnabled)
                return;

            OnDisable();
            Instance = null;
            m_WasEnabled = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected abstract void OnEnable();
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected abstract void OnDisable();
    }
}
