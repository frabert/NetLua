using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Utils;

namespace NetLua.Native.Value
{
    public class LuaTable : LuaObject
    {
        private readonly ConcurrentDictionary<LuaObject, LuaObject> _table;
        private readonly string _hash;

        internal readonly LuaObject Parent;

        internal LuaTable(LuaObject parent) : base(LuaType.Table)
        {
            _table = new ConcurrentDictionary<LuaObject, LuaObject>();
            _hash = StringUtils.GetRandomHexNumber(6);
            Parent = parent;
        }

        public LuaTable() : this(LuaNil.Instance)
        {
        }

        public override LuaObject Length => FromNumber(_table.Count);

        public override object ToObject()
        {
            return _table;
        }

        public override string AsString()
        {
            return $"table: 0x{_hash}";
        }

        public virtual bool ContainsKey(LuaObject key)
        {
            return _table.ContainsKey(key);
        }

        public override Task<LuaObject> IndexAsync(LuaObject key, CancellationToken token = default)
        {
            if (ContainsKey(key))
            {
                return Task.FromResult(IndexRaw(key));
            }

            var index = GetMetaMethod("__index");

            switch (index.Type)
            {
                case LuaType.Nil when !Parent.IsNil():
                    return Parent.IndexAsync(key, token);
                case LuaType.Nil:
                    return Task.FromResult(LuaNil.Instance);
                case LuaType.Function:
                    return index.CallAsync(Lua.Args(this, key), token).FirstAsync();
                default:
                    return Task.FromResult(index.IndexRaw(key));
            }
        }

        public override Task NewIndexAsync(LuaObject key, LuaObject value, CancellationToken token = default)
        {
            var newindex = GetMetaMethod("__newindex");
            var contains = ContainsKey(key);

            if (!Parent.IsNil() && !contains && newindex.IsNil())
            {
                return Parent.NewIndexAsync(key, value, token);
            }

            if (contains || newindex.IsNil())
            {
                NewIndexRaw(key, value);
                return Task.CompletedTask;
            }

            if (newindex.IsFunction())
            {
                return newindex.CallAsync(Lua.Args(this, key), token);
            }

            newindex.NewIndexRaw(key, value);
            return Task.CompletedTask;
        }

        public override LuaObject IndexRaw(LuaObject key)
        {
            return _table.TryGetValue(key, out var value) ? value : LuaNil.Instance;
        }

        public override void NewIndexRaw(LuaObject key, LuaObject value)
        {
            _table.AddOrUpdate(key, value, (k, v) => value);
        }

        public override Task<LuaArguments> CallAsync(LuaArguments args, CancellationToken token = default)
        {
            var call = GetMetaMethod("__call");

            return call.IsFunction() 
                ? call.CallAsync(args, token) 
                : base.CallAsync(args, token);
        }

        protected bool Equals(LuaTable other)
        {
            return string.Equals(_hash, other._hash);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LuaTable) obj);
        }

        public override int GetHashCode()
        {
            return (_hash != null ? _hash.GetHashCode() : 0);
        }
    }
}
