using System;
using System.Linq;
using NetLua.Attributes;
using NetLua.Native;
using NetLua.Native.Value;

namespace NetLua.Libraries
{
    [LuaClass(DefaultMethodVisible = true)]
    public class MathLibrary
    {
        public static LuaArguments Abs(LuaArguments args)
        {
            return Lua.Args(Math.Abs(args[0]));
        }

        public static LuaArguments Acos(LuaArguments args)
        {
            return Lua.Args(Math.Acos(args[0]));
        }

        public static LuaArguments Asin(LuaArguments args)
        {
            return Lua.Args(Math.Asin(args[0]));
        }

        public static LuaArguments Atan(LuaArguments args)
        {
            return Lua.Args(Math.Atan(args[0]));
        }

        public static LuaArguments Atan2(LuaArguments args)
        {
            return Lua.Args(Math.Atan2(args[0], args[1]));
        }

        public static LuaArguments Ceil(LuaArguments args)
        {
            return Lua.Args(Math.Ceiling(args[0]));
        }

        public static LuaArguments Cos(LuaArguments args)
        {
            return Lua.Args(Math.Cos(args[0]));
        }

        public static LuaArguments Cosh(LuaArguments args)
        {
            return Lua.Args(Math.Cosh(args[0]));
        }

        public static LuaArguments Exp(LuaArguments args)
        {
            return Lua.Args(Math.Exp(args[0]));
        }

        public static LuaArguments Floor(LuaArguments args)
        {
            return Lua.Args(Math.Floor(args[0]));
        }

        public static LuaArguments Log(LuaArguments args)
        {
            return Lua.Args(Math.Log(args[0]));
        }

        public static LuaArguments Max(LuaArguments args)
        {
            var max = args.Skip(1).Aggregate(args[0], (current, o) => Math.Max(current, o));

            return Lua.Args(max);
        }

        public static LuaArguments Min(LuaArguments args)
        {
            var min = args.Skip(1).Aggregate(args[0], (current, o) => Math.Min(current, o));

            return Lua.Args(min);
        }

        public static LuaArguments Pow(LuaArguments args)
        {
            return Lua.Args(Math.Pow(args[0], args[1]));
        }

        public static LuaArguments Sin(LuaArguments args)
        {
            return Lua.Args(Math.Sin(args[0]));
        }

        public static LuaArguments Sinh(LuaArguments args)
        {
            return Lua.Args(Math.Sinh(args[0]));
        }

        public static LuaArguments Sqrt(LuaArguments args)
        {
            return Lua.Args(Math.Sqrt(args[0]));
        }

        public static LuaArguments Tan(LuaArguments args)
        {
            return Lua.Args(Math.Tan(args[0]));
        }

        public static LuaArguments Tanh(LuaArguments args)
        {
            return Lua.Args(Math.Tanh(args[0]));
        }
    }
}
