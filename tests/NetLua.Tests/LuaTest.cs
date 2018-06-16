using System;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Libraries;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Runtime;
using NetLua.Tests.Extensions;
using Xunit;

namespace NetLua.Tests
{
    public class LuaTest
    {
        private readonly Engine _engine;

        public LuaTest()
        {
            _engine = new Engine();
            _engine.AddLuaLibrary();
            _engine.AddMathLibrary();
            _engine.AddAssertLibrary();
        }

        [Theory]
        [MemberData(nameof(LuaDataSource.TestData), MemberType = typeof(LuaDataSource))]
        public async Task TestScript(Labeled<string> script)
        {
            await _engine.ExecuteAsync(script.Data);
        }
    }
}
