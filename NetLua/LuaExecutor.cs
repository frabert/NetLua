/*
 * NetLua by Francesco Bertolaccini
 * Project inspired by AluminumLua, a project by Alexander Corrado
 * (See his repo at http://github.com/chkn/AluminumLua)
 * 
 * NetLua - a managed implementation of the Lua dynamic programming language
 * 
 * LuaExecutor.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lua.Ast;

namespace Lua
{
    /*public static class LuaExecutor
    {
        static LuaObject EvaluateExpression(LuaContext Context, IExpression Expression)
        {
            if (Expression is StringLiteral)
            {
                var stringLiteral = Expression as StringLiteral;
                return stringLiteral.Value;
            }

            if (Expression is NumberLiteral)
            {
                var numberLiteral = Expression as NumberLiteral;
                return numberLiteral.Value;
            }

            if (Expression is Variable)
            {
                var expr = Expression as Variable;
                if (expr.Prefix == null)
                {
                    return Context.Get(expr.Name);
                }
                else
                {
                    var prefix = EvaluateExpression(Context, expr.Prefix);
                    return prefix[expr.Name];
                }
            }

            if (Expression is FunctionCall)
            {
                var expr = Expression as FunctionCall;
                LuaObject[] args = new LuaObject[0];
                if (expr.Arguments != null)
                {
                    args = expr.Arguments.ConvertAll<LuaObject>(x => EvaluateExpression(Context, x)).ToArray();
                }
                LuaObject func = EvaluateExpression(Context, expr.Function);
                return func.AsFunction()(args);
            }

            if (Expression is FunctionDefinition)
            {
                LuaContext scope = new LuaContext(Context);
                FunctionDefinition def = Expression as FunctionDefinition;
                LuaFunction f = delegate(LuaObject[] args)
                {
                    if (def.Arguments != null)
                    {
                        if (args == null)
                            args = new LuaObject[0];
                        for (int i = 0; i < def.Arguments.Count; i++)
                        {
                            if (i > args.Length)
                                scope.SetLocal(def.Arguments[i].Name, LuaObject.Nil);
                            else
                                scope.SetLocal(def.Arguments[i].Name, args[i]);
                        }
                    }
                    return ExecuteAst(scope, def.Body);
                };
            }

            return LuaObject.Nil;
        }

        public static LuaObject ExecuteAst(LuaContext Context, Block Block)
        {
            foreach (IStatement statement in Block.Statements)
            {
                if (statement is Assignment)
                {
                    var assignment = statement as Assignment;
                    //TODO: Add support for multiple assignment / return
                    Variable variable = assignment.Variable; //assignment.Variables[0];
                    IExpression expr = assignment.Expression; //assignment.Expressions[0];

                    LuaObject obj = EvaluateExpression(Context, expr);
                    if (statement is LocalAssignment)
                    {
                        // There's no prefixing if creating a local variable
                        Context.SetLocal(variable.Name, obj);
                    }
                    else
                    {
                        if (variable.Prefix == null)
                        {
                            Context.Set(variable.Name, obj);
                        }
                        else
                        {
                            LuaObject prefix = EvaluateExpression(Context, variable.Prefix);
                            prefix[variable.Name] = obj;
                        }
                    }
                }

                if (statement is FunctionCall)
                {
                    var functionCall = statement as FunctionCall;

                    LuaObject[] args = new LuaObject[0];
                    if (functionCall.Arguments != null)
                    {
                        args = functionCall.Arguments.ConvertAll<LuaObject>(x => EvaluateExpression(Context, x)).ToArray();
                    }
                    LuaObject func = EvaluateExpression(Context, functionCall.Function);

                    func.AsFunction()(args);
                }

                if (statement is Block)
                {
                    var block = statement as Block;
                    LuaContext scope = new LuaContext(Context);
                    ExecuteAst(scope, block);
                }
            }
            return LuaObject.Nil;
        }
    }*/
}
