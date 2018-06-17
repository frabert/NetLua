using NetLua.Extensions;
using NetLua.Tests.Libraries;

namespace NetLua.Tests.Extensions
{
    public static class EngineExtensions
    {
        public static void AddAssertLibrary(this Engine engine)
        {
            engine.Set("assert", new AssertLibrary());
        }
    }
}
