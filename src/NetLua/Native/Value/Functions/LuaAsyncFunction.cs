using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetLua.Native.Value.Functions
{
    public class LuaAsyncFunction : LuaFunction
    {
        private readonly Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> _func;

        public LuaAsyncFunction(Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            _func = func;
        }

        public LuaObject Context { get; set; }

        public override Task<LuaArguments> CallAsync(Engine engine, LuaArguments args,
            CancellationToken token = default)
        {
            return _func(engine, args, token);
        }

        public override object ToObject()
        {
            return _func;
        }
    }
}