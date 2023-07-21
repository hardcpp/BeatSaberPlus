using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CP_SDK.Config
{
    /// <summary>
    /// Json config file
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public abstract class JsonConfig<T> where T : JsonConfig<T>, new()
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static T m_Instance = null;
        /// <summary>
        /// Json converters
        /// </summary>
        protected List<JsonConverter> m_JsonConverters = new List<JsonConverter>()
        {
            new JsonConverters.Color32Converter(),
            new JsonConverters.ColorConverter(),
            new JsonConverters.QuaternionConverter(),
            new JsonConverters.Vector2Converter(),
            new JsonConverters.Vector3Converter(),
        };
        /// <summary>
        /// Raw loaded JSON
        /// </summary>
        protected JObject m_RawLoaded = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                    m_Instance.Init();
                }

                return m_Instance;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dummy method for warmup
        /// </summary>
        public void Warmup()
        {
            ;
        }
        /// <summary>
        /// Reset config to default
        /// </summary>
        public void Reset()
        {
            try
            {
                var l_Default           = new T();
                var l_Params            = new JsonSerializerSettings();
                m_JsonConverters.ForEach(x => l_Params.Converters.Add(x));
                l_Params.DefaultValueHandling   = DefaultValueHandling.Include;
                l_Params.NullValueHandling      = NullValueHandling.Ignore;

                var l_DefaultSerialized = JsonConvert.SerializeObject(l_Default, l_Params);
                JsonConvert.PopulateObject(l_DefaultSerialized, m_Instance, l_Params);

                Save();
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Config][Reset<{typeof(T).Name}>.Reset] Failed");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// Save config file
        /// </summary>
        public void Save()
            => WriteFile(GetFullPath());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public abstract string GetRelativePath();
        /// <summary>
        /// Get full config path
        /// </summary>
        /// <returns></returns>
        public virtual string GetFullPath()
            => Path.Combine(ChatPlexSDK.BasePath, $"UserData/{GetRelativePath()}.json");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected virtual void OnInit(bool p_OnCreation)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        private void Init()
        {
            try
            {
                var l_Directory = Path.GetDirectoryName(GetFullPath());
                if (!Directory.Exists(l_Directory))
                    Directory.CreateDirectory(l_Directory);
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Config][JSONConfig<{typeof(T).Name}>.Init] Failed to create directory " + Path.GetDirectoryName(GetFullPath()));
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            bool l_FileExist = false;
            try
            {
                if (File.Exists(GetFullPath()))
                {
                    l_FileExist = true;
                    using (var l_FileStream = new FileStream(GetFullPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var l_StreamReader = new StreamReader(l_FileStream, Encoding.UTF8))
                        {
                            var l_Content = l_StreamReader.ReadToEnd();
                            var l_Params  = new JsonSerializerSettings();
                            m_JsonConverters.ForEach(x => l_Params.Converters.Add(x));
                            l_Params.DefaultValueHandling   = DefaultValueHandling.Include;
                            l_Params.NullValueHandling      = NullValueHandling.Ignore;

                            m_RawLoaded = JObject.Parse(l_Content);
                            JsonConvert.PopulateObject(l_Content, m_Instance, l_Params);
                        }
                    }

                    OnInit(false);
                    m_RawLoaded = null;
                }
                else
                {
                    OnInit(true);
                }

                Save();
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Config][JSONConfig<{typeof(T).Name}>.Init] Failed to read config " + Path.GetDirectoryName(GetFullPath()));
                ChatPlexSDK.Logger.Error(l_Exception);

                try
                {
                    if (l_FileExist)
                    {
                        File.Move(GetFullPath(),
                            Path.Combine(
                                Path.GetDirectoryName(GetFullPath()),
                                Path.GetFileNameWithoutExtension(GetFullPath()) + ".broken_" + Misc.Time.UnixTimeNow() + ".json"
                            )
                        );

                        OnInit(true);
                        Save();
                    }
                    else
                    {
                        OnInit(true);
                        Save();
                    }
                }
                catch (System.Exception)
                {

                }
            }
        }
        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="p_FullPath">File path</param>
        /// <returns></returns>
        private static void WriteFile(string p_FullPath)
        {
            Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    var l_Directory = Path.GetDirectoryName(p_FullPath);
                    if (!Directory.Exists(l_Directory))
                        Directory.CreateDirectory(l_Directory);
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Config][JSONConfig<{typeof(T).Name}>.WriteFile] Failed to create directory " + Path.GetDirectoryName(p_FullPath));
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                try
                {
                    string l_Data = JsonConvert.SerializeObject(m_Instance, Formatting.Indented, m_Instance.m_JsonConverters.ToArray());
                    using (var l_FileStream = new FileStream(p_FullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var l_StreamWritter = new StreamWriter(l_FileStream, Encoding.UTF8))
                        {
                            l_StreamWritter.WriteLine(l_Data);
                        }
                    }
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Config][JSONConfig<{typeof(T).Name}>.WriteFile] Failed to write file " + p_FullPath);
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }
    }
}
