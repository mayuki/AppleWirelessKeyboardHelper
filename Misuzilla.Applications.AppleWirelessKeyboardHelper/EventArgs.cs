using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Misuzilla.Applications.AppleWirelessKeyboardHelper
{
    internal class KeyEventArgs : EventArgs
    {
        public Boolean IsPowerButtonDown;
        public AppleKeyboardKeys AppleKeyboardKey;

        public KeyEventArgs(Boolean isPowerButtonDown, AppleKeyboardKeys appleKeyboardKey)
        {
            IsPowerButtonDown = isPowerButtonDown;
            AppleKeyboardKey = appleKeyboardKey;
        }
    }

    internal class AppleKeyboardEventArgs : EventArgs
    {
        public AppleKeyboardKeys AppleKeyState;
        public Keys Key;
        public Win32.KeyboardHookEventStruct KeyEventStruct;

        public AppleKeyboardEventArgs(AppleKeyboardKeys appleKeyState, Keys key, Win32.KeyboardHookEventStruct keyEventStruct)
        {
            AppleKeyState = appleKeyState;
            Key = key;
            KeyEventStruct = keyEventStruct;
        }
    }
}
