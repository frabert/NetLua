using System;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Native;

namespace NetLua.Runtime
{
    public sealed class LuaReturnState
    {
        public LuaReturnState()
        {
            ReturnArguments = Lua.Args();
        }

        public LuaArguments ReturnArguments { get; private set; }

        public bool ShouldStop => DidReturn || DidBreak;

        public bool DidReturn { get; private set; }

        public bool DidBreak { get; private set; }

        public void Return(LuaArguments args)
        {
            ReturnArguments = args;
            DidReturn = true;
        }
    }
}
