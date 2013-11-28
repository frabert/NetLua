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
