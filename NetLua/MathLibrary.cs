using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua
{
    public static class MathLibrary
    {
        static LuaArguments abs(LuaArguments args)
        {
            return Lua.Return(Math.Abs(args[0]));
        }

        static LuaArguments acos(LuaArguments args)
        {
            return Lua.Return(Math.Acos(args[0]));
        }

        static LuaArguments asin(LuaArguments args)
        {
            return Lua.Return(Math.Asin(args[0]));
        }

        static LuaArguments atan(LuaArguments args)
        {
            return Lua.Return(Math.Atan(args[0]));
        }

        static LuaArguments atan2(LuaArguments args)
        {
            return Lua.Return(Math.Atan2(args[0], args[1]));
        }

        static LuaArguments ceil(LuaArguments args)
        {
            return Lua.Return(Math.Ceiling(args[0]));
        }

        static LuaArguments cos(LuaArguments args)
        {
            return Lua.Return(Math.Cos(args[0]));
        }

        static LuaArguments cosh(LuaArguments args)
        {
            return Lua.Return(Math.Cosh(args[0]));
        }

        static LuaArguments exp(LuaArguments args)
        {
            return Lua.Return(Math.Exp(args[0]));
        }

        static LuaArguments floor(LuaArguments args)
        {
            return Lua.Return(Math.Floor(args[0]));
        }

        static LuaArguments log(LuaArguments args)
        {
            return Lua.Return(Math.Log(args[0], args[1] | Math.E));
        }

        static LuaArguments max(LuaArguments args)
        {
            return Lua.Return(Math.Max(args[0], args[1]));
        }

        static LuaArguments min(LuaArguments args)
        {
            return Lua.Return(Math.Min(args[0], args[1]));
        }

        static LuaArguments pow(LuaArguments args)
        {
            return Lua.Return(Math.Pow(args[0], args[1]));
        }

        static LuaArguments sin(LuaArguments args)
        {
            return Lua.Return(Math.Sin(args[0]));
        }

        static LuaArguments sinh(LuaArguments args)
        {
            return Lua.Return(Math.Sinh(args[0]));
        }

        static LuaArguments sqrt(LuaArguments args)
        {
            return Lua.Return(Math.Sqrt(args[0]));
        }

        static LuaArguments tan(LuaArguments args)
        {
            return Lua.Return(Math.Tan(args[0]));
        }

        static LuaArguments tanh(LuaArguments args)
        {
            return Lua.Return(Math.Tanh(args[0]));
        }

        public static void AddMathLibrary(LuaContext Context)
        {
            Dictionary<LuaObject, LuaObject> table = new Dictionary<LuaObject, LuaObject>();
            table.Add("abs", (LuaFunction)abs);
            table.Add("acos", (LuaFunction)acos);
            table.Add("asin", (LuaFunction)asin);
            table.Add("atan", (LuaFunction)atan);
            table.Add("atan2", (LuaFunction)atan2);
            table.Add("ceil", (LuaFunction)ceil);
            table.Add("cos", (LuaFunction)cos);
            table.Add("cosh", (LuaFunction)cosh);
            table.Add("exp", (LuaFunction)exp);
            table.Add("floor", (LuaFunction)floor);
            table.Add("log", (LuaFunction)log);
            table.Add("max", (LuaFunction)max);
            table.Add("min", (LuaFunction)min);
            table.Add("pow", (LuaFunction)pow);
            table.Add("sin", (LuaFunction)sin);
            table.Add("sinh", (LuaFunction)sinh);
            table.Add("sqrt", (LuaFunction)sqrt);
            table.Add("tan", (LuaFunction)tan);
            table.Add("tanh", (LuaFunction)tanh);

            table.Add("pi", Math.PI);

            Context.SetGlobal("math", LuaObject.FromTable(table));
        }
    }
}
