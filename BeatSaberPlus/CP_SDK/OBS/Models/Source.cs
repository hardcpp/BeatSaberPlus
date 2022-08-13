using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace CP_SDK.OBS.Models
{
    /// <summary>
    /// Source item model
    /// </summary>
    public class Source
    {
        private static Pool.MTObjectPool<Source> s_Pool = new Pool.MTObjectPool<Source>(createFunc: () => new Source(), defaultCapacity: 400);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public Scene        OwnerScene      { get; private set;  } = null;
        public int          alignment       { get; private set;  } = 0;
        public float        cx              { get; private set;  } = 0f;
        public float        cy              { get; private set;  } = 0f;
        public int          id              { get; private set;  } = 0;
        public bool         locked          { get; private set;  } = false;
        public bool         muted           { get; private set;  } = false;
        public string       name            { get; private set;  } = "";
        public bool         render          { get; internal set; } = true;
        public int          source_cx       { get; private set;  } = 0;
        public int          source_cy       { get; private set;  } = 0;
        public string       type            { get; private set;  } = "";
        public float        volume          { get; private set;  } = 1f;
        public float        x               { get; private set;  } = 0f;
        public float        y               { get; private set;  } = 0f;
        public string       parentGroupName { get; private set;  } = null;
        public List<Source> groupChildren   { get; private set;  } = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        private Source() { }
        /// <summary>
        /// Get new source instance from JObject
        /// </summary>
        /// <param name="p_Scene">Owner scene</param>
        /// <param name="p_Object">JObject to deserialize</param>
        /// <returns></returns>
        internal static Source FromJObject(Scene p_Scene, JObject p_Object)
        {
            var l_Source = s_Pool.Get();
            if (l_Source.groupChildren == null)
            {
                l_Source.groupChildren = Pool.MTListPool<Source>.Get();
                l_Source.groupChildren.Clear();
            }
            l_Source.Deserialize(p_Scene, p_Object);

            return l_Source;
        }
        /// <summary>
        /// Release source instance
        /// </summary>
        /// <param name="p_Source"></param>
        internal static void Release(Source p_Source)
        {
            s_Pool.Release(p_Source);

            if (p_Source.groupChildren != null)
            {
                try
                {
                    for (int l_I = 0; l_I < p_Source.groupChildren.Count; ++l_I)
                        Source.Release(p_Source.groupChildren[l_I]);

                    p_Source.groupChildren.Clear();
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.OBS.Models][Source.Release] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                Pool.MTListPool<Source>.Release(p_Source.groupChildren);
            }

            p_Source.groupChildren  = null;
            p_Source.OwnerScene     = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Deserialize from JObject
        /// </summary>
        /// <param name="p_Scene">Owner scene</param>
        /// <param name="p_Object">JObject to deserialize</param>
        internal void Deserialize(Scene p_Scene, JObject p_Object)
        {
            OwnerScene      = p_Scene;
            alignment       = p_Object["alignment"]?.Value<int>()           ?? 0;
            cx              = p_Object["cx"]?.Value<float>()                ?? 0f;
            cy              = p_Object["cy"]?.Value<float>()                ?? 0f;
            id              = p_Object["id"]?.Value<int>()                  ?? 0;
            locked          = p_Object["locked"]?.Value<bool>()             ?? false;
            muted           = p_Object["muted"]?.Value<bool>()              ?? false;
            name            = p_Object["name"]?.Value<string>()             ?? "";
            render          = p_Object["render"]?.Value<bool>()             ?? true;
            source_cx       = p_Object["source_cx"]?.Value<int>()           ?? 0;
            source_cy       = p_Object["source_cy"]?.Value<int>()           ?? 0;
            type            = p_Object["type"]?.Value<string>()             ?? "";
            volume          = p_Object["volume"]?.Value<float>()            ?? 1f;
            x               = p_Object["x"]?.Value<float>()                 ?? 0f;
            y               = p_Object["y"]?.Value<float>()                 ?? 0f;
            parentGroupName = p_Object["parentGroupName"]?.Value<string>()  ?? null;

            var l_groupChildren         = p_Object.ContainsKey("groupChildren") && p_Object["groupChildren"].Type == JTokenType.Array ? p_Object["groupChildren"] as JArray : null;
            var l_groupChildrenCount    = l_groupChildren?.Count;
            var l_OldList               = groupChildren;
            var l_NewList               = Pool.MTListPool<Source>.Get();

            try
            {
                for (int l_I = 0; l_I < l_groupChildrenCount; ++l_I)
                {
                    var l_JObject   = l_groupChildren[l_I] as JObject;
                    var l_Existing  = l_OldList.FirstOrDefault(x => x.name == (l_JObject["name"]?.Value<string>() ?? null));

                    if (l_Existing != null)
                    {
                        l_Existing.Deserialize(p_Scene, l_JObject);
                        l_NewList.Add(l_Existing);
                        l_OldList.Remove(l_Existing);
                    }
                    else
                        l_NewList.Add(FromJObject(p_Scene, l_groupChildren[l_I] as JObject));
                }
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.OBS.Models][Source.Deserialize] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            finally
            {
                groupChildren = l_NewList;

                for (int l_I = 0; l_I < l_OldList.Count; ++l_I)
                    Release(l_OldList[l_I]);

                l_OldList.Clear();
                Pool.MTListPool<Source>.Release(l_OldList);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set this source visibility
        /// </summary>
        /// <param name="p_Visible">New visibility</param>
        public void SetVisible(bool p_Visible)
            => Service.SetSourceVisible(OwnerScene, this, p_Visible);
        /// <summary>
        /// Set this source muted
        /// </summary>
        /// <param name="p_Muted">New state</param>
        public void SetMuted(bool p_Muted)
            => Service.SetSourceMuted(OwnerScene, this, p_Muted);
    }
}
