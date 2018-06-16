using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Native.Value.Functions;
using NetLua.Runtime.Ast;

namespace NetLua.Runtime
{
    public class Engine
    {
        public static async Task<LuaArguments> EvalAsync(string str, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var parser = new LuaParser();

            var def = new FunctionDefinition
            {
                Arguments = new List<Argument>(),
                Body = parser.ParseString(str),
                Varargs = true
            };

            var function = new LuaInterpreterFunction(def, context);

            return await function.CallAsync(Lua.Args(), token);
        }

        private static async Task ExecuteLocalAssignment(LocalAssignment assign, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            for (var i = 0; i < assign.Names.Count; i++)
            {
                var var = assign.Names[i];
                var ret = await CompileExpression(assign.Values[i], context, token).FirstAsync();

                context.NewIndexRaw(var, ret);
            }
        }

        public static async Task ExecuteAssignment(Assignment assign, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            for (var i = 0; i < assign.Variables.Count; i++)
            {
                var expr = assign.Variables[i];
                var ret = await CompileExpression(assign.Expressions[i], context, token);

                if (expr is Variable var)
                {
                    var from = var.Prefix != null
                        ? await CompileExpression(var.Prefix, context, token).FirstAsync()
                        : context;

                    await from.NewIndexAsync(var.Name, ret[0], token);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public static async Task ExecuteReturnStat(ReturnStat stat, LuaTable context, LuaReturnState returnState, CancellationToken token = default(CancellationToken))
        {
            returnState.Return(new LuaArguments(await CompileExpression(stat.Expressions, context, token)));
        }

        public static async Task ExecuteBlock(Block block, LuaTable context, LuaReturnState returnState, CancellationToken token = default(CancellationToken))
        {
            foreach (var statement in block.Statements)
            {
                await ExecuteStatement(statement, context, returnState, token);

                if (returnState.DidReturn)
                {
                    break;
                }
            }
        }

        private static Task<LuaArguments> ExecuteVarargs(LuaTable context)
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

        private static async Task<LuaArguments> ExecuteTableConstructor(TableConstructor constructor, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var table = LuaObject.CreateTable();
            var keyCounter = (int?) 1;

            LuaObject key;

            for (var index = 0; index < constructor.Values.Count; index++)
            {
                var addAll = keyCounter.HasValue && index == constructor.Values.Count - 1;
                var kv = constructor.Values[index];

                // Get the key.
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
                    key = await CompileExpression(kv.Key, context, token).FirstAsync();
                    keyCounter = null;
                    addAll = false;
                }

                // Get the value.
                var value = await CompileExpression(kv.Value, context, token);

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

        public static async Task<LuaArguments> ExecuteFunctionCall(FunctionCall expr, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var args = await CompileExpression(expr.Arguments, context, token);
            var function = await CompileExpression(expr.Function, context, token).FirstAsync();

            return await function.CallAsync(args, token);
        }

        public static async Task ExecuteNumericFor(NumericFor numericFor, LuaTable context, LuaReturnState returnState, CancellationToken token = default(CancellationToken))
        {
            var var = await CompileExpression(numericFor.Var, context, token).FirstAsync();
            var step = await CompileExpression(numericFor.Step, context, token).FirstAsync();
            var limit = await CompileExpression(numericFor.Limit, context, token).FirstAsync();

            var forContext = new LuaTable(context);
            var compareOp = step < 0d ? BinaryOp.GreaterOrEqual : BinaryOp.LessOrEqual;
            
            while (!returnState.ShouldStop && !token.IsCancellationRequested && (await LuaObject.BinaryOperationAsync(compareOp, var, limit, token)).AsBool())
            {
                await forContext.NewIndexAsync(numericFor.Variable, var, token);

                await ExecuteStatement(numericFor.Block, forContext, returnState, token);

                var = await LuaObject.BinaryOperationAsync(BinaryOp.Addition, var, step, token);
            }
        }

        public static async Task<LuaArguments> CompileExpression(IList<IExpression> exprs, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var results = new List<LuaObject>();

            for (var i = 0; i < exprs.Count; i++)
            {
                var addAll = i == exprs.Count - 1;
                var objects = await CompileExpression(exprs[i], context, token);

                if (addAll) results.AddRange(objects);
                else results.Add(objects[0]);
            }

            return Lua.Args(results);
        }

        private static async Task<LuaArguments> ExecuteUnaryExpression(UnaryExpression expr, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var op = await CompileExpression(expr.Expression, context, token).FirstAsync();

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

        private static async Task<LuaArguments> ExecuteBinaryExpressionAsync(BinaryExpression expr, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var left = await CompileExpression(expr.Left, context, token).FirstAsync();
            var right = await CompileExpression(expr.Right, context, token).FirstAsync();

            return await LuaObject.BinaryOperationAsync(expr.Operation, left, right, token).ToArgsAsync();
        }

        private static async Task<LuaArguments> ExecuteTableAccess(TableAccess access, LuaTable context, CancellationToken token)
        {
            var expr = await CompileExpression(access.Expression, context, token).FirstAsync();
            var index = await CompileExpression(access.Index, context, token).FirstAsync();

            return await expr.IndexAsync(index, token).ToArgsAsync();
        }

        private static async Task<LuaArguments> ExecuteGetVariableAsync(Variable variable, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            var from = variable.Prefix != null
                ? await CompileExpression(variable.Prefix, context, token).FirstAsync()
                : context;

            return await from.IndexAsync(variable.Name, token).ToArgsAsync();
        }

        public static Task ExecuteStatement(IStatement stat, LuaTable context, LuaReturnState returnState, CancellationToken token = default(CancellationToken))
        {
            if (returnState.DidReturn || token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            switch (stat)
            {
                case Assignment assignment:
                    return ExecuteAssignment(assignment, context, token);
                case LocalAssignment localAssignment:
                    return ExecuteLocalAssignment(localAssignment, context, token);
                case FunctionCall call:
                    return ExecuteFunctionCall(call, context, token);
                case Block block:
                    return ExecuteBlock(block, context, returnState, token);
                //case IfStat ifStat:
                //    return CompileIfStat(ifStat, returnTarget, breakTarget, context);
                case ReturnStat returnStat:
                    return ExecuteReturnStat(returnStat, context, returnState, token);
                //case WhileStat whileStat:
                //    return CompileWhileStat(whileStat, returnTarget, context);
                //case RepeatStat repeatStat:
                //    return CompileRepeatStatement(repeatStat, returnTarget, context);
                //case GenericFor @for:
                //    return CompileGenericFor(@for, returnTarget, context);
                case NumericFor numericFor:
                    return ExecuteNumericFor(numericFor, context, returnState, token);
                default:
                    throw new NotImplementedException(stat.GetType().Name);
            }
        }

        public static Task<LuaArguments> CompileExpression(IExpression expr, LuaTable context, CancellationToken token = default(CancellationToken))
        {
            switch (expr)
            {
                case NumberLiteral numberLiteral:
                    return Lua.ArgsAsync((LuaObject)numberLiteral.Value);
                case StringLiteral stringLiteral:
                    return Lua.ArgsAsync((LuaObject)stringLiteral.Value);
                case BoolLiteral literal:
                    return Lua.ArgsAsync((LuaObject)literal.Value);
                case NilLiteral _:
                    return Lua.ArgsAsync(LuaNil.Instance);
                case BinaryExpression binaryExpression:
                    return ExecuteBinaryExpressionAsync(binaryExpression, context, token);
                case UnaryExpression expression:
                    return ExecuteUnaryExpression(expression, context, token);
                case Variable variable:
                    return ExecuteGetVariableAsync(variable, context, token);
                case TableAccess access:
                    return ExecuteTableAccess(access, context, token);
                case FunctionCall call:
                    return ExecuteFunctionCall(call, context, token);
                case FunctionDefinition definition:
                    return Lua.ArgsAsync(LuaObject.CreateFunction(definition, context));
                case VarargsLiteral _:
                    return ExecuteVarargs(context);
                case TableConstructor constructor:
                    return ExecuteTableConstructor(constructor, context, token);
            }

            throw new NotImplementedException(expr.GetType().Name);
        }
    }
}
