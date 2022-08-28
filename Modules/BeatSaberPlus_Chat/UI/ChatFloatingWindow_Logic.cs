using CP_SDK.Chat.Interfaces;
using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal partial class ChatFloatingWindow
    {
        /// <summary>
        /// Backup message queue, keep a track of the messages if the game reload
        /// </summary>
        private static ConcurrentQueue<IChatMessage> m_BackupMessageQueue = new ConcurrentQueue<IChatMessage>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Chat font
        /// </summary>
        private Extensions.EnhancedFontInfo m_ChatFont;
        /// <summary>
        /// Message pool
        /// </summary>
        private CP_SDK.Pool.ObjectPool<Components.ChatMessageWidget> m_MessagePool;
        /// <summary>
        /// All allocated messages
        /// </summary>
        private List<Components.ChatMessageWidget> m_MessagePool_Allocated = new List<Components.ChatMessageWidget>();
        /// <summary>
        /// Visible message queue
        /// </summary>
        private List<Components.ChatMessageWidget> m_Messages = new List<Components.ChatMessageWidget>();
        /// <summary>
        /// Should update message positions
        /// </summary>
        private bool m_UpdateMessagePositions = false;
        /// <summary>
        /// Last message added
        /// </summary>
        private Components.ChatMessageWidget m_LastMessage;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init logic
        /// </summary>
        private void InitLogic()
        {
            m_ChatFont = new Extensions.EnhancedFontInfo(CP_SDK.Unity.FontManager.GetChatFont());

            /// Clean reserved characters
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xE000 && x.glyphIndex <= 0xF8FF);
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xF0000);

            /// Setup message pool
            m_MessagePool = new CP_SDK.Pool.ObjectPool<Components.ChatMessageWidget>(
                createFunc: () =>
                {
                    var l_Message = null as Components.ChatMessageWidget;
                    l_Message = new GameObject().AddComponent<Components.ChatMessageWidget>();
                    l_Message.Text.FontInfo                 = m_ChatFont;
                    l_Message.Text.font                     = m_ChatFont.Font;
                    l_Message.Text.fontSize                 = m_FontSize;
                    l_Message.Text.color                    = m_TextColor;
                    l_Message.Text.text                     = ".";
                    l_Message.Text.SetAllDirty();

                    l_Message.SubText.FontInfo              = m_ChatFont;
                    l_Message.SubText.font                  = m_ChatFont.Font;
                    l_Message.SubText.fontSize              = m_FontSize;
                    l_Message.SubText.color                 = m_TextColor;
                    l_Message.SubText.text                  = ".";
                    l_Message.SubText.SetAllDirty();

                    l_Message.transform.SetParent(transform.GetChild(0).transform, false);
                    l_Message.transform.SetAsFirstSibling();
                    l_Message.SetWidth(m_ChatSize.x);

                    //l_Message.gameObject.SetActive(false);
                    l_Message.transform.localScale = Vector3.zero;
                    l_Message.gameObject.ChangerLayerRecursive(LayerMask.NameToLayer("UI"));

                    UpdateMessageStyle(l_Message);

                    l_Message.OnLatePreRenderRebuildComplete += OnMessageRenderRebuildComplete;

                    m_MessagePool_Allocated.Add(l_Message);

                    return l_Message;
                },
                actionOnGet: (p_Message) =>
                {
                    p_Message.EnableCallback = true;
                },
                actionOnRelease: (p_Message) =>
                {
                    try
                    {
                        //p_Message.gameObject.SetActive(false);
                        p_Message.transform.localScale = Vector3.zero;

                        p_Message.HighlightEnabled      = false;
                        p_Message.AccentEnabled         = false;
                        p_Message.SubTextEnabled        = false;
                        p_Message.Text.ChatMessage      = null;
                        p_Message.SubText.ChatMessage   = null;

                        p_Message.EnableCallback = false;

                        p_Message.Text.ClearImages();
                        p_Message.SubText.ClearImages();
                    }
                    catch (Exception p_Exception)
                    {
                        Logger.Instance.Error("An exception occurred while trying to free CustomText object");
                        Logger.Instance.Error(p_Exception);
                    }
                },
                actionOnDestroy: (p_Message) =>
                {
                    GameObject.Destroy(p_Message.gameObject);
                    m_MessagePool_Allocated.Remove(p_Message);
                },
                collectionCheck: false,
                defaultCapacity: 25
            );

            while (m_BackupMessageQueue.TryDequeue(out var l_Current))
                OnTextMessageReceived(l_Current);
        }
        /// <summary>
        /// Destroy logic
        /// </summary>
        private void DestroyLogic()
        {
            /// Backup messages
            for (int l_I = 0; l_I < m_Messages.Count; ++l_I)
            {
                var l_Current = m_Messages[l_I];
                if (l_Current.Text.ChatMessage != null)
                    m_BackupMessageQueue.Enqueue(l_Current.Text.ChatMessage);

                if (l_Current.SubText.ChatMessage != null)
                    m_BackupMessageQueue.Enqueue(l_Current.SubText.ChatMessage);

                m_MessagePool.Release(m_Messages[l_I]);
            }

            m_Messages.Clear();

            if (m_MessagePool != null)
            {
                m_MessagePool.Dispose();
                m_MessagePool = null;
            }

            if (m_ChatFont != null)
            {
                Destroy(m_ChatFont.Font);
                m_ChatFont = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_IsRotatingLevel && CConfig.Instance.FollowEnvironementRotation && m_EnvironmentRotationRef != null && m_EnvironmentRotationRef)
                transform.parent.parent.rotation = m_EnvironmentRotationRef.transform.rotation;

            if (m_UpdateMessagePositions)
            {
                m_UpdateMessagePositions = false;
                float l_PositionY = m_ReverseChatOrder ? m_ChatSize.y : 0;

                for (int l_I = (m_Messages.Count - 1); l_I >= 0; --l_I)
                {
                    var l_CurrentMessage    = m_Messages[l_I];
                    var l_Height            = l_CurrentMessage.Height;

                    if (m_ReverseChatOrder)
                        l_PositionY -= l_Height;

                    l_CurrentMessage.SetPositionY(l_PositionY);

                    if (!m_ReverseChatOrder)
                        l_PositionY += l_Height;
                }

                for (int l_I = 0; l_I < m_Messages.Count;)
                {
                    var l_Current = m_Messages[l_I];
                    if ((m_ReverseChatOrder && l_Current.PositionY < -m_ChatSize.y) || l_Current.PositionY >= m_ChatSize.y)
                    {
                        m_Messages.Remove(l_Current);
                        m_MessagePool.Release(l_Current);
                        continue;
                    }

                    ++l_I;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a new message to the display
        /// </summary>
        /// <param name="p_NewMessage">Message to add</param>
        private void AddMessage(Components.ChatMessageWidget p_NewMessage)
        {
            p_NewMessage.SetPositionY(m_ReverseChatOrder ? m_ChatSize.y : 0);

            m_Messages.Add(p_NewMessage);
            UpdateMessageStyle(p_NewMessage);

            p_NewMessage.transform.localScale = Vector3.one;
            //p_NewMessage.gameObject.SetActive(true);
        }
        /// <summary>
        /// Update all messages
        /// </summary>
        private void UpdateMessagesStyleFull()
        {
            for (int l_I = 0; l_I < m_MessagePool_Allocated.Count; ++l_I)
                UpdateMessageStyleFull(m_MessagePool_Allocated[l_I], true);

            m_UpdateMessagePositions = true;
        }
        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="p_Message">Message to update</param>
        /// <param name="p_SetAllDirty">Should flag childs dirty</param>
        private void UpdateMessageStyleFull(Components.ChatMessageWidget p_Message, bool p_SetAllDirty = false)
        {
            p_Message.SetWidth(m_ChatSize.x);

            p_Message.AccentColor = m_AccentColor.ColorWithAlpha(0.36f);

            p_Message.Text.color    = m_TextColor;
            p_Message.Text.fontSize = m_FontSize;

            p_Message.SubText.color     = m_TextColor;
            p_Message.SubText.fontSize  = m_FontSize;

            UpdateMessageStyle(p_Message);

            if (p_SetAllDirty)
            {
                p_Message.Text.SetAllDirty();

                if (p_Message.SubTextEnabled)
                    p_Message.SubText.SetAllDirty();
            }
        }
        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="p_Message">Message to update</param>
        /// <param name="p_SetAllDirty">Should flag childs dirty</param>
        private void UpdateMessageStyle(Components.ChatMessageWidget p_Message)
        {
            if (p_Message.Text.ChatMessage != null)
            {
                p_Message.HighlightColor    = (p_Message.Text.ChatMessage.IsPing ? m_PingColor.ColorWithAlpha(0.36f) : m_HighlightColor);
                p_Message.HighlightEnabled  = p_Message.Text.ChatMessage.IsHighlighted || p_Message.Text.ChatMessage.IsPing;
                p_Message.AccentEnabled     = !p_Message.Text.ChatMessage.IsPing && (p_Message.HighlightEnabled || p_Message.SubText.ChatMessage != null);
            }
        }
        /// <summary>
        /// Clear message
        /// </summary>
        /// <param name="p_Message">Message instance</param>
        private void ClearMessage(Components.ChatMessageWidget p_Message)
        {
            string BuildClearedMessage(Components.ChatMessageText p_MessageToClear)
            {
                StringBuilder l_StringBuilder = new StringBuilder($"<color={p_MessageToClear.ChatMessage.Sender.Color}>{p_MessageToClear.ChatMessage.Sender.DisplayName}</color>");
                var l_BadgeEndIndex = p_MessageToClear.text.IndexOf("<color=");
                if (l_BadgeEndIndex != -1)
                    l_StringBuilder.Insert(0, p_MessageToClear.text.Substring(0, l_BadgeEndIndex));

                l_StringBuilder.Append(": <color=#bbbbbbbb><message deleted></color>");
                return l_StringBuilder.ToString();
            }

            /// Only clear non-system messages
            if (!p_Message.Text.ChatMessage.IsSystemMessage)
            {
                p_Message.Text.ReplaceContent(BuildClearedMessage(p_Message.Text));
                p_Message.SubTextEnabled = false;
            }

            if (p_Message.SubText.ChatMessage != null && !p_Message.SubText.ChatMessage.IsSystemMessage)
                p_Message.SubText.ReplaceContent(BuildClearedMessage(p_Message.SubText));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a message is rebuilt
        /// </summary>
        private void OnMessageRenderRebuildComplete()
        {
            m_UpdateMessagePositions = true;
        }
    }
}
