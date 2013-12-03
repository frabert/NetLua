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
    public static class LuaObjectTests
    {
        [Test]
        public static void ObjectEqualityNumber()
        {
            LuaObject obj1 = 10;
            LuaObject obj2 = 10;

            Assert.IsTrue(obj1.Equals(obj2));
            Assert.IsTrue(obj2.Equals(obj1));
        }

        [Test]
        public static void ObjectEqualityString()
        {
            LuaObject obj1 = "test";
            LuaObject obj2 = "test";

            Assert.IsTrue(obj1.Equals(obj2));
            Assert.IsTrue(obj2.Equals(obj1));
        }

        [Test]
        public static void ObjectEqualityCoercion()
        {
            LuaObject obj1 = "10";
            LuaObject obj2 = 10;

            Assert.IsFalse(obj1.Equals(obj2));
            Assert.IsFalse(obj2.Equals(obj1));
        }

        [Test]
        public static void GeneralEquality()
        {
            LuaObject a = "test";

            Assert.IsTrue(a == "test");
        }

        [Test]
        public static void LogicalOperators()
        {
            LuaObject a = "test";
            LuaObject b = LuaObject.Nil;

            Assert.IsTrue((a | b) == a);
            Assert.IsTrue((a | null) == a);

            Assert.IsTrue((a & b) == b);
            Assert.IsTrue((a & null) == LuaObject.Nil);
        }
    }
}
