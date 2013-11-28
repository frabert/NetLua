using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetLua;
using NUnit.Framework;

namespace LuaUnits
{
    [TestFixture]
    public static class LuaTableTests
    {
        [Test]
        public static void FirstTableTest()
        {
            NetLua.Lua lua = new NetLua.Lua();
            lua.DoString(
@" a={}
k = 'x'
a[k] = 10        
a[20] = 'great'");
            LuaObject obj1 = 10;
            LuaObject obj2 = lua.DoString("return a['x']")[0];
            Assert.IsTrue(obj1.Equals(obj2));

            lua.DoString("k = 20");
            obj1 = "great";
            obj2 = lua.DoString("return a[k]")[0];
            Assert.IsTrue(obj1.Equals(obj2));
        }

        [Test]
        public static void SecondTableTest()
        {
            NetLua.Lua lua = new NetLua.Lua();
            lua.DoString(
@" a={}
a['x'] = 10        
b = a");
            LuaObject obj1 = 10;
            LuaObject obj2 = lua.DoString("return b['x']")[0];
            Assert.IsTrue(obj1.Equals(obj2));

            lua.DoString("b['x'] = 20");
            obj1 = 20;
            obj2 = lua.DoString("return a['x']")[0];
            Assert.IsTrue(obj1.Equals(obj2));
        }
    }
}
