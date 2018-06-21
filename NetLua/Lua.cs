/*
 * See LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Dynamic;

using NetLua.Ast;

namespace NetLua
{
    public class Lua : DynamicObject
    {
        LuaContext ctx = new LuaContext();
        Parser p = new Parser();

        /// <summary>
        /// Helper function for returning Nil from a function
        /// </summary>
        /// <returns>Nil</returns>
        public static LuaArguments Return()
        {
            return new LuaObject[] { LuaObject.Nil };
        }

        /// <summary>
        /// Helper function for returning objects from a function
        /// </summary>
        /// <param name="values">The objects to return</param>
        public static LuaArguments Return(params LuaObject[] values)
        {
            return values;
        }

        /// <summary>
        /// Creates a new Lua context with the base functions already set
        /// </summary>
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
            ctx.Set("ipairs", (LuaFunction)ipairs);
            ctx.Set("next", (LuaFunction)next);
            ctx.Set("pairs", (LuaFunction)pairs);
        }

        /// <summary>
        /// Parses and executes the specified file
        /// </summary>
        /// <param name="Filename">The file to execute</param>
        public LuaArguments DoFile(string Filename)
        {
            FunctionDefinition def = new FunctionDefinition();
            def.Arguments = new List<Argument>();
            def.Body = p.ParseFile(Filename);
            var function = LuaCompiler.CompileFunction(def, Expression.Constant(ctx)).Compile();
            return function().Call(Lua.Return());
        }

        /// <summary>
        /// Parses and executes the specified string
        /// </summary>
        public LuaArguments DoString(string Chunk)
        {
            FunctionDefinition def = new FunctionDefinition();
            def.Arguments = new List<Argument>();
            def.Body = p.ParseString(Chunk);
            var function = LuaCompiler.CompileFunction(def, Expression.Constant(ctx)).Compile();
            return function().Call(Lua.Return());
        }

        /// <summary>
        /// Parses and executes the specified parsed block
        /// </summary>
        public LuaArguments DoAst(Block block)
        {
            FunctionDefinition def = new FunctionDefinition();
            def.Arguments = new List<Argument>();
            def.Body = block;
            var function = LuaCompiler.CompileFunction(def, Expression.Constant(ctx)).Compile();
            return function().Call(Lua.Return());
        }

        /// <summary>
        /// The base context
        /// </summary>
        public LuaContext Context
        {
            get
            {
                return ctx;
            }
        }

        /// <summary>
        /// The base context
        /// </summary>
        public dynamic DynamicContext
        {
            get
            {
                return ctx;
            }
        }

        #region Dynamic methods

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            LuaObject obj = ctx.Get(binder.Name);
            if (obj.IsNil)
                return false;
            else
            {
                result = LuaObject.getObject(obj);
                return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value is LuaObject)
                ctx.Set(binder.Name, value as LuaObject);
            else
                ctx.Set(binder.Name, LuaObject.FromObject(value));
            return true;
        }

        #endregion

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
                //TODO: Implement tonumber for bases different from 10
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

        LuaArguments ipairs(LuaArguments args)
        {
            LuaObject handler = LuaEvents.getMetamethod(args[0], "__ipairs");
            if (!handler.IsNil)
            {
                return handler.Call(args);
            }
            else
            {
                if (args[0].IsTable)
                {
                    LuaFunction f = delegate(LuaArguments x)
                    {
                        var s = x[0];
                        var var = x[1].AsNumber() + 1;

                        var val = s[var];
                        if (val == LuaObject.Nil)
                            return Return(LuaObject.Nil);
                        else
                            return Return(var, val);
                    };
                    return Return(f, args[0], 0);
                }
                else
                {
                    throw new LuaException("t must be a table");
                }
            }
        }

        LuaArguments next(LuaArguments args)
        {
            var table = args[0];
            var index = args[1];
            if (!table.IsTable)
            {
                throw new LuaException("t must be a table");
            }
            List<LuaObject> keys = new List<LuaObject>(table.AsTable().Keys);
            if (index.IsNil)
            {
                if (keys.Count == 0)
                    return Return();
                return Return(keys[0], table[keys[0]]);
            }
            else
            {
                int pos = keys.IndexOf(index);
                if (pos == keys.Count - 1)
                {
                    return Return();
                }
                else
                {
                    return Return(keys[pos + 1], table[keys[pos + 1]]);
                }
            }
        }

        LuaArguments pairs(LuaArguments args)
        {
            LuaObject handler = LuaEvents.getMetamethod(args[0], "__pairs");
            if (!handler.IsNil)
            {
                return handler.Call(args);
            }
            else
            {
                return Return((LuaFunction)next, args[0], LuaObject.Nil);
            }
        }

        #endregion
    }
}
