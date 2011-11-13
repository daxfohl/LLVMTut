using System.Collections.Generic;

namespace ConsoleApplication76 {
    public struct Lexed {
        public string Identifier;
        public double Num;
        public Token Token;

        public override string ToString() {
            return Token.ToString();
        }
    }

    static class Lexer {
        public static IEnumerable<Lexed> Lex(string s) {
            s += " ";
            for (var i = 0; i < s.Length;) {
                if (char.IsWhiteSpace(s[i])) {
                    ++i;
                } else if (char.IsLetter(s[i])) {
                    var start = i;
                    while (char.IsLetterOrDigit(s[++i])) {}
                    var str = s.Substring(start, i - start);
                    var tok = str == "def" ? Token.tok_def
                                  : str == "extern" ? Token.tok_extern
                                        : Token.tok_identifier;
                    yield return new Lexed {Token = tok, Identifier = str};
                } else if (char.IsDigit(s[i]) || s[i] == '.') {
                    var start = i;
                    while (char.IsDigit(s[++i]) || s[i] == '.') {}
                    var str = s.Substring(start, i - start);
                    yield return new Lexed {Token = Token.tok_number, Num = double.Parse(str)};
                    continue;
                } else if (s[i] == '#') {
                    while (s[++i] != '\n' && s[i] != '\r') {}
                } else {
                    yield return new Lexed {Token = (Token)s[i]};
                    ++i;
                }
            }
            yield return new Lexed {Token = Token.tok_eof};
        }
    }

    public enum Token {
        tok_eof = -1,

        // commands
        tok_def = -2,
        tok_extern = -3,

        // primary
        tok_identifier = -4,
        tok_number = -5
    } ;
}