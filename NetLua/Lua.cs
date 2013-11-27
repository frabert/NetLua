using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetLua.Ast;

namespace NetLua
{
    public class Lua
    {
        LuaContext ctx = new LuaContext();
        Parser p = new Parser();

        public Lua()
        {
            ctx.Set("assert", (LuaFunction)assert);
            ctx.Set("dofile", (LuaFunction)dofile);
            ctx.Set("error", (LuaFunction)error);
            ctx.Set("getmetatable", (LuaFunction)getmetatable);
            ctx.Set("setmetatable", (LuaFunction)setmetatable);
            ctx.Set("rawequal", (LuaFunction)rawequal);
            ctx.Set("rawget", (LuaFunction)rawget);
            ctx.Set("rawset", (LuaFunction)rawset);
            ctx.Set("rawlen", (LuaFunction)rawlen);
            ctx.Set("tonumber", (LuaFunction)tonumber);
            ctx.Set("tostring", (LuaFunction)tostring);
            ctx.Set("type", (LuaFunction)type);
        }

        public LuaObject DoFile(string Filename)
        {
            LuaInterpreter.LuaReturnStatus ret;
            return LuaInterpreter.EvalBlock(p.ParseFile(Filename), ctx, out ret);
        }

        public LuaObject DoString(string Chunk)
        {
            LuaInterpreter.LuaReturnStatus ret;
            return LuaInterpreter.EvalBlock(p.ParseString(Chunk), ctx, out ret);
        }

        public LuaContext Context
        {
            get
            {
                return ctx;
            }
        }

        #region Basic functions

        LuaObject assert(LuaObject[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].AsBool() == false)
                {
                    if (args.Length == 1)
                        throw new LuaException("Assertion failed");
                    else
                        throw new LuaException(args[1].ToString());
                }
            }
            return LuaObject.Nil;
        }

        LuaObject dofile(LuaObject[] args)
        {
            return DoFile(args[0].ToString());
        }

        LuaObject error(LuaObject[] args)
        {
            throw new LuaException(args[0].ToString());
        }

        LuaObject getmetatable(LuaObject[] args)
        {
            return args[0].Metatable;
        }

        LuaObject setmetatable(LuaObject[] args)
        {
            args[0].Metatable = args[1];
            return LuaObject.Nil;
        }

        LuaObject rawequal(LuaObject[] args)
        {
            return args[0] == args[1];
        }

        LuaObject rawget(LuaObject[] args)
        {
            return LuaEvents.rawget(args[0], args[1]);
        }

        LuaObject rawset(LuaObject[] args)
        {
            LuaEvents.rawset(args[0], args[1], args[2]);
            return args[0];
        }

        LuaObject rawlen(LuaObject[] args)
        {
            LuaObject obj = args[0];
            if (obj.IsString)
                return obj.AsString().Length;
            else if (obj.IsTable)
                return obj.AsTable().Count;
            else
                throw new LuaException("invalid argument");
        }

        LuaObject tonumber(LuaObject[] args)
        {
            LuaObject obj = args[0];
            if (args.Length == 1)
            {
                double d = 0;
                if (obj.IsString)
                {
                    if (double.TryParse(obj.AsString(), out d))
                        return d;
                    else
                        return LuaObject.Nil;
                }
                else if (obj.IsNumber)
                {
                    return obj.AsNumber();
                }
                else
                {
                    return LuaObject.Nil;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        LuaObject tostring(LuaObject[] args)
        {
            return LuaEvents.tostring_event(args[0]);
        }

        LuaObject type(LuaObject[] args)
        {
            switch (args[0].Type)
            {
                case LuaType.boolean:
                    return "boolean";
                case LuaType.function:
                    return "function";
                case LuaType.nil:
                    return "nil";
                case LuaType.number:
                    return "number";
                case LuaType.@string:
                    return "string";
                case LuaType.table:
                    return "table";
                case LuaType.thread:
                    return "thread";
                case LuaType.userdata:
                    return "userdata";
            }
            return LuaObject.Nil;
        }

        #endregion
    }
}
