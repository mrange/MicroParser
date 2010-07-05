using System;
using MicroParser;

namespace TestParser
{

   public interface IAstNode
   {
   }

   public sealed class AstNode_Value : IAstNode
   {
      public int Value;
      public override string ToString()
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
      public string Name;
      public override string ToString()
      {
         return new
         {
            NodeType = "Variable",
            Name,
         }.ToString();
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         // ReSharper disable InconsistentNaming

         var p_eos = Parser.EndOfStream ();

         var p_spaces = CharParser.SkipWhiteSpace();

         Func<string, ParserFunction<Empty>> p_token = token => CharParser.SkipString(token).KeepLeft(p_spaces);

         var p_value = CharParser
            .ParseInt()
            .KeepLeft(p_spaces)
            .Map(i => new AstNode_Value { Value = i } as IAstNode);

         var p_variable = CharParser
            .ManyCharSatisfy2(
               CharParser.SatisyLetter,
               CharParser.SatisyLetterOrDigit,
               1)
            .KeepLeft(p_spaces)
            .Map(s => new AstNode_Variable { Name = s } as IAstNode);


         var p_term = Parser.Choice (p_variable, p_value);

         var p = Parser
            .Tuple2(p_variable, p_token ("=").KeepRight(p_term))
            .KeepLeft(p_eos)
            ;
         // ReSharper restore InconsistentNaming

         const string text = "z0Test = 9  ";

         {
            var ps = ParserState.Create(0, text);

            var pr = p(ps);

            Console.WriteLine(pr);
         }

         var then = DateTime.Now;

         for (var iter = 0; iter < 1000000; ++iter)
         {
            var ps = ParserState.Create(0, text);
            var pr = p(ps);
         }

         Console.WriteLine((DateTime.Now - then).TotalMilliseconds);

         Console.ReadKey();

      }
   }
}
