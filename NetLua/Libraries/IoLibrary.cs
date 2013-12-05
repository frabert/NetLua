using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetLua
{
    public static class IoLibrary
    {
        static dynamic FileMetatable = LuaObject.NewTable();

        static LuaObject currentInput = LuaObject.Nil, currentOutput = LuaObject.Nil;

        static bool isStream(LuaObject obj)
        {
            return (obj.IsUserData && obj.luaobj is Stream);
        }

        static LuaObject CreateFileObject(Stream stream)
        {
            LuaObject obj = LuaObject.FromObject(stream);
            obj.Metatable = FileMetatable;

            return obj;
        }

        public static void AddIoLibrary(LuaContext Context)
        {
            dynamic io = LuaObject.NewTable();

            FileMetatable.__index = LuaObject.NewTable();
            FileMetatable.__index.write = (LuaFunction)write;
            FileMetatable.__index.close = (LuaFunction)close;

            io.open = (LuaFunction)io_open;
            io.type = (LuaFunction)io_type;
            io.input = (LuaFunction)io_input;
            io.output = (LuaFunction)io_output;
            io.temp = (LuaFunction)io_temp;

            io.stdin = CreateFileObject(Console.OpenStandardInput());
            io.stdout = CreateFileObject(Console.OpenStandardOutput());
            io.stderr = CreateFileObject(Console.OpenStandardError());

            Context.Set("io", io);
        }

        static LuaArguments io_open(LuaArguments args)
        {
            var file = args[0];
            var mode = args[1];

            if (file.IsString)
            {
                FileMode fmode = FileMode.Open;
                FileAccess faccess = FileAccess.Read;
                if (mode.IsString)
                {
                    switch (mode.ToString())
                    {
                        case "r":
                            faccess = FileAccess.Read; break;
                        case "w":
                            fmode = FileMode.Create; faccess = FileAccess.ReadWrite; break;
                        case "a":
                            fmode = FileMode.Append; faccess = FileAccess.Write; break;
                        case "r+":
                        case "w+":
                        case "a+":
                            // TODO: Implement rwa+
                            throw new NotImplementedException();
                    }
                }
                FileStream stream = new FileStream(file.ToString(), fmode, faccess);

                return Lua.Return(CreateFileObject(stream));
            }
            else
            {
                return Lua.Return();
            }
        }

        static LuaArguments io_type(LuaArguments args)
        {
            var obj = args[0];
            if (isStream(obj))
            {
                Stream stream = obj.luaobj as Stream;
                if (!stream.CanRead && !stream.CanWrite)
                {
                    return Lua.Return("closed file");
                }
                else
                {
                    return Lua.Return("file");
                }
            }
            return Lua.Return();
        }

        static LuaArguments io_input(LuaArguments args)
        {
            var obj = args[0];
            if (isStream(obj))
            {
                currentInput = obj;
                return Lua.Return();
            }
            else if (obj.IsString)
            {
                currentInput = io_open(args)[0];
                return Lua.Return();
            }
            else if (args.Length == 0)
            {
                return Lua.Return(currentInput);
            }
            else
            {
                throw new LuaException("Invalid argument");
            }
        }

        static LuaArguments io_output(LuaArguments args)
        {
            var obj = args[0];
            if (isStream(obj))
            {
                currentOutput = obj;
                return Lua.Return();
            }
            else if (obj.IsString)
            {
                currentOutput = io_open(args)[0];
                return Lua.Return();
            }
            else if (args.Length == 0)
            {
                return Lua.Return(currentOutput);
            }
            else
            {
                throw new LuaException("Invalid argument");
            }
        }

        static LuaArguments io_temp(LuaArguments args)
        {
            string path = Path.GetTempFileName();
            Stream s = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write, Int16.MaxValue, FileOptions.DeleteOnClose);

            return Lua.Return(CreateFileObject(s));
        }

        static LuaArguments write(LuaArguments args)
        {
            var self = args[0];
            if (isStream(self))
            {
                Stream stream = self.luaobj as Stream;
                StreamWriter w = new StreamWriter(stream);
                foreach (var arg in args)
                {
                    if (arg == self)
                        continue;
                    if (!(self.IsString || self.IsNumber))
                    {
                        Lua.Return();
                    }

                    if (stream.CanWrite)
                    {
                        w.Write(arg.ToString());
                        w.Flush();
                    }
                    else
                    {
                        Lua.Return();
                    }
                }
                return Lua.Return(self);
            }
            else
            {
                return Lua.Return();
            }
        }

        static LuaArguments close(LuaArguments args)
        {
            var obj = args[0];
            if (isStream(obj))
            {
                Stream stream = obj.luaobj as Stream;
                stream.Close();
            }
            return Lua.Return();
        }
    }
}
