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

using System.Collections.Generic;

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

    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }

    public interface IStatement : IVisitable { }

    public interface IExpression : IVisitable { }

    public interface IAssignable : IExpression { }

    public interface IVisitor
    {
        void Visit(Variable arg);
        void Visit(StringLiteral arg);
        void Visit(NumberLiteral arg);
        void Visit(NilLiteral arg);
        void Visit(BoolLiteral arg);
        void Visit(VarargsLiteral arg);
        void Visit(FunctionCall arg);
        void Visit(TableAccess arg);
        void Visit(FunctionDefinition arg);
        void Visit(BinaryExpression arg);
        void Visit(UnaryExpression arg);
        void Visit(TableConstructor arg);
        void Visit(Assignment arg);
        void Visit(ReturnStat arg);
        void Visit(BreakStat arg);
        void Visit(LocalAssignment arg);
        void Visit(Block arg);
        void Visit(WhileStat arg);
        void Visit(RepeatStat arg);
        void Visit(NumericFor arg);
        void Visit(GenericFor arg);
        void Visit(IfStat arg);
    }

    public class Variable : IExpression, IAssignable
    {
        // Prefix.Name
        public IExpression Prefix;
        public string Name;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Argument
    {
        public string Name;
    }

    public class StringLiteral : IExpression
    {
        public string Value;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NumberLiteral : IExpression
    {
        public double Value;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NilLiteral : IExpression
    {
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BoolLiteral : IExpression
    {
        public bool Value;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VarargsLiteral : IExpression
    {
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FunctionCall : IStatement, IExpression
    {
        public IExpression Function;
        public List<IExpression> Arguments = new List<IExpression>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class TableAccess : IExpression, IAssignable
    {
        // Expression[Index]
        public IExpression Expression;
        public IExpression Index;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FunctionDefinition : IExpression
    {
        // function(Arguments) Body end
        public List<Argument> Arguments = new List<Argument>();
        public Block Body;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BinaryExpression : IExpression
    {
        public IExpression Left, Right;
        public BinaryOp Operation;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class UnaryExpression : IExpression
    {
        public IExpression Expression;
        public UnaryOp Operation;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class TableConstructor : IExpression
    {
        public Dictionary<IExpression, IExpression> Values = new Dictionary<IExpression,IExpression>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Assignment : IStatement
    {
        // Var1, Var2, Var3 = Exp1, Exp2, Exp3
        //public Variable[] Variables;
        //public IExpression[] Expressions;

        public List<IAssignable> Variables = new List<IAssignable>();
        public List<IExpression> Expressions = new List<IExpression>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ReturnStat : IStatement
    {
        public List<IExpression> Expressions = new List<IExpression>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BreakStat : IStatement
    {
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LocalAssignment : IStatement
    {
        public List<string> Names = new List<string>();
        public List<IExpression> Values = new List<IExpression>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Block : IStatement
    {
        public List<IStatement> Statements = new List<IStatement>();

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class WhileStat : IStatement
    {
        public IExpression Condition;
        public Block Block;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class RepeatStat : IStatement
    {
        public Block Block;
        public IExpression Condition;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NumericFor : IStatement
    {
        public IExpression Var, Limit, Step;
        public string Variable;
        public Block Block;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class GenericFor : IStatement
    {
        public List<string> Variables = new List<string>();
        public List<IExpression> Expressions = new List<IExpression>();
        public Block Block;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IfStat : IStatement
    {
        public IExpression Condition;
        public Block Block;
        public List<IfStat> ElseIfs = new List<IfStat>();
        public Block ElseBlock;

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
