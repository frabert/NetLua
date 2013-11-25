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

    public interface IStatement
    { }

    public interface IExpression
    { }

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
        public List<IExpression> Arguments;
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
        public List<Argument> Arguments;
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
        public IDictionary<IExpression, IExpression> Values;
    }

    public class Assignment : IStatement
    {
        // Var1, Var2, Var3 = Exp1, Exp2, Exp3
        //public Variable[] Variables;
        //public IExpression[] Expressions;

        public IAssignable Variable;
        public IExpression Expression;
    }

    public class Return : IStatement
    {
        public IExpression Expression;
    }

    public class LocalAssignment : IStatement
    {
        public string Name;
        public IExpression Value;
    }

    public class Block : IStatement
    {
        public List<IStatement> Statements;
    }

    public class While : IStatement
    {
        public IExpression Condition;
        public Block Block;
    }

    public class Repeat : IStatement
    {
        public Block Block;
        public IExpression Condition;
    }

    public class If : IStatement
    {
        public IExpression Condition;
        public Block Block;
        public List<If> ElseIfs;
        public Block ElseBlock;
    }
}
