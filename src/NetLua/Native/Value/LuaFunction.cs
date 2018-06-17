using NetLua.Utils;

namespace NetLua.Native.Value
{
    public abstract class LuaFunction : LuaObject
    {
        private readonly string _hash;

        protected LuaFunction()
            : base(LuaType.Function)
        {
            _hash = StringUtils.GetRandomHexNumber(6);
        }

        public override string AsString()
        {
            return $"function: 0x{_hash}";
        }
    }
}