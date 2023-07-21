#if CP_SDK_UNITY
using System;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Sprite helper
    /// </summary>
    public class SpriteU
    {
        /// <summary>
        /// Create sprite from texture
        /// </summary>
        /// <param name="p_Texture">Source texture</param>
        /// <param name="p_PixelsPerUnit">Pixels per unit multiplier</param>
        /// <param name="p_Pivot">Pivot point</param>
        /// <param name="p_Extrude">Extrude amount</param>
        /// <param name="p_Type">Sprite mesh type</param>
        /// <returns></returns>
        public static Sprite CreateFromTextureWithBorders(  Texture2D       p_Texture,
                                                            float           p_PixelsPerUnit = 100.0f,
                                                            Vector2         p_Pivot         = default,
                                                            uint            p_Extrude       = 0,
                                                            SpriteMeshType  p_Type          = SpriteMeshType.FullRect,
                                                            Vector4         p_Borders       = default)
        {
            if (p_Texture != null && p_Texture)
            {
                var l_Sprite = UnityEngine.Sprite.Create(p_Texture, new Rect(0, 0, p_Texture.width, p_Texture.height), p_Pivot, p_PixelsPerUnit, p_Extrude, p_Type, p_Borders);
                l_Sprite.texture.wrapMode = TextureWrapMode.Clamp;

                return l_Sprite;
            }

            return null;
        }
        /// <returns></returns>
        /// <summary>
        /// Create sprite from raw bytes
        /// </summary>
        /// <param name="p_Bytes">Raw data</param>
        /// <param name="p_PixelsPerUnit">Pixels per unit multiplier</param>
        /// <param name="p_Pivot">Pivot point</param>
        /// <param name="p_Extrude">Extrude amount</param>
        /// <param name="p_Type">Sprite mesh type</param>
        /// <param name="p_Borders">Borders</param>
        /// <returns></returns>
        public static Sprite CreateFromRawWithBorders(  byte[]          p_Bytes,
                                                        float           p_PixelsPerUnit = 100.0f,
                                                        Vector2         p_Pivot         = default,
                                                        uint            p_Extrude       = 0,
                                                        SpriteMeshType  p_Type          = SpriteMeshType.FullRect,
                                                        Vector4         p_Borders       = default)
            => CreateFromTextureWithBorders(Texture2DU.CreateFromRaw(p_Bytes), p_PixelsPerUnit, p_Pivot, p_Extrude, p_Type, p_Borders);
        /// <summary>
        /// Create from raw threaded
        /// </summary>
        /// <param name="p_Bytes">Raw data</param>
        /// <param name="p_Callback">On result callback</param>
        /// <param name="p_PixelsPerUnit">Pixels per unit multiplier</param>
        /// <param name="p_Pivot">Pivot point</param>
        /// <param name="p_Extrude">Extrude amount</param>
        /// <param name="p_Type">Sprite mesh type</param>
        /// <param name="p_Borders">Borders</param>
        public static void CreateFromRawWithBordersThreaded(byte[]          p_Bytes,
                                                            Action<Sprite>  p_Callback,
                                                            float           p_PixelsPerUnit = 100.0f,
                                                            Vector2         p_Pivot         = default,
                                                            uint            p_Extrude       = 0,
                                                            SpriteMeshType  p_Type          = SpriteMeshType.FullRect,
                                                            Vector4         p_Borders       = default)
        {
            Texture2DU.CreateFromRawEx(p_Bytes, (p_Texture) =>
            {
                var l_Sprite = null as Sprite;
                if (p_Texture != null)
                    l_Sprite = CreateFromTextureWithBorders(p_Texture, p_PixelsPerUnit, p_Pivot, p_Extrude, p_Type, p_Borders);

                p_Callback?.Invoke(l_Sprite);
            });
        }
    }
}
#endif
