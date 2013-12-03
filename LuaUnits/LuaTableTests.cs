/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2013 Francesco Bertolaccini
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
