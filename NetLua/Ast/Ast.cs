/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2013 Francesco Bertolaccini
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua.Ast
{
    public enum BinaryOp
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Power,
        Modulo,
        Concat,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Equal,
        Different,
        And,
        Or
    }

    public enum UnaryOp
    {
        Negate,
        Invert,
        Length
    }

    public interface IStatement { }

    public interface IExpression { }

    public interface IAssignable : IExpression
    { }

    public class Variable : IExpression, IAssignable
    {
        // Prefix.Name
        public IExpression Prefix;
        public string Name;
    }

    public class Argument
    {
        public string Name;
    }

    public class StringLiteral : IExpression
    {
        public string Value;
    }

    public class NumberLiteral : IExpression
    {
        public double Value;
    }

    public class NilLiteral : IExpression
    { }

    public class BoolLiteral : IExpression
    {
        public bool Value;
    }

    public class FunctionCall : IStatement, IExpression
    {
        public IExpression Function;
        public List<IExpression> Arguments = new List<IExpression>();
    }

    public class TableAccess : IExpression, IAssignable
    {
        // Expression[Index]
        public IExpression Expression;
        public IExpression Index;
    }

    public class FunctionDefinition : IExpression
    {
        // function(Arguments) Body end
        public List<Argument> Arguments = new List<Argument>();
        public Block Body;
    }

    public class BinaryExpression : IExpression
    {
        public IExpression Left, Right;
        public BinaryOp Operation;
    }

    public class UnaryExpression : IExpression
    {
        public IExpression Expression;
        public UnaryOp Operation;
    }

    public class TableConstructor : IExpression
    {
        public Dictionary<IExpression, IExpression> Values = new Dictionary<IExpression,IExpression>();
    }

    public class Assignment : IStatement
    {
        // Var1, Var2, Var3 = Exp1, Exp2, Exp3
        //public Variable[] Variables;
        //public IExpression[] Expressions;

        public List<IAssignable> Variables = new List<IAssignable>();
        public List<IExpression> Expressions = new List<IExpression>();
    }

    public class ReturnStat : IStatement
    {
        public List<IExpression> Expressions = new List<IExpression>();
    }

    public class BreakStat : IStatement { }

    public class LocalAssignment : IStatement
    {
        public List<string> Names = new List<string>();
        public List<IExpression> Values = new List<IExpression>();
    }

    public class Block : IStatement
    {
        public List<IStatement> Statements = new List<IStatement>();
    }

    public class WhileStat : IStatement
    {
        public IExpression Condition;
        public Block Block;
    }

    public class RepeatStat : IStatement
    {
        public Block Block;
        public IExpression Condition;
    }

    public class NumericFor : IStatement
    {
        public IExpression Var, Limit, Step;
        public string Variable;
        public Block Block;
    }

    public class GenericFor : IStatement
    {
        public List<string> Variables = new List<string>();
        public List<IExpression> Expressions = new List<IExpression>();
        public Block Block;
    }

    public class IfStat : IStatement
    {
        public IExpression Condition;
        public Block Block;
        public List<IfStat> ElseIfs = new List<IfStat>();
        public Block ElseBlock;
    }
}
