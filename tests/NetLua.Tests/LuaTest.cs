using System;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Libraries;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Runtime;
using Xunit;

namespace NetLua.Tests
{
    public class LuaTest
    {
        private readonly LuaTable _context;

        public LuaTest()
        {
            _context = new LuaTable();
            _context.AddLuaLibrary();
            _context.AddMathLibrary();
        }

        [Theory]
        [MemberData(nameof(LuaDataSource.TestData), MemberType = typeof(LuaDataSource))]
        public async Task TestScript(Labeled<string> script)
        {
            await Engine.EvalAsync(script.Data, _context);
        }
    }
}
