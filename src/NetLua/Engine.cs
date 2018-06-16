using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetLua.Native;
using NetLua.Native.Value;
using NetLua.Native.Value.Functions;
using NetLua.Runtime;
using NetLua.Runtime.Ast;
using NumberLiteral = NetLua.Runtime.Ast.NumberLiteral;
using StringLiteral = NetLua.Runtime.Ast.StringLiteral;

namespace NetLua
{
    public class Engine
    {
        private readonly LuaParser _parser = new LuaParser();
        private readonly StatementInterpreter _statementInterpreter;
        private readonly ExpressionIntepreter _expressionIntepreter;

        public Engine()
        {
            Global = new LuaTable();
            _statementInterpreter = new StatementInterpreter(this);
            _expressionIntepreter = new ExpressionIntepreter(this);

            Global.NewIndexRaw("_G", Global);
        }

        public LuaTable Global { get; }

        public void Set(LuaObject key, LuaObject value)
        {
            Global.NewIndexRaw(key, value);
        }

        public LuaObject Get(LuaObject key)
        {
            return Global.IndexRaw(key);
        }

        public Task<LuaArguments> ExecuteAsync(string str, CancellationToken token = default)
        {
            return ExecuteAsync(str, Lua.Args(), token);
        }

        public Task<LuaArguments> ExecuteAsync(string str, LuaArguments args, CancellationToken token = default)
        {
            return Parse(str).CallAsync(args, token);
        }

        public LuaFunction Parse(string str)
        {
            var functionDefinition = new FunctionDefinition
            {
                Arguments = new List<Argument>(),
                Body = _parser.ParseString(str),
                Varargs = true
            };

            return new LuaInterpreterFunction(this, functionDefinition, Global, true);
        }

        public Task ExecuteStatement(IStatement stat, LuaTable context, LuaReturnState returnState, CancellationToken token = default)
        {
            if (returnState.DidReturn || token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            switch (stat)
            {
                case Assignment assignment:
                    return _statementInterpreter.ExecuteAssignment(assignment, context, token);
                case LocalAssignment localAssignment:
                    return _statementInterpreter.ExecuteLocalAssignment(localAssignment, context, token);
                case FunctionCall call:
                    return _statementInterpreter.ExecuteFunctionCall(call, context, token);
                case Block block:
                    return _statementInterpreter.ExecuteBlock(block, context, returnState, token);
                case IfStat ifStat:
                    return _statementInterpreter.ExecuteIfStat(ifStat, context, returnState, token);
                case ReturnStat returnStat:
                    return _statementInterpreter.ExecuteReturnStat(returnStat, context, returnState, token);
                //case WhileStat whileStat:
                //    return CompileWhileStat(whileStat, returnTarget, context);
                //case RepeatStat repeatStat:
                //    return CompileRepeatStatement(repeatStat, returnTarget, context);
                //case GenericFor @for:
                //    return CompileGenericFor(@for, returnTarget, context);
                case NumericFor numericFor:
                    return _statementInterpreter.ExecuteNumericFor(numericFor, context, returnState, token);
                default:
                    throw new NotImplementedException(stat.GetType().Name);
            }
        }

        public async Task<LuaArguments> EvaluateExpression(IList<IExpression> exprs, LuaTable context, CancellationToken token = default)
        {
            var results = new List<LuaObject>();

            for (var i = 0; i < exprs.Count; i++)
            {
                var addAll = i == exprs.Count - 1;
                var objects = await EvaluateExpression(exprs[i], context, token);

                if (addAll) results.AddRange(objects);
                else results.Add(objects[0]);
            }

            return Lua.Args(results);
        }

        public Task<LuaArguments> EvaluateExpression(IExpression expr, LuaTable context, CancellationToken token = default)
        {
            switch (expr)
            {
                case NumberLiteral numberLiteral:
                    return Lua.ArgsAsync(LuaObject.FromNumber(numberLiteral.Value));
                case StringLiteral stringLiteral:
                    return Lua.ArgsAsync(LuaObject.FromString(stringLiteral.Value));
                case BoolLiteral literal:
                    return Lua.ArgsAsync(LuaObject.FromBool(literal.Value));
                case NilLiteral _:
                    return Lua.ArgsAsync(LuaNil.Instance);
                case BinaryExpression binaryExpression:
                    return _expressionIntepreter.EvaluateBinaryExpressionAsync(binaryExpression, context, token);
                case UnaryExpression expression:
                    return _expressionIntepreter.EvaluateUnaryExpression(expression, context, token);
                case Variable variable:
                    return _expressionIntepreter.EvaluateGetVariableAsync(variable, context, token);
                case TableAccess access:
                    return _expressionIntepreter.EvaluateTableAccess(access, context, token);
                case FunctionCall call:
                    return _expressionIntepreter.EvaluateFunctionCall(call, context, token);
                case FunctionDefinition definition:
                    return _expressionIntepreter.EvaluateFunctionDefinition(definition, context, token);
                case VarargsLiteral _:
                    return _expressionIntepreter.EvaluateVarargs(context);
                case TableConstructor constructor:
                    return _expressionIntepreter.EvaluateTableConstructor(constructor, context, token);
                default:
                    throw new NotImplementedException(expr.GetType().Name);
            }
        }
    }
}

