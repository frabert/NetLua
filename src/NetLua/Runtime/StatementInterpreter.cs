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
                var ret = await _engine.EvaluateExpression(assign.Values[i], context, token).FirstAsync();

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

        public Task ExecuteFunctionCall(FunctionCall call, LuaTable context, CancellationToken token)
        {
            return _engine.EvaluateExpression(call, context, token);
        }
    }
}
