using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication76 {
    public interface IExprAST { }
    public class NumberExprAST : IExprAST {
        readonly double _val;
        public NumberExprAST(double val) {
            _val = val;
        }

        public double Val {
            get { return _val; }
        }
    }
    public class VariableExprAST : IExprAST {
        readonly string _name;
        public VariableExprAST(string name) {
            _name = name;
        }
    }
    public class BinaryExprAST : IExprAST {
        readonly char _op;
        readonly IExprAST _rhs, _lhs;
        public BinaryExprAST(char op, IExprAST rhs, IExprAST lhs) {
            _op = op;
            _lhs = lhs;
            _rhs = rhs;
        }
    }
    public class CallExprAST : IExprAST {
        readonly string _callee;
        readonly IExprAST[] _args;
        public CallExprAST(string callee, IExprAST[] args) {
            _callee = callee;
            _args = args;
        }
    }
    public class PrototypeAST {
        readonly string _name;
        readonly string[] _args;
        public PrototypeAST(string name, string[] args) {
            _name = name;
            _args = args;
        }
    }
    public class FunctionAST {
        readonly PrototypeAST _proto;
        readonly IExprAST _body;
        public FunctionAST(PrototypeAST proto, IExprAST body) {
            _proto = proto;
            _body = body;
        }
    }
    public static class Parser {
        public static readonly IDictionary<char, int> BinopPrecedence = new Dictionary<char, int> { { '<', 10 }, { '+', 20 }, { '-', 20 }, { '*', 40 } };
        public static int GetTokPrecedence(Token token) {
            try {
                var prec = BinopPrecedence[(char)token];
                return prec <= 0 ? -1 : prec;
            } catch {
                return -1;
            }
        }

        public static IExprAST ParseNumberExpr(Queue<Lexed> tokens) {
            return new NumberExprAST(tokens.Dequeue().Num);
        }
        public static IExprAST ParseParenExpr(Queue<Lexed> tokens) {
            tokens.Dequeue();
            var v = ParseExpression(tokens);
            if ((char)tokens.Dequeue().Token != ')') {
                throw new Exception("expected ')'");
            }
            return v;
        }

        public static IExprAST ParseExpression(Queue<Lexed> tokens) {
            var lhs = ParsePrimary(tokens);
            return lhs == null ? null : ParseBinOpRHS(0, lhs, tokens);
        }

        public static IExprAST ParseBinOpRHS(int exprPrec, IExprAST lhs, Queue<Lexed> tokens) {
            while (true) {
                var tokPrec = GetTokPrecedence(tokens.Peek().Token);
                if (tokPrec < exprPrec) {
                    return lhs;
                }
                var binOp = (char)tokens.Dequeue().Token;
                var rhs = ParsePrimary(tokens);
                if (rhs == null)
                    return null;
                var nextPrec = GetTokPrecedence(tokens.Peek().Token);
                if (tokPrec < nextPrec) {
                    rhs = ParseBinOpRHS(tokPrec + 1, rhs, tokens);
                    if (rhs == null)
                        return null;
                }
                lhs = new BinaryExprAST(binOp, lhs, rhs);
            }
        }

        public static IExprAST ParseIdentifierExpr(Queue<Lexed> tokens) {
            var idName = tokens.Dequeue().Identifier;
            if ((char)tokens.Peek().Token != '(') {
                return new VariableExprAST(idName);
            }
            tokens.Dequeue();
            var args = new List<IExprAST>();
            if ((char)tokens.Peek().Token != ')') {
                while (true) {
                    var arg = ParseExpression(tokens);
                    if (arg == null)
                        return null;
                    args.Add(arg);
                    if ((char)tokens.Peek().Token == ')')
                        break;
                    if ((char)tokens.Peek().Token != ',')
                        throw new Exception("Expected ')' or ',' in argument list");
                    tokens.Dequeue();
                }
            }
            tokens.Dequeue();
            return new CallExprAST(idName, args.ToArray());
        }
        public static IExprAST ParsePrimary(Queue<Lexed> tokens) {
            switch (tokens.Peek().Token) {
                case Token.tok_identifier:
                    return ParseIdentifierExpr(tokens);
                case Token.tok_number:
                    return ParseNumberExpr(tokens);
                case (Token)'(':
                    return ParseParenExpr(tokens);
                default:
                    throw new Exception("Unknown token when expecting an expression");
            }
        }
        public static PrototypeAST ParsePrototype(Queue<Lexed> tokens) {
            if (tokens.Peek().Token != Token.tok_identifier) {
                throw new Exception("Expected function name in prototype");
            }
            var fnName = tokens.Dequeue().Identifier;
            if ((char)tokens.Dequeue().Token != '(') {
                throw new Exception("Expected '(' in prototype");
            }
            var argNames = new List<string>();
            while (tokens.Peek().Token == Token.tok_identifier) {
                argNames.Add(tokens.Dequeue().Identifier);
            }
            if ((char)tokens.Dequeue().Token != ')') {
                throw new Exception("Expected ')' in prototype");
            }
            return new PrototypeAST(fnName, argNames.ToArray());
        }
        public static FunctionAST ParseDefinition(Queue<Lexed> tokens) {
            tokens.Dequeue();
            var proto = ParsePrototype(tokens);
            if (proto == null)
                return null;
            var e = ParseExpression(tokens);
            return e != null ? new FunctionAST(proto, e) : null;
        }
        public static PrototypeAST ParseExtern(Queue<Lexed> tokens) {
            tokens.Dequeue();
            return ParsePrototype(tokens);
        }
        public static FunctionAST ParseTopLevelExpr(Queue<Lexed> tokens) {
            var e = ParseExpression(tokens);
            if (e != null) {
                var proto = new PrototypeAST("", new string[] { });
                return new FunctionAST(proto, e);
            }
            return null;
        }
    }
}
