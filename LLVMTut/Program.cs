using System;
using System.Collections.Generic;
using System.Data;

namespace ConsoleApplication76 {
    public static class Program {
        public static void Main() {
            var lexeds = Lexer.Lex(");");
            foreach (var lexed in lexeds) {
                Console.WriteLine(lexed);
            }
            Parser.ParseDefinition(new Queue<Lexed>(lexeds));
        }
    }

}