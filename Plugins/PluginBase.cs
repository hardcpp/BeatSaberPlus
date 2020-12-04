namespace BeatSaberPlus.Plugins
{
    /// <summary>
    /// Plugin base interface
    /// </summary>
    public abstract class PluginBase
    {
        /// <summary>
        /// Activation type kind
        /// </summary>
        public enum EActivationType
        {
            Never,
            OnStart,
            OnMenuSceneLoaded
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        public abstract bool IsEnabled { get; set; }
        /// <summary>
        /// Activation type
        /// </summary>
        public abstract EActivationType ActivationType { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is enabled
        /// </summary>
        private bool m_IsEnabled = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set enabled
        /// </summary>
        /// <param name="p_Enabled"></param>
        internal virtual void SetEnabled(bool p_Enabled)
        {
            bool l_ShouldEnable  = !IsEnabled &&  p_Enabled;
            bool l_ShouldDisable =  IsEnabled && !p_Enabled;

            IsEnabled = p_Enabled;

            if (l_ShouldEnable)     OnEnable();
            if (l_ShouldDisable)    OnDisable();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the plugin
        /// </summary>
        internal void Enable()
        {
            if (m_IsEnabled)
                return;

            m_IsEnabled = true;
            OnEnable();
        }
        /// <summary>
        /// Disable the plugin
        /// </summary>
        internal void Disable()
        {
            if (!m_IsEnabled)
                return;

            OnDisable();
            m_IsEnabled = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        internal void ShowUI()
        {
            ShowUIImplementation();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected abstract void OnEnable();
        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected abstract void OnDisable();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected abstract void ShowUIImplementation();
    }
}
