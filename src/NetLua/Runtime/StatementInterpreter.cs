using System;
using System.Collections.Generic;
using System.Linq;
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
    public class StatementInterpreter
    {
        private readonly Engine _engine;

        public StatementInterpreter(Engine engine)
        {
            _engine = engine;
        }

        public async Task ExecuteLocalAssignment(LocalAssignment assign, LuaTable context, CancellationToken token = default)
        {
            for (var i = 0; i < assign.Names.Count; i++)
            {
                var var = assign.Names[i];
                var ret = i < assign.Values.Count
                    ? await _engine.EvaluateExpression(assign.Values[i], context, token).FirstAsync()
                    : LuaNil.Instance;

                context.NewIndexRaw(var, ret);
            }
        }

        public async Task ExecuteAssignment(Assignment assign, LuaTable context, CancellationToken token = default)
        {
            for (var i = 0; i < assign.Variables.Count; i++)
            {
                var expr = assign.Variables[i];
                var ret = await _engine.EvaluateExpression(assign.Expressions[i], context, token);

                if (expr is Variable var)
                {
                    var from = var.Prefix != null
                        ? await _engine.EvaluateExpression(var.Prefix, context, token).FirstAsync()
                        : context;

                    await from.NewIndexAsync(var.Name, ret[0], token);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public async Task ExecuteReturnStat(ReturnStat stat, LuaTable context, LuaReturnState returnState, CancellationToken token = default)
        {
            returnState.Return(new LuaArguments(await _engine.EvaluateExpression(stat.Expressions, context, token)));
        }

        public async Task ExecuteBlock(Block block, LuaTable context, LuaReturnState returnState, CancellationToken token = default)
        {
            foreach (var statement in block.Statements)
            {
                await _engine.ExecuteStatement(statement, context, returnState, token);

                if (returnState.DidReturn)
                {
                    break;
                }
            }
        }

        public async Task ExecuteNumericFor(NumericFor numericFor, LuaTable context, LuaReturnState returnState, CancellationToken token = default)
        {
            var var = await _engine.EvaluateExpression(numericFor.Var, context, token).FirstAsync();
            var step = await _engine.EvaluateExpression(numericFor.Step, context, token).FirstAsync();
            var limit = await _engine.EvaluateExpression(numericFor.Limit, context, token).FirstAsync();

            var forContext = new LuaTable(context);
            var compareOp = step < 0d ? BinaryOp.GreaterOrEqual : BinaryOp.LessOrEqual;

            while (!returnState.ShouldStop && !token.IsCancellationRequested && (await LuaObject.BinaryOperationAsync(compareOp, var, limit, token)).AsBool())
            {
                await forContext.NewIndexAsync(numericFor.Variable, var, token);

                await _engine.ExecuteStatement(numericFor.Block, forContext, returnState, token);

                var = await LuaObject.BinaryOperationAsync(BinaryOp.Addition, var, step, token);
            }
        }

        public async Task ExecuteGenericFor(GenericFor forStat, LuaTable context, LuaReturnState returnState, CancellationToken token)
        {
            var forContext = new LuaTable(context);
            var varNames = forStat.Variables.Select(LuaObject.FromString).ToArray();

            var expressions = await _engine.EvaluateExpression(forStat.Expressions, context, token);
            var func = expressions[0];
            var table = expressions[1];
            var args = Lua.Args(expressions.Skip(1));

            while (true)
            {
                var result = await func.CallAsync(args, token);

                if (result[0].IsNil())
                {
                    break;
                }

                for (var i = 0; i < varNames.Length; i++)
                {
                    forContext.NewIndexRaw(varNames[i], result[i]);
                }

                await _engine.ExecuteStatement(forStat.Block, forContext, returnState, token);

                args = Lua.Args(new[] {table}.Concat(result));
            }
        }

        public async Task ExecuteIfStat(IfStat ifStat, LuaTable context, LuaReturnState returnState, CancellationToken token)
        {
            var result = await _engine.EvaluateExpression(ifStat.Condition, context, token).FirstAsync();

            if (result.AsBool())
            {
                await _engine.ExecuteStatement(ifStat.Block, context, returnState, token);
                return;
            }

            if (ifStat.ElseIfs.Count > 0)
            {
                foreach (var elseIf in ifStat.ElseIfs)
                {
                    result = await _engine.EvaluateExpression(elseIf.Condition, context, token).FirstAsync();

                    if (!result.AsBool())
                    {
                        continue;
                    }

                    await _engine.ExecuteStatement(elseIf.Block, context, returnState, token);
                    return;
                }
            }

            if (ifStat.ElseBlock != null)
            {
                await _engine.ExecuteStatement(ifStat.ElseBlock, context, returnState, token);
            }
        }

        public async Task ExecuteWhileStat(WhileStat whileStat, LuaTable context, LuaReturnState returnState, CancellationToken token)
        {
            var whileContext = new LuaTable(context);

            while ((await _engine.EvaluateExpression(whileStat.Condition, context, token).FirstAsync()).AsBool())
            {
                await _engine.ExecuteStatement(whileStat.Block, whileContext, returnState, token);
            }
        }

        public async Task ExecuteRepeatStat(RepeatStat repeatStat, LuaTable context, LuaReturnState returnState, CancellationToken token)
        {
            var repeatContext = new LuaTable(context);

            do
            {
                await _engine.ExecuteStatement(repeatStat.Block, repeatContext, returnState, token);
            } while (!(await _engine.EvaluateExpression(repeatStat.Condition, repeatContext, token).FirstAsync()).AsBool());
        }

        public Task ExecuteFunctionCall(FunctionCall call, LuaTable context, CancellationToken token)
        {
            return _engine.EvaluateExpression(call, context, token);
        }
    }
}
