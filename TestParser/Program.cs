// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------
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
      public double Value;
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

         var p_value = CharParser
            .ParseDouble ()
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

         var p_addOp = CharParser.AnyOf ("+-").KeepLeft (p_spaces);
         var p_mulOp = CharParser.AnyOf ("*/").KeepLeft (p_spaces);
         var p_maxOp = CharParser.AnyOf ("!?").KeepLeft (p_spaces);

         var p_ast_redirect = Parser.Redirect<IAstNode> ();

         var p_ast = p_ast_redirect.Function;

         var p_term = Parser.Choice (
            p_ast.Between (p_token ("(").KeepLeft (p_spaces), p_token (")").KeepLeft (p_spaces)),
            p_variable, 
            p_value
            );

         Func<IAstNode, char, IAstNode, IAstNode> makeBinOp = (l, op, r) => new AstNode_Binary {Left = l, Op = op, Right = r};

         var p_level0 = p_term.Chain (p_mulOp, makeBinOp);
         var p_level1 = p_level0.Chain (p_addOp, makeBinOp);
         var p_level2 = p_level1.Chain (p_maxOp, makeBinOp);

         p_ast_redirect.Redirect = p_level2;

         var p = p_ast.KeepLeft (p_eos);
         // ReSharper restore InconsistentNaming

         const string text = "x.y*3 + y.z.e ! -200.032";

         {
            var pr = Parser.Parse (p, text);
            Console.WriteLine (pr);
         }

         var then = DateTime.Now;

         for (var iter = 0; iter < 1000000; ++iter)
         {
            var pr = Parser.Parse (p, text);
         }

         Console.WriteLine ((DateTime.Now - then).TotalMilliseconds);

         Console.ReadKey ();

      }
   }
}
