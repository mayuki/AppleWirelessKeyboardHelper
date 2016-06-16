using System;
using System.Management;

namespace MonitorBrightnessControlLibrary
{
    public abstract class MonitorBrightnessControl : IDisposable
    {
        public abstract void BrightnessUp();
        public abstract void BrightnessDown();

        public static MonitorBrightnessControl GetControl()
        {

            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major >= 6) // WMI is supported Vista or later
            {
                return new MonitorBrightnessControlWMI();
            }
            else // XP or older has no support of WMI
            {
                return null;
            }
        }
        #region IDisposable member

        public abstract void Dispose();

        #endregion
    }

    public class MonitorBrightnessControlWMI : MonitorBrightnessControl
    {
        private Byte _curBrightness;
        private Byte[] _brTable;
        private UInt32 levels;

        public MonitorBrightnessControlWMI()
        {

            using (var _Brightness =
                new ManagementClass("root/wmi", "WmiMonitorBrightness", null))
            using (var _BrightnessMethods =
                new ManagementClass("root/wmi", "WmiMonitorBrightnessMethods", null))
            {
                foreach (ManagementObject mo in _Brightness.GetInstances())
                {
                    levels = (UInt32)mo["Levels"];
                    if (levels <= 0)
                    {
                        break;
                    }
                    _brTable = new Byte[levels];
                    _brTable = (Byte[])mo["Level"];
                }
            }
        }


        public override void BrightnessUp()
        {

            GetBrightness();

            int i;
            for (i = 0; i < levels - 1 && _brTable[i] <= _curBrightness; i++)
            {
                ;
            }
            SetBrightness(_brTable[i]);
        }

        public override void BrightnessDown()
        {

            GetBrightness();

            int i;
            for (i = (int)levels - 1; i > 0 && _brTable[i] >= _curBrightness; i--)
            {
                ;
            }
            SetBrightness(_brTable[i]);
        }

        public void GetBrightness()
        {
            using (var _Brightness =
                new ManagementClass("root/wmi", "WmiMonitorBrightness", null))
            using (var _BrightnessMethods =
                new ManagementClass("root/wmi", "WmiMonitorBrightnessMethods", null))
            {
                foreach (ManagementObject mo in _Brightness.GetInstances())
                {
                    _curBrightness = (Byte)mo["CurrentBrightness"];
                    break; //get first result only
                }
            }
        }

        public void SetBrightness(Byte brightness)
        {

            using (var _Brightness =
                new ManagementClass("root/wmi", "WmiMonitorBrightness", null))
            using (var _BrightnessMethods =
                new ManagementClass("root/wmi", "WmiMonitorBrightnessMethods", null))
            using (var inParams = _BrightnessMethods.GetMethodParameters("WmiSetBrightness"))
            {
                foreach (ManagementObject mo in _BrightnessMethods.GetInstances())
                {
                    inParams["Brightness"] = brightness; // set brightness to brightness %
                    inParams["Timeout"] = 1;
                    mo.InvokeMethod("WmiSetBrightness", inParams, null);
                    break;
                }
            }
        }

        #region iDisposable member
        public override void Dispose()
        {
            //            Marshal.ReleaseComObject(_Brightness);
            //            _Brightness.Dispose();
            //            Marshal.ReleaseComObject(_BrightnessMethods);
            //            _BrightnessMethods = null;
        }

        #endregion\
    }
}
