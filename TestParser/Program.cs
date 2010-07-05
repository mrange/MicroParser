using System;
using MicroParser;

namespace TestParser
{

   public interface IAstNode
   {
   }

   public sealed class AstNode_Binary : IAstNode
   {
      public IAstNode Left;
      public char Op;
      public IAstNode Right;

      public override string ToString ()
      {
         return new
         {
            NodeType = "Binary",
            Left,
            Op,
            Right,
         }.ToString ();
      }
   }

   public sealed class AstNode_Value : IAstNode
   {
      public object Value;
      public override string ToString ()
      {
         return new 
         {
            NodeType = "Value",
            Value,
         }.ToString ();
      }
   }

   public sealed class AstNode_Variable : IAstNode
   {
      public string Root;
      public string[] Names;

      public override string ToString ()
      {
         return new
                   {
                      NodeType = "Variable",
                      Root,
                      Names = Names.Concatenate (", ", "[", "]"),
                   }.ToString ();
      }
   }

   class Program
   {
      static void Main (string[] args)
      {
         // ReSharper disable InconsistentNaming

         var p_eos = Parser.EndOfStream ();

         var p_spaces = CharParser.SkipWhiteSpace ();

         Func<string, ParserFunction<Empty>> p_token = token => CharParser.SkipString (token).KeepLeft (p_spaces);



         var p_string_value = CharParser
            .ManyCharSatisfy (CharParser.SatisyAnyChar.Except ('"'))
            .Between (p_token ("\""), p_token ("\""))
            .KeepLeft (p_spaces)
            .Map (i => new AstNode_Value { Value = i } as IAstNode);

         var p_int_value = CharParser
            .ParseInt ()
            .KeepLeft (p_spaces)
            .Map (i => new AstNode_Value { Value = i } as IAstNode);

         var p_identifier = CharParser
            .ManyCharSatisfy2 (
               CharParser.SatisyLetter,
               CharParser.SatisyLetterOrDigit,
               1);

         var p_variable = 
            Parser.Tuple (
               p_identifier,
               p_token (".").KeepRight (p_identifier).Many ())
            .KeepLeft (p_spaces)
            .Map (tuple => new AstNode_Variable { Root = tuple.Item1, Names = tuple.Item2} as IAstNode);

         var allOps = new CharSatify (
            "op",
            (c, i) =>
               {
                  switch (c)
                  {
                     case '+':
                     case '-':
                     case '*':
                     case '/':
                     case '!':   // MAX
                     case '?':   // MIN
                        return true;
                     default:
                        return false;
                  }
               });

         var p_op = CharParser.ManyCharSatisfy (allOps, 1, 1).KeepLeft (p_spaces);

         var p_ast_redirect = Parser.Redirect<IAstNode> ();

         var p_ast = p_ast_redirect.Function;

         var p_term = Parser.Choice (
            p_ast.Between (p_token ("(").KeepLeft (p_spaces), p_token (")").KeepLeft (p_spaces)),
            p_string_value, 
            p_variable, 
            p_int_value
            );

         p_ast_redirect.Redirect = p_term.Chain (p_op, (l, op, r) => new AstNode_Binary {Left = l, Op = op[0], Right = r} as IAstNode);

            ;
         // ReSharper restore InconsistentNaming

         const string text = "x.y+(3 * y.z.e) ";

         {
            var ps = ParserState.Create (0, text);

            var pr = p_ast (ps);

            Console.WriteLine (pr);
         }

         var then = DateTime.Now;

         for (var iter = 0; iter < 1000000; ++iter)
         {
            var ps = ParserState.Create (0, text);
            var pr = p_ast (ps);
         }

         Console.WriteLine ((DateTime.Now - then).TotalMilliseconds);

         Console.ReadKey ();

      }
   }
}
