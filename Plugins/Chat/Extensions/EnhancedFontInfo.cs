using System.Collections.Concurrent;
using TMPro;
using UnityEngine.TextCore;
using System;

namespace BeatSaberPlus.Plugins.Chat.Extensions
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
    /// Enhanced font info
    /// </summary>
    internal class EnhancedFontInfo
    {
        /// <summary>
        /// Font instance
        /// </summary>
        internal TMP_FontAsset Font { get; }
        /// <summary>
        /// Next replace char
        /// </summary>
        internal UInt32 NextReplaceChar { get; private set; } = 0xE000;
        /// <summary>
        /// Character lookup table
        /// </summary>
        internal ConcurrentDictionary<string, uint> CharacterLookupTable { get; } = new ConcurrentDictionary<string, uint>();
        /// <summary>
        /// Image info lookup table
        /// </summary>
        internal ConcurrentDictionary<uint, BeatSaberPlus.Utils.EnhancedImageInfo> ImageInfoLookupTable { get; } = new ConcurrentDictionary<uint, BeatSaberPlus.Utils.EnhancedImageInfo>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mutex
        /// </summary>
        private object m_Lock = new object();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Font">Font instance</param>
        internal EnhancedFontInfo(TMP_FontAsset p_Font)
        {
            Font = p_Font;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get next replacement character
        /// </summary>
        /// <returns></returns>
        internal UInt32 GetNextReplaceChar()
        {
            uint l_Result = NextReplaceChar++;

            /// If we used up all the Private Use Area characters, move onto Supplementary Private Use Area-A
            if (NextReplaceChar > 0xF8FF && NextReplaceChar < 0xF0000)
            {
                Logger.Instance.Warn("Font is out of characters! Switching to overflow range.");
                NextReplaceChar = 0xF0000;
            }

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal bool TryGetCharacter(string id, out uint character)
        {
            return CharacterLookupTable.TryGetValue(id, out character);
        }

        internal bool TryGetImageInfo(uint character, out BeatSaberPlus.Utils.EnhancedImageInfo imageInfo)
        {
            return ImageInfoLookupTable.TryGetValue(character, out imageInfo);
        }

        internal bool TryRegisterImageInfo(BeatSaberPlus.Utils.EnhancedImageInfo imageInfo, out uint replaceCharacter)
        {
            if (!CharacterLookupTable.ContainsKey(imageInfo.ImageID))
            {
                uint next;
                do
                {
                    next = GetNextReplaceChar();
                }
                while (Font.characterLookupTable.ContainsKey(next));
                Font.characterLookupTable.Add(next, new TMP_Character(next, new Glyph(next, new UnityEngine.TextCore.GlyphMetrics(0, 0, 0, 0, imageInfo.Width), new UnityEngine.TextCore.GlyphRect(0, 0, 0, 0))));
                CharacterLookupTable.TryAdd(imageInfo.ImageID, next);
                ImageInfoLookupTable.TryAdd(next, imageInfo);
                replaceCharacter = next;
                return true;
            }
            replaceCharacter = 0;
            return false;
        }

        internal bool TryUnregisterImageInfo(string id, out uint unregisteredCharacter)
        {
            lock (m_Lock)
            {
                if (!CharacterLookupTable.TryGetValue(id, out var c))
                {
                    unregisteredCharacter = 0;
                    return false;
                }
                if (Font.characterLookupTable.ContainsKey(c))
                {
                    Font.characterLookupTable.Remove(c);
                }
                CharacterLookupTable.TryRemove(id, out unregisteredCharacter);
                return ImageInfoLookupTable.TryRemove(unregisteredCharacter, out var unregisteredImageInfo);
            }
        }
    }
}
