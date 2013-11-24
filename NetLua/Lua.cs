using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lua.Ast;

namespace Lua
{
    public class Lua
    {
        LuaContext ctx = new LuaContext();
        Parser p = new Parser();

        public LuaObject DoFile(string Filename)
        {
            return DoString(System.IO.File.ReadAllText(Filename));
        }

        public LuaObject DoString(string Chunk)
        {
            bool ret;
            return LuaInterpreter.EvalBlock(p.ParseString(Chunk), ctx, out ret);
        }

        public LuaContext Context
        {
            get
            {
                return ctx;
            }
        }
    }
}
