using System;
using System.Collections.Generic;
using System.Text;

namespace NetLua.Native.Value
{
    internal class LuaTableFunction : LuaTable
    {
        private LuaObject[] _varargs;

        internal LuaTableFunction(LuaObject parent) : base(parent)
        {
        }

        public LuaTableFunction()
        {
        }

        internal LuaObject[] Varargs
        {
            get => _varargs ?? throw new LuaException("cannot use '...' outside a vararg function");
            set => _varargs = value;
        }
    }
}
