using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NetLua.Native.Proxy
{
    internal class LuaProxyCacheItemProperty
    {
        public string Name { get; set; }

        public PropertyInfo Info { get; set; }

        public bool Writeable { get; set; }

        public bool Readable { get; set; }
    }
}
