NetLua
======

NetLua is a completely managed interpreter for the Lua dynamic language.
It was inspired by (and the LuaObject class is loosely based on) AluminumLua
by Alexander Corrado.
Check his project out at http://github.com/chkn/AluminumLua

I started this project because I needed a managed Lua interpreter and AluminumLua
had a few bugs and caveats that were difficult to solve because of its structure.
I so decided to do my own attempt at it.

As of now, the parsing is made using Irony, but any LALR parser can be used
to build the internal AST interpreted by NetLua.