using System;
using System.Collections.Generic;
using System.Text;

namespace NetLua.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaPropertyAttribute : Attribute
    {
        public LuaPropertyAttribute(string name = null, LuaPropertyAccess access = LuaPropertyAccess.Readable | LuaPropertyAccess.Writeable)
        {
            Name = name;
            Access = access;
        }

        public string Name { get; set; }

        public LuaPropertyAccess Access { get; set; }
    }
}
