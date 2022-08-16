namespace BeatSaberPlus.SDK
{
    /// <summary>
    /// Module base interface
    /// </summary>
    public interface IBSPModuleBase : CP_SDK.IModuleBase
    {
        /// <summary>
        /// Get Module settings UI
        /// </summary>
        (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUI();
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Module base interface
    /// </summary>
    public abstract class BSPModuleBase<T> : CP_SDK.ModuleBase<T>, IBSPModuleBase
        where T : BSPModuleBase<T>, new()
    {
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        public (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUI() => GetSettingsUIImplementation();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected abstract (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation();
    }
}
