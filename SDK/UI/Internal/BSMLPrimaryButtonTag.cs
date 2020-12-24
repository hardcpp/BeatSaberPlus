namespace BeatSaberPlus.SDK.UI.Internal
{
    /// <summary>
    /// Primary button tag creator
    /// </summary>
    internal class BSMLPrimaryButtonTag : BeatSaberMarkupLanguage.Tags.ButtonTag
    {
        public override string[] Aliases => new[] { "primary-button", "action-button" };
        public override string PrefabButton => "PlayButton";

        public override UnityEngine.GameObject CreateObject(UnityEngine.Transform parent)
        {
            return base.CreateObject(parent).AddComponent<UnityEngine.UI.LayoutElement>().gameObject;
        }
    }
}
