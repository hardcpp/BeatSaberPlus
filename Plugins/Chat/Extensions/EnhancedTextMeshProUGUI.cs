using BeatSaberMarkupLanguage.Animations;
using BeatSaberPlus.Utils;
using BeatSaberPlusChatCore.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    /// Enhanced TextMeshProUGUI
    /// </summary>
    internal class EnhancedTextMeshProUGUI : TextMeshProUGUI
    {
        /// <summary>
        /// Chat message data
        /// </summary>
        internal IChatMessage ChatMessage { get; set; } = null;
        /// <summary>
        /// Font reference
        /// </summary>
        internal EnhancedFontInfo FontInfo { get; set; } = null;
        /// <summary>
        /// When the rebuild is complete
        /// </summary>
        internal event Action OnLatePreRenderRebuildComplete;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Mutex
        /// </summary>
        private static object m_Lock = new object();
        /// <summary>
        /// Current images
        /// </summary>
        private List<EnhancedImage> m_CurrentImages = new List<EnhancedImage>();
        /// <summary>
        /// Image pool
        /// </summary>
        private static ObjectPool<EnhancedImage> m_ImagePool = new ObjectPool<EnhancedImage>(50,
            p_Constructor: () =>
            {
                var l_Image = new GameObject().AddComponent<EnhancedImage>();
                l_Image.color                   = Color.white;
                l_Image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                l_Image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                l_Image.rectTransform.pivot     = new Vector2(0, 0);
                l_Image.AnimStateUpdater        = l_Image.gameObject.AddComponent<AnimationStateUpdater>();
                l_Image.AnimStateUpdater.image  = l_Image;
                l_Image.gameObject.SetActive(false);

                DontDestroyOnLoad(l_Image.gameObject);

                return l_Image;
            },
            p_OnFree: (p_Image) =>
            {
                try
                {
                    p_Image.AnimStateUpdater.controllerData = null;
                    p_Image.gameObject.SetActive(false);
                    p_Image.rectTransform.SetParent(null);
                    p_Image.sprite = null;
                }
                catch (Exception p_Exception)
                {
                    Logger.Instance?.Error("Exception while freeing EnhancedImage in EnhancedTextMeshProUGUI.");
                    Logger.Instance?.Error(p_Exception);
                }
            }
        );

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear image
        /// </summary>
        internal void ClearImages()
        {
            foreach (var l_Current in m_CurrentImages)
                m_ImagePool.Free(l_Current);

            m_CurrentImages.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuild the text
        /// </summary>
        /// <param name="p_UpdateType">Update type</param>
        public override void Rebuild(CanvasUpdate p_UpdateType)
        {
            if (p_UpdateType == CanvasUpdate.LatePreRender)
            {
                MainThreadInvoker.Invoke(() => ClearImages());

                for (int l_I = 0; l_I < textInfo.characterCount; l_I++)
                {
                    /// Skip invisible/empty/out of range chars
                    TMP_CharacterInfo l_CharacterInfo = textInfo.characterInfo[l_I];
                    if (!l_CharacterInfo.isVisible || string.IsNullOrEmpty(text) || l_CharacterInfo.index >= text.Length)
                        continue;

                    /// If it's a surrogate pair, convert the character
                    uint l_Character = text[l_CharacterInfo.index];
                    if (l_CharacterInfo.index + 1 < text.Length && char.IsSurrogatePair(text[l_CharacterInfo.index], text[l_CharacterInfo.index + 1]))
                        l_Character = (uint)char.ConvertToUtf32(text[l_CharacterInfo.index], text[l_CharacterInfo.index + 1]);

                    /// Skip characters that have no imageInfo registered
                    if (FontInfo == null || !FontInfo.TryGetImageInfo(l_Character, out var l_ImageInfo) || l_ImageInfo is null)
                        continue;

                    MainThreadInvoker.Invoke(() =>
                    {
                        var l_Image = m_ImagePool.Alloc();
                        try
                        {
                            if (l_ImageInfo.AnimControllerData != null)
                            {
                                l_Image.AnimStateUpdater.controllerData = l_ImageInfo.AnimControllerData;
                                l_Image.sprite                          = l_ImageInfo.AnimControllerData.sprites[l_ImageInfo.AnimControllerData.uvIndex];
                            }
                            else
                                l_Image.sprite = l_ImageInfo.Sprite;

                            l_Image.material                    = BeatSaberPlus.Utils.UnityMaterial.UINoGlowMaterial;
                            l_Image.rectTransform.localScale    = new Vector3(fontScale * 1.08f, fontScale * 1.08f, fontScale * 1.08f);
                            l_Image.rectTransform.sizeDelta     = new Vector2(l_ImageInfo.Width, l_ImageInfo.Height);
                            l_Image.rectTransform.SetParent(rectTransform, false);
                            l_Image.rectTransform.localPosition = l_CharacterInfo.topLeft - new Vector3(0, l_ImageInfo.Height * fontScale * 0.558f / 2);
                            l_Image.rectTransform.localRotation = Quaternion.identity;
                            l_Image.gameObject.SetActive(true);
                            m_CurrentImages.Add(l_Image);
                        }
                        catch (Exception p_Exception)
                        {
                            Logger.Instance?.Error("Exception while trying to overlay sprite");
                            Logger.Instance?.Error(p_Exception);

                            m_ImagePool.Free(l_Image);
                        }
                    });
                }
            }

            base.Rebuild(p_UpdateType);

            if (p_UpdateType == CanvasUpdate.LatePreRender)
                MainThreadInvoker.Invoke(OnLatePreRenderRebuildComplete);
        }
    }
}
