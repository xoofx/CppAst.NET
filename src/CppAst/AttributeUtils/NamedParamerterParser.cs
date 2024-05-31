using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Resources;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Irony.Parsing;

namespace CppAst
{

    public class NamedParameterParser
    {
        #region Embeded Types
        public static class TerminalNames
        {
            public const string Identifier = "identifier",
            Number = "number",
            String = "string",
            Boolean = "boolean",
            Comma = ",",
            Equal = "=",
            Expression = "expression",
            Assignment = "assignment",
            LoopPair = "loop_pair",
            NamedArguments = "named_arguments",
            Args = "args",
            Class = "class",
            ClassName = "class_name",
            NameSpace = "namespace",
            Template = "template",
            TemplateElem = "template_elem",
            LeftBracket = "left_bracket",
            RightBracket = "right_bracket";
        }

        [Language("NamedParameter.CppAst", "0.1", "Grammer for named parameter")]
        public class NamedParameterGrammer : Irony.Parsing.Grammar
        {
            static NamedParameterGrammer sGrammer;
            public static NamedParameterGrammer Instance
            {
                get
                {
                    if (sGrammer == null)
                    {
                        sGrammer = new NamedParameterGrammer();
                    }
                    return sGrammer;
                }
            }

            private NamedParameterGrammer() :
                base(true)
            {
                #region Declare Terminals Here
                NumberLiteral NUMBER = CreateNumberLiteral(TerminalNames.Number);
                StringLiteral STRING_LITERAL = new StringLiteral(TerminalNames.String, "\"", StringOptions.AllowsAllEscapes);
                IdentifierTerminal Name = new IdentifierTerminal(TerminalNames.Identifier);


                //  Regular Operators
                var COMMA = ToTerm(TerminalNames.Comma);
                var EQUAL = ToTerm(TerminalNames.Equal);

                #region Keywords
                var TRUE_KEYWORD = Keyword("true");
                var FALSE_KEYWORD = Keyword("false");

               

                var CLASS_KEYWORD = Keyword("__class");
                //var NEW = Keyword("new");
                #endregion

                #endregion
                #region Declare NonTerminals Here
                NonTerminal BOOLEAN = new NonTerminal(TerminalNames.Boolean);
                NonTerminal EXPRESSION = new NonTerminal(TerminalNames.Expression);
                NonTerminal ASSIGNMENT = new NonTerminal(TerminalNames.Assignment);
                NonTerminal NAMED_ARGUMENTS = new NonTerminal(TerminalNames.NamedArguments);
                NonTerminal LOOP_PAIR = new NonTerminal(TerminalNames.LoopPair);
                NonTerminal ARGS = new NonTerminal(TerminalNames.Args);
                NonTerminal CLASS_NAME = new NonTerminal(TerminalNames.ClassName);
                NonTerminal NAMESPACE = new NonTerminal(TerminalNames.NameSpace);
                NonTerminal TEMPLATE = new NonTerminal(TerminalNames.Template);
                NonTerminal TEMPLATE_ELEM = new NonTerminal(TerminalNames.TemplateElem);
                NonTerminal CLASS = new NonTerminal(TerminalNames.Class);
                NonTerminal LEFT_BRACKET = new NonTerminal(TerminalNames.LeftBracket);
                NonTerminal RIGHT_BRACKET = new NonTerminal(TerminalNames.RightBracket);
                #endregion

                #region Place Rules Here
                ////NORMAL_RECORD.Rule = Name + FIELD_FETCH;

                BOOLEAN.Rule = TRUE_KEYWORD | FALSE_KEYWORD;
                LEFT_BRACKET.Rule = ToTerm("(") | ToTerm("{");
                RIGHT_BRACKET.Rule = ToTerm(")") | ToTerm("}");
                
                NAMESPACE.Rule = MakePlusRule(NAMESPACE, ToTerm("::"), Name);
                TEMPLATE_ELEM.Rule = MakeStarRule(ARGS, ToTerm(","), Name | Empty);
                TEMPLATE.Rule = ToTerm("<") + TEMPLATE_ELEM + ToTerm(">");
                CLASS_NAME.Rule = NAMESPACE + TEMPLATE | Name + TEMPLATE | NAMESPACE | Name;
                ARGS.Rule = MakeStarRule(ARGS, ToTerm(","), EXPRESSION | Empty);
                CLASS.Rule = CLASS_KEYWORD + LEFT_BRACKET + CLASS_NAME + LEFT_BRACKET + ARGS + RIGHT_BRACKET + RIGHT_BRACKET;
                
                EXPRESSION.Rule = BOOLEAN | NUMBER | CLASS | STRING_LITERAL;
                ASSIGNMENT.Rule = Name | Name + EQUAL + EXPRESSION;
                LOOP_PAIR.Rule = MakeStarRule(COMMA + ASSIGNMENT);
                NAMED_ARGUMENTS.Rule = ASSIGNMENT + LOOP_PAIR;

                this.Root = NAMED_ARGUMENTS;
                #endregion

                #region Define Keywords and Register Symbols
                ////this.RegisterBracePair("[", "]");

                ////this.MarkPunctuation(",", ";");
                #endregion
            }

            //Must create new overrides here in order to support the "Operator" token color
            public new void RegisterOperators(int precedence, params string[] opSymbols)
            {
                RegisterOperators(precedence, Associativity.Left, opSymbols);
            }

            //Must create new overrides here in order to support the "Operator" token color
            public new void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols)
            {
                foreach (string op in opSymbols)
                {
                    KeyTerm opSymbol = Operator(op);
                    opSymbol.Precedence = precedence;
                    opSymbol.Associativity = associativity;
                }
            }

            BnfExpression MakeStarRule(BnfTerm term)
            {
                return MakeStarRule(new NonTerminal(term.Name + "*"), term);
            }

            public KeyTerm Keyword(string keyword)
            {
                var term = ToTerm(keyword);
                // term.SetOption(TermOptions.IsKeyword, true);
                // term.SetOption(TermOptions.IsReservedWord, true);

                this.MarkReservedWords(keyword);
                term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);

                return term;
            }

            public KeyTerm Operator(string op)
            {
                string opCased = this.CaseSensitive ? op : op.ToLower();
                var term = new KeyTerm(opCased, op);
                //term.SetOption(TermOptions.IsOperator, true);

                term.EditorInfo = new TokenEditorInfo(TokenType.Operator, TokenColor.Keyword, TokenTriggers.None);

                return term;
            }

            protected static NumberLiteral CreateNumberLiteral(string name)
            {

                NumberLiteral term = new NumberLiteral(name);
                //default int types are Integer (32bit) -> LongInteger (BigInt); Try Int64 before BigInt: Better performance?
                term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32 };
                term.DefaultFloatType = TypeCode.Double; // it is default
                                                         ////term.AddPrefix("0x", NumberOptions.Hex);

                return term;
            }
        }

        #endregion


        public static bool ParseNamedParameters(string content, Dictionary<string, object> outNamedParameterDic, out string errorMessage)
        {
            errorMessage = "";

            if(string.IsNullOrWhiteSpace(content))
            {
                return true;
            }

            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(NamedParameterGrammer.Instance);
            var ast = parser.Parse(content);

            if (!ast.HasErrors())
            {
                ParseAssignment(ast.Root.ChildNodes[0], outNamedParameterDic);

                if(ast.Root.ChildNodes.Count >= 2 && ast.Root.ChildNodes[1].ChildNodes.Count > 0)
                {
                    ParseLoopItem(ast.Root.ChildNodes[1].ChildNodes[0], outNamedParameterDic);
                }

                return true;
            }
            else
            {
                errorMessage = ast.ParserMessages.ToString();
            }

            return false;
        }


        private static object ParseExpressionValue(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case TerminalNames.String:
                    return node.Token.ValueString;
                case TerminalNames.Boolean:
                    if(node.ChildNodes[0].Term.Name == "false")
                    {
                        return false;
                    }
                    return true;
                case TerminalNames.Number:
                    return node.Token.Value;
                case TerminalNames.Template:
                case TerminalNames.ClassName:
                    return ParseNodeChildren(node);
                case TerminalNames.Class:
                    return ParseClassToken(node.ChildNodes);
                case TerminalNames.Args:
                    return ParseClassArgs(node.ChildNodes);
                case TerminalNames.NameSpace:
                    return ParseNodeListWithSeparator(node.ChildNodes, "::");
                case TerminalNames.TemplateElem:
                    return ParseNodeListWithSeparator(node.ChildNodes, ",");
                case TerminalNames.LeftBracket:
                case TerminalNames.RightBracket:
                    return node.ChildNodes[0].Token.Value;
                default:
                    if (node.ChildNodes.Count == 0 && node.Token != null)
                    {
                        return node.Token.Value;
                    }
                    else if (node.ChildNodes.Count > 1)
                    {
                        throw new Exception("Can not run to here!");
                    }
                    
                    return ParseExpressionValue(node.ChildNodes[0]);
            }
        }

        private static void ParseAssignment(ParseTreeNode node, Dictionary<string, object> outNamedParameterDic)
        {
            string varName = node.ChildNodes[0].Token.ValueString;
            if(!outNamedParameterDic.ContainsKey(varName))
            {
                if (node.ChildNodes.Count == 1)
                {
                    outNamedParameterDic.Add(varName, true);
                }
                else
                {
                    outNamedParameterDic.Add(varName, ParseExpressionValue(node.ChildNodes[2].ChildNodes[0]));
                }
            }
        }

        private static void ParseLoopItem(Irony.Parsing.ParseTreeNode loopNode, Dictionary<string, object> outNamedParameterDic)
        {
            ParseAssignment(loopNode.ChildNodes[1], outNamedParameterDic);

            for (int i = 2; i < loopNode.ChildNodes.Count; i++)
            {
                ParseAssignment(loopNode.ChildNodes[i], outNamedParameterDic);
            }
        }

        private static string ParseNodeListWithSeparator(ParseTreeNodeList nodeList, string sep)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var node in nodeList)
            {
                if (builder.Length > 0)
                {
                    builder.Append(sep);
                }
                        
                builder.Append(ParseExpressionValue(node));
            }

            return builder.ToString();
        }
        
        private static string ParseNodeChildren(ParseTreeNode node)
        {
            StringBuilder builder = new StringBuilder();
            if (node.ChildNodes != null)
            {
                foreach (var child in node.ChildNodes)
                {
                    builder.Append(ParseExpressionValue(child));
                }
            }
            
            return builder.ToString();
        }
        
        private static string ParseClassArgs(ParseTreeNodeList nodeList)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var node in nodeList)
            {
                var nodeValue = ParseExpressionValue(node);
                object realValue;
                if (nodeValue is string)
                {
                    realValue = "\"" + nodeValue + "\"";
                } 
                else if (nodeValue is bool)
                {
                    var str = nodeValue.ToString();
                    realValue = str == "True" ? "true" : "false";
                }
                else
                {
                    realValue = nodeValue.ToString();
                }

                if (builder.Length > 0)
                {
                    builder.Append(",");
                }
                
                builder.Append(realValue);
            }
            
            return builder.ToString();
        }
        
        private static StringBuilder ParseClassToken(ParseTreeNodeList nodeList)
        {
            if (nodeList.Count == 0)
            {
                return null;
            }
            
            StringBuilder builder = new StringBuilder();
            for (int i = 2; i < nodeList.Count-1; i++)
            {
                builder.Append(ParseExpressionValue(nodeList[i]));
            }

            return builder;
        }
    }
}
