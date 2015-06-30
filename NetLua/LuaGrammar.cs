/*
 * See LICENSE file
 */

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
      var Identifier = new IdentifierTerminal("identifier");
      var SingleString = new StringLiteral("string", "'", StringOptions.AllowsAllEscapes);
      var DoubleString = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes);
      var Number = new NumberLiteral("number", NumberOptions.AllowSign);

      var LineComment = new CommentTerminal("Comment", "--", "\n", "\r");
      var LongComment = new CommentTerminal("LongComment", "--[[", "]]");

      base.NonGrammarTerminals.Add(LineComment);
      base.NonGrammarTerminals.Add(LongComment);
      #endregion

      #region Nonterminals
      var Chunk = new NonTerminal("Chunk");
      
      var Prefix = new NonTerminal("Prefix");
      var Variable = new NonTerminal("Variable");
      var FunctionCall = new NonTerminal("FunctionCall");
      var CallArguments = new NonTerminal("FunctionCallArguments");
      var OopCall = new NonTerminal("OopCall");
      var CallArgumentsFragment = new NonTerminal("");
      var Expression = new NonTerminal("Expression");
      var FunctionDef = new NonTerminal("FunctionDef");
      var DefArguments = new NonTerminal("FunctionDefArguments");
      var DefArgumentsFragment = new NonTerminal("");
      
      var Statement = new NonTerminal("Statement");
      var ReturnStatement = new NonTerminal("ReturnStat");
      var BreakStatement = new NonTerminal("BreakStat");
      var Assignment = new NonTerminal("Assignment");
      var LocalAssignment = new NonTerminal("LocalAssignment");
      var FunctionDecl = new NonTerminal("FunctionDecl");
      var DoBlock = new NonTerminal("DoBlock");
      var If = new NonTerminal("If");
      var Elseif = new NonTerminal("Elseif");
      var Else = new NonTerminal("Else");
      var While = new NonTerminal("While");
      var Repeat = new NonTerminal("Repeat");
      var For = new NonTerminal("For");
      
      var PowerOp = new NonTerminal("PowerOp");
      var MulOp = new NonTerminal("MulOp");
      var AddOp = new NonTerminal("AddOp");
      var ConcatOp = new NonTerminal("ConcatOp");
      var RelOp = new NonTerminal("RelOp");
      var AndOp = new NonTerminal("AndOp");
      var OrOp = new NonTerminal("OrOp");
      var UnaryExpr = new NonTerminal("UnaryExpr");
      
      var TableConstruct = new NonTerminal("TableConstruct");
      var TableConstructFragment = new NonTerminal("TableConstructFragment");


      var varargs = new NonTerminal("Varargs");
      #endregion

      #region Fragments
      CallArgumentsFragment.Rule = Expression | Expression + "," + CallArgumentsFragment;

      CallArguments.Rule = "(" + (CallArgumentsFragment | Empty) + ")";

      DefArgumentsFragment.Rule = Identifier | Identifier + "," + DefArgumentsFragment | varargs;

      DefArguments.Rule = "(" + (DefArgumentsFragment | Empty) + ")";

      Chunk.Rule = MakeStarRule(Chunk, Statement);
      #endregion

      #region Expressions
      PowerOp.Rule = Expression + ("^" + Expression | Empty);
      MulOp.Rule = PowerOp + ((ToTerm("*") | "/" | "%") + PowerOp | Empty);
      AddOp.Rule = MulOp + ((ToTerm("+") | "-") + MulOp | Empty);
      ConcatOp.Rule = AddOp + (".." + AddOp | Empty);
      RelOp.Rule = ConcatOp + ((ToTerm(">") | ">=" | "<" | "<=" | "==" | "~=") + ConcatOp | Empty);
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

      var tableSep = new NonTerminal("");
      tableSep.Rule = ToTerm(";") | ",";
      TableConstructFragment.Rule =
          (
              (
                  (
                      (Identifier | "[" + Expression + "]") + "="
                  )
                  + Expression
                  | Expression
              )
              + (";" + TableConstructFragment | "," + TableConstructFragment | Empty)
          ) | Empty;
      TableConstruct.Rule = "{" + TableConstructFragment + "}";

      varargs.Rule = "...";

      Expression.Rule =
           varargs
          | Prefix
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
          ";"
          | ReturnStatement
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
