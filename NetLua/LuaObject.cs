/*
 * NetLua by Francesco Bertolaccini
 * Project inspired by AluminumLua, a project by Alexander Corrado
 * (See his repo at http://github.com/chkn/AluminumLua)
 * 
 * NetLua - a managed implementation of the Lua dynamic programming language
 *
 * LuaObject.cs
 * Based on a work by Alexander Corrado for AluminumLua (https://github.com/chkn/AluminumLua/blob/master/src/LuaObject.cs)
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Lua
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
    public struct LuaObject : IEnumerable<KeyValuePair<LuaObject, LuaObject>>, IEquatable<LuaObject>
    {

        private object luaobj;
        private LuaType type;

        #region Common objects
        /// <summary>
        /// An empty/unset value
        /// </summary>
        public static readonly LuaObject Nil = new LuaObject();

        /// <summary>
        /// A standard true boolean value
        /// </summary>
        public static readonly LuaObject True = new LuaObject { luaobj = true, type = LuaType.boolean };

        /// <summary>
        /// A standard false boolean value
        /// </summary>
        public static readonly LuaObject False = new LuaObject { luaobj = false, type = LuaType.boolean };

        /// <summary>
        /// Zero (number)
        /// </summary>
        public static readonly LuaObject Zero = new LuaObject { luaobj = 0d, type = LuaType.number };

        /// <summary>
        /// And empty string
        /// </summary>
        public static readonly LuaObject EmptyString = new LuaObject { luaobj = "", type = LuaType.@string };
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

            return new LuaObject { luaobj = number, type = LuaType.number };
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

            return new LuaObject { luaobj = str, type = LuaType.@string };
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

            return new LuaObject { luaobj = fn, type = LuaType.function };
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

            return new LuaObject { luaobj = table, type = LuaType.table };
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
                var table = AsTable();
                if (table == null)
                    throw new LuaException("cannot index non-table");

                // we don't care whether the get was successful, because the default LuaObject is nil.
                LuaObject result;
                table.TryGetValue(key, out result);
                return result;
            }
            set
            {
                var table = AsTable();
                if (table == null)
                    throw new LuaException("cannot index non-table");

                table[key] = value;
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
        public bool Equals(LuaObject other)
        {
            // luaobj will not be null unless type is Nil
            return (other.type == type) && (luaobj == null || luaobj.Equals(other.luaobj));
        }

        public override bool Equals(object obj)
        {
            if (obj is LuaObject)
                return Equals((LuaObject)obj);
            // FIXME: It would be nice to automatically compare other types (strings, ints, doubles, etc.) to LuaObjects.
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (luaobj != null ? luaobj.GetHashCode() : 0) ^ type.GetHashCode();
            }
        }
    }
}