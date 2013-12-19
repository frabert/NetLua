/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2013 Francesco Bertolaccini
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
            return Lua.Return();
        }

        static LuaArguments io_write(LuaArguments args)
        {
            Console.Write(args[0].ToString());
            return Lua.Return();
        }

        static LuaArguments read(LuaArguments args)
        {
            return Lua.Return(Console.ReadLine());
        }

        static void Main(string[] args)
        {
            Lua lua = new Lua();
            lua.DynamicContext.print = (LuaFunction)print;
            lua.DynamicContext.read = (LuaFunction)read;

            MathLibrary.AddMathLibrary(lua.Context);
            IoLibrary.AddIoLibrary(lua.Context);

            //lua.DoFile("C:\\Users\\Francesco\\life.lua");

            while (true)
            {
                lua.DoString(Console.ReadLine());
            }
        }
    }
}
