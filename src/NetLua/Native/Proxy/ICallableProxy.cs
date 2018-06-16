using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Attributes;

namespace NetLua.Native.Proxy
{
    public interface ICallableProxy
    {
        [LuaMethod(Visible = false)]
        Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default);
    }
}
