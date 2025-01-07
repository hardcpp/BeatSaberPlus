﻿using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models.Twitch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlexMod_Chat.Utils
{
    /// <summary>
    /// Message builder for twitch
    /// </summary>
    class ChatMessageBuilder
    {
        /// <summary>
        /// This function *blocks* the calling thread, and caches all the images required to display the message, then registers them with the provided font.
        /// </summary>
        /// <param name="p_Message">The chat message to get images from</param>
        /// <param name="p_Font">The font to register these images to</param>
        public static bool PrepareImages(IChatMessage p_Message, Extensions.EnhancedFontInfo p_Font)
        {
            var l_Tasks                 = new List<Task<CP_SDK.Unity.EnhancedImage>>();
            var l_PendingImageDownloads = new HashSet<string>();

            if (p_Message.Emotes != null)
            {
                for (int l_I = 0; l_I < p_Message.Emotes.Length; ++l_I)
                {
                    var l_Emote = p_Message.Emotes[l_I];
                    if (l_PendingImageDownloads.Contains(l_Emote.Id))
                        continue;

                    if (!p_Font.HasReplaceCharacter(l_Emote.Id))
                    {
                        l_PendingImageDownloads.Add(l_Emote.Id);
                        var l_TaskCompletionSource = new TaskCompletionSource<CP_SDK.Unity.EnhancedImage>();

                        CP_SDK.Chat.ChatImageProvider.TryCacheSingleImage(EChatResourceCategory.Emote, l_Emote.Id, l_Emote.Uri, l_Emote.Animation, (l_Info) => {
                            if (l_Info != null && !p_Font.TryRegisterImageInfo(l_Info, out var l_Character))
                                Logger.Instance.Warning($"Failed to register emote \"{l_Emote.Id}\" in font {p_Font.Font.name}.");

                            l_TaskCompletionSource.SetResult(l_Info);
                        });

                        l_Tasks.Add(l_TaskCompletionSource.Task);
                    }
                }
            }

            if (p_Message.Sender.Badges != null)
            {
                for (int l_I = 0; l_I < p_Message.Sender.Badges.Length; ++l_I)
                {
                    var l_Badge = p_Message.Sender.Badges[l_I];
                    if (l_Badge.Type != EBadgeType.Image || l_PendingImageDownloads.Contains(l_Badge.Id))
                        continue;

                    if (!p_Font.HasReplaceCharacter(l_Badge.Id))
                    {
                        l_PendingImageDownloads.Add(l_Badge.Id);
                        var l_TaskCompletionSource = new TaskCompletionSource<CP_SDK.Unity.EnhancedImage>();

                        CP_SDK.Chat.ChatImageProvider.TryCacheSingleImage(EChatResourceCategory.Badge, l_Badge.Id, l_Badge.Content, CP_SDK.Animation.EAnimationType.NONE, (p_Info) => {
                            if (p_Info != null && !p_Font.TryRegisterImageInfo(p_Info, out var l_Character))
                                Logger.Instance.Warning($"Failed to register badge \"{l_Badge.Id}\" in font {p_Font.Font.name}.");

                            l_TaskCompletionSource.SetResult(p_Info);
                        });

                        l_Tasks.Add(l_TaskCompletionSource.Task);
                    }
                }
            }

            return Task.WaitAll(l_Tasks.ToArray(), 15000);
        }
        /// <summary>
        /// Build a message
        /// </summary>
        /// <param name="p_Message">Message informations</param>
        /// <param name="p_Font">Font</param>
        /// <returns></returns>
#pragma warning disable CS1998
        public static async Task<string> BuildMessage(IChatMessage p_Message, Extensions.EnhancedFontInfo p_Font)
#pragma warning restore CS1998
        {
            try
            {
                if (!PrepareImages(p_Message, p_Font))
                    Logger.Instance.Warning($"Failed to prepare some/all images for msg \"{p_Message.Message}\"!");

                /// Replace all instances of <;> with a zero-width non-breaking character
                StringBuilder l_StringBuilder = new StringBuilder(p_Message.Message).Replace("<", "<\u200B").Replace("<​\u200B3", "<3").Replace(">", "\u200B>").Replace("\r", "").Replace("\n", "");

                if (p_Message.Emotes != null)
                {
                    for (int l_EmoteI = 0; l_EmoteI < p_Message.Emotes.Length; ++l_EmoteI)
                    {
                        var l_Emote = p_Message.Emotes[l_EmoteI];
                        if (!CP_SDK.Chat.ChatImageProvider.CachedImageInfo.TryGetValue(l_Emote.Id, out var l_ImageInfo))
                        {
                            Logger.Instance.Warning($"Emote {l_Emote.Name} was missing from the emote dict! The request to {l_Emote.Uri} may have timed out?");
                            continue;
                        }
                        if (l_ImageInfo == null)
                        {
                            Logger.Instance.Warning($"Emote {l_Emote.Name} is invalid ! The request to {l_Emote.Uri} may have timed out?");
                            continue;
                        }
                        if (!p_Font.TryGetReplaceCharacter(l_ImageInfo.ImageID, out uint p_Character))
                        {
                            Logger.Instance.Warning($"Emote {l_Emote.Name} was missing from the character dict! Font hay have run out of usable characters.");
                            continue;
                        }

                        try
                        {
                            l_StringBuilder.Replace(l_Emote.Name,
                                (l_Emote is TwitchEmote l_TwitchEmote && l_TwitchEmote.Bits > 0)
                                    ?
                                        $"{char.ConvertFromUtf32((int)p_Character)}\u00A0<color={((TwitchEmote)l_Emote).Color.PadRight(9, 'F').ToLower()}><size=77%><b>{((TwitchEmote)l_Emote).Bits}\u00A0</b></size></color>"
                                    :
                                        char.ConvertFromUtf32((int)p_Character)
                                , l_Emote.StartIndex, l_Emote.EndIndex - l_Emote.StartIndex + 1);
                        }
                        catch (Exception p_Exception)
                        {
                            Logger.Instance.Error($"An unknown error occurred while trying to swap emote {l_Emote.Name} into string of length {l_StringBuilder.Length} at location ({l_Emote.StartIndex}, {l_Emote.EndIndex})");
                            Logger.Instance.Error(p_Exception);
                        }
                    }
                }

                if (p_Message.IsGiganticEmote)
                    l_StringBuilder.Insert(0, "\n<size=600%>");

                /// System messages get a grayish color to differentiate them from normal messages in chat, and do not receive a username/badge prefix
                if (p_Message.IsSystemMessage)
                {
                    l_StringBuilder.Insert(0, $"<color=#bbbbbbbb>");
                    l_StringBuilder.Append("</color>");
                }
                else
                {
                    /// Message becomes the color of their name if it's an action message
                    if (p_Message.IsActionMessage)
                    {
                        l_StringBuilder.Insert(0, $"<color={p_Message.Sender.Color}><b>{p_Message.Sender.PaintedName}</b> ");
                        l_StringBuilder.Append("</color>");
                    }
                    /// Insert username w/ color
                    else
                        l_StringBuilder.Insert(0, $"<color={p_Message.Sender.Color}><b>{p_Message.Sender.PaintedName}</b></color>: ");

                    if (p_Message.Sender.Badges != null)
                    {
                        for (int l_BadgeI = 0; l_BadgeI < p_Message.Sender.Badges.Length; ++l_BadgeI)
                        {
                            var l_Badge = p_Message.Sender.Badges[l_BadgeI];
                            if (l_Badge.Type == EBadgeType.Image)
                            {
                                if (!CP_SDK.Chat.ChatImageProvider.CachedImageInfo.TryGetValue(l_Badge.Id, out var l_BadgeInfo))
                                {
                                    Logger.Instance.Warning($"Failed to find cached image info for badge \"{l_Badge.Id}\"!");
                                    continue;
                                }

                                if (p_Font.TryGetReplaceCharacter(l_BadgeInfo.ImageID, out var l_Character))
                                    l_StringBuilder.Insert(0, $"{char.ConvertFromUtf32((int)l_Character)} ");
                            }
                            else if (l_Badge.Type == EBadgeType.Emoji)
                                l_StringBuilder.Insert(0, $"{l_Badge.Content} ");
                        }
                    }
                }

                return l_StringBuilder.ToString();
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error($"An exception occurred in ChatMessageBuilder while parsing msg with {p_Message.Emotes.Length} emotes. Msg: \"{p_Message.Message}\"");
                Logger.Instance.Error(p_Exception);
            }

            return p_Message.Message;
        }
    }
}
