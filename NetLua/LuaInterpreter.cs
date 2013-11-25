using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua.Ast;

namespace Lua
{
    public static class LuaInterpreter
    {
        static LuaObject EvalBinaryExpression(BinaryExpression Expression, LuaContext Context)
        {
            LuaObject left = EvalExpression(Expression.Left, Context), right = EvalExpression(Expression.Right, Context);
            switch (Expression.Operation)
            {
                case BinaryOp.Addition:
                    return left + right;
                case BinaryOp.And:
                    return left.AsBool() && right.AsBool();
                case BinaryOp.Concat:
                    return left.AsString() + right.AsString();
                case BinaryOp.Different:
                    return !(left.Equals(right));
                case BinaryOp.Division:
                    return left / right;
                case BinaryOp.Equal:
                    return left.Equals(right);
                case BinaryOp.GreaterOrEqual:
                    return left.AsNumber() >= right.AsNumber();
                case BinaryOp.GreaterThan:
                    return left.AsNumber() > right.AsNumber();
                case BinaryOp.LessOrEqual:
                    return left.AsNumber() <= right.AsNumber();
                case BinaryOp.LessThan:
                    return left.AsNumber() < right.AsNumber();
                case BinaryOp.Modulo:
                        return left % right;
                case BinaryOp.Multiplication:
                    return left * right;
                case BinaryOp.Or:
                    return left.AsBool() || right.AsBool();
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
            LuaObject obj = EvalExpression(Expression.Expression, Context);
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
                LuaObject prefix = EvalExpression(Expression.Prefix, Context);
                return prefix[Expression.Name];
            }
        }

        static LuaObject EvalFunctionCall(FunctionCall Expression, LuaContext Context)
        {
            LuaObject func = EvalExpression(Expression.Function, Context);

            LuaObject[] args = null;
            if (Expression.Arguments != null || Expression.Arguments.Count != 0)
            {
                args = Array.ConvertAll<IExpression, LuaObject>(Expression.Arguments.ToArray(),
                    x => EvalExpression(x, Context));
            }
            //return func.AsFunction()(args);
            return func.Call(args);
        }

        static LuaObject EvalTableAccess(TableAccess Expression, LuaContext Context)
        {
            LuaObject table = EvalExpression(Expression.Expression, Context);
            LuaObject index = EvalExpression(Expression.Index, Context);
            return table[index];
        }

        static LuaObject EvalExpression(IExpression Expression, LuaContext Context)
        {
            if (Expression is NumberLiteral)
                return ((NumberLiteral)Expression).Value;
            else if (Expression is StringLiteral)
                return ((StringLiteral)Expression).Value;
            else if (Expression is NilLiteral)
                return LuaObject.Nil;
            else if (Expression is BoolLiteral)
                return ((BoolLiteral)Expression).Value;
            else if (Expression is BinaryExpression)
            {
                BinaryExpression exp = (BinaryExpression)Expression;
                return EvalBinaryExpression(exp, Context);
            }
            else if (Expression is UnaryExpression)
            {
                UnaryExpression exp = (UnaryExpression)Expression;
                return EvalUnaryExpression(exp, Context);
            }
            else if (Expression is Variable)
            {
                Variable var = (Variable)Expression;
                return EvalVariable(var, Context);
            }
            else if (Expression is FunctionCall)
            {
                FunctionCall call = (FunctionCall)Expression;
                return EvalFunctionCall(call, Context);
            }
            else if (Expression is TableAccess)
            {
                TableAccess taccess = (TableAccess)Expression;
                return EvalTableAccess(taccess, Context);
            }
            else if (Expression is FunctionDefinition)
            {
                FunctionDefinition fdef = (FunctionDefinition)Expression;
                return EvalFunctionDefinition(fdef, Context);
            }
            else if (Expression is TableConstructor)
            {
                TableConstructor tctor = (TableConstructor)Expression;
                return EvalTableConstructor(tctor, Context);
            }

            return LuaObject.Nil;
        }

        static LuaObject EvalTableConstructor(TableConstructor tctor, LuaContext Context)
        {
            Dictionary<LuaObject, LuaObject> table = new Dictionary<LuaObject, LuaObject>();
            foreach (KeyValuePair<IExpression, IExpression> pair in tctor.Values)
            {
                LuaObject key = EvalExpression(pair.Key, Context);
                LuaObject value = EvalExpression(pair.Value, Context);

                table.Add(key, value);
            }
            return LuaObject.FromTable(table);
        }

        static LuaObject EvalFunctionDefinition(FunctionDefinition fdef, LuaContext Context)
        {
            LuaObject obj = LuaObject.FromFunction(delegate(LuaObject[] args)
            {
                LuaContext ctx = new LuaContext(Context);
                bool ret;
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

        static LuaObject EvalIf(If stat, LuaContext Context, out bool returned)
        {
            returned = false;
            LuaObject obj = LuaObject.Nil;

            if (EvalExpression(stat.Condition, Context).AsBool())
            {
                LuaContext ctx = new LuaContext(Context);
                obj = EvalBlock(stat.Block, ctx, out returned);
            }
            else
            {
                bool found = false;
                foreach (If branch in stat.ElseIfs)
                {
                    if (EvalExpression(branch.Condition, Context).AsBool())
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
                EvalExpression(Expression.Prefix, Context)[Expression.Name] = Value;
            }
        }

        static void SetTable(TableAccess Expression, LuaObject Value, LuaContext Context)
        {
            EvalExpression(Expression.Expression, Context)[EvalExpression(Expression.Index, Context)] = Value;
        }

        public static LuaObject EvalBlock(Block Block, LuaContext Context, out bool returned)
        {
            returned = false;
            foreach (IStatement stat in Block.Statements)
            {
                if (stat is Assignment)
                {
                    Assignment assign = stat as Assignment;
                    SetAssignable(assign.Variable, EvalExpression(assign.Expression, Context), Context);
                }
                else if (stat is LocalAssignment)
                {
                    LocalAssignment assign = stat as LocalAssignment;
                    Context.SetLocal(assign.Name, EvalExpression(assign.Value, Context));
                }
                else if (stat is Return)
                {
                    Return ret = stat as Return;
                    returned = true;
                    return EvalExpression(ret.Expression, Context);
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
                    bool ret = false;
                    LuaObject val = EvalBlock(block, ctx, out ret);
                    if (ret)
                    {
                        returned = true;
                        return val;
                    }
                }
                else if (stat is If)
                {
                    bool ret;
                    LuaObject obj = EvalIf(stat as If, Context, out ret);
                    if (ret)
                    {
                        returned = true;
                        return obj;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return LuaObject.Nil;
        }
    }
}
