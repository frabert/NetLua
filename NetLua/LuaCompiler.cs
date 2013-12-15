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
        static Type LuaContext_Type = typeof(LuaContext);
        static Type LuaArguments_Type = typeof(LuaArguments);
        static Type LuaEvents_Type = typeof(LuaEvents);
        static Type LuaObject_Type = typeof(LuaObject);

        static MethodInfo LuaContext_Get = LuaContext_Type.GetMethod("Get");
        static MethodInfo LuaContext_SetLocal = LuaContext_Type.GetMethod("SetLocal");
        static MethodInfo LuaContext_SetGlobal = LuaContext_Type.GetMethod("SetGlobal");
        static MethodInfo LuaContext_Set = LuaContext_Type.GetMethod("Set");

        static MethodInfo LuaArguments_Concat = LuaArguments_Type.GetMethod("Concat");

        static ConstructorInfo LuaContext_New_parent = LuaContext_Type.GetConstructor(new[] { typeof(LuaContext) });
        static ConstructorInfo LuaArguments_New = LuaArguments_Type.GetConstructor(new[] { typeof(LuaObject[]) });
        static ConstructorInfo LuaArguments_New_void = LuaArguments_Type.GetConstructor(new Type[] { });

        static MethodInfo LuaEvents_eq = LuaEvents_Type.GetMethod("eq_event");
        static MethodInfo LuaEvents_concat = LuaEvents_Type.GetMethod("concat_event");

        #region Helpers
        static Expression CreateLuaArguments(params Expression[] Expressions)
        {
            var array = Expression.NewArrayInit(typeof(LuaObject), Expressions);
            return Expression.New(LuaArguments_New, array);
        }

        static Expression GetFirstArgument(Expression Expression)
        {
            if (Expression.Type == typeof(LuaArguments))
            {
                return Expression.Property(Expression, "Item", Expression.Constant(0));
            }
            else
            {
                return Expression.ArrayAccess(Expression, Expression.Constant(0));
            }
        }

        static Expression GetArgument(Expression Expression, int n)
        {
            if (Expression.Type == typeof(LuaArguments))
            {
                return Expression.Property(Expression, "Item", Expression.Constant(n));
            }
            else
            {
                return Expression.ArrayAccess(Expression, Expression.Constant(n));
            }
        }
        #endregion

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
                return Expression.Property(p, "Item", Expression.Constant(expr.Name));
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
            if (expr.Prefix == null)
            {
                return Expression.Call(Context, LuaContext_Set, Expression.Constant(expr.Name), value);
            }
            else
            {
                var prefix = GetFirstArgument(CompileExpression(expr.Prefix, Context));
                var index = Expression.Constant(expr.Name);
                var set = Expression.Property(prefix, "Item", index);
                return Expression.Assign(set, value);
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

        static Expression CompileStatement(IStatement stat, Expression Context)
        {
            if (stat is Ast.Assignment)
            {
                var assign = stat as Ast.Assignment;

                var variable = Expression.Parameter(typeof(LuaArguments), "vars");
                List<Expression> stats = new List<Expression>();

                stats.Add(Expression.Assign(variable, Expression.New(LuaArguments_New_void)));
                foreach (IExpression expr in assign.Expressions)
                {
                    var ret = CompileExpression(expr, Context);
                    stats.Add(Expression.Call(variable, LuaArguments_Concat, ret));
                }
                int i = 0;
                foreach (IAssignable a in assign.Variables)
                {
                    var arg = GetArgument(variable, i);
                    if (a is Ast.Variable)
                    {
                        var x = a as Ast.Variable;
                        stats.Add(SetVariable(x, arg, Context));
                    }
                    else if (a is Ast.TableAccess)
                    {
                        var x = a as Ast.TableAccess;

                        var expression = GetFirstArgument(CompileExpression(x.Expression, Context));
                        var index = GetFirstArgument(CompileExpression(x.Index, Context));

                        var set = Expression.Property(expression, "Item", index);
                        stats.Add(Expression.Assign(set, arg));
                    }
                    i++;
                }

                return Expression.Block(new[] {variable}, stats.ToArray());
            }
            throw new NotImplementedException();
        }

        public delegate void TestFunc();

        public static TestFunc Comp(IStatement expr, LuaContext ctx)
        {
            Expression e = CompileStatement(expr, Expression.Constant(ctx));
            return Expression.Lambda<TestFunc>(e).Compile();
        }
    }
}
