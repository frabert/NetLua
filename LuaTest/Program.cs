using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetLua;
using NetLua.Ast;

namespace LuaTest
{
    class Program
    {
        static LuaObject print(params LuaObject[] args)
        {
            Console.WriteLine(String.Join("\t", Array.ConvertAll<LuaObject, string>(args, x=>x.ToString())));
            return LuaObject.Nil;
        }

        static LuaObject read(params LuaObject[] args)
        {
            return Console.ReadLine();
        }

        static LuaObject getmetatable(params LuaObject[] args)
        {
            return args[0].Metatable;
        }

        static LuaObject setmetatable(params LuaObject[] args)
        {
            args[0].Metatable = args[1];
            return LuaObject.Nil;
        }

        static void Main(string[] args)
        {
            Lua lua = new Lua();
            lua.Context.SetGlobal("print", LuaObject.FromFunction(print));
            lua.Context.SetGlobal("read", LuaObject.FromFunction(read));
            lua.Context.SetGlobal("getmetatable", LuaObject.FromFunction(getmetatable));
            lua.Context.SetGlobal("setmetatable", LuaObject.FromFunction(setmetatable));

            while (true)
            {
                lua.DoString(Console.ReadLine());
            }
        }
    }
}
