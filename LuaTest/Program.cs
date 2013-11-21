using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lua;
using Lua.Ast;

namespace LuaTest
{
    class Program
    {
        static LuaObject print(params LuaObject[] args)
        {
            Console.WriteLine(String.Join("\t", args));
            return LuaObject.Nil;
        }

        static LuaObject read(params LuaObject[] args)
        {
            return Console.ReadLine();
        }

        static void Main(string[] args)
        {
            while (true)
            {
                new Parser(Console.ReadLine());
            }
        }
    }
}
