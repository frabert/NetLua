using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NetLua
{
    public static class IoLibrary
    {
        class FileObject
        {
            public FileObject(Stream s)
            {
                stream = s;
                if(s.CanRead)
                    reader = new StreamReader(s);
                if(s.CanWrite)
                    writer = new StreamWriter(s);
            }

            public Stream stream;
            public StreamReader reader;
            public StreamWriter writer;
        }

        static dynamic FileMetatable = LuaObject.NewTable();

        static LuaObject currentInput = LuaObject.Nil, currentOutput = LuaObject.Nil;

        static bool isStream(LuaObject obj)
        {
            return (obj.IsUserData && obj.luaobj is FileObject);
        }

        static LuaObject CreateFileObject(Stream stream)
        {
            LuaObject obj = LuaObject.FromObject(new FileObject(stream));
            obj.Metatable = FileMetatable;

            return obj;
        }

        static LuaObject CreateFileObject(Stream stream, bool autoflush)
        {
            FileObject fobj = new FileObject(stream);
            fobj.writer.AutoFlush = autoflush;
            LuaObject obj = LuaObject.FromObject(fobj);
            obj.Metatable = FileMetatable;

            return obj;
        }

        public static void AddIoLibrary(LuaContext Context)
        {
            dynamic io = LuaObject.NewTable();

            FileMetatable.__index = LuaObject.NewTable();
            FileMetatable.__index.write = (LuaFunction)write;
            FileMetatable.__index.close = (LuaFunction)close;
            FileMetatable.__index.flush = (LuaFunction)flush;

            io.open = (LuaFunction)io_open;
            io.type = (LuaFunction)io_type;
            io.input = (LuaFunction)io_input;
            io.output = (LuaFunction)io_output;
            io.temp = (LuaFunction)io_temp;

            io.stdin = CreateFileObject(Console.OpenStandardInput());
            io.stdout = CreateFileObject(Console.OpenStandardOutput(), true);
            io.stderr = CreateFileObject(Console.OpenStandardError(), true);

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
                FileObject fobj = obj.luaobj as FileObject;
                if (!fobj.stream.CanWrite && !fobj.stream.CanRead)
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
                FileObject fobj = self.luaobj as FileObject;
                foreach (var arg in args)
                {
                    if (arg == self)
                        continue;
                    if (!(arg.IsString || arg.IsNumber))
                    {
                        Lua.Return();
                    }

                    if (fobj.stream.CanWrite)
                    {
                        fobj.writer.Write(arg.ToString());
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
                FileObject fobj = obj.luaobj as FileObject;
                fobj.stream.Close();
            }
            return Lua.Return();
        }

        static LuaArguments flush(LuaArguments args)
        {
            var obj = args[0];
            if(isStream(obj))
            {
                FileObject fobj = obj.luaobj as FileObject;
                fobj.writer.Flush();
            }
            return Lua.Return();
        }
    }
}
