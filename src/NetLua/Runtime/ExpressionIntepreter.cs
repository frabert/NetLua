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

        public async Task<LuaArguments> EvaluateBinaryExpressionAsync(BinaryExpression expr, LuaState state, CancellationToken token = default)
        {
            var left = await _engine.EvaluateExpression(expr.Left, state, token).FirstAsync();
            var right = await _engine.EvaluateExpression(expr.Right, state, token).FirstAsync();

            return await LuaObject.BinaryOperationAsync(expr.Operation, left, right, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateTableAccess(TableAccess access, LuaState state, CancellationToken token)
        {
            var expr = await _engine.EvaluateExpression(access.Expression, state, token).FirstAsync();
            var index = await _engine.EvaluateExpression(access.Index, state, token).FirstAsync();

            return await expr.IndexAsync(index, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateGetVariableAsync(Variable variable, LuaState state, CancellationToken token = default)
        {
            var from = variable.Prefix != null
                ? await _engine.EvaluateExpression(variable.Prefix, state, token).FirstAsync()
                : state.Context;

            if (from.IsNil())
            {
                throw new LuaException($"attempt to index a nil value near {variable.Name}");
            }

            return await from.IndexAsync(variable.Name, token).ToArgsAsync();
        }

        public async Task<LuaArguments> EvaluateFunctionCall(FunctionCall expr, LuaState state, CancellationToken token = default)
        {
            var args = await _engine.EvaluateExpression(expr.Arguments, state, token);
            var function = await _engine.EvaluateExpression(expr.Function, state, token).FirstAsync();

            return await function.CallAsync(args, token);
        }

        public async Task<LuaArguments> EvaluateUnaryExpression(UnaryExpression expr, LuaState state, CancellationToken token = default)
        {
            var op = await _engine.EvaluateExpression(expr.Expression, state, token).FirstAsync();

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

        public Task<LuaArguments> EvaluateVarargs(LuaState state)
        {
            var current = state.Context;

            while (!(current is LuaTableFunction))
            {
                if (current.Parent.IsNil())
                {
                    return Lua.ArgsAsync();
                }

                current = (LuaTable) current.Parent;
            }

            return Lua.ArgsAsync(((LuaTableFunction) current).Varargs);
        }

        public async Task<LuaArguments> EvaluateTableConstructor(TableConstructor constructor, LuaState state, CancellationToken token = default)
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
                    key = await _engine.EvaluateExpression(kv.Key, state, token).FirstAsync();
                    keyCounter = null;
                    addAll = false;
                }

                // Get the value.
                var value = await _engine.EvaluateExpression(kv.Value, state, token);

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

        public Task<LuaArguments> EvaluateFunctionDefinition(FunctionDefinition definition, LuaState state, CancellationToken token = default)
        {
            return Lua.ArgsAsync(new LuaInterpreterFunction(_engine, definition, state.Context, false));
        }
    }
}
