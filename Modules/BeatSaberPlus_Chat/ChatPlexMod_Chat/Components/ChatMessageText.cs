using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.Components
{
    /// <summary>
    /// Enhanced TextMeshProUGUI
    /// </summary>
    internal class ChatMessageText : TextMeshProUGUI
    {
        /// <summary>
        /// Chat message data
        /// </summary>
        internal IChatMessage ChatMessage { get; set; } = null;
        /// <summary>
        /// Font reference
        /// </summary>
        internal Extensions.EnhancedFontInfo FontInfo { get; set; } = null;
        /// <summary>
        /// When the rebuild is complete
        /// </summary>
        internal event Action OnLatePreRenderRebuildComplete;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current images
        /// </summary>
        private List<ChatImage> m_CurrentImages;
        /// <summary>
        /// Image pool
        /// </summary>
        private CP_SDK.Pool.ObjectPool<ChatImage> m_ImagePool;
        /// <summary>
        /// Images to add
        /// </summary>
        private List<(Vector3, CP_SDK.Unity.EnhancedImage)> m_ImagesToAdd;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Component first frame
        /// </summary>
        private new void Awake()
        {
            base.Awake();

            m_CurrentImages = new List<ChatImage>(20);

            m_ImagePool = new CP_SDK.Pool.ObjectPool<ChatImage>(
                createFunc: () =>
                {
                    var l_Image = new GameObject().AddComponent<ChatImage>();
                    l_Image.rectTransform.SetParent(rectTransform, false);
                    l_Image.rectTransform.anchorMin     = new Vector2(0.5f, 0.5f);
                    l_Image.rectTransform.anchorMax     = new Vector2(0.5f, 0.5f);
                    l_Image.rectTransform.pivot         = new Vector2(0, 0);
                    l_Image.rectTransform.localRotation = Quaternion.identity;
                    l_Image.material                    = CP_SDK.UI.UISystem.Override_GetUIMaterial();
                    l_Image.color                       = Color.white;
                    l_Image.raycastTarget               = false;
                    l_Image.AnimStateUpdater            = l_Image.gameObject.AddComponent<CP_SDK.Animation.AnimationStateUpdater>();
                    l_Image.AnimStateUpdater.TargetImage= l_Image;

                    l_Image.gameObject.layer = CP_SDK.UI.UISystem.UILayer;
                    l_Image.gameObject.SetActive(false);

                    return l_Image;
                },
                actionOnGet: (p_Image) =>
                {
                    p_Image.gameObject.SetActive(true);
                },
                actionOnRelease: (p_Image) =>
                {
                    try
                    {
                        p_Image.AnimStateUpdater.ControllerDataInstance = null;

                        p_Image.gameObject.SetActive(false);
                    }
                    catch (Exception p_Exception)
                    {
                        Logger.Instance.Error("Exception while freeing EnhancedImage in EnhancedTextMeshProUGUI.");
                        Logger.Instance.Error(p_Exception);
                    }
                },
                actionOnDestroy: (p_Image) =>
                {
                    GameObject.Destroy(p_Image.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: 10
            );

            m_ImagesToAdd = new List<(Vector3, CP_SDK.Unity.EnhancedImage)>(20);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set text
        /// </summary>
        /// <param name="p_Text">New text value</param>
        internal void ReplaceContent(string p_Text)
        {
            if (text != p_Text)
                text = p_Text;
            else /// Only rebuild emotes
            {
                for (int l_I = 0; l_I < m_CurrentImages.Count; ++l_I)
                    m_ImagePool.Release(m_CurrentImages[l_I]);

                RebuildImagesInternal();

                /// Delay to next frame
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(OnLatePreRenderRebuildComplete);
            }
        }
        /// <summary>
        /// Clear image
        /// </summary>
        internal void ClearImages()
        {
            var l_CountToClear = m_CurrentImages.Count;
            while (l_CountToClear-- > 0)
                m_ImagePool.Release(m_CurrentImages[l_CountToClear]);

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
                if (textInfo != null)
                {
                    m_ImagesToAdd.Clear();
                    for (int l_I = 0; l_I < textInfo.characterCount; l_I++)
                    {
                        /// Skip invisible/empty/out of range chars
                        TMP_CharacterInfo l_CharacterInfo = textInfo.characterInfo[l_I];
                        if (!l_CharacterInfo.isVisible || l_CharacterInfo.index >= text.Length)
                            continue;

                        /// If it's a surrogate pair, convert the character
                        uint l_Character = text[l_CharacterInfo.index];
                        if (l_CharacterInfo.index + 1 < text.Length && char.IsSurrogatePair(text[l_CharacterInfo.index], text[l_CharacterInfo.index + 1]))
                            l_Character = (uint)char.ConvertToUtf32(text[l_CharacterInfo.index], text[l_CharacterInfo.index + 1]);

                        /// Skip characters that have no imageInfo registered
                        if (FontInfo == null || !FontInfo.TryGetImageInfo(l_Character, out var l_ImageInfo))
                            continue;

                        m_ImagesToAdd.Add((l_CharacterInfo.topLeft, l_ImageInfo));
                    }

                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => RebuildImagesInternal());
                }
            }

            base.Rebuild(p_UpdateType);

            if (p_UpdateType == CanvasUpdate.LatePreRender)
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => {
                    if (this)
                        OnLatePreRenderRebuildComplete?.Invoke();
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuild images
        /// </summary>
        private void RebuildImagesInternal()
        {
            if (!this)
                return;

            var l_ScaleFactor       = (float)((double)m_currentFontSize / (double)m_currentFontAsset.faceInfo.pointSize * (double)m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1.0 : 0.1f));
            var l_LocalScale        = new Vector3(l_ScaleFactor * 1.08f, l_ScaleFactor * 1.08f, l_ScaleFactor * 1.08f);
            var l_ImagesToAddCount  = m_ImagesToAdd.Count;
            var l_AlreadyAllocated  = m_CurrentImages.Count;
            var l_Iterator          = 0;

            /// Clear
            if (l_ImagesToAddCount < l_AlreadyAllocated)
            {
                for (var l_I = l_ImagesToAddCount; l_I < l_AlreadyAllocated; ++l_I)
                    m_ImagePool.Release(m_CurrentImages[l_I]);

                m_CurrentImages.RemoveRange(l_ImagesToAddCount, l_AlreadyAllocated - l_ImagesToAddCount);
            }

            /// Reuse
            for (; l_Iterator < l_AlreadyAllocated && l_Iterator < l_ImagesToAddCount; ++l_Iterator)
            {
                var l_Infos = m_ImagesToAdd[l_Iterator];
                var l_Image = m_CurrentImages[l_Iterator];

                if (l_Infos.Item2.AnimControllerData != null)
                {
                    l_Image.AnimStateUpdater.ControllerDataInstance = l_Infos.Item2.AnimControllerData;
                    l_Image.sprite                                  = l_Infos.Item2.AnimControllerData.Frames[l_Infos.Item2.AnimControllerData.CurrentFrameIndex];
                }
                else
                {
                    l_Image.AnimStateUpdater.ControllerDataInstance = null;
                    l_Image.sprite                                  = l_Infos.Item2.Sprite;
                }

                var l_RectTransform = l_Image.rectTransform;
                l_RectTransform.localScale    = l_LocalScale;
                l_RectTransform.sizeDelta     = new Vector2(l_Infos.Item2.Width, l_Infos.Item2.Height);
                l_RectTransform.localPosition = l_Infos.Item1 - new Vector3(0, l_Infos.Item2.Height * l_ScaleFactor * 0.558f / 2);
            }

            /// Allocate
            for (; l_Iterator < l_ImagesToAddCount; ++l_Iterator)
            {
                var l_Infos = m_ImagesToAdd[l_Iterator];
                var l_Image = m_ImagePool.Get();

                if (l_Infos.Item2.AnimControllerData != null)
                {
                    l_Image.AnimStateUpdater.ControllerDataInstance = l_Infos.Item2.AnimControllerData;
                    l_Image.sprite                                  = l_Infos.Item2.AnimControllerData.Frames[l_Infos.Item2.AnimControllerData.CurrentFrameIndex];
                }
                else
                {
                    l_Image.AnimStateUpdater.ControllerDataInstance = null;
                    l_Image.sprite                                  = l_Infos.Item2.Sprite;
                }

                var l_RectTransform = l_Image.rectTransform;
                l_RectTransform.localScale    = l_LocalScale;
                l_RectTransform.sizeDelta     = new Vector2(l_Infos.Item2.Width, l_Infos.Item2.Height);
                l_RectTransform.localPosition = l_Infos.Item1 - new Vector3(0, l_Infos.Item2.Height * l_ScaleFactor * 0.558f / 2);

                m_CurrentImages.Add(l_Image);
            }
        }
    }
}
