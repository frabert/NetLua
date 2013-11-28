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

    public class Return : IStatement
    {
        public List<IExpression> Expressions = new List<IExpression>();
    }

    public class Break : IStatement { }

    public class LocalAssignment : IStatement
    {
        public List<string> Names = new List<string>();
        public List<IExpression> Values = new List<IExpression>();
    }

    public class Block : IStatement
    {
        public List<IStatement> Statements = new List<IStatement>();
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

    public class If : IStatement
    {
        public IExpression Condition;
        public Block Block;
        public List<If> ElseIfs = new List<If>();
        public Block ElseBlock;
    }
}
