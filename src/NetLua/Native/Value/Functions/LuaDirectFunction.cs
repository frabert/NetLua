using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetLua.Native.Value.Functions
{

    public class LuaDirectFunction : LuaFunction
    {
        private readonly Func<Engine, LuaArguments, LuaArguments> _func;

        public LuaDirectFunction(Func<Engine, LuaArguments, LuaArguments> func) 
        {
            _func = func;
        }

        public LuaObject Context { get; set; }

        public override Task<LuaArguments> CallAsync(Engine engine, LuaArguments args,
            CancellationToken token = default)
        {
            return Task.FromResult(_func(engine, args));
        }

        public override object ToObject()
        {
            return _func;
        }
    }
}