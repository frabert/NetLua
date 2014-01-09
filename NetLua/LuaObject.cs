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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using System.Dynamic;

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
    public delegate LuaArguments LuaFunction(LuaArguments args);

    public class LuaArguments : IEnumerable<LuaObject>
    {
        List<LuaObject> list;

        public LuaArguments()
        {
            list = new List<LuaObject>();
        }

        public LuaArguments(params LuaObject[] Objects)
        {
            //list.AddRange(Objects);
            list = new List<LuaObject>(Objects);
        }

        public LuaArguments(params LuaArguments[] Objects)
        {
            foreach (LuaArguments arg in Objects)
            {
                if (list == null)
                {
                    list = new List<LuaObject>(arg.list);
                }
                else
                {
                    list.AddRange(arg.list);
                }
            }
        }

        public int Length
        {
            get
            {
                return list.Count;
            }
        }

        public LuaObject this[int Index]
        {
            get
            {
                if (Index < list.Count)
                    return list[Index];
                else
                    return LuaObject.Nil;
            }
            set
            {
                if (Index < list.Count)
                    list[Index] = value;
            }
        }

        public void Add(LuaObject obj)
        {
            list.Add(obj);
        }

        public LuaArguments Concat(LuaArguments args)
        {
            list.AddRange(args.list);
            return this;
        }

        public IEnumerator<LuaObject> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static implicit operator LuaArguments(LuaObject[] array)
        {
            return new LuaArguments(array);
        }

        public static implicit operator LuaObject[](LuaArguments args)
        {
            return args.list.ToArray();
        }

        public LuaArguments GetSubset(int startIndex)
        {
            if (startIndex >= list.Count)
            {
                return new LuaArguments(new LuaObject[] { });
            }
            else
            {
                return new LuaArguments(list.GetRange(startIndex, list.Count - startIndex).ToArray());
            }
        }
    }

    // http://www.lua.org/pil/2.html
    /// <summary>
    /// A Lua type
    /// </summary>
    public enum LuaType : byte
    {
        nil = 0,
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
    public class LuaObject :  DynamicObject, IEnumerable<KeyValuePair<LuaObject, LuaObject>> //, IEquatable<LuaObject>
    {
        internal object luaobj;
        internal LuaType type;
        private LuaObject metatable = Nil;

        public LuaObject()
        {
            this.metatable = Nil;
        }

        private LuaObject(object Obj, LuaType Type)
        {
            this.metatable = Nil;
            this.luaobj = Obj;
            this.type = Type;
        }

        ~LuaObject()
        {
            LuaEvents.gc_event(this);
        }

        /// <summary>
        /// Gets or sets the object's metatable
        /// </summary>
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
        public LuaType Type { get { return type; } internal set { type = value; } }

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
                    return FromString(str);
            }

            {
                var @delegate = obj as LuaFunction;
                if (@delegate != null)
                    return FromFunction(@delegate);
            }

            {
                var dictionary = obj as LuaTable;
                if (dictionary != null)
                    return FromTable(dictionary);
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

        public static implicit operator bool(LuaObject obj)
        {
            return obj.AsBool();
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

            return new LuaObject(number, LuaType.number);
        }

        public static implicit operator LuaObject(double number)
        {
            return FromNumber(number);
        }

        public static implicit operator double(LuaObject obj)
        {
            return obj.AsNumber();
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

            return new LuaObject(str, LuaType.@string);
        }

        public static implicit operator LuaObject(string str)
        {
            return FromString(str);
        }

        public static implicit operator string(LuaObject obj)
        {
            return obj.AsString();
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

            return new LuaObject(fn, LuaType.function);
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
        #endregion
        
        #region Table
        /// <summary>
        /// Creates a Lua object from a Lua table
        /// </summary>
        public static LuaObject FromTable(LuaTable table)
        {
            if (table == null)
                return Nil;

            return new LuaObject(table, LuaType.table);
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

        #region Userdata
        /// <summary>
        /// Creates a Lua object from a .NET object
        /// </summary>
        public static LuaObject FromUserData(object userdata)
        {
            if (userdata == null)
                return Nil;

            //return new LuaObject { luaobj = userdata, type = LuaType.userdata };
            return new LuaObject(userdata, LuaType.userdata);
        }

        /// <summary>
        /// Gets whether this object is nil
        /// </summary>
        public bool IsNil { get { return type == LuaType.nil; } }

        /// <summary>
        /// Gets whether this object is userdata
        /// </summary>
        public bool IsUserData { get { return type == LuaType.userdata; } }

        /// <summary>
        /// Returns the CLI object underneath the wrapper
        /// </summary>
        public object AsUserData()
        {
            return luaobj;
        }
        #endregion

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
            if (b == null)
                return LuaObject.FromObject(a).IsNil;

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
                    {
                        if ((b as LuaObject).IsNil)
                            return a.IsNil;

                        return (b as LuaObject).luaobj.Equals(a.luaobj);
                    }
                    else
                        return a.luaobj.Equals(b);
                }
            }
        }

        public static bool operator !=(LuaObject a, object b)
        {
            return !(a == b);
        }

        public static LuaObject operator |(LuaObject a, object b)
        {
            if (a.IsNil || !a.AsBool())
            {
                if (b is LuaObject)
                    return b as LuaObject;
                else
                    return LuaObject.FromObject(b);
            }
            else
            {
                return a;
            }
        }

        public static LuaObject operator &(LuaObject a, object b)
        {
            if (a.IsNil || !a.AsBool())
            {
                return a;
            }
            else
            {
                if (b is LuaObject)
                    return b as LuaObject;
                else
                    return LuaObject.FromObject(b);
            }
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
                        return LuaEvents.index_event(this, key);
                }
                else
                    return LuaEvents.index_event(this, key);
            }
            set
            {
                if (IsTable)
                {
                    var table = AsTable();
                    if (table.ContainsKey(key))
                        table[key] = value;
                    else
                        LuaEvents.newindex_event(this, key, value);
                }
                else
                    LuaEvents.newindex_event(this, key, value);
            }
        }

        // Unlike AsString, this will return string representations of nil, tables, and functions
        public override string ToString()
        {
            if (IsNil)
                return "nil";

            if (IsTable)
                return "table";

            if (IsFunction)
                return "function";

            if (IsBool)
                return luaobj.ToString().ToLower();

            return luaobj.ToString();
        }

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
                return (luaobj != null ? luaobj.GetHashCode() : 0) ^ (byte)type;
            }
        }

        /// <summary>
        /// Calls the object passing the instance as first argument. Uses the metafield __call
        /// </summary>
        /// <param name="instance">The object to be passed as first argument</param>
        /// <param name="args">Arguments to be passed after the object</param>
        public LuaArguments MethodCall(LuaObject instance, LuaArguments args)
        {
            LuaObject[] objs = new LuaObject[args.Length + 1];
            objs[0] = instance;
            for (int i = 0; i < args.Length; i++)
            {
                objs[i + 1] = args[i];
            }

            return this.Call(objs);
        }

        /// <summary>
        /// Calls the object. If this is not a function, it calls the metatable field __call
        /// </summary>
        /// <param name="args">The arguments to pass</param>
        public LuaArguments Call(params LuaObject[] args)
        {
            return this.Call(new LuaArguments(args));
        }

        /// <summary>
        /// Calls the object. If this is not a function, it calls the metatable field __call
        /// </summary>
        /// <param name="args">The arguments to pass</param>
        public LuaArguments Call(LuaArguments args)
        {
            return LuaEvents.call_event(this, args);
        }

        #region DynamicObject

        /// <summary>
        /// Gets a standard .NET value froma LuaObject
        /// </summary>
        /// <returns>The LuaObject is <paramref name="a"/> is a function or a table, its underlying luaobj if not</returns>
        internal static object getObject(LuaObject a)
        {
            if (a.Type != LuaType.table && a.Type != LuaType.function)
                return a.luaobj;
            else
                return a;
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            LuaObject obj = this[binder.Name];
            if (obj.IsNil)
                return false;
            else
            {
                result = getObject(obj);
                return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value is LuaObject)
                this[binder.Name] = (LuaObject)value;
            else
                this[binder.Name] = LuaObject.FromObject(value);
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            LuaObject[] passingArgs = Array.ConvertAll<object, LuaObject>(args,
                x => (x is LuaObject ? (LuaObject)x : LuaObject.FromObject(x)));
            LuaArguments ret = this.Call(passingArgs);
            if (ret.Length == 1)
            {
                if (ret[0].IsNil)
                    result = null;
                else
                    result = getObject(ret[0]);
                return true;
            }
            else
            {
                object[] res = Array.ConvertAll<LuaObject, object>(ret.ToArray(), x => getObject(x));
                result = res;
                return true;
            }
        }
        #endregion
    }
}