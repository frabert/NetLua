using System;
using System.Collections.Generic;
using System.Text;

namespace NetLua.Attributes
{
    [Flags]
    public enum LuaPropertyAccess
    {
        None = 0,
        Readable = 1,
        Writeable = 2
    }
}
