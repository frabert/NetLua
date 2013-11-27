using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua
{
    static class LuaEvents
    {
        public static LuaObject rawget(LuaObject table, LuaObject index)
        {
            if (table.IsTable)
            {
                LuaObject obj;
                if (table.AsTable().TryGetValue(index, out obj))
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
            LuaObject f1 = getMetamethod(a, f);
            LuaObject f2 = getMetamethod(b, f);

            return f1 | f2;
        }

        static LuaObject getEqualhandler(LuaObject a, LuaObject b)
        {
            if ((a.Type != b.Type) || (a.IsTable && b.IsUserData))
            {
                return LuaObject.Nil;
            }
            LuaObject mm1 = getMetamethod(a, "__eq");
            LuaObject mm2 = getMetamethod(b, "__eq");
            if (mm1 == mm2)
                return mm1;
            else
                return LuaObject.Nil;
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

        internal static LuaObject getMetamethod(LuaObject obj, string e)
        {
            if (obj.Metatable == null || obj.Metatable.IsNil)
                return LuaObject.Nil;
            else
                return obj.Metatable[e];
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
                var handler = getBinhandler(op1, op2, "__add");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                var handler = getBinhandler(op1, op2, "__sub");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                var handler = getBinhandler(op1, op2, "__mul");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                var handler = getBinhandler(op1, op2, "__div");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                var handler = getBinhandler(op1, op2, "__mod");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                return LuaObject.FromNumber(Math.Pow(a.AsNumber(), b.AsNumber()));
            }
            else
            {
                var handler = getBinhandler(op1, op2, "__pow");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
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
                var handler = getMetamethod(op, "__unm");
                if (!handler.IsNil)
                {
                    return handler.Call(op)[0];
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
                    handler = getMetamethod(table, "__index");
                    if (handler.IsNil)
                        return LuaObject.Nil;
                }
            }
            else
            {
                handler = getMetamethod(table, "__index");
                if (!handler.IsNil)
                    return handler.Call(table, key)[0];
                else
                    throw new LuaException("Invalid argument");
            }

            if (handler.IsFunction)
            {
                return handler.AsFunction()(table, key)[0];
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
                handler = getMetamethod(table, "__newindex");
                if (handler.IsNil)
                {
                    rawset(table, key, value);
                    return LuaObject.Nil;
                }
            }
            else
            {
                handler = getMetamethod(table, "__newindex");
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

        internal static LuaObject[] call_event(LuaObject func, LuaObject[] args)
        {
            if (func.IsFunction)
            {
                return func.AsFunction()(args);
            }
            else
            {
                var handler = getMetamethod(func, "__call");
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

        internal static LuaObject len_event(LuaObject op)
        {
            if (op.IsString)
                return op.AsString().Length;
            else
            {
                var handler = getMetamethod(op, "__len");
                if (!handler.IsNil)
                {
                    return handler.Call(op)[0];
                }
                else if (op.IsTable)
                {
                    return op.AsTable().Count;
                }
                else
                {
                    throw new LuaException("Invalid op");
                }
            }
        }

        internal static LuaObject concat_event(LuaObject op1, LuaObject op2)
        {
            if ((op1.IsString || op1.IsNumber) && (op2.IsString || op2.IsNumber))
            {
                return op1.ToString() + op2.ToString();
            }
            else
            {
                var handler = getBinhandler(op1, op2, "__concat");
                if (!handler.IsNil)
                {
                    return handler.Call(op1, op2)[0];
                }
                else
                {
                    throw new LuaException("Invalid op");
                }
            }
        }

        internal static LuaObject eq_event(LuaObject op1, LuaObject op2)
        {
            if (op1 == op2)
            {
                return true;
            }
            var handler = getEqualhandler(op1, op2);
            if (!handler.IsNil)
                return !(!(handler.Call(op1, op2)[0].AsBool()));
            else
                return false;
        }

        internal static LuaObject lt_event(LuaObject op1, LuaObject op2)
        {
            if (op1.IsNumber && op2.IsNumber)
                return op1.AsNumber() < op2.AsNumber();
            else if (op1.IsString && op2.IsString)
                throw new NotImplementedException();
            else
            {
                var handler = getBinhandler(op1, op2, "__lt");
                if (!handler.IsNil)
                    return !(!(handler.Call(op1, op2)[0].AsBool()));
                else
                    return LuaObject.Nil;
            }
        }

        internal static LuaObject le_event(LuaObject op1, LuaObject op2)
        {
            if (op1.IsNumber && op2.IsNumber)
                return op1.AsNumber() <= op2.AsNumber();
            else if (op1.IsString && op2.IsString)
                throw new NotImplementedException();
            else
            {
                var handler = getBinhandler(op1, op2, "__le");
                if (!handler.IsNil)
                    return !(!(handler.Call(op1, op2)[0].AsBool()));
                else
                    return LuaObject.Nil;
            }
        }

        internal static LuaObject gc_event(LuaObject op)
        {
            var handler = getMetamethod(op, "__gc");
            if (handler.IsFunction)
                handler.AsFunction()(op);
            return LuaObject.Nil;
        }

        internal static LuaObject tostring_event(LuaObject op)
        {
            var handler = getMetamethod(op, "__tostring");
            if (!handler.IsNil)
                return handler.Call(op)[0];
            else
                return op.ToString();
        }
    }
}
