using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Native.Value.Functions;

namespace NetLua.Utils
{
    internal static class MethodUtils
    {
        public static object[] GetArguments(MethodBase method, Engine engine, LuaArguments args, int offset, CancellationToken token = default)
        {
            var parameters = method.GetParameters();
            var methodArgs = new object[parameters.Length];
            var argCount = args.Length - offset;
            var argCounter = 0;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                // Get the argument
                if (argCounter >= argCount)
                {
                    if (parameter.HasDefaultValue)
                    {
                        methodArgs[i] = parameter.DefaultValue;
                    }
                    else
                    {
                        throw new LuaException($"expected {parameters.Length + offset} arguments, got {args.Length} instead");
                    }
                }

                // Convert the argument
                object arg;

                if (parameter.ParameterType == typeof(LuaArguments))
                {
                    arg = args;
                }
                else if (parameter.ParameterType == typeof(CancellationToken))
                {
                    arg = token;
                }
                else if (parameter.ParameterType == typeof(Engine))
                {
                    arg = engine;
                }
                else
                {
                    arg = args[argCounter + offset].ToObject(parameter.ParameterType);
                    argCounter++;
                }

                methodArgs[i] = arg;
            }

            return methodArgs;
        }

        private static LuaArguments CreateArgs(object obj)
        {
            if (obj is LuaArguments args)
            {
                return args;
            }

            return Lua.Args(LuaObject.FromObject(obj));
        }

        public static LuaFunction CreateFunction(MethodInfo method)
        {
            var returnType = method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            // Non-async
            if (!isTask)
            {
                return new LuaDirectFunction((e, args) =>
                {
                    object self;
                    object[] methodArgs;

                    if (method.IsStatic)
                    {
                        self = null;
                        methodArgs = GetArguments(method, e, args, 0);
                    }
                    else
                    {
                        self = args[0].ToObject(method.DeclaringType);
                        methodArgs = GetArguments(method, e, args, 1);
                    }

                    return CreateArgs(method.Invoke(self, methodArgs));
                });
            }

            // Async
            var hasReturn = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);

            return new LuaAsyncFunction(async (e, args, token) =>
            {
                object self;
                object[] methodArgs;

                if (method.IsStatic)
                {
                    self = null;
                    methodArgs = GetArguments(method, e, args, 0, token);
                }
                else
                {
                    self = args[0].ToObject(method.DeclaringType);
                    methodArgs = GetArguments(method, e, args, 1, token);
                }

                var task = (Task)method.Invoke(self, methodArgs);

                await task;

                if (!hasReturn)
                {
                    return Lua.Args();
                }

                var returnProperty = task.GetType().GetProperty("Result") ?? throw new InvalidOperationException();

                return CreateArgs(returnProperty.GetValue(task));
            });
        }
    }
}
