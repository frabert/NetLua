/*
 * NetLua by Francesco Bertolaccini
 * Project inspired by AluminumLua, a project by Alexander Corrado
 * (See his repo at http://github.com/chkn/AluminumLua)
 * 
 * NetLua - a managed implementation of the Lua dynamic programming language
 * 
 * LuaContext.cs
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lua
{
    public class LuaContext
    {
        LuaContext parent;
        Dictionary<string, LuaObject> variables;

        /// <summary>
        /// Used to create scopes
        /// </summary>
        public LuaContext(LuaContext Parent)
        {
            parent = Parent;
            variables = new Dictionary<string, LuaObject>();
        }

        public LuaContext() : this(null) { }

        /// <summary>
        /// Sets or creates a variable in the local scope
        /// </summary>
        public void SetLocal(string Name, LuaObject Value)
        {
            if (!variables.ContainsKey(Name))
                variables.Add(Name, Value);
            else
                variables[Name] = Value;
        }

        /// <summary>
        /// Sets or creates a variable in the global scope
        /// </summary>
        public void SetGlobal(string Name, LuaObject Value)
        {
            if (parent == null)
                SetLocal(Name, Value);
            else
                parent.SetGlobal(Name, Value);
        }

        /// <summary>
        /// Returns the nearest declared variable value or nil
        /// </summary>
        public LuaObject Get(string Name)
        {
            if (variables.ContainsKey(Name))
                return variables[Name];
            else if (parent != null)
                return parent.Get(Name);
            else
                return LuaObject.Nil;
        }

        public void Set(string Name, LuaObject Value)
        {
            if (parent == null || variables.ContainsKey(Name))
                SetLocal(Name, Value);
            else
                SetGlobal(Name, Value);
        }
    }
}
