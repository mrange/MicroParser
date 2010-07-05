using System;
using MicroParser;

namespace TestParser
{

   public interface IAstNode
   {
   }

   public sealed class AstNode_IntValue : IAstNode
   {
      public int Value;
      public override string ToString ()
      {
         return new 
         {
            NodeType = "IntValue",
            Value,
         }.ToString ();
      }
   }

   public sealed class AstNode_StringValue : IAstNode
   {
      public string Value;
      public override string ToString ()
      {
         return new
         {
            NodeType = "StringValue",
            Value,
         }.ToString ();
      }
   }

   public sealed class AstNode_Variable : IAstNode
   {
      public string Name;
      public override string ToString ()
      {
         return new
         {
            NodeType = "Variable",
            Name,
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
            .Map (i => new AstNode_StringValue { Value = i } as IAstNode);

         var p_int_value = CharParser
            .ParseInt ()
            .KeepLeft (p_spaces)
            .Map (i => new AstNode_IntValue { Value = i } as IAstNode);

         var p_variable = CharParser
            .ManyCharSatisfy2 (
               CharParser.SatisyLetter,
               CharParser.SatisyLetterOrDigit,
               1)
            .KeepLeft (p_spaces)
            .Map (s => new AstNode_Variable { Name = s } as IAstNode);


         var p_term = Parser.Choice (p_string_value, p_variable, p_int_value);

         var p = Parser
            .Tuple2 (p_variable, p_token ("=").KeepRight (p_term))
            .KeepLeft (p_eos)
            ;
         // ReSharper restore InconsistentNaming

         //const string text = "z0Test = \"Test\"  ";
         //const string text = "z0Test = 9  ";
         const string text = "z0Test = var  ";

         {
            var ps = ParserState.Create (0, text);

            var pr = p (ps);

            Console.WriteLine (pr);
         }

         var then = DateTime.Now;

         for (var iter = 0; iter < 1000000; ++iter)
         {
            var ps = ParserState.Create (0, text);
            var pr = p (ps);
         }

         Console.WriteLine ((DateTime.Now - then).TotalMilliseconds);

         Console.ReadKey ();

      }
   }
}
