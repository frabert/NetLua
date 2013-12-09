using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

using NetLua.Ast;

namespace NetLua
{
    public static class LuaCompiler
    {
        static MethodInfo LuaContext_Get = typeof(LuaContext).GetMethod("Get");
        static MethodInfo LuaContext_SetLocal = typeof(LuaContext).GetMethod("SetLocal");
        static MethodInfo LuaContext_SetGlobal = typeof(LuaContext).GetMethod("SetGlobal");
        static MethodInfo LuaContext_Set = typeof(LuaContext).GetMethod("Set");

        static ConstructorInfo LuaContext_New_parent = typeof(LuaContext).GetConstructor(new[] { typeof(LuaContext) });
        static ConstructorInfo LuaArguments_New = typeof(LuaArguments).GetConstructor(new[] { typeof(LuaObject[]) });

        static MethodInfo LuaEvents_eq = typeof(LuaEvents).GetMethod("eq_event");
        static MethodInfo LuaEvents_concat = typeof(LuaEvents).GetMethod("concat_event");

        static Expression CreateLuaArguments(params Expression[] Expressions)
        {
            var array = Expression.NewArrayInit(typeof(LuaObject), Expressions);
            return Expression.New(LuaArguments_New, array);
        }

        static Expression GetFirstArgument(Expression Expression)
        {
            if (Expression.Type == typeof(LuaArguments))
            {
                return Expression.ArrayAccess(Expression.Convert(Expression, typeof(LuaObject[])), Expression.Constant(0));
            }
            else
            {
                return Expression.ArrayAccess(Expression, Expression.Constant(0));
            }
        }

        static Expression CompileBinaryExpression(Ast.BinaryExpression expr, Expression Context)
        {
            Expression left = GetFirstArgument(CompileExpression(expr.Left, Context)), right = GetFirstArgument(CompileExpression(expr.Right, Context));
            switch (expr.Operation)
            {
                case BinaryOp.Addition:
                    return Expression.Add(left, right);
                case BinaryOp.And:
                    return Expression.And(left, right);
                case BinaryOp.Concat:
                    return Expression.Call(LuaEvents_concat, left, right);
                case BinaryOp.Different:
                    return Expression.Negate(Expression.Call(LuaEvents_eq, left, right));
                case BinaryOp.Division:
                    return Expression.Divide(left, right);
                case BinaryOp.Equal:
                    return Expression.Call(LuaEvents_eq, left, right);
                case BinaryOp.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                case BinaryOp.GreaterThan:
                    return Expression.GreaterThan(left, right);
                case BinaryOp.LessOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case BinaryOp.LessThan:
                    return Expression.LessThan(left, right);
                case BinaryOp.Modulo:
                    return Expression.Modulo(left, right);
                case BinaryOp.Multiplication:
                    return Expression.Multiply(left, right);
                case BinaryOp.Or:
                    return Expression.Or(left, right);
                case BinaryOp.Power:
                    return Expression.ExclusiveOr(left, right);
                case BinaryOp.Subtraction:
                    return Expression.Subtract(left, right);
            }
            throw new NotImplementedException();
        }

        static Expression CompileUnaryExpression(Ast.UnaryExpression expr, Expression Context)
        {
            Expression e = CompileExpression(expr.Expression, Context);
            switch (expr.Operation)
            {
                case UnaryOp.Invert:
                    return Expression.Negate(e);
                case UnaryOp.Negate:
                    return Expression.Not(e);
                case UnaryOp.Length:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        static Expression GetVariable(Ast.Variable expr, Expression Context)
        {
            if (expr.Prefix == null)
            {
                return Expression.Call(Context, LuaContext_Get, Expression.Constant(expr.Name));
            }
            else
            {
                Expression p = GetFirstArgument(CompileExpression(expr.Prefix, Context));
                return Expression.ArrayIndex(p, Expression.Constant(expr.Name));
            }
        }

        static Expression GetTableAccess(Ast.TableAccess expr, Expression Context)
        {
            Expression e = GetFirstArgument(CompileExpression(expr.Expression, Context));
            Expression i = GetFirstArgument(CompileExpression(expr.Index, Context));

            return Expression.ArrayAccess(e, i);
        }

        static Expression SetVariable(Ast.Variable expr, Expression value, Expression Context)
        {
            //Expression v = CompileExpression(value, Context);
            if (expr.Prefix == null)
            {
                return Expression.Call(Context, LuaContext_Set, Expression.Constant(expr.Name), value);
            }
            else
            {
                /*Expression p = GetFirstArgument(CompileExpression(expr.Prefix, Context));
                return Expression.ArrayIndex(p, Expression.Constant(expr.Name));*/
                throw new NotImplementedException();
            }
        }

        static Expression SetLocalVariable(Ast.Variable expr, IExpression value, Expression Context)
        {
            Expression v = CompileExpression(value, Context);
            if (expr.Prefix == null)
            {
                return Expression.Call(Context, LuaContext_SetLocal, Expression.Constant(expr.Name), v);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static Expression CompileExpression(IExpression expr, Expression Context)
        {
            if (expr is NumberLiteral)
                return CreateLuaArguments(Expression.Constant((LuaObject)((expr as NumberLiteral).Value)));
            if (expr is StringLiteral)
                return CreateLuaArguments(Expression.Constant((LuaObject)((expr as StringLiteral).Value)));
            if (expr is BoolLiteral)
                return CreateLuaArguments(Expression.Constant((LuaObject)((expr as BoolLiteral).Value)));
            if (expr is NilLiteral)
                return CreateLuaArguments(Expression.Constant(LuaObject.Nil));

            if (expr is Ast.BinaryExpression)
                return CreateLuaArguments(CompileBinaryExpression(expr as Ast.BinaryExpression, Context));
            if (expr is Ast.UnaryExpression)
                return CreateLuaArguments(CompileUnaryExpression(expr as Ast.UnaryExpression, Context));
            if (expr is Ast.Variable)
                return CreateLuaArguments(GetVariable(expr as Ast.Variable, Context));
            if (expr is Ast.TableAccess)
                return CreateLuaArguments(GetTableAccess(expr as Ast.TableAccess, Context));
            throw new NotImplementedException();
        }

        public static Func<LuaObject> Comp(IExpression expr, LuaContext ctx)
        {
            var variable = Expression.Parameter(typeof(LuaObject), "val");
            var call = CompileExpression(expr, Expression.Constant(ctx));
            var value = GetFirstArgument(call);
            var assign = Expression.Assign(variable, value);

            var block = Expression.Block(new[] {variable}, assign, variable);
            return Expression.Lambda<Func<LuaObject>>(block).Compile();
        }
    }
}
