using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua
{
    public static class MathLibrary
    {
        static LuaArguments math_abs(LuaArguments args)
        {
            return Lua.Return(Math.Abs(args[0]));
        }

        static LuaArguments math_acos(LuaArguments args)
        {
            return Lua.Return(Math.Acos(args[0]));
        }

        static LuaArguments math_asin(LuaArguments args)
        {
            return Lua.Return(Math.Asin(args[0]));
        }

        static LuaArguments math_atan(LuaArguments args)
        {
            return Lua.Return(Math.Atan(args[0]));
        }

        static LuaArguments math_atan2(LuaArguments args)
        {
            return Lua.Return(Math.Atan2(args[0], args[1]));
        }

        static LuaArguments math_ceil(LuaArguments args)
        {
            return Lua.Return(Math.Ceiling(args[0]));
        }

        static LuaArguments math_cos(LuaArguments args)
        {
            return Lua.Return(Math.Cos(args[0]));
        }

        static LuaArguments math_cosh(LuaArguments args)
        {
            return Lua.Return(Math.Cosh(args[0]));
        }

        static LuaArguments math_exp(LuaArguments args)
        {
            return Lua.Return(Math.Exp(args[0]));
        }

        static LuaArguments math_floor(LuaArguments args)
        {
            return Lua.Return(Math.Floor(args[0]));
        }

        static LuaArguments math_log(LuaArguments args)
        {
            return Lua.Return(Math.Log(args[0], args[1] | Math.E));
        }

        static LuaArguments math_max(LuaArguments args)
        {
            double max = args[0];
            foreach (LuaObject o in args)
            {
                max = Math.Max(max, o);
            }
            return Lua.Return(max);
        }

        static LuaArguments math_min(LuaArguments args)
        {
            double min = args[0];
            foreach (LuaObject o in args)
            {
                min = Math.Min(min, o);
            }
            return Lua.Return(min);
        }

        static LuaArguments math_pow(LuaArguments args)
        {
            return Lua.Return(Math.Pow(args[0], args[1]));
        }

        static LuaArguments math_sin(LuaArguments args)
        {
            return Lua.Return(Math.Sin(args[0]));
        }

        static LuaArguments math_sinh(LuaArguments args)
        {
            return Lua.Return(Math.Sinh(args[0]));
        }

        static LuaArguments math_sqrt(LuaArguments args)
        {
            return Lua.Return(Math.Sqrt(args[0]));
        }

        static LuaArguments math_tan(LuaArguments args)
        {
            return Lua.Return(Math.Tan(args[0]));
        }

        static LuaArguments math_tanh(LuaArguments args)
        {
            return Lua.Return(Math.Tanh(args[0]));
        }

        public static void AddMathLibrary(LuaContext Context)
        {
            dynamic math = LuaObject.NewTable();
            math.abs = (LuaFunction)math_abs;
            math.acos = (LuaFunction)math_acos;
            math.asin = (LuaFunction)math_asin;
            math.atan = (LuaFunction)math_atan;
            math.atan2 = (LuaFunction)math_atan2;
            math.ceil = (LuaFunction)math_ceil;
            math.cos = (LuaFunction)math_cos;
            math.cosh = (LuaFunction)math_cosh;
            math.exp = (LuaFunction)math_exp;
            math.floor = (LuaFunction)math_floor;
            math.log = (LuaFunction)math_log;
            math.max = (LuaFunction)math_max;
            math.min = (LuaFunction)math_min;
            math.pow = (LuaFunction)math_pow;
            math.sin = (LuaFunction)math_sin;
            math.sinh = (LuaFunction)math_sinh;
            math.sqrt = (LuaFunction)math_sqrt;
            math.tan = (LuaFunction)math_tan;
            math.tanh = (LuaFunction)math_tanh;

            math.pi = Math.PI;

            Context.Set("math", math);
        }
    }
}
