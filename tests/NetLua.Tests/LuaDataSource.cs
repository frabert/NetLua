using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetLua.Tests
{
    public class Labeled<T>
    {
        public Labeled(T data, string label)
        {
            Data = data;
            Label = label;
        }

        public T Data { get; }
        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
    }

    public static class LuaDataSource
    {
        public static IEnumerable<object[]> TestData => CreateTestData();

        private static IEnumerable<object[]> CreateTestData()
        {
            var assembly = typeof(LuaDataSource).Assembly;
            var prefix = typeof(LuaDataSource).Namespace + ".Lua.";
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(prefix) && x.EndsWith(".lua"));

            foreach (var resourceName in resourceNames)
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    
                    yield return new object[] { new Labeled<string>(content, Path.GetFileNameWithoutExtension(resourceName.Substring(prefix.Length))) };
                }
            }
        }
    }
}
