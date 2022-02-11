using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeatSaberPlus_Chat.Utils
{
    /*
       Code from https://github.com/brian91292/EnhancedStreamChat-v3

       MIT License

       Copyright (c) 2020 brian91292

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

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
            var l_Tasks                 = new List<Task<BeatSaberPlus.SDK.Unity.EnhancedImage>>();
            var l_PendingEmoteDownloads = new HashSet<string>();

            foreach (var l_Emote in p_Message.Emotes)
            {
                if (l_PendingEmoteDownloads.Contains(l_Emote.Id))
                    continue;

                if (!p_Font.HasReplaceCharacter(l_Emote.Id))
                {
                    l_PendingEmoteDownloads.Add(l_Emote.Id);

                    var l_TaskCompletionSource = new TaskCompletionSource<BeatSaberPlus.SDK.Unity.EnhancedImage>();

                    switch (l_Emote.Type)
                    {
                        case EmoteType.SingleImage:
                            BeatSaberPlus.SDK.Chat.ImageProvider.TryCacheSingleImage(EChatResourceCategory.Emote, l_Emote.Id, l_Emote.Uri, l_Emote.Animation, (l_Info) => {
                                if (l_Info != null && !p_Font.TryRegisterImageInfo(l_Info, out var l_Character))
                                    Logger.Instance.Warn($"Failed to register emote \"{l_Emote.Id}\" in font {p_Font.Font.name}.");

                                l_TaskCompletionSource.SetResult(l_Info);
                            });
                            break;

                        case EmoteType.SpriteSheet:
                            BeatSaberPlus.SDK.Chat.ImageProvider.TryCacheSpriteSheetImage(EChatResourceCategory.Emote, l_Emote.Id, l_Emote.Uri, l_Emote.UVs, (l_Info) =>
                            {
                                if (l_Info != null && !p_Font.TryRegisterImageInfo(l_Info, out var l_Character))
                                    Logger.Instance.Warn($"Failed to register emote \"{l_Emote.Id}\" in font {p_Font.Font.name}.");

                                l_TaskCompletionSource.SetResult(l_Info);
                            });
                            break;

                        default:
                            l_TaskCompletionSource.SetResult(null);
                            break;
                    }

                    l_Tasks.Add(l_TaskCompletionSource.Task);
                }
            }

            foreach (var l_Badge in p_Message.Sender.Badges)
            {
                if (l_PendingEmoteDownloads.Contains(l_Badge.Id))
                    continue;

                if (!p_Font.HasReplaceCharacter(l_Badge.Id))
                {
                    l_PendingEmoteDownloads.Add(l_Badge.Id);
                    var l_TaskCompletionSource = new TaskCompletionSource<BeatSaberPlus.SDK.Unity.EnhancedImage>();

                    BeatSaberPlus.SDK.Chat.ImageProvider.TryCacheSingleImage(EChatResourceCategory.Badge, l_Badge.Id, l_Badge.Uri, BeatSaberPlus.SDK.Animation.AnimationType.NONE, (p_Info) => {
                        if (p_Info != null && !p_Font.TryRegisterImageInfo(p_Info, out var l_Character))
                            Logger.Instance.Warn($"Failed to register badge \"{l_Badge.Id}\" in font {p_Font.Font.name}.");

                        l_TaskCompletionSource.SetResult(p_Info);
                    });

                    l_Tasks.Add(l_TaskCompletionSource.Task);
                }
            }

            // Wait on all the resources to be ready
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
                    Logger.Instance.Warn($"Failed to prepare some/all images for msg \"{p_Message.Message}\"!");

                ConcurrentStack<BeatSaberPlus.SDK.Unity.EnhancedImage> l_Badges = new ConcurrentStack<BeatSaberPlus.SDK.Unity.EnhancedImage>();
                foreach (var l_Badge in p_Message.Sender.Badges)
                {
                    if (!BeatSaberPlus.SDK.Chat.ImageProvider.CachedImageInfo.TryGetValue(l_Badge.Id, out var l_BadgeInfo))
                    {
                        Logger.Instance.Warn($"Failed to find cached image info for badge \"{l_Badge.Id}\"!");
                        continue;
                    }

                    l_Badges.Push(l_BadgeInfo);
                }

                /// Replace all instances of <;> with a zero-width non-breaking character
                StringBuilder l_StringBuilder = new StringBuilder(ProcessRTL(p_Message.Message, out bool l_IsRtl)).Replace("<", "<\u200B").Replace(">", "\u200B>");

                foreach (var l_Emote in p_Message.Emotes)
                {
                    if (!BeatSaberPlus.SDK.Chat.ImageProvider.CachedImageInfo.TryGetValue(l_Emote.Id, out var l_ImageInfo))
                    {
                        Logger.Instance.Warn($"Emote {l_Emote.Name} was missing from the emote dict! The request to {l_Emote.Uri} may have timed out?");
                        continue;
                    }

                    if (l_ImageInfo == null)
                    {
                        Logger.Instance.Warn($"Emote {l_Emote.Name} is invalid ! The request to {l_Emote.Uri} may have timed out?");
                        continue;
                    }

                    if (!p_Font.TryGetReplaceCharacter(l_ImageInfo.ImageID, out uint p_Character))
                    {
                        Logger.Instance.Warn($"Emote {l_Emote.Name} was missing from the character dict! Font hay have run out of usable characters.");
                        continue;
                    }

                    try
                    {
                        l_StringBuilder.Replace(l_Emote.Name,
                            ((l_Emote is TwitchEmote) && ((TwitchEmote)l_Emote).Bits > 0)
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
                        if (l_IsRtl)
                        {
                            l_StringBuilder.Insert(0, $"<color={p_Message.Sender.Color}>");
                            l_StringBuilder.Append($" <b>{p_Message.Sender.PaintedName}</b> </color>");
                        }
                        else
                        {
                            l_StringBuilder.Insert(0, $"<color={p_Message.Sender.Color}><b>{p_Message.Sender.PaintedName}</b> ");
                            l_StringBuilder.Append("</color>");
                        }
                    }
                    /// Insert username w/ color
                    else
                    {
                        if (l_IsRtl)
                            l_StringBuilder.Append($" :<color={p_Message.Sender.Color}><b>{p_Message.Sender.PaintedName}</b></color> ");
                        else
                            l_StringBuilder.Insert(0, $"<color={p_Message.Sender.Color}><b>{p_Message.Sender.PaintedName}</b></color>: ");
                    }

                    if (l_IsRtl)
                    {
                        for (int l_I = 0; l_I < p_Message.Sender.Badges.Length; l_I++)
                        {
                            /// Insert user badges at the beginning of the string in reverse order
                            if (l_Badges.TryPop(out var l_Badge) && p_Font.TryGetReplaceCharacter(l_Badge.ImageID, out var l_Character))
                                l_StringBuilder.Append($"{char.ConvertFromUtf32((int)l_Character)} ");
                        }
                    }
                    else
                    {
                        for (int l_I = 0; l_I < p_Message.Sender.Badges.Length; l_I++)
                        {
                            /// Insert user badges at the beginning of the string in reverse order
                            if (l_Badges.TryPop(out var l_Badge) && p_Font.TryGetReplaceCharacter(l_Badge.ImageID, out var l_Character))
                                l_StringBuilder.Insert(0, $"{char.ConvertFromUtf32((int)l_Character)} ");
                        }
                    }
                }

                return (l_IsRtl ? "<align=\"right\">" : "") + l_StringBuilder.Replace("\r", "").Replace("\n", "").ToString();
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error($"An exception occurred in ChatMessageBuilder while parsing msg with {p_Message.Emotes.Length} emotes. Msg: \"{p_Message.Message}\"");
                Logger.Instance.Error(p_Exception);
            }

            return p_Message.Message;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process RTL support
        /// </summary>
        /// <param name="p_Input">Input string</param>
        /// <param name="p_IsRTL">Is right to left</param>
        /// <returns></returns>
        public static string ProcessRTL(string p_Input, out bool p_IsRTL)
        {
            p_IsRTL = false;

            /// temp
            return p_Input;

            const char FirstHebChar = (char)1488; /// א
            const char LastHebChar  = (char)1514; /// ת
            bool IsHebrew(char c) => c >= FirstHebChar && c <= LastHebChar;


            if (p_Input == null || !p_Input.ToCharArray().Any(x => IsHebrew(x)))
                return p_Input;

            p_IsRTL = true;

            var matches = Regex.Matches(p_Input, @"[\p{IsHebrew}\P{L}]+|\P{IsHebrew}+");

            string l_Result = "";

            foreach (var l_Match in matches)
            {
                var l_MatchStr = l_Match.ToString();
                if (l_MatchStr.ToCharArray().Any(x => IsHebrew(x)))
                {
                    var l_Chars = l_MatchStr.ToCharArray();
                    Array.Reverse(l_Chars);
                    l_Result += (l_Result.Length == 0 ? "" : " ") + new string(l_Chars);
                }
                else
                    l_Result += (l_Result.Length == 0 ? "" : " ") + l_MatchStr;
            }

            return l_Result;
        }
    }
}
