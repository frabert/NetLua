using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony.Parsing;
using Irony.Ast;

namespace NetLua
{
    class LuaGrammar : Grammar
    {
        public LuaGrammar()
            : base(true)
        {
            #region Terminals
            Terminal Identifier = new IdentifierTerminal("identifier");
            Terminal SingleString = new StringLiteral("string", "'", StringOptions.AllowsAllEscapes);
            Terminal DoubleString = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes);
            Terminal Number = new NumberLiteral("number", NumberOptions.AllowSign);

            Terminal LineComment = new CommentTerminal("Comment", "--", "\n", "\r");
            Terminal LongComment = new CommentTerminal("LongComment", "--[[", "]]");

            base.NonGrammarTerminals.Add(LineComment);
            base.NonGrammarTerminals.Add(LongComment);
            #endregion

            #region Nonterminals
            NonTerminal Chunk = new NonTerminal("Chunk");

            NonTerminal Prefix = new NonTerminal("Prefix");
            NonTerminal Variable = new NonTerminal("Variable");
            NonTerminal FunctionCall = new NonTerminal("FunctionCall");
            NonTerminal CallArguments = new NonTerminal("FunctionCallArguments");
            NonTerminal OopCall = new NonTerminal("OopCall");
            NonTerminal CallArgumentsFragment = new NonTerminal("");
            NonTerminal Expression = new NonTerminal("Expression");
            NonTerminal FunctionDef = new NonTerminal("FunctionDef");
            NonTerminal DefArguments = new NonTerminal("FunctionDefArguments");
            NonTerminal DefArgumentsFragment = new NonTerminal("");

            NonTerminal Statement = new NonTerminal("Statement");
            NonTerminal ReturnStatement = new NonTerminal("ReturnStat");
            NonTerminal BreakStatement = new NonTerminal("BreakStat");
            NonTerminal Assignment = new NonTerminal("Assignment");
            NonTerminal LocalAssignment = new NonTerminal("LocalAssignment");
            NonTerminal FunctionDecl = new NonTerminal("FunctionDecl");
            NonTerminal DoBlock = new NonTerminal("DoBlock");
            NonTerminal If = new NonTerminal("If");
            NonTerminal Elseif = new NonTerminal("Elseif");
            NonTerminal Else = new NonTerminal("Else");
            NonTerminal While = new NonTerminal("While");
            NonTerminal Repeat = new NonTerminal("Repeat");
            NonTerminal For = new NonTerminal("For");

            NonTerminal PowerOp = new NonTerminal("PowerOp");
            NonTerminal MulOp = new NonTerminal("MulOp");
            NonTerminal AddOp = new NonTerminal("AddOp");
            NonTerminal ConcatOp = new NonTerminal("ConcatOp");
            NonTerminal RelOp = new NonTerminal("RelOp");
            NonTerminal AndOp = new NonTerminal("AndOp");
            NonTerminal OrOp = new NonTerminal("OrOp");
            NonTerminal UnaryExpr = new NonTerminal("UnaryExpr");

            NonTerminal TableConstruct = new NonTerminal("TableConstruct");
            NonTerminal TableConstructFragment = new NonTerminal("TableConstructFragment");
            #endregion

            #region Fragments
            CallArgumentsFragment.Rule = Expression | Expression + "," + CallArgumentsFragment;

            CallArguments.Rule = "(" + (CallArgumentsFragment | Empty) + ")";

            DefArgumentsFragment.Rule = Identifier | Identifier + "," + DefArgumentsFragment;

            DefArguments.Rule = "(" + (DefArgumentsFragment | Empty) + ")";

            Chunk.Rule = MakeStarRule(Chunk, Statement);
            #endregion

            #region Expressions
            PowerOp.Rule = Expression + ("^" + Expression | Empty);
            MulOp.Rule = PowerOp + ((ToTerm("*") | "/" | "%") + PowerOp | Empty);
            AddOp.Rule = MulOp + ((ToTerm("+") | "-") + MulOp | Empty);
            RelOp.Rule = AddOp + ((ToTerm(">") | ">=" | "<" | "<=" | "==" | "~=") + AddOp | Empty);
            AndOp.Rule = RelOp + ("and" + RelOp | Empty);
            OrOp.Rule = AndOp + ("or" + AndOp | Empty);

            UnaryExpr.Rule = (ToTerm("not") | "-" | "#") + Expression;

            Prefix.Rule =
                OopCall
                | FunctionCall
                | Variable
                | "(" + Expression + ")";

            Variable.Rule =
                Prefix + "." + Identifier
                | Prefix + "[" + Expression + "]"
                | Identifier;

            FunctionCall.Rule = Prefix + CallArguments;
            OopCall.Rule = Prefix + ":" + Identifier + CallArguments;

            FunctionDef.Rule =
                ToTerm("function") + DefArguments
                + Chunk + ToTerm("end");

            TableConstructFragment.Rule = ((ToTerm("[") + Expression + "]" + "=" | Identifier + "=" | Empty) + Expression) + ("," + TableConstructFragment | Empty) | Empty;
            TableConstruct.Rule = "{" + TableConstructFragment + "}";

            Expression.Rule =
                Prefix
                | OrOp
                | UnaryExpr
                | ToTerm("true")
                | "false"
                | "nil"
                | SingleString
                | DoubleString
                | Number
                | FunctionDef
                | TableConstruct;
            #endregion

            #region Statements
            FunctionDecl.Rule = "function" + Variable + (":" + Identifier | Empty) + DefArguments + Chunk + "end";


            var RetChunk = new NonTerminal("RetChunk");
            RetChunk.Rule = Expression + (("," + RetChunk) | Empty);

            ReturnStatement.Rule =
                "return" + (RetChunk | Empty);

            var AssignExpChunk = new NonTerminal("AssignExpChunk");
            AssignExpChunk.Rule = Expression + (("," + AssignExpChunk) | Empty);
            var AssignVarChunk = new NonTerminal("AssignVarChunk");
            AssignVarChunk.Rule = Variable + (("," + AssignVarChunk) | Empty);

            Assignment.Rule =
                AssignVarChunk + "=" + AssignExpChunk;

            var LocalAssignNameChunk = new NonTerminal("AssignNameChunk");
            var LocalFunction = new NonTerminal("LocalFunction");
            LocalAssignNameChunk.Rule = Identifier + (("," + LocalAssignNameChunk) | Empty);
            LocalFunction.Rule = "function" + Identifier + DefArguments + Chunk + "end";
            LocalAssignment.Rule = "local" + (LocalAssignNameChunk + ("=" + AssignExpChunk | Empty) | LocalFunction);

            Elseif.Rule = "elseif" + Expression + "then" + Chunk + (Elseif | Empty);
            Else.Rule = "else" + Chunk;

            If.Rule = "if" + Expression + "then" + Chunk + (Elseif | Empty) + (Else | Empty) + "end";

            While.Rule = "while" + Expression + DoBlock;

            DoBlock.Rule = "do" + Chunk + "end";

            Repeat.Rule = "repeat" + Chunk + "until" + Expression;

            BreakStatement.Rule = "break";

            var NumericFor = new NonTerminal("NumericFor");
            NumericFor.Rule = Identifier + "=" + Expression + "," + Expression + ("," + Expression | Empty);
            var GenericFor = new NonTerminal("GenericFor");
            var NameList = new NonTerminal("NameList");
            var ExpList = new NonTerminal("ExpList");
            NameList.Rule = Identifier + (("," + NameList) | Empty);
            ExpList.Rule = Expression + (("," + ExpList) | Empty);
            GenericFor.Rule = NameList + "in" + ExpList;

            For.Rule = "for" + (GenericFor | NumericFor) + DoBlock;

            Statement.Rule =
                ReturnStatement
                | BreakStatement
                | Assignment
                | LocalAssignment
                | FunctionCall
                | OopCall
                | FunctionDecl
                | For
                | If
                | While
                | DoBlock
                | Repeat;
            #endregion

            this.Root = Chunk;
            this.MarkReservedWords(
                "true", "false",
                "nil", "local",
                "function", "while",
                "if", "for", "repeat", "until",
                "end", "do", "return", "break");

            this.MarkPunctuation(".", ",", ";", "(", ")", "[", "]", "{", "}", "=", ":");
            this.MarkTransient(Statement);
        }
    }
}
