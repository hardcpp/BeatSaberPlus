using System.Collections.Concurrent;
using TMPro;
using UnityEngine.TextCore;
using System;

namespace ChatPlexMod_Chat.Extensions
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
        /// Mutex
        /// </summary>
        private object m_Lock = new object();
        /// <summary>
        /// Next replace char
        /// </summary>
        private UInt32 m_NextReplaceChar = 0xE000;
        /// <summary>
        /// Character lookup table
        /// </summary>
        private ConcurrentDictionary<string, uint> m_ReplaceCharacters = new ConcurrentDictionary<string, uint>();
        /// <summary>
        /// Image info lookup table
        /// </summary>
        private ConcurrentDictionary<uint, CP_SDK.Unity.EnhancedImage> m_ImageInfos = new ConcurrentDictionary<uint, CP_SDK.Unity.EnhancedImage>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Font instance
        /// </summary>
        internal TMP_FontAsset Font { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Font">Font instance</param>
        internal EnhancedFontInfo(TMP_FontAsset p_Font) => Font = p_Font;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Has replace character for image ID
        /// </summary>
        /// <param name="p_ImageID">ID of the image</param>
        /// <returns></returns>
        internal bool HasReplaceCharacter(string p_ImageID)
            => m_ReplaceCharacters.ContainsKey(p_ImageID);
        /// <summary>
        /// Try get replace character for image ID
        /// </summary>
        /// <param name="p_ImageID">ID of the image</param>
        /// <param name="p_ReplaceCharacter">Result replace character</param>
        /// <returns></returns>
        internal bool TryGetReplaceCharacter(string p_ImageID, out uint p_ReplaceCharacter)
            => m_ReplaceCharacters.TryGetValue(p_ImageID, out p_ReplaceCharacter);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Has image info for replace character
        /// </summary>
        /// <param name="p_ImageID">ID of the image</param>
        /// <returns></returns>
        internal bool HasImageInfo(uint p_ReplaceCharacter)
            => m_ImageInfos.ContainsKey(p_ReplaceCharacter);
        /// <summary>
        /// Try get image info for replace character
        /// </summary>
        /// <param name="p_ImageID">ID of the image</param>
        /// <param name="p_ImageInfo">Result image info</param>
        /// <returns></returns>
        internal bool TryGetImageInfo(uint p_ReplaceCharacter, out CP_SDK.Unity.EnhancedImage p_ImageInfo)
            => m_ImageInfos.TryGetValue(p_ReplaceCharacter, out p_ImageInfo);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try register image info
        /// </summary>
        /// <param name="p_ImageInfo">Image info to register</param>
        /// <param name="p_ReplaceCharacter">Out replace character</param>
        /// <returns></returns>
        internal bool TryRegisterImageInfo(CP_SDK.Unity.EnhancedImage p_ImageInfo, out uint p_ReplaceCharacter)
        {
            p_ReplaceCharacter = 0;

            if (p_ImageInfo == null)
                return false;

            if (!m_ReplaceCharacters.ContainsKey(p_ImageInfo.ImageID))
            {
                lock (m_Lock)
                {
                    if (m_ReplaceCharacters.TryGetValue(p_ImageInfo.ImageID, out var l_ExistingReplaceCharacter))
                    {
                        p_ReplaceCharacter = l_ExistingReplaceCharacter;
                        return true;
                    }

                    uint l_ReplaceCharacter;
                    do
                        l_ReplaceCharacter = GetNextReplaceChar();
                    while (Font.characterLookupTable.ContainsKey(l_ReplaceCharacter));

                    var l_Glypth = new Glyph(l_ReplaceCharacter, new GlyphMetrics(0, 0, 0, 0, p_ImageInfo.Width), new GlyphRect(0, 0, 0, 0));

#if BEATSABER || UNITY_TESTING || SYNTHRIDERS || AUDIOTRIP || BOOMBOX || DANCEDASH
                    Font.characterLookupTable.Add(l_ReplaceCharacter, new TMP_Character(l_ReplaceCharacter, Font, l_Glypth));
#else
#error Missing game implementation
#endif

                    m_ReplaceCharacters.TryAdd(p_ImageInfo.ImageID, l_ReplaceCharacter);
                    m_ImageInfos.TryAdd(l_ReplaceCharacter, p_ImageInfo);

                    p_ReplaceCharacter = l_ReplaceCharacter;
                }

                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get next replacement character
        /// </summary>
        /// <returns></returns>
        private UInt32 GetNextReplaceChar()
        {
            uint l_Result = m_NextReplaceChar++;

            /// If we used up all the Private Use Area characters, move onto Supplementary Private Use Area-A
            if (m_NextReplaceChar > 0xF8FF && m_NextReplaceChar < 0xF0000)
            {
                Logger.Instance.Error("[Modules.Chat.Extensions][EnhancedFontInfo.GetNextReplaceChar] Font is out of characters! Switching to overflow range.");
                m_NextReplaceChar = 0xF0000;
            }

            return l_Result;
        }
    }
}
