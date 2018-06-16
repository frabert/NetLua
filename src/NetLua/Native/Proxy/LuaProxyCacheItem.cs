using System.Collections.Generic;
using System.Reflection;
using NetLua.Native.Value;

namespace NetLua.Native.Proxy
{
    internal struct LuaProxyCacheItem
    {
        public IDictionary<string, LuaObject> Methods { get; set; }

        public IDictionary<string, LuaProxyCacheItemProperty> Properties { get; set; }
    }
}
