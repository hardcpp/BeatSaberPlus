using System;
using System.Runtime.InteropServices;

namespace BeatSaberPlus.SDK.Animation.Natives
{
    public class WEBP
    {
        private static readonly int WEBP_DECODER_ABI_VERSION = 0x0209;

        #region Types
        public enum VP8StatusCode
        {
            VP8_STATUS_OK = 0,
            VP8_STATUS_OUT_OF_MEMORY,
            VP8_STATUS_INVALID_PARAM,
            VP8_STATUS_BITSTREAM_ERROR,
            VP8_STATUS_UNSUPPORTED_FEATURE,
            VP8_STATUS_SUSPENDED,
            VP8_STATUS_USER_ABORT,
            VP8_STATUS_NOT_ENOUGH_DATA
        }
        public enum WEBP_CSP_MODE
        {
            MODE_RGB = 0, MODE_RGBA = 1,
            MODE_BGR = 2, MODE_BGRA = 3,
            MODE_ARGB = 4, MODE_RGBA_4444 = 5,
            MODE_RGB_565 = 6,
            // RGB-premultiplied transparent modes (alpha value is preserved)
            MODE_rgbA = 7,
            MODE_bgrA = 8,
            MODE_Argb = 9,
            MODE_rgbA_4444 = 10,
            // YUV modes must come after RGB ones.
            MODE_YUV = 11, MODE_YUVA = 12,  // yuv 4:2:0
            MODE_LAST = 13
        }
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct WebPBitstreamFeatures
        {
            public int width;
            public int height;
            public int has_alpha;
            public int has_animation;
            public int format;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
            private readonly uint[] pad;
        };
        #endregion

        #region Methods
        public static VP8StatusCode WebPGetFeatures(byte[] rawWebP, ref WebPBitstreamFeatures features)
        {
            var l_NativeArray   = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            var l_Result        = VP8StatusCode.VP8_STATUS_INVALID_PARAM;

            try
            {
                l_Result = Native_WebPGetFeaturesInternal(l_NativeArray.AddrOfPinnedObject(), (UIntPtr)rawWebP.Length, ref features, WEBP_DECODER_ABI_VERSION);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Animation.WEBP.Natives][WEBP.WebPGetFeatures] Error:");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                if (l_NativeArray.IsAllocated) l_NativeArray.Free();
            }

            return l_Result;
        }
        public static int WebPDecodeBGRInto(byte[] data, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            var l_NativeArray   = GCHandle.Alloc(data, GCHandleType.Pinned);
            var l_Result        = 0;

            try
            {
                l_Result = Native_WebPDecodeBGRInto(l_NativeArray.AddrOfPinnedObject(), (UIntPtr)data.Length, output_buffer, output_buffer_size, output_stride);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Animation.WEBP.Natives][WEBP.WebPDecodeBGRInto] Error:");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                if (l_NativeArray.IsAllocated) l_NativeArray.Free();
            }

            return l_Result;
        }
        public static int WebPDecodeBGRAInto(byte[] data, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            var l_NativeArray   = GCHandle.Alloc(data, GCHandleType.Pinned);
            var l_Result        = 0;

            try
            {
                l_Result = Native_WebPDecodeBGRAInto(l_NativeArray.AddrOfPinnedObject(), (UIntPtr)data.Length, output_buffer, output_buffer_size, output_stride);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Animation.WEBP.Natives][WEBP.WebPDecodeBGRInto] Error:");
                Logger.Instance.Error(l_Exception);
            }
            finally
            {
                if (l_NativeArray.IsAllocated) l_NativeArray.Free();
            }

            return l_Result;
        }
        #endregion

        #region Natives
        [DllImport("Libs/Natives/libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode Native_WebPGetFeaturesInternal([In()] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);
        [DllImport("Libs/Natives/libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int Native_WebPDecodeBGRInto([In()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);
        [DllImport("Libs/Natives/libwebp.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
        private static extern int Native_WebPDecodeBGRAInto([In()] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);
        #endregion
    }
}
