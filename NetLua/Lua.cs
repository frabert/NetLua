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

        public static LuaArguments Return()
        {
            return new LuaObject[] { LuaObject.Nil };
        }

        public static LuaArguments Return(params LuaObject[] values)
        {
            return values;
        }

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

        public LuaArguments DoFile(string Filename)
        {
            LuaInterpreter.LuaReturnStatus ret;
            return LuaInterpreter.EvalBlock(p.ParseFile(Filename), ctx, out ret);
        }

        public LuaArguments DoString(string Chunk)
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

        LuaArguments assert(LuaArguments args)
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
            return Return();
        }

        LuaArguments dofile(LuaArguments args)
        {
            return DoFile(args[0].ToString());
        }

        LuaArguments error(LuaArguments args)
        {
            throw new LuaException(args[0].ToString());
        }

        LuaArguments getmetatable(LuaArguments args)
        {
            return Return(args[0].Metatable);
        }

        LuaArguments setmetatable(LuaArguments args)
        {
            args[0].Metatable = args[1];
            return Return();
        }

        LuaArguments rawequal(LuaArguments args)
        {
            return Return(args[0] == args[1]);
        }

        LuaArguments rawget(LuaArguments args)
        {
            return Return(LuaEvents.rawget(args[0], args[1]));
        }

        LuaArguments rawset(LuaArguments args)
        {
            LuaEvents.rawset(args[0], args[1], args[2]);
            return Return(args[0]);
        }

        LuaArguments rawlen(LuaArguments args)
        {
            LuaObject obj = args[0];
            if (obj.IsString)
                return Return(obj.AsString().Length);
            else if (obj.IsTable)
                return Return(obj.AsTable().Count);
            else
                throw new LuaException("invalid argument");
        }

        LuaArguments tonumber(LuaArguments args)
        {
            LuaObject obj = args[0];
            if (args.Length == 1)
            {
                double d = 0;
                if (obj.IsString)
                {
                    if (double.TryParse(obj.AsString(), out d))
                        return Return(d);
                    else
                        return Return();
                }
                else if (obj.IsNumber)
                {
                    return Return(obj.AsNumber());
                }
                else
                {
                    return Return();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        LuaArguments tostring(LuaArguments args)
        {
            return Return(LuaEvents.tostring_event(args[0]));
        }

        LuaArguments type(LuaArguments args)
        {
            switch (args[0].Type)
            {
                case LuaType.boolean:
                    return Return("boolean");
                case LuaType.function:
                    return Return("function");
                case LuaType.nil:
                    return Return("nil");
                case LuaType.number:
                    return Return("number");
                case LuaType.@string:
                    return Return("string");
                case LuaType.table:
                    return Return("table");
                case LuaType.thread:
                    return Return("thread");
                case LuaType.userdata:
                    return Return("userdata");
            }
            return Return();
        }

        #endregion
    }
}
