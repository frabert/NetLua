namespace NetLua.Native.Value
{
    public sealed class LuaBool : LuaObject
    {
        public static LuaObject False = new LuaBool(false);
        public static LuaObject True = new LuaBool(true);

        private readonly bool _value;

        private LuaBool(bool value) : base(LuaType.Boolean)
        {
            _value = value;
        }

        public override string AsString()
        {
            return _value ? "true" : "false";
        }

        public override object ToObject()
        {
            return _value;
        }

        public override LuaObject ToBoolean()
        {
            return this;
        }
    }
}
