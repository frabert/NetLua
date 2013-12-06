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

        static Expression CompileBinaryExpression(Ast.BinaryExpression expr, Expression Context)
        {
            Expression left = CompileExpression(expr.Left, Context), right = CompileExpression(expr.Right, Context);
            switch (expr.Operation)
            {
                case BinaryOp.Addition:
                    return Expression.Add(left, right);
                case BinaryOp.And:
                    return Expression.And(left, right);
                case BinaryOp.Concat:
                    throw new NotImplementedException();
                case BinaryOp.Different:
                    return Expression.Negate(Expression.Equal(left, right));
                case BinaryOp.Division:
                    return Expression.Divide(left, right);
                case BinaryOp.Equal:
                    return Expression.Equal(left, right);
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
                Expression p = CompileExpression(expr.Prefix, Context);
                return Expression.ArrayIndex(p, Expression.Constant(expr.Name));
            }
        }

        static Expression GetTableAccess(Ast.TableAccess expr, Expression Context)
        {
            Expression e = CompileExpression(expr.Expression, Context);
            Expression i = CompileExpression(expr.Index, Context);

            return Expression.ArrayAccess(e, i);
        }

        static Expression SetVariable(Ast.Variable expr, IExpression value, Expression Context)
        {
            Expression v = CompileExpression(value, Context);
            if (expr.Prefix == null)
            {
                return Expression.Call(Context, LuaContext_Set, Expression.Constant(expr.Name), v);
            }
            else
            {
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
            if (expr is Ast.BinaryExpression)
                return CompileBinaryExpression(expr as Ast.BinaryExpression, Context);
            if (expr is Ast.UnaryExpression)
                return CompileUnaryExpression(expr as Ast.UnaryExpression, Context);
            if (expr is Ast.Variable)
                return GetVariable(expr as Ast.Variable, Context);
            if (expr is Ast.TableAccess)
                return GetTableAccess(expr as Ast.TableAccess, Context);
            throw new NotImplementedException();
        }
    }
}
