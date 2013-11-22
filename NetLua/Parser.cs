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

        #region Binary expression tree
        Lua.Ast.IExpression ParseOrOp(ParseTreeNode node)
        {
            if (node.Term.Name == "OrOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParseAndOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParseAndOp(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Lua.Ast.BinaryOp.Or };
            }
            throw new Exception("Invalid OrOp node");
        }

        Lua.Ast.IExpression ParseAndOp(ParseTreeNode node)
        {
            if (node.Term.Name == "AndOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParseRelOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParseRelOp(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Lua.Ast.BinaryOp.And };
            }
            throw new Exception("Invalid AndOp node");
        }

        Lua.Ast.IExpression ParseRelOp(ParseTreeNode node)
        {
            if (node.Term.Name == "RelOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParseAddOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Lua.Ast.BinaryOp op;
                switch (opstring)
                {
                    case ">":
                        op = Ast.BinaryOp.GreaterThan; break;
                    case ">=":
                        op = Ast.BinaryOp.GreaterOrEqual; break;
                    case "<":
                        op = Ast.BinaryOp.LessThan; break;
                    case "<=":
                        op = Ast.BinaryOp.LessOrEqual; break;
                    case "==":
                        op = Ast.BinaryOp.Equal; break;
                    case "~=":
                        op = Ast.BinaryOp.Different; break;
                    default:
                        throw new Exception("Invalid operator");
                }

                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParseAddOp(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid RelOp node");
        }

        Lua.Ast.IExpression ParseAddOp(ParseTreeNode node)
        {
            if (node.Term.Name == "AddOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParseMulOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Lua.Ast.BinaryOp op;
                switch (opstring)
                {
                    case "+":
                        op = Ast.BinaryOp.Addition; break;
                    case "-":
                        op = Ast.BinaryOp.Subtraction; break;
                    default:
                        throw new Exception("Invalid operator");
                }

                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParseMulOp(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid AddOp node");
        }

        private Ast.IExpression ParseMulOp(ParseTreeNode node)
        {
            if (node.Term.Name == "MulOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParsePowerOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Lua.Ast.BinaryOp op;
                switch (opstring)
                {
                    case "*":
                        op = Ast.BinaryOp.Multiplication; break;
                    case "/":
                        op = Ast.BinaryOp.Division; break;
                    case "%":
                        op = Ast.BinaryOp.Modulo; break;
                    default:
                        throw new Exception("Invalid operator");
                }

                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParsePowerOp(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid MulOp node");
        }

        private Ast.IExpression ParsePowerOp(ParseTreeNode node)
        {
            if (node.Term.Name == "PowerOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Lua.Ast.IExpression lexpr = ParseExpression(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;

                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Lua.Ast.IExpression rexpr = ParseExpression(right);

                return new Lua.Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Lua.Ast.BinaryOp.Power };
            }
            throw new Exception("Invalid PowerOp node");
        }
        #endregion

        Lua.Ast.FunctionCall ParseFunctionCall(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionCall")
            {
                Lua.Ast.IExpression expr = ParsePrefix(node.ChildNodes[0]);
                Lua.Ast.FunctionCall call = new Ast.FunctionCall();
                call.Arguments = new List<Ast.IExpression>();
                call.Function = expr;

                var root = node.ChildNodes[1].ChildNodes[0];
                if (root.ChildNodes.Count != 0)
                {
                    root = root.ChildNodes[0];
                    while (true)
                    {
                        call.Arguments.Add(ParseExpression(root.ChildNodes[0]));
                        if (root.ChildNodes.Count == 1)
                            break;
                        else
                            root = root.ChildNodes[1];
                    }
                }
                return call;
            }
            throw new Exception("Invalid FunctionCall node");
        }

        Lua.Ast.FunctionDefinition ParseFunctionDef(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionDef")
            {
                ParseTreeNode argsNode = node.ChildNodes[1].ChildNodes[0];
                ParseTreeNode chunkNode = node.ChildNodes[2];

                Lua.Ast.Block block = ParseBlock(chunkNode);
                Lua.Ast.FunctionDefinition def = new Ast.FunctionDefinition();
                def.Body = block;
                def.Arguments = new List<Ast.Argument>();

                if (argsNode.ChildNodes.Count == 0)
                    return def;
                if (argsNode.ChildNodes.Count > 0)
                {
                    argsNode = argsNode.ChildNodes[0];
                    while (argsNode.ChildNodes.Count > 0)
                    {
                        string ident = argsNode.ChildNodes[0].Token.ValueString;
                        def.Arguments.Add(new Ast.Argument() { Name = ident });
                        if (argsNode.ChildNodes.Count == 1)
                            break;
                        argsNode = argsNode.ChildNodes[1];
                    }
                }
                return def;
            }
            throw new Exception("Invalid FunctionDef node");
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
            throw new Exception("Invalid Prefix node");
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
            throw new Exception("Invalid Variable node");
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
                else if (child.Term != null && child.Term.Name == "OrOp")
                {
                    return ParseOrOp(child);
                }
                else if (child.Term != null && child.Term.Name == "FunctionDef")
                {
                    return ParseFunctionDef(child);
                }
            }
            throw new Exception("Invalid Expression node");
        }

        Lua.Ast.Assignment ParseAssign(ParseTreeNode node)
        {
            if (node.Term.Name == "Assignment")
            {
                Lua.Ast.IAssignable left = ParseVariable(node.ChildNodes[0]);
                Lua.Ast.IExpression right = ParseExpression(node.ChildNodes[1]);

                return new Lua.Ast.Assignment() { Expression = right, Variable = left };
            }
            throw new Exception("Invalid Assignment node");
        }

        Lua.Ast.LocalAssignment ParseLocalAssign(ParseTreeNode node)
        {
            if (node.Term.Name == "LocalAssignment")
            {
                Lua.Ast.IAssignable left = ParseVariable(node.ChildNodes[1]);
                Lua.Ast.IExpression right;
                if (node.ChildNodes[2].ChildNodes.Count == 0)
                    right = new Lua.Ast.NilLiteral();
                else
                    right = ParseExpression(node.ChildNodes[2].ChildNodes[0]);

                return new Lua.Ast.LocalAssignment() { Expression = right, Variable = left };
            }
            throw new Exception("Invalid LocalAssignment node");
        }

        Lua.Ast.Return ParseReturnStat(ParseTreeNode node)
        {
            if (node.Term.Name == "ReturnStat")
            {
                return new Ast.Return() { Expression = ParseExpression(node.ChildNodes[1]) };
            }
            throw new Exception("Invalid ReturnStat node");
        }

        Lua.Ast.Block ParseDoBlock(ParseTreeNode node)
        {
            if (node.Term.Name == "DoBlock")
            {
                
            }
            throw new Exception("Invalid DoBlock node");
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
                else if (child.Term.Name == "ReturnStat")
                {
                    block.Statements.Add(ParseReturnStat(child));
                }
                else if (child.Term.Name == "DoBlock")
                {
                    block.Statements.Add(ParseDoBlock(child));
                }
            }
            return block;
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

            NonTerminal PowerOp = new NonTerminal("PowerOp");
            NonTerminal MulOp = new NonTerminal("MulOp");
            NonTerminal AddOp = new NonTerminal("AddOp");
            NonTerminal ConcatOp = new NonTerminal("ConcatOp");
            NonTerminal RelOp = new NonTerminal("RelOp");
            NonTerminal AndOp = new NonTerminal("AndOp");
            NonTerminal OrOp = new NonTerminal("OrOp");

            #endregion

            CallArgumentsFragment.Rule = Expression | Expression + "," + CallArgumentsFragment;

            CallArguments.Rule = "(" + (CallArgumentsFragment | Empty) + ")";

            DefArgumentsFragment.Rule = Identifier | Identifier + "," + DefArgumentsFragment;

            DefArguments.Rule = "(" + (DefArgumentsFragment | Empty) + ")";

            Chunk.Rule = MakeStarRule(Chunk, Statement);

            #region Expressions
            PowerOp.Rule = Expression + ("^" + Expression | Empty);
            MulOp.Rule = PowerOp + ((ToTerm("*") | "/" | "%") + PowerOp | Empty);
            AddOp.Rule = MulOp + ((ToTerm("+") | "-") + MulOp | Empty);
            RelOp.Rule = AddOp + ((ToTerm(">") | ">=" | "<" | "<=" | "==" | "~=") + AddOp | Empty);
            AndOp.Rule = RelOp + ("and" + RelOp | Empty);
            OrOp.Rule = AndOp + ("or" + AndOp | Empty);

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
                OrOp
                | ToTerm("true")
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
