using System;
using System.Runtime.InteropServices;

namespace CP_SDK.Animation.WEBP.Natives
{
    public class WEBPDemux
    {
        private static readonly int WEBP_DEMUX_ABI_VERSION = 0x0107;

        #region Types
        [StructLayout(LayoutKind.Sequential)]
        public struct WebPData
        {
            public IntPtr bytes;
            public UIntPtr size;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct WebPAnimDecoderOptions
        {
            public WEBP.WEBP_CSP_MODE color_mode;
            public int use_threads;
            private readonly int pad1;
            private readonly int pad2;
            private readonly int pad3;
            private readonly int pad4;
            private readonly int pad5;
            private readonly int pad6;
            private readonly int pad7;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct WebPAnimInfo
        {
            public UInt32 canvas_width;
            public UInt32 canvas_height;
            public UInt32 loop_count;
            public UInt32 bgcolor;
            public UInt32 frame_count;
            private readonly int pad1;
            private readonly int pad2;
            private readonly int pad3;
            private readonly int pad4;
        }
        #endregion

        #region Methods
        public static IntPtr WebPAnimDecoderNew(byte[] webp_data)
        {
            var l_NativeArray   = GCHandle.Alloc(webp_data, GCHandleType.Pinned);
            var l_Result        = IntPtr.Zero;

            try
            {
                var l_WEBPData = new WebPData();
                l_WEBPData.bytes = l_NativeArray.AddrOfPinnedObject();
                l_WEBPData.size = (UIntPtr)webp_data.Length;

                l_Result = Native_WebPAnimDecoderNewInternal(ref l_WEBPData, 0, (ulong)WEBP_DEMUX_ABI_VERSION);
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP.Natives][WEBPDemux.WebPAnimDecoderNew] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            finally
            {
                if (l_NativeArray.IsAllocated) l_NativeArray.Free();
            }

            return l_Result;
        }
        public static void WebPAnimDecoderDelete(IntPtr dec)
        {
            Native_WebPAnimDecoderDelete(dec);
        }
        public static bool WebPAnimDecoderGetInfo(IntPtr dec, ref WebPAnimInfo info)
        {
            return Native_WebPAnimDecoderGetInfo(dec, ref info) == 1;
        }
        public static bool WebPAnimDecoderHasMoreFrames(IntPtr dec)
        {
            return Native_WebPAnimDecoderHasMoreFrames(dec) == 1;
        }
        public static bool WebPAnimDecoderGetNext(IntPtr dec, byte[] outputbuffer, ref int timestamp)
        {
            var l_ResultPtr     = IntPtr.Zero;
            var l_Result        = false;

            try
            {
                l_Result = Native_WebPAnimDecoderGetNext(dec, ref l_ResultPtr, ref timestamp) == 1;
                if (l_Result)
                    Marshal.Copy(l_ResultPtr, outputbuffer, 0, outputbuffer.Length);
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP.Natives][WEBPDemux.WebPAnimDecoderGetNext] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return l_Result;
        }
        #endregion

        #region Natives
        [DllImport("Libs/Natives/libwebpdemux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPAnimDecoderNewInternal")]
        private static extern IntPtr Native_WebPAnimDecoderNewInternal(ref WebPData buffer, UInt64 dec_options, UInt64 abi_version);
        [DllImport("Libs/Natives/libwebpdemux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPAnimDecoderDelete")]
        private static extern int Native_WebPAnimDecoderDelete(IntPtr dec);
        [DllImport("Libs/Natives/libwebpdemux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPAnimDecoderGetInfo")]
        private static extern int Native_WebPAnimDecoderGetInfo(IntPtr dec, ref WebPAnimInfo info);
        [DllImport("Libs/Natives/libwebpdemux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPAnimDecoderHasMoreFrames")]
        private static extern int Native_WebPAnimDecoderHasMoreFrames(IntPtr dec);
        [DllImport("Libs/Natives/libwebpdemux.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPAnimDecoderGetNext")]
        private static extern int Native_WebPAnimDecoderGetNext(IntPtr dec, ref IntPtr output_buffer, ref int timestamp);
        #endregion
    }
}
