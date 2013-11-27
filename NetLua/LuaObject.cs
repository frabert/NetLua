using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NetLua
{
    using LuaTable = IDictionary<LuaObject, LuaObject>;
    using LuaTableImpl = Dictionary<LuaObject, LuaObject>;
    using LuaTableItem = KeyValuePair<LuaObject, LuaObject>;

    /// <summary>
    /// An exception thrown by the Lua interpreter
    /// </summary>
    public class LuaException : Exception
    {
        /// <summary>
        /// An exception thrown by a syntactical error
        /// </summary>
        /// <param name="file">The file wich contains the error</param>
        /// <param name="row">The row of the error</param>
        /// <param name="col">The column of the error</param>
        /// <param name="message">The kind of the error</param>
        public LuaException(string file, int row, int col, string message)
            : base(string.Format("Error in {0}({1},{2}): {3}", file, row, col, message))
        { }

        public LuaException(string message)
            : base("Error (unknown context): " + message)
        { }
    }

    /// <summary>
    /// A Lua function
    /// </summary>
    public delegate LuaObject LuaFunction(params LuaObject[] args);

    // http://www.lua.org/pil/2.html
    /// <summary>
    /// A Lua type
    /// </summary>
    public enum LuaType
    {
        nil,
        boolean,
        number,
        @string,
        userdata,
        function,
        thread,
        table
    }

    /// <summary>
    /// A Lua object. Can be any of the standard Lua objects
    /// </summary>
    public class LuaObject : IEnumerable<KeyValuePair<LuaObject, LuaObject>> //, IEquatable<LuaObject>
    {
        private object luaobj;
        private LuaType type;
        private LuaObject metatable = Nil;

        public LuaObject()
        {
            this.metatable = Nil;
        }

        ~LuaObject()
        {
            LuaEvents.gc_event(this);
        }

        public LuaObject Metatable
        {
            get
            {
                return metatable;
            }
            set
            {
                metatable = value;
            }
        }

        #region Common objects
        /// <summary>
        /// An empty/unset value
        /// </summary>
        public static readonly LuaObject Nil = new LuaObject() { luaobj = null, type = LuaType.nil };

        /// <summary>
        /// A standard true boolean value
        /// </summary>
        public static readonly LuaObject True = new LuaObject { luaobj = true, type = LuaType.boolean, Metatable = Nil };

        /// <summary>
        /// A standard false boolean value
        /// </summary>
        public static readonly LuaObject False = new LuaObject { luaobj = false, type = LuaType.boolean, Metatable = Nil };

        /// <summary>
        /// Zero (number)
        /// </summary>
        public static readonly LuaObject Zero = new LuaObject { luaobj = 0d, type = LuaType.number, Metatable = Nil };

        /// <summary>
        /// And empty string
        /// </summary>
        public static readonly LuaObject EmptyString = new LuaObject { luaobj = "", type = LuaType.@string, Metatable = Nil };
        #endregion

        /// <summary>
        /// Gets the underlying Lua type
        /// </summary>
        public LuaType Type { get { return type; } }

        /// <summary>
        /// Checks whether the type matches or not
        /// </summary>
        public bool Is(LuaType type)
        {
            return this.type == type;
        }

        /// <summary>
        /// Creates a Lua object from a .NET object
        /// Automatically checks if there is a matching Lua type.
        /// If not, creates a userdata value
        /// </summary>
        public static LuaObject FromObject(object obj)
        {
            if (obj == null)
                return Nil;
            if (obj is LuaObject)
                return (LuaObject)obj;

            if (obj is bool)
                return FromBool((bool)obj);

            {
                var str = obj as string;
                if (str != null)
                {
                    return FromString(str);
                }
            }

            {
                var @delegate = obj as LuaFunction;
                if (@delegate != null)
                {
                    return FromFunction(@delegate);
                }
            }

            {
                var @delegate = obj as Delegate;
                if (@delegate != null)
                {
                    return FromDelegate(@delegate);
                }
            }

            {
                var dictionary = obj as LuaTable;
                if (dictionary != null)
                {
                    return FromTable(dictionary);
                }
            }

            if (obj is double) return FromNumber((double)obj);
            if (obj is float) return FromNumber((float)obj);
            if (obj is int) return FromNumber((int)obj);
            if (obj is uint) return FromNumber((uint)obj);
            if (obj is short) return FromNumber((short)obj);
            if (obj is ushort) return FromNumber((ushort)obj);
            if (obj is long) return FromNumber((long)obj);
            if (obj is ulong) return FromNumber((ulong)obj);
            if (obj is byte) return FromNumber((byte)obj);
            if (obj is sbyte) return FromNumber((sbyte)obj);
            if (obj is Thread) return new LuaObject { luaobj = obj, type = LuaType.thread };
            return FromUserData(obj);
        }

        #region Boolean
        /// <summary>
        /// Creates a Lua object from a boolean value
        /// </summary>
        public static LuaObject FromBool(bool bln)
        {
            if (bln)
                return True;

            return False;
        }

        public static implicit operator LuaObject(bool bln)
        {
            return FromBool(bln);
        }

        /// <summary>
        /// Gets whether this is a boolean object
        /// </summary>
        public bool IsBool { get { return type == LuaType.boolean; } }

        /// <summary>
        /// Converts this Lua object into a boolean
        /// </summary>
        /// <returns></returns>
        public bool AsBool()
        {
            if (luaobj == null)
                return false;

            if (luaobj is bool && ((bool)luaobj) == false)
                return false;

            return true;
        }
        #endregion

        #region Number
        /// <summary>
        /// Creates a Lua object from a double
        /// </summary>
        public static LuaObject FromNumber(double number)
        {
            if (number == 0d)
                return Zero;

            return new LuaObject { luaobj = number, type = LuaType.number, Metatable = Nil };
        }

        public static implicit operator LuaObject(double number)
        {
            return FromNumber(number);
        }

        /// <summary>
        /// Gets whether this is a number object
        /// </summary>
        public bool IsNumber { get { return type == LuaType.number; } }

        /// <summary>
        /// Converts this object into a number
        /// </summary>
        public double AsNumber()
        {
            return (double)luaobj;
        }
        #endregion

        #region String
        /// <summary>
        /// Creates a Lua object from a string
        /// </summary>
        public static LuaObject FromString(string str)
        {
            if (str == null)
                return Nil;

            if (str.Length == 0)
                return EmptyString;

            return new LuaObject { luaobj = str, type = LuaType.@string, Metatable = Nil };
        }

        public static implicit operator LuaObject(string str)
        {
            return FromString(str);
        }

        public bool IsString { get { return type == LuaType.@string; } }

        public string AsString()
        {
            return luaobj.ToString();
        }
        #endregion

        #region Function
        /// <summary>
        /// Creates a Lua object from a Lua function
        /// </summary>
        public static LuaObject FromFunction(LuaFunction fn)
        {
            if (fn == null)
                return Nil;

            return new LuaObject { luaobj = fn, type = LuaType.function, Metatable = Nil };
        }

        /// <summary>
        /// Creates a Lua object from a delegate
        /// </summary>
        /// <returns>A function</returns>
        public static LuaObject FromDelegate(Delegate a)
        {
            return FromFunction((args) => DelegateAdapter(a, args));
        }

        public static implicit operator LuaObject(LuaFunction fn)
        {
            return FromFunction(fn);
        }

        public bool IsFunction { get { return type == LuaType.function; } }

        public LuaFunction AsFunction()
        {
            var fn = luaobj as LuaFunction;
            if (fn == null)
                throw new LuaException("cannot call non-function");

            return fn;
        }

        private static LuaObject DelegateAdapter(Delegate @delegate, IEnumerable<LuaObject> args)
        {
            return FromObject(@delegate.DynamicInvoke((from a in args select a.luaobj).ToArray()));
        }
        #endregion

        #region Table
        /// <summary>
        /// Creates a Lua object from a Lua table
        /// </summary>
        public static LuaObject FromTable(LuaTable table)
        {
            if (table == null)
                return Nil;

            return new LuaObject { luaobj = table, type = LuaType.table, Metatable = Nil };
        }

        /// <summary>
        /// Creates and initializes a Lua object with a table value
        /// </summary>
        /// <param name="initItems">The initial items of the table to create</param>
        public static LuaObject NewTable(params LuaTableItem[] initItems)
        {
            var table = FromTable(new LuaTableImpl());

            foreach (var item in initItems)
                table[item.Key] = item.Value;

            return table;
        }

        public bool IsTable { get { return type == LuaType.table; } }

        public LuaTable AsTable()
        {
            return luaobj as LuaTable;
        }
        #endregion

        /// <summary>
        /// Creates a Lua object from a .NET object
        /// </summary>
        public static LuaObject FromUserData(object userdata)
        {
            if (userdata == null)
                return Nil;

            return new LuaObject { luaobj = userdata, type = LuaType.userdata };
        }

        /// <summary>
        /// Gets whether this object is nil
        /// </summary>
        public bool IsNil { get { return type == LuaType.nil; } }

        /// <summary>
        /// Gets whether this object is userdata
        /// </summary>
        public bool IsUserData { get { return type == LuaType.userdata; } }

        public object AsUserData()
        {
            return luaobj;
        }

        public static LuaObject operator +(LuaObject a, LuaObject b)
        {
            return LuaEvents.add_event(a, b);
        }

        public static LuaObject operator -(LuaObject a, LuaObject b)
        {
            return LuaEvents.sub_event(a, b);
        }

        public static LuaObject operator *(LuaObject a, LuaObject b)
        {
            return LuaEvents.mul_event(a, b);
        }

        public static LuaObject operator /(LuaObject a, LuaObject b)
        {
            return LuaEvents.div_event(a, b);
        }

        public static LuaObject operator %(LuaObject a, LuaObject b)
        {
            return LuaEvents.mod_event(a, b);
        }

        public static LuaObject operator ^(LuaObject a, LuaObject b)
        {
            return LuaEvents.pow_event(a, b);
        }

        public static LuaObject operator <(LuaObject a, LuaObject b)
        {
            return LuaEvents.lt_event(a, b);
        }

        public static LuaObject operator >(LuaObject a, LuaObject b)
        {
            return LuaEvents.lt_event(b, a);
        }

        public static LuaObject operator <=(LuaObject a, LuaObject b)
        {
            return LuaEvents.le_event(a, b);
        }

        public static LuaObject operator >=(LuaObject a, LuaObject b)
        {
            return LuaEvents.le_event(b, a);
        }

        public static bool operator ==(LuaObject a, object b)
        {
            if (a.IsNil)
            {
                if (b == null)
                    return true;
                else
                {
                    if (b is LuaObject)
                        return (b as LuaObject).IsNil;
                    else
                        return false;
                }
            }
            else
            {
                if (b == null)
                    return false;
                else
                {
                    if (b is LuaObject)
                        return (b as LuaObject).luaobj.Equals(a.luaobj);
                    else
                        return a.luaobj.Equals(b);
                }
            }
        }

        public static bool operator !=(LuaObject a, object b)
        {
            return !(a == b);
        }

        public IEnumerator<KeyValuePair<LuaObject, LuaObject>> GetEnumerator()
        {
            var table = luaobj as IEnumerable<KeyValuePair<LuaObject, LuaObject>>;
            if (table == null)
                return null;

            return table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public LuaObject this[LuaObject key]
        {
            get
            {
                if (IsTable)
                {
                    LuaTable table = AsTable();
                    if (table.ContainsKey(key))
                        return table[key];
                    else
                    {
                        return LuaEvents.index_event(this, key);
                    }
                }
                else
                {
                    return LuaEvents.index_event(this, key);
                }
            }
            set
            {
                /*var table = AsTable();
                if (table == null)
                    throw new LuaException("cannot index non-table");

                table[key] = value;*/
                if (IsTable)
                {
                    var table = AsTable();
                    if (table.ContainsKey(key))
                        table[key] = value;
                    else
                        LuaEvents.newindex_event(this, key, value);
                }
                else
                {
                    LuaEvents.newindex_event(this, key, value);
                }
            }
        }

        // Unlike AsString, this will return string representations of nil, tables, and functions
        public override string ToString()
        {
            if (IsNil)
                return "nil";

            if (IsTable)
                return "{ " + string.Join(", ", AsTable().Select(kv => string.Format("[{0}]={1}", kv.Key, kv.Value.ToString())).ToArray()) + " }";

            if (IsFunction)
                return AsFunction().Method.ToString();

            if (IsBool)
                return luaobj.ToString().ToLower();

            return luaobj.ToString();
        }

        // See last paragraph in http://www.lua.org/pil/13.2.html
        /*public bool Equals(LuaObject other)
        {
            // luaobj will not be null unless type is Nil
            return (other.type == type) && (luaobj == null || luaobj.Equals(other.luaobj));
        }

        public override bool Equals(object obj)
        {
            if (obj is LuaObject)
                return Equals((LuaObject)obj);
            else
                return Equals(FromObject(obj));
            //return false;
        }*/

        public override bool Equals(object obj)
        {
            if (obj is LuaObject)
                return luaobj.Equals((obj as LuaObject).luaobj);
            else
                return luaobj.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (luaobj != null ? luaobj.GetHashCode() : 0) ^ type.GetHashCode();
            }
        }

        public LuaObject Call(params LuaObject[] args)
        {
            return LuaEvents.call_event(this, args);
        }
    }
}