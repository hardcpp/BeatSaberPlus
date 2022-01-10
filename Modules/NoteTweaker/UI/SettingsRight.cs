using UnityEngine;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Utilities;

namespace BeatSaberPlus.Modules.NoteTweaker.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal class SettingsRight : SDK.UI.ResourceViewController<SettingsRight>
    {
        private GameObject m_Parent = null;
        private GameObject m_NoteTemplate = null;
        private GameObject m_BombTemplate = null;
        private GameObject m_CustomPreviewTL = null;
        private GameObject m_CustomPreviewTR = null;
        private GameObject m_CustomPreviewDL = null;
        private GameObject m_CustomPreviewDR = null;
        private GameObject m_CustomPreviewBomb = null;
        private GameObject m_DefaultPreviewTL = null;
        private GameObject m_DefaultPreviewTR = null;
        private GameObject m_DefaultPreviewDL = null;
        private GameObject m_DefaultPreviewDR = null;
        private GameObject m_DefaultPreviewBomb = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            m_CustomPreviewTL   = null;
            m_CustomPreviewTR   = null;
            m_CustomPreviewDL   = null;
            m_CustomPreviewDR   = null;
            m_DefaultPreviewTL  = null;
            m_DefaultPreviewTR  = null;
            m_DefaultPreviewDL  = null;
            m_DefaultPreviewDR  = null;
            m_DefaultPreviewBomb = null;

            m_Parent = new GameObject();
            m_Parent.transform.position = new Vector3(3.50f, 1.35f, 2.28f);
            m_Parent.transform.rotation = Quaternion.Euler(0.0f, 140.30f, 0.0f);

            GameObject.DontDestroyOnLoad(m_Parent);

            var l_MenuTransitionsHelper                  = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();
            var l_StandardLevelScenesTransitionSetupData = l_MenuTransitionsHelper.GetField<StandardLevelScenesTransitionSetupDataSO, MenuTransitionsHelper>("_standardLevelScenesTransitionSetupData");
            var l_StandardGameplaySceneInfo              = l_StandardLevelScenesTransitionSetupData.GetField<SceneInfo, StandardLevelScenesTransitionSetupDataSO>("_standardGameplaySceneInfo");
            var l_GameCoreSceneInfo                      = l_StandardLevelScenesTransitionSetupData.GetField<SceneInfo, StandardLevelScenesTransitionSetupDataSO>("_gameCoreSceneInfo");

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(l_GameCoreSceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed += (_) =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(l_StandardGameplaySceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed += (__) =>
                {
                    var l_BeatmapObjectsInstaller   = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault();
                    var l_OriginalNotePrefab        = l_BeatmapObjectsInstaller.GetField<GameNoteController, BeatmapObjectsInstaller>("_normalBasicNotePrefab");

                    m_NoteTemplate = GameObject.Instantiate(l_OriginalNotePrefab.transform.GetChild(0).gameObject);
                    m_NoteTemplate.gameObject.SetActive(false);

                    GameObject.DontDestroyOnLoad(m_NoteTemplate);

                    /// _Smoothness                 Range                       System.Single[]
                    /// _NoteSize                   Float                       0,25
                    /// _EnableColorInstancing      Float                       0
                    /// _SimpleColor                Color                       RGBA(0.251, 0.251, 0.251, 0.000)
                    /// _FinalColorMul              Float                       1
                    /// _EnvironmentReflectionCube  Texture
                    /// _EnableReflectionProbe      Float                       0
                    /// _ReflectionProbeIntensity   Float                       1
                    /// _EnableFog                  Float                       1
                    /// _FogStartOffset             Float                       100
                    /// _FogScale                   Float                       0,5
                    /// _EnableCutout               Float                       1
                    /// _CutoutTexScale             Float                       0,5
                    /// _EnableCloseToCameraCutout  Float                       0
                    /// _CloseToCameraCutoutOffset  Float                       0,5
                    /// _CloseToCameraCutoutScale   Float                       0,5
                    /// _EnablePlaneCut             Float                       0
                    /// _CutPlaneEdgeGlowWidth      Float                       0,01
                    /// _CutPlane                   Vector                      (1.0, 0.0, 0.0, 0.0)
                    /// _EnableRimDim               Float                       0
                    /// _RimScale                   Float                       2
                    /// _RimOffset                  Float                       -0,3
                    /// _RimCameraDistanceOffset    Float                       4
                    /// _RimCameraDistanceScale     Float                       0,01
                    /// _RimDarkening               Float                       0
                    /// _CullMode                   Float                       2
                    var l_OriginalBombPrefab = l_BeatmapObjectsInstaller.GetField<BombNoteController, BeatmapObjectsInstaller>("_bombNotePrefab");
                    m_BombTemplate = GameObject.Instantiate(l_OriginalBombPrefab.transform.GetChild(0).gameObject);
                    m_BombTemplate.gameObject.SetActive(false);

                    CreatePreview(m_NoteTemplate, m_BombTemplate);

                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(l_StandardGameplaySceneInfo.sceneName);
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(l_GameCoreSceneInfo.sceneName);
                };
            };
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);

            if (m_CustomPreviewTL == null || !m_CustomPreviewTL
             || m_CustomPreviewTR == null || !m_CustomPreviewTR)
            {
                if (m_NoteTemplate != null)
                    CreatePreview(m_NoteTemplate, m_BombTemplate);
            }
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            if (m_CustomPreviewTL != null && m_CustomPreviewTL)     GameObject.Destroy(m_CustomPreviewTL);
            if (m_CustomPreviewTR != null && m_CustomPreviewTR)     GameObject.Destroy(m_CustomPreviewTR);
            if (m_CustomPreviewDL != null && m_CustomPreviewDL)     GameObject.Destroy(m_CustomPreviewDL);
            if (m_CustomPreviewDR != null && m_CustomPreviewDR)     GameObject.Destroy(m_CustomPreviewDR);

            m_CustomPreviewTL = null;
            m_CustomPreviewTR = null;
            m_CustomPreviewDL = null;
            m_CustomPreviewDR = null;

            if (m_CustomPreviewBomb != null && m_CustomPreviewBomb) GameObject.Destroy(m_CustomPreviewBomb);

            m_CustomPreviewBomb = null;

            if (m_DefaultPreviewTL != null && m_DefaultPreviewTL)   GameObject.Destroy(m_DefaultPreviewTL);
            if (m_DefaultPreviewTR != null && m_DefaultPreviewTR)   GameObject.Destroy(m_DefaultPreviewTR);
            if (m_DefaultPreviewDL != null && m_DefaultPreviewDL)   GameObject.Destroy(m_DefaultPreviewDL);
            if (m_DefaultPreviewDR != null && m_DefaultPreviewDR)   GameObject.Destroy(m_DefaultPreviewDR);

            m_DefaultPreviewTL  = null;
            m_DefaultPreviewTR  = null;
            m_DefaultPreviewDL  = null;
            m_DefaultPreviewDR  = null;

            if (m_DefaultPreviewBomb != null && m_DefaultPreviewBomb) GameObject.Destroy(m_DefaultPreviewBomb);
            m_DefaultPreviewBomb = null;
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected override void OnViewDestruction()
        {
            if (m_Parent != null && m_Parent)
                GameObject.Destroy(m_Parent);
            if (m_NoteTemplate != null && m_NoteTemplate)
                GameObject.Destroy(m_NoteTemplate);

            m_Parent        = null;
            m_NoteTemplate  = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh settings
        /// </summary>
        internal void RefreshSettings()
        {
            if (m_CustomPreviewTL == null || !m_CustomPreviewTL
             || m_CustomPreviewTR == null || !m_CustomPreviewTR
             || m_CustomPreviewDR == null || !m_CustomPreviewDR
             || m_CustomPreviewDR == null || !m_CustomPreviewDR)
                return;

            m_CustomPreviewTL.transform.localScale = Vector3.one * Config.NoteTweaker.Scale;
            m_CustomPreviewTR.transform.localScale = Vector3.one * Config.NoteTweaker.Scale;
            m_CustomPreviewDL.transform.localScale = Vector3.one * Config.NoteTweaker.Scale;
            m_CustomPreviewDR.transform.localScale = Vector3.one * Config.NoteTweaker.Scale;

            var l_PlayerData    = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            var l_ColorScheme   = l_PlayerData.colorSchemesSettings.overrideDefaultColors ? l_PlayerData.colorSchemesSettings.GetSelectedColorScheme() : null;
            var l_LeftColor     = l_ColorScheme != null ? l_ColorScheme.saberAColor : new Color(0.658823549747467f, 0.125490203499794f,  0.125490203499794f);
            var l_RightColor    = l_ColorScheme != null ? l_ColorScheme.saberBColor : new Color(0.125490203499794f, 0.3921568691730499f, 0.658823549747467f);

            var l_ArrowLColor = Config.NoteTweaker.OverrideArrowColors ? Config.NoteTweaker.ArrowLColor : l_LeftColor.ColorWithAlpha(Config.NoteTweaker.ArrowA);
            var l_ArrowRColor = Config.NoteTweaker.OverrideArrowColors ? Config.NoteTweaker.ArrowRColor : l_RightColor.ColorWithAlpha(Config.NoteTweaker.ArrowA);

            PatchArrow(m_CustomPreviewTL,  Config.NoteTweaker.ArrowScale,        l_ArrowLColor, true);
            PatchArrow(m_CustomPreviewTR,  Config.NoteTweaker.ArrowScale,        l_ArrowRColor, true);
            PatchArrow(m_CustomPreviewDL,  Config.NoteTweaker.ArrowScale,        l_ArrowLColor, false);
            PatchArrow(m_CustomPreviewDR,  Config.NoteTweaker.ArrowScale,        l_ArrowRColor, false);

            var l_DotLColor = Config.NoteTweaker.OverrideDotColors ? Config.NoteTweaker.DotLColor : l_LeftColor.ColorWithAlpha(Config.NoteTweaker.DotA);
            var l_DotRColor = Config.NoteTweaker.OverrideDotColors ? Config.NoteTweaker.DotRColor : l_RightColor.ColorWithAlpha(Config.NoteTweaker.DotA);

            PatchCircle(m_CustomPreviewTL, Config.NoteTweaker.PrecisionDotScale, l_DotLColor,   Config.NoteTweaker.ShowDotsWithArrow);
            PatchCircle(m_CustomPreviewTR, Config.NoteTweaker.PrecisionDotScale, l_DotRColor,   Config.NoteTweaker.ShowDotsWithArrow);
            PatchCircle(m_CustomPreviewDL, Config.NoteTweaker.DotScale,          l_DotLColor,   true);
            PatchCircle(m_CustomPreviewDR, Config.NoteTweaker.DotScale,          l_DotRColor,   true);

            PatchBomb(m_CustomPreviewBomb, Config.NoteTweaker.OverrideBombColor ? Config.NoteTweaker.BombColor : new Color(0.251f, 0.251f, 0.251f, 0.000f));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create note preview
        /// </summary>
        /// <param name="p_NoteTemplate">Note template</param>
        private void CreatePreview(GameObject p_NoteTemplate, GameObject p_BombTemplate)
        {
            var l_PlayerData    = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            var l_ColorScheme   = l_PlayerData.colorSchemesSettings.overrideDefaultColors ? l_PlayerData.colorSchemesSettings.GetSelectedColorScheme() : null;
            var l_LeftColor     = l_ColorScheme != null ? l_ColorScheme.saberAColor : new Color(0.658823549747467f, 0.125490203499794f, 0.125490203499794f);
            var l_RightColor    = l_ColorScheme != null ? l_ColorScheme.saberBColor : new Color(0.125490203499794f, 0.3921568691730499f, 0.658823549747467f);

            m_CustomPreviewTL   = GameObject.Instantiate(p_NoteTemplate);
            m_CustomPreviewTR   = GameObject.Instantiate(p_NoteTemplate);
            m_CustomPreviewDL   = GameObject.Instantiate(p_NoteTemplate);
            m_CustomPreviewDR   = GameObject.Instantiate(p_NoteTemplate);

            m_CustomPreviewTL.transform.SetParent(m_Parent.transform, false);
            m_CustomPreviewTR.transform.SetParent(m_Parent.transform, false);
            m_CustomPreviewDL.transform.SetParent(m_Parent.transform, false);
            m_CustomPreviewDR.transform.SetParent(m_Parent.transform, false);

            m_CustomPreviewTL.name  = "BSP_NOTE_TWEAKER_CUSTOM";
            m_CustomPreviewTR.name  = "BSP_NOTE_TWEAKER_CUSTOM";
            m_CustomPreviewDL.name  = "BSP_NOTE_TWEAKER_CUSTOM";
            m_CustomPreviewDR.name  = "BSP_NOTE_TWEAKER_CUSTOM";

            m_CustomPreviewTL.transform.localPosition   = Vector3.zero - new Vector3(0, -0.25f, 1.0f);
            m_CustomPreviewTR.transform.localPosition   = Vector3.zero - new Vector3(0, -0.25f, 0.5f);
            m_CustomPreviewDL.transform.localPosition   = Vector3.zero - new Vector3(0,  0.25f, 1.0f);
            m_CustomPreviewDR.transform.localPosition   = Vector3.zero - new Vector3(0,  0.25f, 0.5f);

            m_CustomPreviewTL.transform.localRotation   = Quaternion.Euler(0, 272, 0);
            m_CustomPreviewTR.transform.localRotation   = Quaternion.Euler(0, 272, 0);
            m_CustomPreviewDL.transform.localRotation   = Quaternion.Euler(0, 272, 0);
            m_CustomPreviewDR.transform.localRotation   = Quaternion.Euler(0, 272, 0);

            PatchNote(m_CustomPreviewTL, l_LeftColor);
            PatchNote(m_CustomPreviewTR, l_RightColor);
            PatchNote(m_CustomPreviewDL, l_LeftColor);
            PatchNote(m_CustomPreviewDR, l_RightColor);

            m_CustomPreviewBomb = GameObject.Instantiate(p_BombTemplate);
            m_CustomPreviewBomb.transform.SetParent(m_Parent.transform, false);
            m_CustomPreviewBomb.name = "BSP_NOTE_TWEAKER_CUSTOM";
            m_CustomPreviewBomb.transform.localPosition = Vector3.zero - new Vector3(0, 0.75f, 0.75f);
            m_CustomPreviewBomb.SetActive(true);

            PatchBomb(m_CustomPreviewBomb, Config.NoteTweaker.OverrideBombColor ? Config.NoteTweaker.BombColor : new Color(0.251f, 0.251f, 0.251f, 0.000f));

            m_DefaultPreviewTL = GameObject.Instantiate(p_NoteTemplate);
            m_DefaultPreviewTR = GameObject.Instantiate(p_NoteTemplate);
            m_DefaultPreviewDL = GameObject.Instantiate(p_NoteTemplate);
            m_DefaultPreviewDR = GameObject.Instantiate(p_NoteTemplate);
            m_DefaultPreviewTL.transform.SetParent(m_Parent.transform, false);
            m_DefaultPreviewTR.transform.SetParent(m_Parent.transform, false);
            m_DefaultPreviewDL.transform.SetParent(m_Parent.transform, false);
            m_DefaultPreviewDR.transform.SetParent(m_Parent.transform, false);
            m_DefaultPreviewTL.name                    = "BSP_NOTE_TWEAKER_DEFAULT";
            m_DefaultPreviewTR.name                    = "BSP_NOTE_TWEAKER_DEFAULT";
            m_DefaultPreviewDL.name                    = "BSP_NOTE_TWEAKER_DEFAULT";
            m_DefaultPreviewDR.name                    = "BSP_NOTE_TWEAKER_DEFAULT";
            m_DefaultPreviewTL.transform.localPosition = Vector3.zero + new Vector3(0,  0.25f, 0.5f);
            m_DefaultPreviewTR.transform.localPosition = Vector3.zero + new Vector3(0,  0.25f, 1.0f);
            m_DefaultPreviewDL.transform.localPosition = Vector3.zero + new Vector3(0, -0.25f, 0.5f);
            m_DefaultPreviewDR.transform.localPosition = Vector3.zero + new Vector3(0, -0.25f, 1.0f);
            m_DefaultPreviewTL.transform.localRotation = Quaternion.Euler(0, 272, 0);
            m_DefaultPreviewTR.transform.localRotation = Quaternion.Euler(0, 272, 0);
            m_DefaultPreviewDL.transform.localRotation = Quaternion.Euler(0, 272, 0);
            m_DefaultPreviewDR.transform.localRotation = Quaternion.Euler(0, 272, 0);

            PatchNote(m_DefaultPreviewTL, l_LeftColor);
            PatchNote(m_DefaultPreviewTR, l_RightColor);
            PatchNote(m_DefaultPreviewDL, l_LeftColor);
            PatchNote(m_DefaultPreviewDR, l_RightColor);

            PatchArrow(m_DefaultPreviewTL, 1f, l_LeftColor.ColorWithAlpha(0.6f), true);
            PatchArrow(m_DefaultPreviewTR, 1f, l_RightColor.ColorWithAlpha(0.6f), true);
            PatchArrow(m_DefaultPreviewDL, 0f, Color.white, false);
            PatchArrow(m_DefaultPreviewDR, 0f, Color.white, false);

            PatchCircle(m_DefaultPreviewDL, 1f, l_LeftColor,  true);
            PatchCircle(m_DefaultPreviewDR, 1f, l_RightColor, true);

            m_DefaultPreviewBomb = GameObject.Instantiate(p_BombTemplate);
            m_DefaultPreviewBomb.transform.SetParent(m_Parent.transform, false);
            m_DefaultPreviewBomb.name = "BSP_NOTE_TWEAKER_DEFAULT";
            m_DefaultPreviewBomb.transform.localPosition = Vector3.zero + new Vector3(0, -0.75f, 0.75f);
            m_DefaultPreviewBomb.SetActive(true);

            RefreshSettings();
        }
        /// <summary>
        /// Patch note
        /// </summary>
        /// <param name="p_Object">Object</param>
        /// <param name="p_Color">Color</param>
        private void PatchNote(GameObject p_Object, Color p_Color)
        {
            p_Object.SetActive(true);

            foreach (Transform l_Child in p_Object.transform)
                l_Child.gameObject.SetActive(true);

            var l_Renderer = p_Object.GetComponent<MeshRenderer>();
            if (l_Renderer != null && l_Renderer)
            {
                /// _Smoothness                 Float                       0,95
                /// _NoteSize                   Float                       0,25
                /// _EnableColorInstancing      Float                       1
                /// _SimpleColor                Color                       RGBA(0.000, 0.000, 0.000, 0.000)
                /// _FinalColorMul              Float                       1
                /// _EnvironmentReflectionCube  Texture
                /// _EnableFog                  Float                       1
                /// _FogStartOffset             Float                       100
                /// _FogScale                   Float                       0,5
                /// _EnableCutout               Float                       1
                /// _CutoutTexScale             Float                       0,5
                /// _EnableCloseToCameraCutout  Float                       0
                /// _CloseToCameraCutoutOffset  Float                       0,5
                /// _CloseToCameraCutoutScale   Float                       0,5
                /// _EnablePlaneCut             Float                       0
                /// _CutPlaneEdgeGlowWidth      Float                       0,01
                /// _CutPlane                   Vector                      (1.0, 0.0, 0.0, 0.0)
                /// _CullMode                   Float                       2
                /// _EnableRimDim               Float                       1
                /// _RimScale                   Float                       2
                /// _RimOffset                  Float                       -0,1
                /// _RimCameraDistanceOffset    Float                       5
                /// _RimCameraDistanceScale     Float                       0,03
                /// _RimDarkenning              Float                       0,5

                ///for (var l_PropertyIndex = 0; l_PropertyIndex < l_Renderer.material.shader.GetPropertyCount(); ++l_PropertyIndex)
                ///{
                ///    var l_SpaceBuffer = "                            ";
                ///    var l_Name = l_Renderer.material.shader.GetPropertyName(l_PropertyIndex);
                ///    var l_Type = l_Renderer.material.shader.GetPropertyType(l_PropertyIndex);
                ///    var l_Line = "";
                ///
                ///    l_Line += l_Name + l_SpaceBuffer.Substring(l_Name.Length);
                ///    l_Line += l_Type + l_SpaceBuffer.Substring(l_Type.ToString().Length);
                ///
                ///    switch (l_Type)
                ///    {
                ///        case UnityEngine.Rendering.ShaderPropertyType.Color:
                ///            l_Line += l_Renderer.material.GetColor(l_Name);
                ///            break;
                ///
                ///        case UnityEngine.Rendering.ShaderPropertyType.Float:
                ///            l_Line += l_Renderer.material.GetFloat(l_Name);
                ///            break;
                ///
                ///        case UnityEngine.Rendering.ShaderPropertyType.Range:
                ///            l_Line += l_Renderer.material.GetFloatArray(l_Name);
                ///            break;
                ///
                ///        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                ///            l_Line += l_Renderer.material.GetVector(l_Name);
                ///            break;
                ///    }
                ///    Logger.Instance.Error(l_Line);
                ///}

                foreach (var l_PropertyBlockController in p_Object.GetComponents<MaterialPropertyBlockController>())
                {
                    l_PropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"),           p_Color.ColorWithAlpha(1f));
                    l_PropertyBlockController.materialPropertyBlock.SetFloat(Shader.PropertyToID("_EnableRimDim"),    0f);
                    l_PropertyBlockController.materialPropertyBlock.SetFloat(Shader.PropertyToID("_EnableFog"),       0f);
                    l_PropertyBlockController.materialPropertyBlock.SetFloat(Shader.PropertyToID("_RimDarkenning"),   0f);

                    l_PropertyBlockController.ApplyChanges();
                }

                l_Renderer.receiveShadows    = false;
                l_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        /// <summary>
        /// Patch arrow
        /// </summary>
        /// <param name="p_Note">Instance</param>
        /// <param name="p_Scale">Scale</param>
        /// <param name="p_Color">Color</param>
        /// <param name="p_Show">Show</param>
        private void PatchArrow(GameObject p_Note, float p_Scale, Color p_Color, bool p_Show)
        {
            p_Note.transform.Find("NoteArrow").transform.localScale = Vector3.one * p_Scale;
            p_Note.transform.Find("NoteArrow").gameObject.SetActive(p_Show);

            var l_Glow = p_Note.transform.Find("NoteArrowGlow");
            if (l_Glow)
            {
                //Debug.LogError("l_Glow Original scale " + l_Glow.transform.localScale);
                l_Glow.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f) * p_Scale;

                foreach (var l_PropertyBlockController in l_Glow.GetComponents<MaterialPropertyBlockController>())
                {
                    l_PropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), p_Color);
                    l_PropertyBlockController.ApplyChanges();
                }

                l_Glow.GetComponent<Renderer>().enabled   = p_Show;
            }
        }
        /// <summary>
        /// Patch circle
        /// </summary>
        /// <param name="p_Note">Instance</param>
        /// <param name="p_Scale">Scale</param>
        /// <param name="p_Color">Color</param>
        /// <param name="p_Show">Show</param>
        private void PatchCircle(GameObject p_Note, float p_Scale, Color p_Color, bool p_Show)
        {
            var l_CircleGlow = p_Note.transform.Find("NoteCircleGlow");
            if (l_CircleGlow)
            {
                //Debug.LogError("l_CircleGlow Original scale " + l_CircleGlow.transform.localScale);
                l_CircleGlow.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f) * p_Scale;

                foreach (var l_PropertyBlockController in l_CircleGlow.GetComponents<MaterialPropertyBlockController>())
                {
                    l_PropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), p_Color);
                    l_PropertyBlockController.ApplyChanges();
                }

                l_CircleGlow.GetComponent<Renderer>().enabled = p_Show;
            }
        }
        /// <summary>
        /// Patch bomb
        /// </summary>
        /// <param name="p_Object">Object</param>
        /// <param name="p_Color">Color</param>
        private void PatchBomb(GameObject p_Object, Color p_Color)
        {
            p_Object.GetComponent<Renderer>().material.SetColor("_SimpleColor", p_Color);
        }
    }
}
