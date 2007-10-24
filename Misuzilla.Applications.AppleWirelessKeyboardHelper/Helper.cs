using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using WiimoteLib;

namespace Misuzilla.Applications.AppleWirelessKeyboardHelper
{
    internal class Helper : IDisposable
    {
        public event EventHandler<AppleKeyboardEventArgs> FnKeyCombinationDown;
        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler Disconnected;

        public Boolean CurrentPowerButtonIsDown;
        public AppleKeyboardKeys CurrentKeyState;

        private Stream _stream;
        private Win32.HookHandle _hHook;

        private const UInt32 VIDApple = 0x5ac;
        private const UInt32 PIDAppleWirelessKeyboardUS = 0x22c;
        private const UInt32 PIDAppleWirelessKeyboardJIS = 0x22e;

        /// <summary>
        /// 
        /// </summary>
        internal Boolean Start()
        {
            if (_stream != null)
                throw new InvalidOperationException("ヘルパーはすでに実行中です。");

            Guid guid;
            HIDImports.HidD_GetHidGuid(out guid);

            IntPtr hDevInfo = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, HIDImports.DIGCF_DEVICEINTERFACE);
            HIDImports.SP_DEVICE_INTERFACE_DATA diData = new HIDImports.SP_DEVICE_INTERFACE_DATA();
            diData.cbSize = Marshal.SizeOf(diData);

            UInt32 index = 0;
            while (HIDImports.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, index++, ref diData))
            {
                HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA();
                diDetail.cbSize = (IntPtr.Size == 8) ? (UInt32)8 : 5; // x64:8, x86:5

                UInt32 size;
                HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);
                if (HIDImports.SetupDiGetDeviceInterfaceDetail(hDevInfo, ref diData, ref diDetail, size, out size, IntPtr.Zero))
                {
                    Debug.WriteLine("Device: " + diDetail.DevicePath); Debug.Indent();
                    SafeFileHandle mHandle = HIDImports.CreateFile(diDetail.DevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);
                    HIDImports.HIDD_ATTRIBUTES attrib = new HIDImports.HIDD_ATTRIBUTES();
                    attrib.Size = Marshal.SizeOf(attrib);

                    if (HIDImports.HidD_GetAttributes(mHandle.DangerousGetHandle(), ref attrib))
                    {
                        Debug.WriteLine(String.Format("VendorID:{0:x}, ProductID:{1:x}, VersionNumber:{2:x}", attrib.VendorID, attrib.ProductID, attrib.VersionNumber));
                        if (attrib.VendorID == VIDApple &&
                           (attrib.ProductID == PIDAppleWirelessKeyboardUS || attrib.ProductID == PIDAppleWirelessKeyboardJIS))
                        {
                            _stream = new FileStream(mHandle, FileAccess.ReadWrite, 22, true);
                            //break;
                        }
                        else
                        {
                            mHandle.Close();
                        }
                    }
                    Debug.Unindent();
                }
            }

            if (_stream != null)
            {
                Byte[] buffer = new Byte[22];
                _stream.BeginRead(buffer, 0, buffer.Length, SpecialKeyStateChanged, buffer);
                return true;
            }
            else
            {
                // Not Connected
                return false;
            }
        }

        public Boolean Hook()
        {
            if (_hHook != null)
                throw new InvalidOperationException("フックはすでに実行されています。");

            // hook
            Win32.HookProcedure = KeyboardHookProc;
            _hHook = Win32.SetWindowsHookEx(Win32.WH_KEYBOARD_LL, Win32.HookProcedure, Win32.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

            return !_hHook.IsInvalid;
        }

        private void OnKeyDown()
        {
            if (KeyDown != null)
                KeyDown(this, new KeyEventArgs(CurrentPowerButtonIsDown, CurrentKeyState));
        }
        
        private void OnFnKeyCombinationDown(AppleKeyboardKeys appleKeyState, Keys key, Win32.KeyboardHookEventStruct keyEventStruct)
        {
            if (FnKeyCombinationDown != null)
                FnKeyCombinationDown(this, new AppleKeyboardEventArgs(appleKeyState, key, keyEventStruct));
        }

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32.KeyboardHookEventStruct keyEventStruct = (Win32.KeyboardHookEventStruct)Marshal.PtrToStructure(lParam, typeof(Win32.KeyboardHookEventStruct));
            //Debug.WriteLine(String.Format("{0}, {1}, {2}", nCode, wParam, lParam));
            //Debug.WriteLine(keyEventStruct);

            switch ((Keys)keyEventStruct.wVk)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return Win32.CallNextHookEx(_hHook, nCode, wParam, lParam);
            }

            if ((CurrentKeyState & AppleKeyboardKeys.Fn) == AppleKeyboardKeys.Fn &&
                 keyEventStruct.dwExtraInfo != (IntPtr)0x37564
               )
            {
                if ((Int32)wParam == Win32.WM_KEYDOWN || (Int32)wParam == Win32.WM_SYSKEYDOWN) // KEYDOWN
                    OnFnKeyCombinationDown(CurrentKeyState, (Keys)keyEventStruct.wVk, keyEventStruct);
                //else if ((Int32)wParam == Win32.WM_KEYUP || (Int32)wParam == Win32.WM_SYSKEYUP) // KEYUP
                //    OnFnKeyCombinationUp(CurrentKeyState, (Keys)keyEventStruct.wVk, keyEventStruct);
                return IntPtr.Zero;
            }
            return Win32.CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        private void SpecialKeyStateChanged(IAsyncResult ar)
        {
            if (_stream == null || !ar.IsCompleted)
                return;

            try
            {
                _stream.EndRead(ar);
            }
            catch (OperationCanceledException) { }
            catch (IOException ioe)
            {
                // restart (reconnected)
                Debug.WriteLine("Restart: " + ioe.Message);
                if (Disconnected != null)
                    Disconnected(this, EventArgs.Empty);
                return;
            }

            Byte[] buffer = ar.AsyncState as Byte[];
            foreach (Byte b in buffer)
                Debug.Write(String.Format("{0:x2} ", b));

            if (buffer[0] == 0x11)
            {
                Debug.Write((AppleKeyboardKeys) buffer[1]);
                CurrentKeyState = (AppleKeyboardKeys) buffer[1];
            }
            else if (buffer[0] == 0x13)
            {
                Debug.Write(buffer[1] == 1 ? "Power (Down)" : "Power (Up)");
                CurrentPowerButtonIsDown = (buffer[1] == 1);
            }

            OnKeyDown();

            Debug.WriteLine("");

            _stream.BeginRead(buffer, 0, buffer.Length, SpecialKeyStateChanged, buffer);
        }

        public void Unhook()
        {
            if (_hHook != null && !_hHook.IsInvalid)
            {
                _hHook.Dispose();
            }
            _hHook = null;
        }
        
        public void Shutdown()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            Unhook();
            Shutdown();
        }

        #endregion
    }
}
