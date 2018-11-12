#if UNITY_IOS || (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
#if (UNITY_IPHONE && !UNITY_EDITOR) || __IOS__
#define DLL_IMPORT_INTERNAL
#endif
using System;
using System.Runtime.InteropServices;

namespace Photon.Voice.Apple
{
    public class AppleAudioInReader : Photon.Voice.IAudioReader<float>
    {
#if DLL_IMPORT_INTERNAL
		const string lib_name = "__Internal";
#else
        const string lib_name = "AudioIn";
#endif
        [DllImport(lib_name)]
        private static extern IntPtr Photon_Audio_In_CreateReader(int deviceID);
        [DllImport(lib_name)]
        private static extern void Photon_Audio_In_Destroy(IntPtr handler);
        [DllImport(lib_name)]
        private static extern bool Photon_Audio_In_Read(IntPtr handle, float[] buf, int len);

        IntPtr audioIn;

        public AppleAudioInReader(int deviceID, ILogger logger)
        {
            try
            {
                audioIn = Photon_Audio_In_CreateReader(deviceID);
            }
            catch (Exception e)
            {
                Error = e.ToString();
                if (Error == null) // should never happen but since Error used as validity flag, make sure that it's not null
                {
                    Error = "Exception in AppleAudioInReader constructor";
                }
                logger.LogError("[PV] AppleAudioInReader: " + Error);
            }
        }
        public int Channels { get { return 1; } }

#if (UNITY_IPHONE && !UNITY_EDITOR) || __IOS__
        public int SamplingRate { get { return 48000; } }
#else
		public int SamplingRate { get { return 44100; } }
#endif

        public string Error { get; private set; }

        public void Dispose()
        {
            if (audioIn != IntPtr.Zero)
            {
                Photon_Audio_In_Destroy(audioIn);
            }
        }

        public bool Read(float[] buf)
        {
            return audioIn != IntPtr.Zero && Photon_Audio_In_Read(audioIn, buf, buf.Length);            
        }
    }
}
#endif
