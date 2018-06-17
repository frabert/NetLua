using System;

namespace NetLua.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LuaClassAttribute : Attribute
    {
        public LuaClassAttribute()
        {
            DefaultPropertyAccess = LuaPropertyAccess.None;
        }

        public LuaPropertyAccess DefaultPropertyAccess { get; set; }

        public bool DefaultMethodVisible { get; set; }
    }
}
