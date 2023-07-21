using UnityEngine;

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
        EIModuleBaseType            Type                { get;      }
        string                      Name                { get;      }
        string                      FancyName           { get;      }
        string                      Description         { get;      }
        string                      DocumentationURL    { get;      }
        bool                        UseChatFeatures     { get;      }
        bool                        IsEnabled           { get; set; }
        EIModuleBaseActivationType  ActivationType      { get;      }

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        (UI.IViewController, UI.IViewController, UI.IViewController) GetSettingsViewControllers();
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Module base interface
    /// </summary>
    public abstract class ModuleBase<t_Type> : IModuleBase
        where t_Type : ModuleBase<t_Type>, new()
    {
        public abstract EIModuleBaseType            Type                { get;      }
        public abstract string                      Name                { get;      }
        public virtual  string                      FancyName           => Name;
        public abstract string                      Description         { get;      }
        public virtual  string                      DocumentationURL    => string.Empty;
        public abstract bool                        UseChatFeatures     { get;      }
        public abstract bool                        IsEnabled           { get; set; }
        public abstract EIModuleBaseActivationType  ActivationType      { get;      }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        public static t_Type Instance { get; private set; } = null;

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
            Instance = this as t_Type;
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        public (UI.IViewController, UI.IViewController, UI.IViewController) GetSettingsViewControllers() => GetSettingsViewControllersImplementation();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected virtual (UI.IViewController, UI.IViewController, UI.IViewController) GetSettingsViewControllersImplementation()
            => (null, null, null);
    }
}
