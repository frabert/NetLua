using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Native;
using NetLua.Native.Value;

namespace NetLua.Libraries
{
    public static class LuaLibrary
    {
        public static LuaArguments Assert(LuaArguments args)
        {
            if (args.Length > 0 && !args[0].AsBool())
            {
                throw new LuaException(args.Length == 1 ? "Assertion failed" : args[1].ToString());
            }

            return Lua.Args();
        }

        public static LuaArguments Error(LuaArguments args)
        {
            throw new LuaException(args[0].ToString());
        }

        public static LuaArguments GetMetaTable(LuaArguments args)
        {
            return Lua.Args(args[0].MetaTable);
        }

        public static LuaArguments SetMetaTable(LuaArguments args)
        {
            args.Expect(0, LuaType.Table, LuaType.Nil);

            args[0].MetaTable = args[1];
            return Lua.Args();
        }

        public static LuaArguments RawEqual(LuaArguments args)
        {
            return Lua.Args(args[0] == args[1]);
        }

        public static LuaArguments RawGet(LuaArguments args)
        {
            return Lua.Args(args[0].IndexRaw(args[1]));
        }

        public static LuaArguments Rawset(LuaArguments args)
        {
            args[0].NewIndexRaw(args[1], args[2]);
            return Lua.Args(args[0]);
        }

        public static LuaArguments RawLen(LuaArguments args)
        {
            return Lua.Args(args[0].Length);
        }

        public static LuaArguments ToNumber(LuaArguments args)
        {
            return Lua.Args(args[0].ToNumber());
        }

        public static Task<LuaArguments> Tostring(LuaArguments args, CancellationToken token = default)
        {
            return Lua.ArgsAsync(args[0].AsString());
        }

        public static LuaArguments Type(LuaArguments args)
        {
            switch (args[0].Type)
            {
                case LuaType.Boolean:
                    return Lua.Args("boolean");
                case LuaType.Function:
                    return Lua.Args("function");
                case LuaType.Nil:
                    return Lua.Args("nil");
                case LuaType.Number:
                    return Lua.Args("number");
                case LuaType.String:
                    return Lua.Args("string");
                case LuaType.Table:
                    return Lua.Args("table");
                case LuaType.Thread:
                    return Lua.Args("thread");
                case LuaType.UserData:
                    return Lua.Args("userdata");
                default:
                    return Lua.Args();
            }
        }

        private static async Task<LuaArguments> GetNext(Engine engine, LuaArguments x, CancellationToken token = default)
        {
            var s = x[0];
            var var = x[1].AsNumber() + 1;
            var val = await s.IndexAsync(engine, var, token);

            return val.IsNil() ? Lua.Args(LuaNil.Instance) : Lua.Args(var, val);
        }

        public static Task<LuaArguments> Ipairs(Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var handler = args[0].GetMetaMethod(engine, "__ipairs");

            if (!handler.IsNil())
            {
                return handler.CallAsync(engine, args, token);
            }

            if (!args[0].IsTable())
            {
                throw new LuaException("t must be a table");
            }

            return Lua.ArgsAsync(LuaObject.FromFunction(GetNext), args[0], 0);
        }

        public static Task<LuaArguments> Pairs(Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var handler = args[0].GetMetaMethod(engine, "__pairs");

            return !handler.IsNil() 
                ? handler.CallAsync(engine, args, token) 
                : Lua.ArgsAsync(LuaObject.FromFunction(Next), args[0], LuaNil.Instance);
        }

        public static LuaArguments Next(Engine engine, LuaArguments args)
        {
            var table = args[0];
            var index = args[1];

            if (!table.IsTable())
            {
                throw new LuaException("t must be a table");
            }

            var keys = table.Keys.ToArray();

            if (index.IsNil())
            {
                return keys.Length == 0 
                    ? Lua.Args() 
                    : Lua.Args(keys[0], table.IndexRaw(keys[0]));
            }

            var pos = Array.IndexOf(keys, index);

            return pos == keys.Length - 1
                ? Lua.Args() 
                : Lua.Args(keys[pos + 1], table.IndexRaw(keys[pos + 1]));
        }
    }
}
