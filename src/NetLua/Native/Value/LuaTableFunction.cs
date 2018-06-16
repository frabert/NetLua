using System;
using System.Collections.Generic;
using System.Text;

namespace NetLua.Native.Value
{
    internal class LuaTableFunction : LuaTable
    {
        private LuaObject[] _varargs;
        private readonly bool _useParent;

        internal LuaTableFunction(LuaObject parent, bool useParent) 
            : base(parent)
        {
            _useParent = useParent;
        }

        public override LuaObject IndexRaw(LuaObject key)
        {
            if (_useParent && Parent != null)
            {
                return Parent.IndexRaw(key);
            }

            return base.IndexRaw(key);
        }

        public override void NewIndexRaw(LuaObject key, LuaObject value)
        {
            if (_useParent && Parent != null)
            {
                Parent.NewIndexRaw(key, value);
                return;
            }

            base.NewIndexRaw(key, value);
        }

        internal LuaObject[] Varargs
        {
            get => _varargs ?? throw new LuaException("cannot use '...' outside a vararg function");
            set => _varargs = value;
        }
    }
}
