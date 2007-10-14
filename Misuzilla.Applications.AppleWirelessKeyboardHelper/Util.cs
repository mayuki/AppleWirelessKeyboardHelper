using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using Microsoft.Scripting;

namespace Misuzilla.Applications.AppleWirelessKeyboardHelper
{
    [SuppressMessage("Microsoft.Naming", "CA1724")] // CA1724: System.Web.Util とかぶる
    [SecurityPermission(SecurityAction.LinkDemand)]
    public static class Util
    {
        internal readonly static IntPtr InputExtraInfoValue = (IntPtr)0x37564;

        /// <summary>
        /// 現在フォーカスを持つウィンドウに対して指定したキーのUp/Downを送信します。
        /// </summary>
        /// <param name="keyInputs"></param>
        public static UInt32 SendInput(params Keys[] keyInputs)
        {
            List<Win32.INPUT> inputs = new List<Win32.INPUT>();
            foreach (Keys key in keyInputs)
            {
                Debug.WriteLine("SendInput: " + key.ToString());
                Win32.INPUT input = new Win32.INPUT();
                input.ki = new Win32.KEYBDINPUT();
                input.type = Win32.INPUT_KEYBOARD;
                input.ki.wScan = 0;
                input.ki.wVk = (Byte)key;
                input.ki.dwFlags = 0; // KEYDOWN
                input.ki.dwExtraInfo = InputExtraInfoValue;
                inputs.Add(input);

                input = new Win32.INPUT();
                input.ki = new Win32.KEYBDINPUT();
                input.type = Win32.INPUT_KEYBOARD;
                input.ki.wScan = 0;
                input.ki.wVk = (Byte)key;
                input.ki.dwFlags = Win32.KEYEVENTF_KEYUP; // KEYUP
                input.ki.dwExtraInfo = InputExtraInfoValue;
                inputs.Add(input);
            }
            Win32.INPUT[] inputsArr = inputs.ToArray();

            return Win32.SendInput((UInt32)inputsArr.Length, inputsArr, Marshal.SizeOf(typeof(Win32.INPUT)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUp"></param>
        /// <param name="keyInput"></param>
        public static UInt32 SendInput(Boolean isUp, Keys key)
        {
            Debug.WriteLine("SendInput: " + key.ToString() + " ( " + (isUp ? "Up" : "Down" ) + ")");
            Win32.INPUT input = new Win32.INPUT();
            input.ki = new Win32.KEYBDINPUT();
            input.type = Win32.INPUT_KEYBOARD;
            input.ki.wScan = 0;
            input.ki.wVk = (Byte)key;
            input.ki.dwFlags = (UInt16)(isUp ? Win32.KEYEVENTF_KEYUP : 0);
            input.ki.dwExtraInfo = InputExtraInfoValue;

            Win32.INPUT[] inputsArr = new Win32.INPUT[1];
            inputsArr[0] = input;

            return Win32.SendInput((UInt32)inputsArr.Length, inputsArr, Marshal.SizeOf(typeof(Win32.INPUT)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drive"></param>
        public static void Eject(String drive)
        {
            using (SafeFileHandle hDevice = Win32.CreateFile(String.Format(@"\\.\{0}:", drive), FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, Win32.EFileAttributes.Normal, IntPtr.Zero))
            {
                UInt32 retVal = 0;
                NativeOverlapped retOverlapped = new NativeOverlapped();
                Win32.DeviceIoControl(hDevice, Win32.EIOControlCode.StorageEjectMedia, null, 0, null, 0, ref retVal, ref retOverlapped);
            }
        }
    }
}
