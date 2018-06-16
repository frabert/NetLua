using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetLua.Native.Value.Functions
{

    public class LuaDirectFunction : LuaFunction
    {
        private readonly Func<LuaArguments, LuaArguments> _func;

        public LuaDirectFunction(Func<LuaArguments, LuaArguments> func) 
        {
            _func = func;
        }

        public LuaObject Context { get; set; }

        public override Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default)
        {
            return Task.FromResult(_func(args));
        }

        public override object ToObject()
        {
            return _func;
        }
    }
}