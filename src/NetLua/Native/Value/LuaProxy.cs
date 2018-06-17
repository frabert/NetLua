using System;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Native.Proxy;

namespace NetLua.Native.Value
{
    public class LuaProxy : LuaTable
    {
        private readonly object _instance;
        private readonly LuaProxyCacheItem _cacheItem;

        public LuaProxy(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _cacheItem = LuaProxyCache.Get(instance.GetType());
        }

        public override LuaObject Length => _cacheItem.Methods.Count + _cacheItem.Properties.Count + base.Length;

        public override object ToObject()
        {
            return _instance;
        }

        public override bool ContainsKey(LuaObject key)
        {
            var str = key.AsString();

            return _cacheItem.Methods.ContainsKey(str) || _cacheItem.Properties.ContainsKey(str) || base.ContainsKey(key);
        }

        public override LuaObject IndexRaw(LuaObject key)
        {
            if (base.ContainsKey(key))
            {
                return base.IndexRaw(key);
            }

            var str = key.AsString();

            if (_cacheItem.Methods.ContainsKey(str))
            {
                return _cacheItem.Methods[str];
            }

            if (!_cacheItem.Properties.ContainsKey(str))
            {
                return LuaNil.Instance;
            }

            var property = _cacheItem.Properties[str];

            if (!property.Writeable)
            {
                throw new LuaException($"the index {str} is not readable");
            }

            return FromObject(property.Info.GetValue(_instance));
        }

        public override void NewIndexRaw(LuaObject key, LuaObject value)
        {
            var str = key.AsString();

            if (!_cacheItem.Properties.ContainsKey(str))
            {
                base.NewIndexRaw(key, value);
                return;
            }

            var property = _cacheItem.Properties[str];

            if (!property.Writeable)
            {
                throw new LuaException($"the index {str} is not writeable");
            }

            property.Info.SetValue(_instance, value.ToObject(property.Info.PropertyType));
        }

        public override Task<LuaArguments> CallAsync(Engine engine, LuaArguments args,
            CancellationToken token = default)
        {
            if (_instance is ICallableProxy proxy)
            {
                return proxy.CallAsync(args, token);
            }

            return base.CallAsync(engine, args, token);
        }
    }
}
