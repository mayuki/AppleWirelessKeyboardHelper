using System;
using System.Collections.Generic;
using System.Text;
using WaveLib.AudioMixer;
using Misuzilla.InteropServices.AudioEndpointVolume;
using Interop.MMDeviceAPI;
using System.Runtime.InteropServices;

namespace MasterVolumeControlLibrary
{
    public abstract class MasterVolumeControl : IDisposable
    {
        public abstract void VolumeUp();
        public abstract void VolumeDown();
        public abstract Boolean Mute { get; set; }

        public static MasterVolumeControl GetControl()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major >= 6) // Vista or later
            {
                return new MasterVolumeControlCoreAudio();
            }
            else
            {
                return new MasterVolumeControlLegacy();
            }
        }

        #region IDisposable ÉÅÉìÉo

        public abstract void Dispose();

        #endregion
    }

    public class MasterVolumeControlLegacy : MasterVolumeControl
    {
        private Mixers _mixers;
        public MasterVolumeControlLegacy()
        {
            _mixers = new Mixers();
            _mixers.Playback.DeviceId = -1;
        }

        public override void VolumeUp()
        {
            MixerLine masterLine = GetMasterLine();
            masterLine.Volume = Math.Min(masterLine.Volume + masterLine.VolumeMax / 10, masterLine.VolumeMax);
        }

        public override void VolumeDown()
        {
            MixerLine masterLine = GetMasterLine();
            masterLine.Volume = Math.Max(masterLine.Volume - masterLine.VolumeMax / 10, masterLine.VolumeMin);
        }

        public override bool Mute
        {
            get
            {
                return GetMasterLine().Mute;
            }
            set
            {
                GetMasterLine().Mute = value;
            }
        }

        public override void Dispose()
        {
            _mixers = null;
        }

        private MixerLine GetMasterLine()
        {
            return _mixers.Playback.Lines.GetMixerFirstLineByComponentType(MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
        }
    }

    public class MasterVolumeControlCoreAudio : MasterVolumeControl
    {
        private const UInt32 CLSCTX_ALL = 0x17;
        private Guid guidDummy = Guid.Empty;
        private IAudioEndpointVolume endPointVol;

        public MasterVolumeControlCoreAudio()
        {
            endPointVol = GetAudioEndpointVolume();
        }

        public override void VolumeUp()
        {
            endPointVol.VolumeStepUp(ref guidDummy);
        }

        public override void VolumeDown()
        {
            endPointVol.VolumeStepDown(ref guidDummy);
        }

        public override Boolean Mute
        {
            set
            {
                endPointVol.SetMute(value, ref guidDummy);
            }
            get
            {
                return endPointVol.GetMute();
            }
        }

        private IAudioEndpointVolume GetAudioEndpointVolume()
        {
            MMDeviceEnumeratorClass devEnum = new MMDeviceEnumeratorClass();
            IMMDevice endPoint;
            devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out endPoint);
            Marshal.ReleaseComObject(devEnum);

            Guid IID_IAudioEndpointVolume = new Guid(IID.IAudioEndpointVolume);
            tag_inner_PROPVARIANT prop = new tag_inner_PROPVARIANT();
            IntPtr ppInterface;
            endPoint.Activate(ref IID_IAudioEndpointVolume, CLSCTX_ALL, ref prop, out ppInterface);
            IAudioEndpointVolume endPointVol = Marshal.GetObjectForIUnknown(ppInterface) as IAudioEndpointVolume;

            return endPointVol;
        }

        #region IDisposable ÉÅÉìÉo

        public override void Dispose()
        {
            Marshal.ReleaseComObject(endPointVol);
            endPointVol = null;
        }

        #endregion
    }
}
