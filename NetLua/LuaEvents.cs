using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lua
{
    static class LuaEvents
    {
        public static LuaObject rawget(LuaObject table, LuaObject index)
        {
            if(table.IsTable)
            {
                LuaObject obj;
                if(table.AsTable().TryGetValue(index, out obj))
                {
                    return obj;
                }
                else 
                {
                    return LuaObject.Nil;
                }
            }
            else
            {
                throw new LuaException("Invalid operation");
            }
        }

        public static void rawset(LuaObject table, LuaObject key, LuaObject value)
        {
            if (table.IsTable)
            {
                IDictionary<LuaObject, LuaObject> t = table.AsTable();
                if (t.ContainsKey(key))
                    t[key] = value;
                else
                    t.Add(key, value);
            }
            else
            {
                throw new LuaException("Invalid operation");
            }
        }

        static LuaObject getBinhandler(LuaObject a, LuaObject b, string f)
        {
            LuaObject f1 = LuaObject.Nil, f2 = LuaObject.Nil;
            if (!a.Metatable.IsNil)
                f1 = a.Metatable[f];
            if (!b.Metatable.IsNil)
                f2 = b.Metatable[f];

            if (f1.IsNil)
                return f2;
            else
                return f1;
        }

        static LuaObject toNumber(LuaObject obj)
        {
            if (obj.IsNumber)
                return LuaObject.FromNumber(obj.AsNumber());
            else if (obj.IsString)
            {
                double d;
                if (double.TryParse(obj.AsString(), out d))
                    return LuaObject.FromNumber(d);
                else
                    return LuaObject.Nil;
            }
            else
            {
                return LuaObject.Nil;
            }
        }

        internal static LuaObject add_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(a.AsNumber() + b.AsNumber());
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__add");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject sub_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(a.AsNumber() - b.AsNumber());
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__sub");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject mul_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(a.AsNumber() * b.AsNumber());
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__mul");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject div_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(a.AsNumber() / b.AsNumber());
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__div");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject mod_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(a.AsNumber() - Math.Floor(a.AsNumber() / b.AsNumber()) * b.AsNumber());
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__mod");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject pow_event(LuaObject op1, LuaObject op2)
        {
            LuaObject a = toNumber(op1);
            LuaObject b = toNumber(op2);

            if (!a.IsNil && !b.IsNil)
            {
                return LuaObject.FromNumber(Math.Pow(a.AsNumber() , b.AsNumber()));
            }
            else
            {
                LuaObject handler = getBinhandler(op1, op2, "__pow");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject unm_event(LuaObject op)
        {
            LuaObject a = toNumber(op);

            if (!a.IsNil)
            {
                double o1 = a.AsNumber();
                return LuaObject.FromNumber(-o1);
            }
            else
            {
                LuaObject handler = op.Metatable["__unm"];
                if (!handler.IsNil)
                {
                    return handler.Call(op);
                }
            }

            throw new LuaException("Invalid arithmetic operation");
        }

        internal static LuaObject index_event(LuaObject table, LuaObject key)
        {
            LuaObject handler;

            if (table.IsTable)
            {
                LuaObject v = rawget(table, key);
                if (!v.IsNil)
                    return v;
                else
                {
                    handler = table.Metatable["__index"];
                    if (handler.IsNil || handler == null)
                        return LuaObject.Nil;
                }
            }
            else
            {
                handler = table.Metatable["__index"];
                if (!handler.IsNil)
                    return handler.Call(table, key);
                else
                    throw new LuaException("Invalid argument");
            }

            if (handler.IsFunction)
            {
                return handler.AsFunction()(table, key);
            }
            else if (!handler.IsNil)
            {
                return handler[key];
            }
            else
            {
                return LuaObject.Nil;
            }
        }

        internal static LuaObject newindex_event(LuaObject table, LuaObject key, LuaObject value)
        {
            LuaObject handler;
            if (table.IsTable)
            {
                LuaObject v = rawget(table, key);
                if (!v.IsNil)
                {
                    rawset(table, key, value);
                    return LuaObject.Nil;
                }
                handler = table.Metatable["__newindex"];
                if (handler.IsNil)
                {
                    rawset(table, key, value);
                    return LuaObject.Nil;
                }
            }
            else
            {
                handler = table.Metatable["__newindex"];
                if (handler.IsNil)
                    throw new LuaException("Invalid op");
            }

            if (handler.IsFunction)
            {
                handler.AsFunction()(table, key, value);
            }
            else
            {
                handler[key] = value;
            }
            return LuaObject.Nil;
        }

        internal static LuaObject call_event(LuaObject func, LuaObject[] args)
        {
            if (func.IsFunction)
            {
                return func.AsFunction()(args);
            }
            else
            {
                LuaObject handler = func.Metatable["__call"];
                if (handler.IsFunction)
                {
                    List<LuaObject> argslist = new List<LuaObject>();
                    argslist.Add(func); argslist.AddRange(args);
                    return handler.AsFunction()(argslist.ToArray());
                }
                else
                {
                    throw new LuaException("Invalid op");
                }
            }
        }
    }
}
