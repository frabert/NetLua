using System;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Libraries;
using NetLua.Native;
using NetLua.Native.Value;

namespace NetLua.Extensions
{
    public static class LuaObjectExtensions
    {
        public static void AddMathLibrary(this LuaObject obj)
        {
            obj.NewIndexRaw("math", LuaObject.FromObject(new MathLibrary()));
        }

        public static void AddLuaLibrary(this LuaObject obj)
        {
            obj.NewIndexLocal("assert", LuaLibrary.Assert);
            obj.NewIndexLocal("error", LuaLibrary.Error);
            obj.NewIndexLocal("getmetatable", LuaLibrary.GetMetaTable);
            obj.NewIndexLocal("setmetatable", LuaLibrary.SetMetaTable);
            obj.NewIndexLocal("rawequal", LuaLibrary.RawEqual);
            obj.NewIndexLocal("rawget", LuaLibrary.RawGet);
            obj.NewIndexLocal("rawset", LuaLibrary.Rawset);
            obj.NewIndexLocal("rawlen", LuaLibrary.RawLen);
            obj.NewIndexLocal("tonumber", LuaLibrary.ToNumber);
            obj.NewIndexLocal("tostring", LuaLibrary.Tostring);
            obj.NewIndexLocal("type", LuaLibrary.Type);
            obj.NewIndexLocal("ipairs", LuaLibrary.Ipairs);
            obj.NewIndexLocal("next", LuaLibrary.Next);
            obj.NewIndexLocal("pairs", LuaLibrary.Pairs);
        }

        public static bool IsNil(this LuaObject obj) => obj.Type == LuaType.Nil;

        public static bool IsTable(this LuaObject obj) => obj.Type == LuaType.Table;

        public static bool IsFunction(this LuaObject obj) => obj.Type == LuaType.Function;

        public static bool IsNumber(this LuaObject obj) => obj.Type == LuaType.Number;

        public static bool IsString(this LuaObject obj) => obj.Type == LuaType.String;

        public static bool TryAsDouble(this LuaObject obj, out double dbl)
        {
            dbl = obj.AsNumber();
            return !double.IsNaN(dbl);
        }

        public static void NewIndexLocal(this LuaObject table, LuaObject key, Func<LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            table.NewIndexRaw(key, LuaObject.CreateFunction(func));
        }

        public static void NewIndexLocal(this LuaObject table, LuaObject key, Func<LuaArguments, LuaArguments> func)
        {
            table.NewIndexRaw(key, LuaObject.CreateFunction(func));
        }
    }
}
