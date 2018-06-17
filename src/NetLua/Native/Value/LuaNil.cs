namespace NetLua.Native.Value
{
    public sealed class LuaNil : LuaObject
    {
        public static readonly LuaObject Instance;

        static LuaNil()
        {
            Instance = new LuaNil();
            Instance.MetaTable = Instance;
        }

        private LuaNil() : base(LuaType.Nil)
        {
        }

        public override object ToObject()
        {
            return null;
        }

        public override LuaObject ToBoolean()
        {
            return LuaBool.False;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LuaNil;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
