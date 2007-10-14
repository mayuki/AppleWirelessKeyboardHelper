using System;
using System.Collections.Generic;
using System.Text;

namespace Misuzilla.Applications.AppleWirelessKeyboardHelper
{
    [Flags]
    internal enum AppleKeyboardKeys : byte
    {
        None = 0x00,
        Fn = 0x10,
        Eject = 0x08
    }

}
