using System.Globalization;

namespace NetLua.Native.Value
{
    public class LuaNumber : LuaObject
    {
        private readonly double _value;

        public LuaNumber(double value) : base(LuaType.Number)
        {
            _value = value;
        }

        public override LuaObject ToNumber()
        {
            return this;
        }

        public override object ToObject()
        {
            return _value;
        }

        public override string AsString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }

        protected bool Equals(LuaNumber other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LuaNumber) obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
