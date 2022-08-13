using BeatSaberPlus.SDK.Chat.Services;
using System;
using System.Collections.Concurrent;

namespace BeatSaberPlus.SDK.Chat.Utilities
{
    public static class DictionaryUtils
    {
        public static void AddAction(this ConcurrentDictionary<ChatServiceBase, Action> dict, ChatServiceBase p_Service, Action value)
        {
            dict.AddOrUpdate(p_Service, value, (callingChatServiceBase, existingActions) => existingActions + value);
        }

        public static void AddAction<A>(this ConcurrentDictionary<ChatServiceBase, Action<A>> dict, ChatServiceBase p_Service, Action<A> value)
        {
            dict.AddOrUpdate(p_Service, value, (callingChatServiceBase, existingActions) => existingActions + value);
        }

        public static void AddAction<A, B>(this ConcurrentDictionary<ChatServiceBase, Action<A, B>> dict, ChatServiceBase p_Service, Action<A, B> value)
        {
            dict.AddOrUpdate(p_Service, value, (callingChatServiceBase, existingActions) => existingActions + value);
        }

        public static void AddAction<A, B, C>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C>> dict, ChatServiceBase p_Service, Action<A, B, C> value)
        {
            dict.AddOrUpdate(p_Service, value, (callingChatServiceBase, existingActions) => existingActions + value);
        }

        public static void AddAction<A, B, C, D>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C, D>> dict, ChatServiceBase p_Service, Action<A, B, C, D> value)
        {
            dict.AddOrUpdate(p_Service, value, (callingChatServiceBase, existingActions) => existingActions + value);
        }

        public static void RemoveAction(this ConcurrentDictionary<ChatServiceBase, Action> dict, ChatServiceBase p_Service, Action value)
        {
            if (dict.ContainsKey(p_Service))
            {
                dict[p_Service] -= value;
            }
        }

        public static void RemoveAction<A>(this ConcurrentDictionary<ChatServiceBase, Action<A>> dict, ChatServiceBase p_Service, Action<A> value)
        {
            if (dict.ContainsKey(p_Service))
            {
                dict[p_Service] -= value;
            }
        }

        public static void RemoveAction<A, B>(this ConcurrentDictionary<ChatServiceBase, Action<A, B>> dict, ChatServiceBase p_Service, Action<A, B> value)
        {
            if (dict.ContainsKey(p_Service))
            {
                dict[p_Service] -= value;
            }
        }

        public static void RemoveAction<A, B, C>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C>> dict, ChatServiceBase p_Service, Action<A, B, C> value)
        {
            if (dict.ContainsKey(p_Service))
            {
                dict[p_Service] -= value;
            }
        }
        public static void RemoveAction<A, B, C, D>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C, D>> dict, ChatServiceBase p_Service, Action<A, B, C, D> value)
        {
            if (dict.ContainsKey(p_Service))
            {
                dict[p_Service] -= value;
            }
        }

        public static void InvokeAll(this ConcurrentDictionary<ChatServiceBase, Action> dict)
        {
            foreach (var kvp in dict)
            {
                try
                {
                    kvp.Value?.Invoke();
                }
                catch (Exception ex)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"An exception occurred while invoking action no params.");
                    CP_SDK.ChatPlexSDK.Logger.Error(ex);
                }
            }
        }

        public static void InvokeAll<A>(this ConcurrentDictionary<ChatServiceBase, Action<A>> dict, A a)
        {
            foreach (var kvp in dict)
            {
                try
                {
                    kvp.Value?.Invoke(a);
                }
                catch (Exception ex)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"An exception occurred while invoking action with param type {typeof(A).Name}");
                    CP_SDK.ChatPlexSDK.Logger.Error(ex);
                }
            }
        }

        public static void InvokeAll<A, B>(this ConcurrentDictionary<ChatServiceBase, Action<A, B>> dict, A a, B b)
        {
            foreach (var kvp in dict)
            {
                try
                {
                    kvp.Value?.Invoke(a, b);
                }
                catch (Exception ex)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"An exception occurred while invoking action with param types {typeof(A).Name}, {typeof(B).Name}");
                    CP_SDK.ChatPlexSDK.Logger.Error(ex);
                }
            }
        }

        public static void InvokeAll<A, B, C>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C>> dict, A a, B b, C c)
        {
            foreach (var kvp in dict)
            {
                try
                {
                    kvp.Value?.Invoke(a, b, c);
                }
                catch (Exception ex)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"An exception occurred while invoking action with param types {typeof(A).Name}, {typeof(B).Name}, {typeof(C).Name}");
                    CP_SDK.ChatPlexSDK.Logger.Error(ex);
                }
            }
        }
        public static void InvokeAll<A, B, C, D>(this ConcurrentDictionary<ChatServiceBase, Action<A, B, C, D>> dict, A a, B b, C c, D d)
        {
            foreach (var kvp in dict)
            {
                try
                {
                    kvp.Value?.Invoke(a, b, c, d);
                }
                catch (Exception ex)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"An exception occurred while invoking action with param types {typeof(A).Name}, {typeof(B).Name}, {typeof(C).Name}, {typeof(D).Name}");
                    CP_SDK.ChatPlexSDK.Logger.Error(ex);
                }
            }
        }
    }
}