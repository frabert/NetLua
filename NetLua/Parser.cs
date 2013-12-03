using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Irony.Parsing;
using Irony.Ast;

namespace NetLua
{
    public class Parser
    {
        public Ast.Block block;

        LuaGrammar grammar = new LuaGrammar();
        LanguageData language;
        Irony.Parsing.Parser parser;

        public Parser()
        {
            language = new LanguageData(grammar);
            parser = new Irony.Parsing.Parser(language);
        }

        public Ast.Block ParseString(string Chunk)
        {
            ParseTree parseTree = parser.Parse(Chunk);
            ParseTreeNode root = parseTree.Root;
            if (root == null)
            {
                Irony.LogMessage msg = parseTree.ParserMessages[0];
                throw new LuaException("", msg.Location.Line, msg.Location.Column, msg.Message);
            }
            return (ParseBlock(root));
        }

        public Ast.Block ParseFile(string Filename)
        {
            string source = System.IO.File.ReadAllText(Filename);
            ParseTree parseTree = parser.Parse(source, Filename);
            ParseTreeNode root = parseTree.Root;
            if (root == null)
            {
                Irony.LogMessage msg = parseTree.ParserMessages[0];
                throw new LuaException(Filename, msg.Location.Line, msg.Location.Column, msg.Message);
            }
            return (ParseBlock(root));
        }

        #region Binary expression tree
        Ast.IExpression ParseOrOp(ParseTreeNode node)
        {
            if (node.Term.Name == "OrOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParseAndOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Ast.IExpression rexpr = ParseAndOp(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Ast.BinaryOp.Or };
            }
            throw new Exception("Invalid OrOp node");
        }

        Ast.IExpression ParseAndOp(ParseTreeNode node)
        {
            if (node.Term.Name == "AndOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParseRelOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Ast.IExpression rexpr = ParseRelOp(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Ast.BinaryOp.And };
            }
            throw new Exception("Invalid AndOp node");
        }

        Ast.IExpression ParseRelOp(ParseTreeNode node)
        {
            if (node.Term.Name == "RelOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParseAddOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Ast.BinaryOp op;
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
                Ast.IExpression rexpr = ParseAddOp(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid RelOp node");
        }

        Ast.IExpression ParseAddOp(ParseTreeNode node)
        {
            if (node.Term.Name == "AddOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParseMulOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Ast.BinaryOp op;
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
                Ast.IExpression rexpr = ParseMulOp(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid AddOp node");
        }

        Ast.IExpression ParseMulOp(ParseTreeNode node)
        {
            if (node.Term.Name == "MulOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParsePowerOp(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;
                string opstring = node.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.ValueString;
                Ast.BinaryOp op;
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
                Ast.IExpression rexpr = ParsePowerOp(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = op };
            }
            throw new Exception("Invalid MulOp node");
        }

        Ast.IExpression ParsePowerOp(ParseTreeNode node)
        {
            if (node.Term.Name == "PowerOp")
            {
                ParseTreeNode left = node.ChildNodes[0];
                Ast.IExpression lexpr = ParseExpression(left);
                if (node.ChildNodes[1].ChildNodes.Count == 0)
                    return lexpr;

                ParseTreeNode right = node.ChildNodes[1].ChildNodes[1];
                Ast.IExpression rexpr = ParseExpression(right);

                return new Ast.BinaryExpression() { Left = lexpr, Right = rexpr, Operation = Ast.BinaryOp.Power };
            }
            throw new Exception("Invalid PowerOp node");
        }
        #endregion

        #region Statements
        Ast.FunctionCall ParseFunctionCall(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionCall")
            {
                Ast.IExpression expr = ParsePrefix(node.ChildNodes[0]);
                Ast.FunctionCall call = new Ast.FunctionCall();
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

        Ast.FunctionCall ParseOopCall(ParseTreeNode node)
        {
            if (node.Term.Name == "OopCall")
            {
                Ast.IExpression expr = ParsePrefix(node.ChildNodes[0]);
                string name = node.ChildNodes[1].Token.ValueString;
                Ast.FunctionCall call = new Ast.FunctionCall();
                call.Arguments.Add(expr);
                call.Function = new Ast.Variable() { Name = name, Prefix = expr };

                var root = node.ChildNodes[2].ChildNodes[0];
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
            throw new Exception("Invalid OopCall node");
        }

        Ast.Assignment ParseAssign(ParseTreeNode node)
        {
            if (node.Term.Name == "Assignment")
            {
                Ast.Assignment assign = new Ast.Assignment();

                var left = node.ChildNodes[0];
                var right = node.ChildNodes[1];

                assign.Variables.Add(ParseVariable(left.ChildNodes[0]));
                assign.Expressions.Add(ParseExpression(right.ChildNodes[0]));

                left = left.ChildNodes[1];
                right = right.ChildNodes[1];

                while (left.ChildNodes.Count > 0)
                {
                    left = left.ChildNodes[0];
                    assign.Variables.Add(ParseVariable(left.ChildNodes[0]));
                    left = left.ChildNodes[1];
                }

                while (right.ChildNodes.Count > 0)
                {
                    right = right.ChildNodes[0];
                    assign.Expressions.Add(ParseExpression(right.ChildNodes[0]));
                    right = right.ChildNodes[1];
                }

                return assign;
            }
            throw new Exception("Invalid Assignment node");
        }

        Ast.LocalAssignment ParseLocalAssign(ParseTreeNode node)
        {
            if (node.Term.Name == "LocalAssignment")
            {
                Ast.LocalAssignment assign = new Ast.LocalAssignment();


                var child = node.ChildNodes[1];

                if (child.ChildNodes[0].Term.Name == "LocalFunction")
                {
                    child = child.ChildNodes[0];

                    var argsNode = child.ChildNodes[2];
                    var blockNode = child.ChildNodes[3];

                    assign.Names.Add(child.ChildNodes[1].Token.ValueString);
                    var func = new Ast.FunctionDefinition();

                    if (argsNode.ChildNodes.Count > 0)
                    {
                        argsNode = argsNode.ChildNodes[0];
                        while (argsNode.ChildNodes.Count > 0)
                        {
                            string ident = argsNode.ChildNodes[0].Token.ValueString;
                            func.Arguments.Add(new Ast.Argument() { Name = ident });
                            if (argsNode.ChildNodes.Count == 1)
                                break;
                            argsNode = argsNode.ChildNodes[1];
                        }
                    }

                    func.Body = ParseBlock(blockNode);

                    assign.Values.Add(func);
                    return assign;
                }

                var left = child.ChildNodes[0];
                var right = child.ChildNodes[1];

                assign.Names.Add(left.ChildNodes[0].Token.ValueString);

                left = left.ChildNodes[1];

                while (left.ChildNodes.Count > 0)
                {
                    left = left.ChildNodes[0];
                    assign.Names.Add(left.ChildNodes[0].Token.ValueString);
                    left = left.ChildNodes[1];
                }

                while (right.ChildNodes.Count > 0)
                {
                    right = right.ChildNodes[0];
                    assign.Values.Add(ParseExpression(right.ChildNodes[0]));
                    right = right.ChildNodes[1];
                }

                return assign;
            }
            throw new Exception("Invalid LocalAssignment node");
        }

        Ast.Return ParseReturnStat(ParseTreeNode node)
        {
            if (node.Term.Name == "ReturnStat")
            {
                Ast.Return ret = new Ast.Return();
                var child = node.ChildNodes[1];
                while (child.ChildNodes.Count > 0)
                {
                    child = child.ChildNodes[0];
                    ret.Expressions.Add(ParseExpression(child.ChildNodes[0]));
                    child = child.ChildNodes[1];
                }
                return ret;
            }
            throw new Exception("Invalid ReturnStat node");
        }

        Ast.Block ParseDoBlock(ParseTreeNode node)
        {
            if (node.Term.Name == "DoBlock")
            {
                return ParseBlock(node.ChildNodes[1]);
            }
            throw new Exception("Invalid DoBlock node");
        }

        Ast.Repeat ParseRepeat(ParseTreeNode node)
        {
            if (node.Term.Name == "Repeat")
            {
                Ast.Block block = ParseBlock(node.ChildNodes[1]);
                Ast.IExpression condition = ParseExpression(node.ChildNodes[3]);

                return new Ast.Repeat()
                {
                    Block = block,
                    Condition = condition
                };
            }
            throw new Exception("Invalid Repeat node");
        }

        Ast.While ParseWhile(ParseTreeNode node)
        {
            if (node.Term.Name == "While")
            {
                return new Ast.While()
                {
                    Condition = ParseExpression(node.ChildNodes[1]),
                    Block = ParseDoBlock(node.ChildNodes[2])
                };
            }
            throw new Exception("Invalid While node");
        }

        Ast.If ParseIf(ParseTreeNode node)
        {
            if (node.Term.Name == "If")
            {
                Ast.IExpression condition = ParseExpression(node.ChildNodes[1]);
                Ast.Block block = ParseBlock(node.ChildNodes[3]);

                Ast.If If = new Ast.If();
                If.Block = block;
                If.Condition = condition;
                If.ElseIfs = new List<Ast.If>();

                ParseTreeNode ElseifNode = node.ChildNodes[4];
                ParseTreeNode ElseNode = node.ChildNodes[5];

                while (ElseifNode.ChildNodes.Count != 0)
                {
                    var childnode = ElseifNode.ChildNodes[0];
                    Ast.If elseif = new Ast.If();
                    elseif.Condition = ParseExpression(childnode.ChildNodes[1]);
                    elseif.Block = ParseBlock(childnode.ChildNodes[3]);

                    If.ElseIfs.Add(elseif);

                    ElseifNode = childnode.ChildNodes[4];
                }

                if (ElseNode.ChildNodes.Count != 0)
                {
                    If.ElseBlock = ParseBlock(ElseNode.ChildNodes[0].ChildNodes[1]);
                }

                return If;
            }
            throw new Exception("Invalid If node");
        }

        Ast.Assignment ParseFunctionDecl(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionDecl")
            {
                Ast.IAssignable expr = ParseVariable(node.ChildNodes[1]);

                ParseTreeNode argsNode = node.ChildNodes[3].ChildNodes[0];
                ParseTreeNode chunkNode = node.ChildNodes[4];

                Ast.Block block = ParseBlock(chunkNode);
                Ast.FunctionDefinition def = new Ast.FunctionDefinition();
                def.Arguments = new List<Ast.Argument>();

                var nameNode = node.ChildNodes[2];
                if (nameNode.ChildNodes.Count > 0)
                {
                    def.Arguments.Add(new Ast.Argument() { Name = "self" });
                    expr = new Ast.Variable()
                    {
                        Name = nameNode.ChildNodes[0].Token.ValueString,
                        Prefix = expr
                    };
                }
                def.Body = block;
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
                Ast.Assignment assign = new Ast.Assignment();
                assign.Variables.Add(expr);
                assign.Expressions.Add(def);
                return assign;
            }
            throw new Exception("Invalid FunctionDecl node");
        }
        #endregion

        #region Expressions
        Ast.UnaryExpression ParseUnaryExpr(ParseTreeNode node)
        {
            if (node.Term.Name == "UnaryExpr")
            {
                Ast.IExpression expr = ParseExpression(node.ChildNodes[1]);
                var opNode = node.ChildNodes[0].ChildNodes[0];
                Ast.UnaryOp op = Ast.UnaryOp.Invert;
                switch (opNode.Token.ValueString)
                {
                    case "not":
                        op = Ast.UnaryOp.Negate; break;
                    case "-":
                        op = Ast.UnaryOp.Invert; break;
                    case "#":
                        op = Ast.UnaryOp.Length; break;
                }
                return new Ast.UnaryExpression()
                {
                    Expression = expr,
                    Operation = op
                };
            }
            throw new Exception("Invalid UnaryExpr node");
        }

        Ast.FunctionDefinition ParseFunctionDef(ParseTreeNode node)
        {
            if (node.Term.Name == "FunctionDef")
            {
                ParseTreeNode argsNode = node.ChildNodes[1].ChildNodes[0];
                ParseTreeNode chunkNode = node.ChildNodes[2];

                Ast.Block block = ParseBlock(chunkNode);
                Ast.FunctionDefinition def = new Ast.FunctionDefinition();
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

        Ast.IExpression ParsePrefix(ParseTreeNode node)
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
                    else if (child.Term.Name == "OopCall")
                    {
                        return ParseOopCall(child);
                    }
                }
            }
            throw new Exception("Invalid Prefix node");
        }

        Ast.IAssignable ParseVariable(ParseTreeNode node)
        {
            if (node.Term != null)
            {
                if (node.ChildNodes.Count == 1)
                {
                    string name = node.ChildNodes[0].Token.ValueString;
                    return new Ast.Variable() { Name = name };
                }
                else
                {
                    Ast.IExpression prefix = ParsePrefix(node.ChildNodes[0]);
                    if (node.ChildNodes[1].Term.Name == "Expression")
                    {
                        Ast.IExpression index = ParseExpression(node.ChildNodes[1]);
                        return new Ast.TableAccess() { Expression = prefix, Index = index };
                    }
                    else
                    {
                        string name = node.ChildNodes[1].Token.ValueString;
                        return new Ast.Variable() { Name = name, Prefix = prefix };
                    }
                }
            }
            throw new Exception("Invalid Variable node");
        }

        Ast.TableConstructor ParseTableConstruct(ParseTreeNode node)
        {
            if (node.Term.Name == "TableConstruct")
            {
                if (node.ChildNodes.Count == 0)
                {
                    return new Ast.TableConstructor() { Values = new Dictionary<Ast.IExpression, Ast.IExpression>() };
                }
                else
                {
                    var child = node.ChildNodes[0];
                    Ast.TableConstructor t = new Ast.TableConstructor();
                    t.Values = new Dictionary<Ast.IExpression, Ast.IExpression>();
                    int i = 1;
                    if (child.ChildNodes.Count > 0)
                    {
                        while (true)
                        {
                            ParseTreeNode indexNode = child.ChildNodes[0];
                            ParseTreeNode valueNode = child.ChildNodes[1];
                            ParseTreeNode nextNode = child.ChildNodes[2];

                            Ast.IExpression index;
                            if (indexNode.ChildNodes.Count == 0)
                            {
                                index = new Ast.NumberLiteral() { Value = i };
                                i++;
                            }
                            else if (indexNode.ChildNodes[0].Term.Name == "identifier")
                            {
                                index = new Ast.StringLiteral() { Value = indexNode.ChildNodes[0].Token.ValueString };
                            }
                            else
                            {
                                index = ParseExpression(indexNode.ChildNodes[0]);
                            }

                            Ast.IExpression value = ParseExpression(valueNode);
                            t.Values.Add(index, value);

                            child = nextNode;
                            if (child.ChildNodes.Count == 0)
                                break;
                            else
                                child = child.ChildNodes[0];
                        }
                    }
                    return t;
                }
            }
            throw new Exception("Invalid TableConstruct node");
        }

        Ast.IExpression ParseExpression(ParseTreeNode node)
        {
            if (node.Term.Name == "Expression")
            {
                ParseTreeNode child = node.ChildNodes[0];
                if (child.Token != null && child.Token.Terminal is NumberLiteral)
                {
                    return new Ast.NumberLiteral() { Value = (child.Token.Value is double ? (double)(child.Token.Value) : (int)(child.Token.Value)) };
                }
                else if (child.Token != null && child.Token.Terminal is StringLiteral)
                {
                    return new Ast.StringLiteral() { Value = (string)(child.Token.Value) };
                }
                else if (child.Token != null && child.Token.Terminal is KeyTerm)
                {
                    string val = child.Token.ValueString;
                    if (val == "true")
                        return new Ast.BoolLiteral() { Value = true };
                    else if (val == "false")
                        return new Ast.BoolLiteral() { Value = false };
                    else if (val == "nil")
                        return new Ast.NilLiteral();
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
                else if (child.Term != null && child.Term.Name == "UnaryExpr")
                {
                    return ParseUnaryExpr(child);
                }
                else if (child.Term != null && child.Term.Name == "TableConstruct")
                {
                    return ParseTableConstruct(child);
                }
                else if (child.Term != null && child.Term.Name == "OopCall")
                {
                    return ParseOopCall(child);
                }
            }
            throw new Exception("Invalid Expression node");
        }
        #endregion

        Ast.Block ParseBlock(ParseTreeNode node)
        {
            Ast.Block block = new Ast.Block();
            block.Statements = new List<Ast.IStatement>();
            foreach (ParseTreeNode child in node.ChildNodes)
            {
                switch (child.Term.Name)
                {
                    case "Assignment":
                        block.Statements.Add(ParseAssign(child)); break;
                    case "LocalAssignment":
                        block.Statements.Add(ParseLocalAssign(child)); break;
                    case "FunctionCall":
                        block.Statements.Add(ParseFunctionCall(child)); break;
                    case "ReturnStat":
                        block.Statements.Add(ParseReturnStat(child)); break;
                    case "BreakStat":
                        block.Statements.Add(new Ast.Break()); break;
                    case "DoBlock":
                        block.Statements.Add(ParseDoBlock(child)); break;
                    case "If":
                        block.Statements.Add(ParseIf(child)); break;
                    case "While":
                        block.Statements.Add(ParseWhile(child)); break;
                    case "Repeat":
                        block.Statements.Add(ParseRepeat(child)); break;
                    case "FunctionDecl":
                        block.Statements.Add(ParseFunctionDecl(child)); break;
                    case "For":
                        block.Statements.Add(ParseFor(child)); break;
                    case "OopCall":
                        block.Statements.Add(ParseOopCall(child)); break;
                    default:
                        throw new NotImplementedException("Node not yet implemented");
                }
            }
            return block;
        }

        Ast.IStatement ParseFor(ParseTreeNode node)
        {
            if (node.Term.Name == "For")
            {
                var block = ParseDoBlock(node.ChildNodes[2]);
                var type = node.ChildNodes[1].ChildNodes[0];
                if (type.Term.Name == "NumericFor")
                {
                    var cycle = new Ast.NumericFor();
                    cycle.Block = block;
                    cycle.Variable = type.ChildNodes[0].Token.ValueString;
                    cycle.Var = ParseExpression(type.ChildNodes[1]);
                    cycle.Limit = ParseExpression(type.ChildNodes[2]);
                    cycle.Step = new Ast.NumberLiteral() { Value = 1 };
                    if (type.ChildNodes[3].ChildNodes.Count > 0)
                    {
                        var child = type.ChildNodes[3].ChildNodes[0];
                        cycle.Step = ParseExpression(child);
                    }

                    return cycle;
                }
                else
                {
                    var cycle = new Ast.GenericFor();
                    cycle.Block = block;

                    var nameList = type.ChildNodes[0];
                    var exprList = type.ChildNodes[2];

                    while (true)
                    {
                        var name = nameList.ChildNodes[0].Token.ValueString;
                        cycle.Variables.Add(name);
                        var child = nameList.ChildNodes[1];
                        if (child.ChildNodes.Count > 0)
                            nameList = child.ChildNodes[0];
                        else
                            break;
                    }

                    while (true)
                    {
                        var expr = ParseExpression(exprList.ChildNodes[0]);
                        cycle.Expressions.Add(expr);
                        var child = exprList.ChildNodes[1];
                        if (child.ChildNodes.Count > 0)
                            exprList = child.ChildNodes[0];
                        else
                            break;
                    }

                    return cycle;
                }
            }
            throw new Exception("Invalid For node");
        }
    }
}
