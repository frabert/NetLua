using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetLua;

namespace LuaTest
{
    class Program
    {
        static LuaArguments print(LuaArguments args)
        {
            Console.WriteLine(String.Join("\t", Array.ConvertAll<LuaObject, string>(args, x=>x.ToString())));
            return Lua.Return(LuaObject.Nil);
        }

        static LuaArguments read(LuaArguments args)
        {
            return Lua.Return(Console.ReadLine());
        }

        static void Main(string[] args)
        {
            Lua lua = new Lua();
            lua.Context.SetGlobal("print", LuaObject.FromFunction(print));
            lua.Context.SetGlobal("read", LuaObject.FromFunction(read));

            MathLibrary.AddMathLibrary(lua.Context);
            dynamic math = lua.Context.Get("math");

            double d = math.sin(Math.PI);

            while (true)
            {
                lua.DoString(Console.ReadLine());
            }
        }
    }
}
