using System;

namespace NetLua.Native
{
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
            : base($"Error in {file}({row},{col}): {message}")
        { }

        public LuaException(string message)
            : base(message)
        { }
    }
}