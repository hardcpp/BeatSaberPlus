using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BeatSaberPlus.SDK.OBS.Models
{
    /// <summary>
    /// Scene model
    /// </summary>
    public class Scene
    {
        private static Pool.MTObjectPool<Scene> s_Pool = new Pool.MTObjectPool<Scene>(createFunc: () => new Scene(), defaultCapacity: 40);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string       name    { get; private set; } = "";
        public List<Source> sources { get; private set; } = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        private Scene() { }
        /// <summary>
        /// Get new scene instance from JObject
        /// </summary>
        /// <param name="p_Object">JObject to deserialize</param>
        /// <returns></returns>
        internal static Scene FromJObject(JObject p_Object)
        {
            var l_Scene = s_Pool.Get();
            if (l_Scene.sources == null)
            {
                l_Scene.sources = Pool.MTListPool<Source>.Get();
                l_Scene.sources.Clear();
            }

            l_Scene.Deserialize(p_Object);

            return l_Scene;
        }
        /// <summary>
        /// Release scene instance
        /// </summary>
        /// <param name="p_Scene"></param>
        internal static void Release(Scene p_Scene)
        {
            s_Pool.Release(p_Scene);

            if (p_Scene.sources != null)
            {
                try
                {
                    for (int l_I = 0; l_I < p_Scene.sources.Count; ++l_I)
                        Source.Release(p_Scene.sources[l_I]);

                    p_Scene.sources.Clear();
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[SDK.OBS.Models][Scene.Release] Error:");
                    Logger.Instance.Error(l_Exception);
                }

                Pool.MTListPool<Source>.Release(p_Scene.sources);
            }

            p_Scene.sources = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deserialize from JObject
        /// </summary>
        /// <param name="p_Object">JObject to deserialize</param>
        /// <param name="p_SourcesOnly">Only sources?</param>
        internal void Deserialize(JObject p_Object, bool p_SourcesOnly = false)
        {
            if (!p_SourcesOnly)
            {
                name = p_Object["name"]?.Value<string>() ?? (p_Object["scene-name"]?.Value<string>() ?? "");
            }

            var l_Sources       = p_Object.ContainsKey("sources") && p_Object["sources"].Type == JTokenType.Array ? p_Object["sources"] as JArray : null;
            var l_SourcesCount  = l_Sources?.Count;
            var l_OldList       = sources;
            var l_NewList       = Pool.MTListPool<Source>.Get();

            try
            {
                for (int l_I = 0; l_I < l_SourcesCount; ++l_I)
                {
                    var l_JObject   = l_Sources[l_I] as JObject;
                    var l_Existing  = l_OldList.FirstOrDefault(x => x.name == (l_JObject["name"]?.Value<string>() ?? null));

                    if (l_Existing != null)
                    {
                        l_Existing.Deserialize(this, l_JObject);
                        l_NewList.Add(l_Existing);
                        l_OldList.Remove(l_Existing);
                    }
                    else
                    {
                        var l_New = Source.FromJObject(this, l_Sources[l_I] as JObject);
                        l_NewList.Add(l_New);
                    }
                }
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.OBS.Models][Source.Deserialize] Error:");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                sources = l_NewList;

                for (int l_I = 0; l_I < l_OldList.Count; ++l_I)
                    Source.Release(l_OldList[l_I]);

                l_OldList.Clear();
                Pool.MTListPool<Source>.Release(l_OldList);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set as active scene
        /// </summary>
        public void SwitchTo()
            => Service.SwitchToScene(this);
        /// <summary>
        /// Set as active preview scene
        /// </summary>
        public void SetAsPreview()
            => Service.SwitchPreviewToScene(this);
        /// <summary>
        /// Get source by name
        /// </summary>
        /// <param name="p_Name">Name of the source</param>
        /// <returns></returns>
        public Source GetSourceByName(string p_Name)
        {
            for (int l_I = 0; l_I < sources.Count; ++l_I)
            {
                var l_Source = sources[l_I];
                if (l_Source.name == p_Name)
                    return l_Source;

                if (l_Source.groupChildren.Count != 0)
                {
                    for (int l_Y = 0; l_Y < l_Source.groupChildren.Count; ++l_Y)
                    {
                        var l_SubSource = l_Source.groupChildren[l_Y];
                        if (l_SubSource.name == p_Name)
                            return l_SubSource;
                    }
                }
            }

            return null;
        }
    }
}
