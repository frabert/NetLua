using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Libraries;
using NetLua.Native;
using NetLua.Native.Value;

namespace NetLua.Extensions
{
    public static class EngineExtensions
    {
        public static void Set(this Engine engine, LuaObject key, Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            engine.Set(key, LuaObject.FromFunction(func));
        }

        public static void Set(this Engine engine, LuaObject key, Func<Engine, LuaArguments, LuaArguments> func)
        {
            engine.Set(key, LuaObject.FromFunction(func));
        }


        public static void Set(this Engine engine, LuaObject key, Func<LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            engine.Set(key, LuaObject.FromFunction((e, a, t) => func(a, t)));
        }

        public static void Set(this Engine engine, LuaObject key, Func<LuaArguments, LuaArguments> func)
        {
            engine.Set(key, LuaObject.FromFunction((e, a) => func(a)));
        }

        public static void Set(this Engine engine, LuaObject key, object value)
        {
            engine.Set(key, LuaObject.FromObject(value));
        }

        public static void AddMathLibrary(this Engine obj)
        {
            obj.Set("math", LuaObject.FromObject(new MathLibrary()));
        }

        public static void AddLuaLibrary(this Engine obj)
        {
            obj.Set("assert", LuaLibrary.Assert);
            obj.Set("error", LuaLibrary.Error);
            obj.Set("getmetatable", LuaLibrary.GetMetaTable);
            obj.Set("setmetatable", LuaLibrary.SetMetaTable);
            obj.Set("rawequal", LuaLibrary.RawEqual);
            obj.Set("rawget", LuaLibrary.RawGet);
            obj.Set("rawset", LuaLibrary.Rawset);
            obj.Set("rawlen", LuaLibrary.RawLen);
            obj.Set("tonumber", LuaLibrary.ToNumber);
            obj.Set("tostring", LuaLibrary.Tostring);
            obj.Set("type", LuaLibrary.Type);
            obj.Set("ipairs", LuaLibrary.Ipairs);
            obj.Set("next", LuaLibrary.Next);
            obj.Set("pairs", LuaLibrary.Pairs);
        }
    }
}
