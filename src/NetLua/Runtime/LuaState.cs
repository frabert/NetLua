using System;
using System.Collections.Generic;
using System.Text;
using NetLua.Native.Value;

namespace NetLua.Runtime
{
    public class LuaState
    {
        public LuaState(LuaTable context, LuaFunctionState functionState = null)
        {
            Context = context;
            FunctionState = functionState ?? new LuaFunctionState();
        }

        public LuaTable Context { get; set; }

        public LuaFunctionState FunctionState { get; set; }

        public LuaState WithContext(LuaTable luaTable)
        {
            return new LuaState(luaTable, functionState: FunctionState);
        }

        public LuaState WithNewContext()
        {
            return WithContext(new LuaTable(Context));
        }
    }
}
