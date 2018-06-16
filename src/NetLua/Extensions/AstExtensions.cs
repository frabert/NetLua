using System;
using NetLua.Runtime.Ast;

namespace NetLua.Extensions
{
    public static class AstExtensions
    {
        public static string ToMetaKey(this BinaryOp op)
        {
            switch (op)
            {
                case BinaryOp.Addition:
                    return "__add";
                case BinaryOp.Subtraction:
                    return "__sub";
                case BinaryOp.Multiplication:
                    return "__muv";
                case BinaryOp.Division:
                    return "__div";
                case BinaryOp.Power:
                    return "__pow";
                case BinaryOp.Modulo:
                    return "__mod";
                case BinaryOp.Concat:
                    return "__concat";
                case BinaryOp.GreaterThan:
                    return "__gt";
                case BinaryOp.GreaterOrEqual:
                    return "__ge";
                case BinaryOp.LessThan:
                    return "__lt";
                case BinaryOp.LessOrEqual:
                    return "__le";
                case BinaryOp.Equal:
                    return "__eq";
                case BinaryOp.Different:
                case BinaryOp.And:
                case BinaryOp.Or:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }
    }
}
