// $Id$
#region LICENSE
/*
 * This source-code licensed under Public Domain
 */
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

//
// Windows SDK 6.0 / endpointvolume.h
//
namespace Misuzilla.InteropServices.AudioEndpointVolume
{
    public static class IID
    {
        public const String IAudioEndpointVolume = "5CDF2C82-841E-4546-9722-0CF74078229A";
        public const String IAudioEndpointVolumeCallback = "657804FA-D6AD-4496-8A60-352752AF4F89";
        public const String IAudioMeterInformation = "C02216F6-8C67-4B5B-9D00-D008E73E0064";
    }
    
    [Guid(IID.IAudioEndpointVolume)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolume
    {
        void RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        void UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        UInt32 GetChannelCount();
        void SetMasterVolumeLevel(Single fLevelDB, ref Guid pguidEventContext);
        void SetMasterVolumeLevelScalar(Single fLevelDB, ref Guid pguidEventContext);
        Single GetMasterVolumeLevel();
        Single GetMasterVolumeLevelScalar();
        void SetChannelVolumeLevel(UInt32 nChannel, Single fLevelDB, ref Guid pguidEventContext);
        void SetChannelVolumeLevelScalar(UInt32 nChannel, Single fLevelDB, ref Guid pguidEventContext);
        Single GetMasterVolumeLevel(UInt32 nChannel);
        Single GetMasterVolumeLevelScalar(UInt32 nChannel);
        void SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, ref Guid pguidEventContext);
        [return: MarshalAs(UnmanagedType.Bool)]
        Boolean GetMute();
        void GetVolumeStepInfo(out UInt32 pnStep, out UInt32 pnStepCount);
        void VolumeStepUp(ref Guid pguidEventContext);
        void VolumeStepDown(ref Guid pguidEventContext);
        void QueryHardwareSupport();
        void GetVolumeRange(Single pflVolumeMindB, Single pflVolumeMaxdb, Single pfVolumeIncrementdB);
    }
    
    [Guid(IID.IAudioEndpointVolumeCallback)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback
    {
        void OnNotify(AUDIO_VOLUME_NOTIFICATION_DATA pNotify);
    }

    [Guid(IID.IAudioMeterInformation)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioMeterInformation
    {
        Single GetPeakValue();
        UInt32 GetMeteringChannelCount();
        Single GetChannelsPeakValues(UInt32 u32ChannelCount);
        UInt32 QueryHardwareSupport();
    }

    [StructLayout(LayoutKind.Sequential)]
    public class AUDIO_VOLUME_NOTIFICATION_DATA
    {
        public Guid guidEventContext;
        public Boolean bMuted;
        public Single fMasterVolume;
        public UInt32 nChannels;
        public Single afChannelVolumes;//[ 1 ];
    }
}
