using CP_SDK.XUI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Settings left view controller
    /// </summary>
    public sealed class SettingsLeftView : ViewController<SettingsLeftView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Tools"),

                XUIVLayout.Make(
                    XUIPrimaryButton.Make("Export LIV to camera2", OnLIVToCamera2Button)
                )
                .SetWidth(60f)
                .SetPadding(0)
                .ForEachDirect<XUIPrimaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
                .ForEachDirect<XUISecondaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                }),

                XUIVSpacer.Make(50f)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On LIV to camera2 button
        /// </summary>
        private void OnLIVToCamera2Button()
        {
            var l_LIVCamera = Resources.FindObjectsOfTypeAll<Camera>().FirstOrDefault(x => x.name == "LIV Camera");
            if (!l_LIVCamera)
            {
                ShowMessageModal("LIV not found!");
                return;
            }

            var l_Profile = @"
{
  ""type"": ""Positionable"",
  ""worldCamVisibility"": ""HiddenWhilePlaying"",
  ""previewScreenSize"": 1.0,
  ""FOV"": $$FOV$$,
  ""layer"": -998,
  ""renderScale"": 1,
  ""farZ"": 1000.0,
  ""targetPos"": {
    ""x"": $$POSX$$,
    ""y"": $$POSY$$,
    ""z"": $$POSZ$$
  },
  ""targetRot"": {
    ""x"": $$ROTX$$,
    ""y"": $$ROTY$$,
    ""z"": $$ROTZ$$
  }
}";
            l_Profile = l_Profile.Replace("$$FOV$$", l_LIVCamera.fieldOfView.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$POSX$$", l_LIVCamera.transform.position.x.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$POSY$$", l_LIVCamera.transform.position.y.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$POSZ$$", l_LIVCamera.transform.position.z.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$ROTX$$", l_LIVCamera.transform.eulerAngles.x.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$ROTY$$", l_LIVCamera.transform.eulerAngles.y.ToString().Replace(',', '.'));
            l_Profile = l_Profile.Replace("$$ROTZ$$", l_LIVCamera.transform.eulerAngles.z.ToString().Replace(',', '.'));

            try
            {
                System.IO.File.WriteAllText("UserData/Camera2/Cameras/BSP_LIV.json", l_Profile, System.Text.Encoding.UTF8);
                ShowMessageModal("Camera \"BSP_LIV\" created in camera2!");
            }
            catch (System.Exception)
            {
                ShowMessageModal("Error!");
            }
        }
    }
}
