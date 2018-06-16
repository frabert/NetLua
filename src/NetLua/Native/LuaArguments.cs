using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetLua.Native.Value;

namespace NetLua.Native
{
    public struct LuaArguments : IEnumerable<LuaObject>
    {
        private readonly LuaObject[] _list;

        public LuaArguments(IEnumerable<LuaObject> objects)
        {
            _list = objects.ToArray();
        }

        public LuaArguments(params LuaObject[] objects)
        {
            _list = objects;
        }

        public int Length => _list.Length;

        public LuaObject this[int index] => index < _list.Length ? _list[index] : LuaNil.Instance;

        public IEnumerator<LuaObject> GetEnumerator()
        {
            return _list.Cast<LuaObject>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
