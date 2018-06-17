/*
 * See LICENSE file
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using NetLua.Native;
using NetLua.Native.Value;

namespace NetLua
{
    public class Lua
    {
        /// <summary>
        /// Helper function for returning Nil from a function
        /// </summary>
        /// <returns>Nil</returns>
        public static LuaArguments Args()
        {
            return new LuaArguments(LuaNil.Instance);
        }

        /// <summary>
        /// Helper function for returning Nil from a function
        /// </summary>
        /// <returns>Nil</returns>
        public static Task<LuaArguments> ArgsAsync()
        {
            return Task.FromResult(new LuaArguments(LuaNil.Instance));
        }

        /// <summary>
        /// Helper function for returning objects from a function
        /// </summary>
        /// <param name="values">The objects to return</param>
        public static LuaArguments Args(params LuaObject[] values)
        {
            return new LuaArguments(values);
        }

        /// <summary>
        /// Helper function for returning objects from a function
        /// </summary>
        /// <param name="values">The objects to return</param>
        public static LuaArguments Args(IEnumerable<LuaObject> values)
        {
            return new LuaArguments(values);
        }

        /// <summary>
        /// Helper function for returning Nil from a function
        /// </summary>
        /// <returns>Nil</returns>
        public static Task<LuaArguments> ArgsAsync(params LuaObject[] values)
        {
            return Task.FromResult(new LuaArguments(values));
        }
    }
}
