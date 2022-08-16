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
        /// <param name="p_PixelsPerUnit">Pixel per unit</param>
        /// <param name="p_Pivot">Pivot point</param>
        /// <returns></returns>
        public static Sprite CreateFromTexture(Texture2D p_Texture, float p_PixelsPerUnit = 100.0f, Vector2 p_Pivot = default, uint p_Extrude = 0, SpriteMeshType p_Type = SpriteMeshType.FullRect)
        {
            if (p_Texture != null && p_Texture)
            {
                var l_Sprite = UnityEngine.Sprite.Create(p_Texture, new Rect(0, 0, p_Texture.width, p_Texture.height), p_Pivot, p_PixelsPerUnit, p_Extrude, p_Type);
                l_Sprite.texture.wrapMode = TextureWrapMode.Clamp;

                return l_Sprite;
            }

            return null;
        }
        /// <summary>
        /// Create sprite from raw bytes
        /// </summary>
        /// <param name="p_Bytes">Raw bytes array</param>
        /// <param name="p_PixelsPerUnit">Pixel per unit</param>
        /// <returns></returns>
        public static Sprite CreateFromRaw(byte[] p_Bytes, float p_PixelsPerUnit = 100.0f, Vector2 p_Pivot = default, uint p_Extrude = 0, SpriteMeshType p_Type = SpriteMeshType.FullRect)
        {
            return CreateFromTexture(Texture2DU.CreateFromRaw(p_Bytes), p_PixelsPerUnit, p_Pivot, p_Extrude, p_Type);
        }

        public static void CreateFromRawEx(byte[] p_Bytes, Action<Sprite> p_Callback, float p_PixelsPerUnit = 100.0f, Vector2 p_Pivot = default, uint p_Extrude = 0, SpriteMeshType p_Type = SpriteMeshType.FullRect)
        {
            Texture2DU.CreateFromRawEx(p_Bytes, (p_Texture) =>
            {
                var l_Sprite = null as Sprite;
                if (p_Texture != null)
                    l_Sprite = CreateFromTexture(p_Texture, p_PixelsPerUnit, p_Pivot, p_Extrude, p_Type);

                p_Callback?.Invoke(l_Sprite);
            });
        }
    }
}
#endif
