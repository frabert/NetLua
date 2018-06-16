using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Native.Value.Functions;
using NetLua.Runtime.Ast;

namespace NetLua.Runtime
{
    public class ExpressionIntepreter
    {
        private readonly Engine _engine;

        public ExpressionIntepreter(Engine engine)
        {
            _engine = engine;
        }

        public async Task<LuaArguments> EvaluateBinaryExpressionAsync(BinaryExpression expr, LuaTable context, CancellationToken token = default)
        {
            var left = await _engine.EvaluateExpression(expr.Left, context, token).FirstAsync();
            var right = await _engine.EvaluateExpression(expr.Right, context, token).FirstAsync();

            return await LuaObject.BinaryOperationAsync(expr.Operation, left, right, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateTableAccess(TableAccess access, LuaTable context, CancellationToken token)
        {
            var expr = await _engine.EvaluateExpression(access.Expression, context, token).FirstAsync();
            var index = await _engine.EvaluateExpression(access.Index, context, token).FirstAsync();

            return await expr.IndexAsync(index, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateGetVariableAsync(Variable variable, LuaTable context, CancellationToken token = default)
        {
            var from = variable.Prefix != null
                ? await _engine.EvaluateExpression(variable.Prefix, context, token).FirstAsync()
                : context;

            return await from.IndexAsync(variable.Name, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateFunctionCall(FunctionCall expr, LuaTable context, CancellationToken token = default)
        {
            var args = await _engine.EvaluateExpression(expr.Arguments, context, token);
            var function = await _engine.EvaluateExpression(expr.Function, context, token).FirstAsync();

            return await function.CallAsync(args, token);
        }

        public async Task<LuaArguments> EvaluateUnaryExpression(UnaryExpression expr, LuaTable context, CancellationToken token = default)
        {
            var op = await _engine.EvaluateExpression(expr.Expression, context, token).FirstAsync();

            switch (expr.Operation)
            {
                case UnaryOp.Negate:
                    return Lua.Args(!op.AsBool());
                case UnaryOp.Invert:
                    return Lua.Args(-op.AsNumber());
                case UnaryOp.Length:
                    return Lua.Args(op.Length);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<LuaArguments> EvaluateVarargs(LuaTable context)
        {
            LuaObject current = context;

            while (!(current is LuaTableFunction))
            {
                current = context.Parent;

                if (current.IsNil())
                {
                    return Lua.ArgsAsync();
                }
            }

            return Lua.ArgsAsync(((LuaTableFunction)current).Varargs);
        }

        public async Task<LuaArguments> EvaluateTableConstructor(TableConstructor constructor, LuaTable context, CancellationToken token = default)
        {
            var table = LuaObject.CreateTable();
            var keyCounter = (int?)1;

            for (var index = 0; index < constructor.Values.Count; index++)
            {
                var addAll = keyCounter.HasValue && index == constructor.Values.Count - 1;
                var kv = constructor.Values[index];

                // Get the key.
                LuaObject key;

                if (kv.Key == null)
                {
                    if (addAll)
                    {
                        key = LuaNil.Instance;
                    }
                    else if (keyCounter.HasValue)
                    {
                        key = keyCounter.Value;
                        keyCounter++;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    key = await _engine.EvaluateExpression(kv.Key, context, token).FirstAsync();
                    keyCounter = null;
                    addAll = false;
                }

                // Get the value.
                var value = await _engine.EvaluateExpression(kv.Value, context, token);

                if (addAll)
                {
                    for (var i = 0; i < value.Length; i++)
                    {
                        table.NewIndexRaw(keyCounter.Value + i, value[i]);
                    }
                }
                else
                {
                    table.NewIndexRaw(key, value[0]);
                }
            }

            return Lua.Args(table);
        }

        public Task<LuaArguments> EvaluateFunctionDefinition(FunctionDefinition definition, LuaTable context, CancellationToken token = default)
        {
            return Lua.ArgsAsync(new LuaInterpreterFunction(_engine, definition, context, false));
        }
    }
}
