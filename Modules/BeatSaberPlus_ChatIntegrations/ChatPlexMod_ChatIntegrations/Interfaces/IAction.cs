using CP_SDK.XUI;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Interfaces
{
    /// <summary>
    /// IAction generic class
    /// </summary>
    public abstract class IAction<t_Action, t_Model> : IActionBase
        where t_Action : IAction<t_Action, t_Model>, new()
        where t_Model  : Models.Action, new()
    {
        public override bool    IsEnabled               { get => Model.Enabled; set { Model.Enabled = value; } }
        public virtual  string  UIPlaceHolder           => "<alpha=#AA><i>No available settings...</i>";
        public virtual  bool    UIPlaceHolderTestButton => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public t_Model Model { get; protected set; } = new t_Model();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public override string GetTypeName() => typeof(t_Action).Name;
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public override JObject Serialize()
        {
            Model.Type = GetTypeName();
            return JObject.FromObject(Model);
        }
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        public override bool Unserialize(JObject p_Serialized)
        {
            if (!p_Serialized.ContainsKey("Type"))
                return false;

            var l_Type = p_Serialized["Type"].Value<string>();
            if (l_Type != GetTypeName())
                return false;

            try
            {
                Model = p_Serialized.ToObject<t_Model>();
                Model.OnDeserialized(p_Serialized);

                return true;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IAction<{typeof(t_Action).Name}, {typeof(t_Model).Name}.Unserialize] Error:");
                Logger.Instance.Error(l_Exception);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override void BuildUI(Transform p_Parent)
        {
            var l_Label = XUIText.Make(UIPlaceHolder);
            if (UIPlaceHolderTestButton)
            {
                var l_Button = XUIPrimaryButton.Make("Test");
                l_Button.OnClick(OnUIPlaceholderTestButton);

                XUIElements = new IXUIElement[] { l_Label, l_Button };
            }
            else
                XUIElements = new IXUIElement[] { l_Label };

            BuildUIAuto(p_Parent);
        }
        /// <summary>
        /// On UI placeholder test button pressed
        /// </summary>
        protected virtual void OnUIPlaceholderTestButton() { }
    }
}
