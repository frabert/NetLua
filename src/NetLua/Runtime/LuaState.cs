using System;
using System.Collections.Generic;
using System.Text;
using NetLua.Native.Value;

namespace NetLua.Runtime
{
    public class LuaState
    {
        public LuaState(Engine engine, LuaTable context, LuaFunctionState functionState = null)
        {
            Context = context;
            Engine = engine;
            FunctionState = functionState ?? new LuaFunctionState();
        }

        public Engine Engine { get; set; }

        public LuaTable Context { get; set; }

        public LuaFunctionState FunctionState { get; set; }

        public LuaState WithContext(LuaTable luaTable)
        {
            return new LuaState(Engine, luaTable, functionState: FunctionState);
        }

        public LuaState WithNewContext()
        {
            return WithContext(new LuaTable(Context));
        }
    }
}
