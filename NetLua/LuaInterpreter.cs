using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetLua.Ast;

namespace NetLua
{
    public static class LuaInterpreter
    {
        internal struct LuaReturnStatus
        {
            public bool returned, broke;
        }

        static LuaObject EvalBinaryExpression(BinaryExpression Expression, LuaContext Context)
        {
            LuaObject left = EvalExpression(Expression.Left, Context)[0], right = EvalExpression(Expression.Right, Context)[0];
            switch (Expression.Operation)
            {
                case BinaryOp.Addition:
                    return left + right;
                case BinaryOp.And:
                    return left & right;
                case BinaryOp.Concat:
                    return LuaEvents.concat_event(left, right);
                case BinaryOp.Different:
                    return !(LuaEvents.eq_event(left, right));
                case BinaryOp.Division:
                    return left / right;
                case BinaryOp.Equal:
                    return LuaEvents.eq_event(left, right);
                case BinaryOp.GreaterOrEqual:
                    return left >= right;
                case BinaryOp.GreaterThan:
                    return left > right;
                case BinaryOp.LessOrEqual:
                    return left <= right;
                case BinaryOp.LessThan:
                    return left < right;
                case BinaryOp.Modulo:
                    return left % right;
                case BinaryOp.Multiplication:
                    return left * right;
                case BinaryOp.Or:
                    return left | right;
                case BinaryOp.Power:
                    return left ^ right;
                case BinaryOp.Subtraction:
                    return left - right;
                default:
                    throw new NotImplementedException();
            }
        }

        static LuaObject EvalUnaryExpression(UnaryExpression Expression, LuaContext Context)
        {
            LuaObject obj = EvalExpression(Expression.Expression, Context)[0];
            switch (Expression.Operation)
            {
                case UnaryOp.Invert:
                    return -(obj.AsNumber());
                case UnaryOp.Length:
                    {
                        if (obj.Is(LuaType.table))
                            return obj.AsTable().Count;
                        else
                            return obj.AsString().Length;
                    }
                case UnaryOp.Negate:
                    return !(obj.AsBool());
                default:
                    throw new NotImplementedException();
            }
        }

        static LuaObject EvalVariable(Variable Expression, LuaContext Context)
        {
            if (Expression.Prefix == null)
            {
                return Context.Get(Expression.Name);
            }
            else
            {
                LuaObject prefix = EvalExpression(Expression.Prefix, Context)[0];
                return prefix[Expression.Name];
            }
        }

        static LuaArguments EvalFunctionCall(FunctionCall Expression, LuaContext Context)
        {
            LuaObject func = EvalExpression(Expression.Function, Context)[0];

            LuaArguments args = null;
            if (Expression.Arguments != null || Expression.Arguments.Count != 0)
            {
                List<LuaObject> values = new List<LuaObject>();
                foreach (IExpression expr in Expression.Arguments)
                {
                    values.AddRange(EvalExpression(expr, Context));
                }
                args = values.ToArray();
            }
            //return func.AsFunction()(args);
            return func.Call(args);
        }

        static LuaObject EvalTableAccess(TableAccess Expression, LuaContext Context)
        {
            LuaObject table = EvalExpression(Expression.Expression, Context)[0];
            LuaObject index = EvalExpression(Expression.Index, Context)[0];
            return table[index];
        }

        static LuaArguments EvalExpression(IExpression Expression, LuaContext Context)
        {
            if (Expression is NumberLiteral)
                return Lua.Return(((NumberLiteral)Expression).Value);
            else if (Expression is StringLiteral)
                return Lua.Return(((StringLiteral)Expression).Value);
            else if (Expression is NilLiteral)
                return Lua.Return(LuaObject.Nil);
            else if (Expression is BoolLiteral)
                return Lua.Return(((BoolLiteral)Expression).Value);
            else if (Expression is BinaryExpression)
            {
                BinaryExpression exp = (BinaryExpression)Expression;
                return Lua.Return(EvalBinaryExpression(exp, Context));
            }
            else if (Expression is UnaryExpression)
            {
                UnaryExpression exp = (UnaryExpression)Expression;
                return Lua.Return(EvalUnaryExpression(exp, Context));
            }
            else if (Expression is Variable)
            {
                Variable var = (Variable)Expression;
                return Lua.Return(EvalVariable(var, Context));
            }
            else if (Expression is FunctionCall)
            {
                FunctionCall call = (FunctionCall)Expression;
                return EvalFunctionCall(call, Context);
            }
            else if (Expression is TableAccess)
            {
                TableAccess taccess = (TableAccess)Expression;
                return Lua.Return(EvalTableAccess(taccess, Context));
            }
            else if (Expression is FunctionDefinition)
            {
                FunctionDefinition fdef = (FunctionDefinition)Expression;
                return Lua.Return(EvalFunctionDefinition(fdef, Context));
            }
            else if (Expression is TableConstructor)
            {
                TableConstructor tctor = (TableConstructor)Expression;
                return Lua.Return(EvalTableConstructor(tctor, Context));
            }

            return Lua.Return();
        }

        static LuaObject EvalTableConstructor(TableConstructor tctor, LuaContext Context)
        {
            Dictionary<LuaObject, LuaObject> table = new Dictionary<LuaObject, LuaObject>();
            foreach (KeyValuePair<IExpression, IExpression> pair in tctor.Values)
            {
                LuaObject key = EvalExpression(pair.Key, Context)[0];
                LuaObject value = EvalExpression(pair.Value, Context)[0];

                table.Add(key, value);
            }
            return LuaObject.FromTable(table);
        }

        static LuaObject EvalFunctionDefinition(FunctionDefinition fdef, LuaContext Context)
        {
            LuaObject obj = LuaObject.FromFunction(delegate(LuaArguments args)
            {
                LuaContext ctx = new LuaContext(Context);
                LuaReturnStatus ret;
                for (int i = 0; i < fdef.Arguments.Count; i++)
                {
                    if (i > args.Length)
                    {
                        ctx.SetLocal(fdef.Arguments[i].Name, LuaObject.Nil);
                    }
                    else
                    {
                        ctx.SetLocal(fdef.Arguments[i].Name, args[i]);
                    }
                }
                return EvalBlock(fdef.Body, ctx, out ret);
            });

            return obj;
        }

        static LuaArguments EvalIf(If stat, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.broke = false;
            returned.returned = false;
            LuaArguments obj = new LuaObject[] { LuaObject.Nil };

            if (EvalExpression(stat.Condition, Context)[0].AsBool())
            {
                LuaContext ctx = new LuaContext(Context);
                obj = EvalBlock(stat.Block, ctx, out returned);
            }
            else
            {
                bool found = false;
                foreach (If branch in stat.ElseIfs)
                {
                    if (EvalExpression(branch.Condition, Context)[0].AsBool())
                    {
                        LuaContext ctx = new LuaContext(Context);
                        obj = EvalBlock(stat.Block, ctx, out returned);
                        found = true;
                        break;
                    }
                }
                if (!found && stat.ElseBlock != null)
                {
                    LuaContext ctx = new LuaContext(Context);
                    obj = EvalBlock(stat.ElseBlock, ctx, out returned);
                }
            }

            return obj;
        }

        static LuaArguments EvalWhile(While stat, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.returned = false;
            returned.broke = false;
            LuaObject cond = EvalExpression(stat.Condition, Context)[0];
            LuaContext ctx = new LuaContext(Context);
            while (cond.AsBool())
            {
                LuaArguments obj = EvalBlock(stat.Block, ctx, out returned);
                if (returned.broke)
                    break;
                if (returned.returned)
                    return obj;
                else
                    cond = EvalExpression(stat.Condition, Context)[0];
            }
            return Lua.Return();
        }

        static LuaArguments EvalRepeat(Repeat stat, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.returned = false;
            returned.broke = false;
            LuaContext ctx = new LuaContext(Context);
            while (true)
            {
                LuaArguments obj = EvalBlock(stat.Block, ctx, out returned);

                if (returned.broke)
                    break;
                if (returned.returned)
                    return obj;
                LuaObject cond = EvalExpression(stat.Condition, ctx)[0];
                if (cond)
                    break;
            }
            return Lua.Return();
        }

        static LuaArguments EvalNumericFor(NumericFor stat, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.broke = false;
            returned.returned = false;
            var varList = EvalExpression(stat.Var, Context);
            var limitList = EvalExpression(stat.Limit, Context);
            var stepList = EvalExpression(stat.Step, Context);
            var var = LuaEvents.toNumber(varList[0]);
            var limit = LuaEvents.toNumber(limitList[0]);
            var step = LuaEvents.toNumber(stepList[0]);

            if (!(var & limit & step).AsBool())
            {
                throw new LuaException("Error in for loop");
            }
            LuaContext ctx = new LuaContext(Context);
            while ((step > 0 & var <= limit) | (step <= 0 & var >= limit))
            {
                ctx.SetLocal(stat.Variable, var);
                LuaArguments obj = EvalBlock(stat.Block, ctx, out returned);
                if (returned.broke)
                    break;
                if (returned.returned)
                    return obj;
                var = var + step;
            }
            return Lua.Return();
        }

        static LuaArguments EvalGenericFor(GenericFor stat, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.broke = false;
            returned.returned = false;

            LuaArguments args = null;
            foreach (IExpression expr in stat.Expressions)
            {
                if (args == null)
                    args = EvalExpression(expr, Context);
                else
                    args.Concat(EvalExpression(expr, Context));
            }
            LuaObject f = args[0], s = args[1], var = args[2];
            LuaContext ctx = new LuaContext(Context);
            while (true)
            {
                LuaArguments res = f.Call(s, var);
                for (int i = 0; i < stat.Variables.Count; i++)
                {
                    ctx.SetLocal(stat.Variables[i], res[i]);
                }
                if (res[0].IsNil)
                    break;
                var = res[0];
                LuaArguments obj = EvalBlock(stat.Block, ctx, out returned);
                if (returned.broke)
                    break;
                if (returned.returned)
                    return obj;
            }

            return Lua.Return();
        }

        static void SetAssignable(IAssignable Expression, LuaObject Value, LuaContext Context)
        {
            if (Expression is Variable)
                SetVariable(Expression as Variable, Value, Context);
            else
                SetTable(Expression as TableAccess, Value, Context);
        }

        static void SetVariable(Variable Expression, LuaObject Value, LuaContext Context)
        {
            if (Expression.Prefix == null)
            {
                Context.Set(Expression.Name, Value);
            }
            else
            {
                EvalExpression(Expression.Prefix, Context)[0][Expression.Name] = Value;
            }
        }

        static void SetTable(TableAccess Expression, LuaObject Value, LuaContext Context)
        {
            EvalExpression(Expression.Expression, Context)[0][EvalExpression(Expression.Index, Context)[0]] = Value;
        }

        internal static LuaArguments EvalBlock(Block Block, LuaContext Context, out LuaReturnStatus returned)
        {
            returned.broke = false;
            returned.returned = false;
            LuaArguments obj = new LuaObject[] { LuaObject.Nil };
            foreach (IStatement stat in Block.Statements)
            {
                if (stat is Assignment)
                {
                    Assignment assign = stat as Assignment;
                    LuaArguments values = null;
                    foreach (IExpression expr in assign.Expressions)
                    {
                        if (values == null)
                            values = EvalExpression(expr, Context);
                        else
                            values.Concat(EvalExpression(expr, Context));
                    }
                    for (int i = 0; i < assign.Variables.Count; i++)
                    {
                            SetAssignable(assign.Variables[i], values[i], Context);
                    }
                }
                else if (stat is LocalAssignment)
                {
                    LocalAssignment assign = stat as LocalAssignment;
                    LuaArguments values = null;
                    foreach (IExpression expr in assign.Values)
                    {
                        if (values == null)
                            values = EvalExpression(expr, Context);
                        else
                            values.Concat(EvalExpression(expr, Context));
                    }
                    for (int i = 0; i < assign.Names.Count; i++)
                    {
                        Context.SetLocal(assign.Names[i], values[i]);
                    }
                }
                else if (stat is Return)
                {
                    Return ret = stat as Return;
                    returned.returned = true;
                    List<LuaObject> values = new List<LuaObject>();
                    foreach (IExpression expr in ret.Expressions)
                    {
                        values.AddRange(EvalExpression(expr, Context));
                    }
                    return values.ToArray();
                }
                else if (stat is FunctionCall)
                {
                    FunctionCall call = stat as FunctionCall;
                    EvalFunctionCall(call, Context);
                }
                else if (stat is Block)
                {
                    Block block = stat as Block;
                    LuaContext ctx = new LuaContext(Context);
                    obj = EvalBlock(block, ctx, out returned);
                    if (returned.returned)
                        return obj;
                }
                else if (stat is If)
                {
                    obj = EvalIf(stat as If, Context, out returned);
                    if (returned.returned)
                        return obj;
                }
                else if (stat is While)
                {
                    obj = EvalWhile(stat as While, Context, out returned);
                    if (returned.returned)
                        return obj;
                }
                else if (stat is Repeat)
                {
                    obj = EvalRepeat(stat as Repeat, Context, out returned);
                    if (returned.returned)
                        return obj;
                }
                else if (stat is Break)
                {
                    returned.returned = false;
                    returned.broke = true;
                    return Lua.Return(LuaObject.Nil);
                }
                else if (stat is NumericFor)
                {
                    obj = EvalNumericFor(stat as NumericFor, Context, out returned);
                    if (returned.returned)
                        return obj;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj;
        }
    }
}
