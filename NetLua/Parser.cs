using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony.Parsing;
using Irony.Ast;

namespace Lua
{
    public class Parser
    {
        public Parser(string parse)
        {
            LuaGrammar grammar = new LuaGrammar();
            LanguageData language = new LanguageData(grammar);
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(language);
            ParseTree parseTree = parser.Parse(parse);
            ParseTreeNode root = parseTree.Root;
            if (root == null)
            {
                for (int i = 0; i < parseTree.ParserMessages[0].Location.Column; i++)
                {
                    Console.Write(" ");
                }
                Console.WriteLine("^");
                Console.WriteLine(parseTree.ParserMessages[0].Message);
                return;
            }

            dispTree(root, 0);
            ParseBlock(root);
        }

        Lua.Ast.FunctionCall ParseFunctionCall(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionCall")
            {
                Lua.Ast.IExpression expr = ParsePrefix(node.ChildNodes[0]);

            }
            return null;
        }

        Lua.Ast.IExpression ParsePrefix(ParseTreeNode node)
        {
            if (node.Term != null)
            {
                if (node.Term.Name == "Prefix")
                {
                    ParseTreeNode child = node.ChildNodes[0];
                    if (child.Term.Name == "Variable")
                    {
                        return ParseVariable(child);
                    }
                    else if (child.Term.Name == "Expression")
                    {
                        return ParseExpression(child);
                    }
                    else if (child.Term.Name == "FunctionCall")
                    {
                        return ParseFunctionCall(child);
                    }
                }
            }
            return null;
        }

        Lua.Ast.IAssignable ParseVariable(ParseTreeNode node)
        {
            if (node.Term != null)
            {
                if (node.ChildNodes.Count == 1)
                {
                    string name = node.ChildNodes[0].Token.ValueString;
                    return new Lua.Ast.Variable() { Name = name };
                }
                else
                {
                    Lua.Ast.IExpression prefix = ParsePrefix(node.ChildNodes[0]);
                    if (node.ChildNodes[1].Term.Name == "Expression")
                    {
                        Lua.Ast.IExpression index = ParseExpression(node.ChildNodes[1]);
                        return new Lua.Ast.TableAccess() { Expression = prefix, Index = index };
                    }
                    else
                    {
                        string name = node.ChildNodes[1].Token.ValueString;
                        return new Lua.Ast.Variable() { Name = name, Prefix = prefix };
                    }
                }
            }
            return null;
        }

        Lua.Ast.IExpression ParseExpression(ParseTreeNode node)
        {
            if (node.Term.Name == "Expression")
            {
                ParseTreeNode child = node.ChildNodes[0];
                if (child.Token != null && child.Token.Terminal is NumberLiteral)
                {
                    return new Lua.Ast.NumberLiteral() { Value = (child.Token.Value is double ? (double)(child.Token.Value) : (int)(child.Token.Value)) };
                }
                else if (child.Token != null && child.Token.Terminal is StringLiteral)
                {
                    return new Lua.Ast.StringLiteral() { Value = (string)(child.Token.Value) };
                }
                else if (child.Token != null && child.Token.Terminal is KeyTerm)
                {
                    string val = child.Token.ValueString;
                    if (val == "true")
                        return new Lua.Ast.BoolLiteral() { Value = true };
                    else if (val == "false")
                        return new Lua.Ast.BoolLiteral() { Value = false };
                    else if (val == "nil")
                        return new Lua.Ast.NilLiteral();
                }
                else if (child.Term != null && child.Term.Name == "Prefix")
                {
                    return ParsePrefix(child);
                }
            }
            return null;
        }

        Lua.Ast.Assignment ParseAssign(ParseTreeNode node)
        {
            Lua.Ast.IAssignable left = ParseVariable(node.ChildNodes[0]);
            Lua.Ast.IExpression right = ParseExpression(node.ChildNodes[1]);

            return new Lua.Ast.Assignment() { Expression = right, Variable = left };
        }

        Lua.Ast.LocalAssignment ParseLocalAssign(ParseTreeNode node)
        {
            Lua.Ast.IAssignable left = ParseVariable(node.ChildNodes[1]);
            Lua.Ast.IExpression right;
            if (node.ChildNodes[2].ChildNodes.Count == 0)
                right = new Lua.Ast.NilLiteral();
            else
                right = ParseExpression(node.ChildNodes[2].ChildNodes[0]);

            return new Lua.Ast.LocalAssignment() { Expression = right, Variable = left };
        }

        Lua.Ast.Block ParseBlock(ParseTreeNode node)
        {
            Lua.Ast.Block block = new Ast.Block();
            block.Statements = new List<Ast.IStatement>();
            foreach (ParseTreeNode child in node.ChildNodes)
            {
                if (child.Term.Name == "Assignment")
                {
                    block.Statements.Add(ParseAssign(child));
                }
                else if (child.Term.Name == "LocalAssignment")
                {
                    block.Statements.Add(ParseLocalAssign(child));
                }
                else if (child.Term.Name == "FunctionCall")
                {
                    block.Statements.Add(ParseFunctionCall(child));
                }
            }
            return null;
        }

        public static void dispTree(ParseTreeNode node, int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write(" ");
            Console.WriteLine(node);

            foreach (ParseTreeNode child in node.ChildNodes)
                dispTree(child, level + 1);
        }
    }

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
            NonTerminal CallArgumentsFragment = new NonTerminal("");
            NonTerminal Expression = new NonTerminal("Expression");
            NonTerminal FunctionDef = new NonTerminal("FunctionDef");
            NonTerminal DefArguments = new NonTerminal("FunctionDefArguments");
            NonTerminal DefArgumentsFragment = new NonTerminal("");

            NonTerminal Statement = new NonTerminal("Statement");
            NonTerminal ReturnStatement = new NonTerminal("ReturnStat");
            NonTerminal Assignment = new NonTerminal("Assignment");
            NonTerminal LocalAssignment = new NonTerminal("LocalAssignment");
            NonTerminal FunctionDecl = new NonTerminal("FunctionDecl");
            NonTerminal DoBlock = new NonTerminal("DoBlock");
            NonTerminal If = new NonTerminal("If");
            NonTerminal Elseif = new NonTerminal("Elseif");
            NonTerminal Else = new NonTerminal("Else");
            #endregion

            CallArgumentsFragment.Rule = Expression | Expression + "," + CallArgumentsFragment;

            CallArguments.Rule = "(" + (CallArgumentsFragment | Empty) + ")";

            DefArgumentsFragment.Rule = Identifier | Identifier + "," + DefArgumentsFragment;

            DefArguments.Rule = "(" + (DefArgumentsFragment | Empty) + ")";

            Chunk.Rule = MakeStarRule(Chunk, Statement);

            #region Expressions
            Prefix.Rule =
                "(" + Expression + ")"
                | FunctionCall
                | Variable;

            Variable.Rule =
                Prefix + "." + Identifier
                | Prefix + "[" + Expression + "]"
                | Identifier;

            FunctionCall.Rule = Prefix + CallArguments;

            FunctionDef.Rule =
                ToTerm("function") + DefArguments
                + Chunk + ToTerm("end");

            Expression.Rule =
                ToTerm("true")
                | "false"
                | "nil"
                | SingleString
                | DoubleString
                | Number
                | Prefix
                | FunctionDef;
            #endregion

            #region Statements
            FunctionDecl.Rule = "function" + Identifier + DefArguments + Chunk + "end";

            ReturnStatement.Rule =
                "return" + Expression;

            Assignment.Rule =
                Variable + "=" + Expression;

            LocalAssignment.Rule = "local" + Variable + ("=" + Expression | Empty);

            Elseif.Rule = "elseif" + Expression + "then" + Chunk;
            Else.Rule = "else" + Chunk;

            If.Rule = "if" + Expression + "then" + Chunk + MakeStarRule(Elseif, Elseif) + (Else | Empty) + "end";

            DoBlock.Rule = "do" + Chunk + "end";

            Statement.Rule =
                ReturnStatement
                | Assignment
                | LocalAssignment
                | FunctionCall
                | FunctionDecl
                //| If
                | DoBlock;
            #endregion

            this.Root = Chunk;
            this.MarkReservedWords(
                "true", "false",
                "nil", "local",
                "function", "while",
                "if", "for",
                "end", "do", "return");

            this.MarkPunctuation(".", ",", ";", "(", ")", "[", "]", "=");
            this.MarkTransient(Statement);
        }
    }
}
