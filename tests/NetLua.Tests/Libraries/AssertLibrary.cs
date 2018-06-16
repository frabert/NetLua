using System.Threading;
using System.Threading.Tasks;
using NetLua.Attributes;
using NetLua.Native;
using NetLua.Native.Proxy;
using NetLua.Native.Value;
using NetLua.Runtime.Ast;
using Xunit;
using Xunit.Sdk;

namespace NetLua.Tests.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class AssertLibrary : ICallableProxy
    {
        [LuaMethod("Equal")]
        public static async Task Equal(LuaObject left, LuaObject right)
        {
            var areEqual = await LuaObject.BinaryOperationAsync(BinaryOp.Equal, left, right);

            if (!areEqual)
            {
                throw new EqualException(left, right);
            }
        }

        [LuaMethod("NotEqual")]
        public static async Task NotEqual(LuaObject left, LuaObject right)
        {
            var areDifferent = await LuaObject.BinaryOperationAsync(BinaryOp.Different, left, right);

            if (!areDifferent)
            {
                throw new NotEqualException(left, right);
            }
        }

        [LuaMethod("True")]
        public static void True(LuaObject obj)
        {
            Assert.True(obj.AsBool());
        }

        [LuaMethod("False")]
        public static void False(LuaObject obj)
        {
            Assert.True(obj.AsBool());
        }

        public Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default)
        {
            Assert.True(args[0].ToBoolean());

            return Lua.ArgsAsync();
        }
    }
}