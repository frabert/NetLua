NetLua
======

Project description
-------------------

NetLua is a completely managed interpreter for the Lua dynamic language.
It was inspired by (and the LuaObject class is loosely based on) AluminumLua
by Alexander Corrado.
Check his project out at http://github.com/chkn/AluminumLua

Why?
----

I started this project because I needed a managed Lua interpreter and AluminumLua
had a few bugs and caveats that were difficult to solve because of its structure.
I so decided to do my own attempt at it.

Source code parsing
-------------------

As of now, the parsing is made using Irony, but any LALR parser can be used
to build the internal AST interpreted by NetLua.
Irony looks a bit slow, but it is the easiest solution for a neat C# LALR Parser.

Examples
========

Using Lua from C#
-----------------

```c#
Lua lua = new Lua();
lua.DoString("a={4, b=6, [7]=10}"); // Interpreting Lua

var a = lua.Context.Get("a"); // Accessing Lua from C#
var a_b = a["b"].AsNumber();

double number = a[7]; // Automatic type coercion
```

Registering C# methods
-----------------

```c#
static LuaArguments print(LuaArguments args)
{
  string[] strings = Array.ConvertAll<LuaObject, string>(args, x => x.ToString()); // LuaArguments can be used as a LuaObject array
  Console.WriteLine(String.Join("\t", strings));
  return Lua.Return(); // You can use the Lua.Return helper function to return values
}

Lua lua = new Lua();
lua.Context.SetGlobal("print", (LuaFunction)print);
```

Using .NET 4.0 dynamic features
-------------------------------

```c#

Lua lua = new Lua();
dynamic luaVariable = lua.Context.Get("var");

double a = luaVariable.numberValue; // Automatic type casting
double d = luaVariable.someFunc(a); // Automatic function arguments and result boxing / unboxing
```
