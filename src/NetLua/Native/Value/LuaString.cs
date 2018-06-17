using System;

namespace NetLua.Native.Value
{
    public class LuaString : LuaObject
    {
        private readonly string _value;

        public LuaString(string value) : base(LuaType.String)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override LuaObject Length => FromNumber(_value.Length);

        public override string AsString()
        {
            return _value;
        }

        public override LuaObject ToNumber()
        {
            return double.TryParse(_value, out var number) ? (LuaObject) number : LuaNil.Instance;
        }

        public override object ToObject()
        {
            return _value;
        }

        protected bool Equals(LuaString other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LuaString) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }
    }
}
