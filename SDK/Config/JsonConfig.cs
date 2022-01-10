using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.SDK.Config
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
            new JsonConverters.Vector3Converter()
        };

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
                var l_DefaultSerialized = JsonConvert.SerializeObject(l_Default);
                var l_Params            = new JsonSerializerSettings();

                m_JsonConverters.ForEach(x => l_Params.Converters.Add(x));
                JsonConvert.PopulateObject(l_DefaultSerialized, m_Instance, l_Params);

                Save();
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error($"[SDK.Config][Reset<{typeof(T).Name}>.Reset] Failed");
                Logger.Instance?.Error(l_Exception);
            }
        }
        /// <summary>
        /// Save config file
        /// </summary>
        public void Save()
        {
            try
            {
                string l_Data = JsonConvert.SerializeObject(m_Instance, Formatting.Indented, m_JsonConverters.ToArray());
                SharedCoroutineStarter.instance.StartCoroutine(WriteFile(GetFullPath(), l_Data));
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error($"[SDK.Config][JSONConfig<{typeof(T).Name}>.Save] Failed to serialize config");
                Logger.Instance?.Error(l_Exception);
            }
        }
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
        {
            return Path.Combine(Environment.CurrentDirectory, $"UserData/{GetRelativePath()}.json");
        }

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
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error($"[SDK.Config][JSONConfig<{typeof(T).Name}>.Init] Failed to create directory " + Path.GetDirectoryName(GetFullPath()));
                Logger.Instance?.Error(l_Exception);
            }

            bool l_FileExist = false;
            try
            {
                if (File.Exists(GetFullPath()))
                {
                    l_FileExist = true;
                    using (var l_FileStream = new System.IO.FileStream(GetFullPath(), System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        using (var l_StreamReader = new System.IO.StreamReader(l_FileStream, Encoding.UTF8))
                        {
                            var l_Content = l_StreamReader.ReadToEnd();
                            var l_Params  = new JsonSerializerSettings();
                            m_JsonConverters.ForEach(x => l_Params.Converters.Add(x));
                            l_Params.DefaultValueHandling   = DefaultValueHandling.Include;
                            l_Params.NullValueHandling      = NullValueHandling.Ignore;

                            JsonConvert.PopulateObject(l_Content, m_Instance, l_Params);
                        }
                    }

                    OnInit(false);
                }
                else
                {
                    OnInit(true);
                }

                Save();
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error($"[SDK.Config][JSONConfig<{typeof(T).Name}>.Init] Failed to read config " + Path.GetDirectoryName(GetFullPath()));
                Logger.Instance?.Error(l_Exception);

                try
                {
                    if (l_FileExist)
                    {
                        File.Move(GetFullPath(),
                            Path.Combine(
                                Path.GetDirectoryName(GetFullPath()),
                                Path.GetFileNameWithoutExtension(GetFullPath()) + ".broken_" + SDK.Misc.Time.UnixTimeNow() + ".json"
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
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static IEnumerator WriteFile(string p_FullPath, string p_Content)
        {
            /// Wait until menu scene
            yield return new WaitUntil(() => Game.Logic.ActiveScene == Game.Logic.SceneType.Menu);

            try
            {
                var l_Directory = Path.GetDirectoryName(p_FullPath);
                if (!Directory.Exists(l_Directory))
                    Directory.CreateDirectory(l_Directory);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error($"[SDK.Config][JSONConfig<{typeof(T).Name}>.WriteFile] Failed to create directory " + Path.GetDirectoryName(p_FullPath));
                Logger.Instance?.Error(l_Exception);
            }

            Task.Run(() =>
            {
                try
                {
                    using (var l_FileStream = new System.IO.FileStream(p_FullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                    {
                        using (var l_StreamWritter = new System.IO.StreamWriter(l_FileStream, Encoding.UTF8))
                        {
                            l_StreamWritter.WriteLine(p_Content);
                        }
                    }
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance?.Error($"[SDK.Config][JSONConfig<{typeof(T).Name}>.WriteFile] Failed to write file " + p_FullPath);
                    Logger.Instance?.Error(l_Exception);
                }
            });
        }
    }
}
