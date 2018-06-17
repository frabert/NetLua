using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Extensions;
using NetLua.Native;
using NetLua.Native.Value;
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

        public async Task ExecuteLocalAssignment(LocalAssignment assign, LuaState state, CancellationToken token = default)
        {
            for (var i = 0; i < assign.Names.Count; i++)
            {
                var var = assign.Names[i];
                var ret = i < assign.Values.Count
                    ? await _engine.EvaluateExpression(assign.Values[i], state, token).FirstAsync()
                    : LuaNil.Instance;

                state.Context.NewIndexRaw(var, ret);
            }
        }

        public async Task ExecuteAssignment(Assignment assign, LuaState state, CancellationToken token = default)
        {
            for (var i = 0; i < assign.Variables.Count; i++)
            {
                var expr = assign.Variables[i];
                var ret = await _engine.EvaluateExpression(assign.Expressions[i], state, token);

                if (expr is Variable var)
                {
                    var from = var.Prefix != null
                        ? await _engine.EvaluateExpression(var.Prefix, state, token).FirstAsync()
                        : state.Context;

                    await from.NewIndexAsync(var.Name, ret[0], token);
                }
                else if (expr is TableAccess tableAccess)
                {
                    var table = await _engine.EvaluateExpression(tableAccess.Expression, state, token).FirstAsync();
                    var index = await _engine.EvaluateExpression(tableAccess.Index, state, token).FirstAsync();

                    await table.NewIndexAsync(index, ret[0], token);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public async Task ExecuteReturnStat(ReturnStat stat, LuaState state, CancellationToken token = default)
        {
            state.FunctionState.SetResult(new LuaArguments(await _engine.EvaluateExpression(stat.Expressions, state, token)));
        }

        public async Task ExecuteBlock(Block block, LuaState state, CancellationToken token = default)
        {
            foreach (var statement in block.Statements)
            {
                await _engine.ExecuteStatement(statement, state, token);

                if (state.FunctionState.DidReturn)
                {
                    break;
                }
            }
        }

        public async Task ExecuteNumericFor(NumericFor numericFor, LuaState state, CancellationToken token = default)
        {
            var var = await _engine.EvaluateExpression(numericFor.Var, state, token).FirstAsync();
            var step = await _engine.EvaluateExpression(numericFor.Step, state, token).FirstAsync();
            var limit = await _engine.EvaluateExpression(numericFor.Limit, state, token).FirstAsync();

            var forState = state.WithNewContext();
            var compareOp = step < 0d ? BinaryOp.GreaterOrEqual : BinaryOp.LessOrEqual;

            while (!state.FunctionState.DidReturn && !token.IsCancellationRequested && (await LuaObject.BinaryOperationAsync(compareOp, var, limit, token)).AsBool())
            {
                await forState.Context.NewIndexAsync(numericFor.Variable, var, token);

                await _engine.ExecuteStatement(numericFor.Block, forState, token);

                var = await LuaObject.BinaryOperationAsync(BinaryOp.Addition, var, step, token);
            }
        }

        public async Task ExecuteGenericFor(GenericFor forStat, LuaState state, CancellationToken token)
        {
            var forState = state.WithNewContext();
            var varNames = forStat.Variables.Select(LuaObject.FromString).ToArray();

            var expressions = await _engine.EvaluateExpression(forStat.Expressions, state, token);
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
                    forState.Context.NewIndexRaw(varNames[i], result[i]);
                }

                await _engine.ExecuteStatement(forStat.Block, forState, token);

                args = Lua.Args(new[] {table}.Concat(result));
            }
        }

        public async Task ExecuteIfStat(IfStat ifStat, LuaState state, CancellationToken token)
        {
            var result = await _engine.EvaluateExpression(ifStat.Condition, state, token).FirstAsync();

            if (result.AsBool())
            {
                await _engine.ExecuteStatement(ifStat.Block, state, token);
                return;
            }

            if (ifStat.ElseIfs.Count > 0)
            {
                foreach (var elseIf in ifStat.ElseIfs)
                {
                    result = await _engine.EvaluateExpression(elseIf.Condition, state, token).FirstAsync();

                    if (!result.AsBool())
                    {
                        continue;
                    }

                    await _engine.ExecuteStatement(elseIf.Block, state, token);
                    return;
                }
            }

            if (ifStat.ElseBlock != null)
            {
                await _engine.ExecuteStatement(ifStat.ElseBlock, state, token);
            }
        }

        public async Task ExecuteWhileStat(WhileStat whileStat, LuaState state, CancellationToken token)
        {
            var whileState = state.WithNewContext();

            while ((await _engine.EvaluateExpression(whileStat.Condition, state, token).FirstAsync()).AsBool())
            {
                await _engine.ExecuteStatement(whileStat.Block, whileState, token);
            }
        }

        public async Task ExecuteRepeatStat(RepeatStat repeatStat, LuaState state, CancellationToken token)
        {
            var repeatState = state.WithNewContext();

            do
            {
                await _engine.ExecuteStatement(repeatStat.Block, repeatState, token);
            } while (!(await _engine.EvaluateExpression(repeatStat.Condition, repeatState, token).FirstAsync()).AsBool());
        }

        public Task ExecuteFunctionCall(FunctionCall call, LuaState state, CancellationToken token)
        {
            return _engine.EvaluateExpression(call, state, token);
        }
    }
}
