/*
 * See LICENSE file
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
      Console.WriteLine(String.Join("\t", Array.ConvertAll<LuaObject, string>(args, x => x.ToString())));
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


      while (true)
      {
        string line = Console.ReadLine();
        try
        {
          lua.DoString(line);
        }
        catch (LuaException ex)
        {
          for (int i = 0; i < ex.col - 1; i++)
            Console.Write(" ");

          Console.WriteLine("^");
          Console.WriteLine(ex.message);
        }
      }
    }
  }
}
