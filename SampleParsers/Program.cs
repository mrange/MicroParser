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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MicroParser;

// ReSharper disable InconsistentNaming

namespace SampleParsers
{
   static class Program
   {
      static void Main (string[] args)
      {
         Sample1 ();

         Sample2 ();

         Console.ReadKey ();
      }

      static Program ()
      {
         // Sample1

         {
            // Int () is a builtin parser for ints
            var p_int = CharParser.Int ();

            var p_identifier = CharParser
               .ManyCharSatisfy2 (              // Creates a string parser
               CharSatisfy.Letter,              // A test function applied to the 
                                                // first character
               CharSatisfy.LetterOrDigit,       // A test function applied to the
                                                // rest of the characters
               minCount: 1                      // We require the identifier to be 
                                                // at least 1 character long
               );

            var p_spaces = CharParser.SkipWhiteSpace ();
            var p_assignment = CharParser.SkipChar ('=');

            s_parserSample1 = Parser.Group (
               p_identifier.KeepLeft (p_spaces),
               p_assignment.KeepRight (p_spaces).KeepRight (p_int));

         }

         // Sample2

         {
            // Define a parameter expression that represent a dictionary, this dictionary
            // will contain the variable values
            var inputParameter = Expression.Parameter (
               typeof (IDictionary<string, double>),
               "input"
               );

            Func<string, Parser<Empty>> p_str = CharParser.SkipString;

            var p_spaces = CharParser.SkipWhiteSpace ();
            // Parse a double and map it into a ConstantExpression
            var p_value = CharParser.Double ().Map (d => (Expression) Expression.Constant (d));
            var p_variable = CharParser
               .ManyCharSatisfy2 (
                  CharSatisfy.Letter,
                  CharSatisfy.LetterOrDigit,
                  minCount: 1
               )
               .Map (identifier => (Expression) Expression.Call (
                  null,
                  s_findVariableValueSample2,
                  inputParameter,
                  Expression.Constant (identifier.ToString ()))
               );

            var p_astRedirect = Parser.Redirect<Expression> ();

            // p_ast is the complete parser (AST = Abstract Syntax Tree)
            var p_ast = p_astRedirect.Parser;

            var p_term = Parser.Choice (
               p_ast.Between (p_str ("(").KeepLeft (p_spaces), p_str (")")),
               p_value,
               p_variable
               ).KeepLeft (p_spaces);

            // p_level is a support parser generator
            // it accepts a parser it will apply on the input separated by the operators
            // in the ops parameter
            Func<Parser<Expression>, string, Parser<Expression>> p_level =
               (parser, ops) => parser.Chain (
                  CharParser.AnyOf (ops, minCount:1, maxCount:1).KeepLeft (p_spaces),
                  (left, op, right) => 
                     Expression.MakeBinary (OperatorToExpressionType (op), left, right)
                  );

            // By splitting */ and +- like this we ensure */ binds _harder_
            var p_lvl0 = p_level (p_term, "*/");
            var p_lvl1 = p_level (p_lvl0, "+-");

            // This completes the parser
            p_astRedirect.ParserRedirect = p_lvl1;

            s_parameterSample2 = inputParameter;
            s_parserSample2 = p_ast;
         }
      }

      // Sample1

      static readonly Parser<MicroParser.Tuple<SubString, int>> s_parserSample1;

      static void Sample1 ()
      {

         var result = Parser.Parse (
            s_parserSample1,
            "AnIdentifier = 3"
            );

         if (result.IsSuccessful)
         {
            Console.WriteLine (
               "{0} = {1}", 
               result.Value.Item1, 
               result.Value.Item2
               );
         }
         else
         {
            Console.WriteLine (
               result.ErrorMessage 
               );
         }
      }


      // Sample2

      static readonly ParameterExpression s_parameterSample2;
      static readonly Parser<Expression> s_parserSample2;
      static readonly MethodInfo s_findVariableValueSample2 = GetMethodInfo (() => FindVariableValue (null, null));

      static MethodInfo GetMethodInfo (Expression<Action> expression)
      {
         return ((MethodCallExpression)expression.Body).Method;
      }

      static double FindVariableValue (IDictionary<string, double> input, string name)
      {
         double value;
         return input.TryGetValue (name ?? "", out value) ? value : 0.0;
      }


      static ExpressionType OperatorToExpressionType (SubString op)
      {
         switch (op[0])
         {
            case '+':
               return ExpressionType.Add;
            case '-':
               return ExpressionType.Subtract;
            case '*':
               return ExpressionType.Multiply;
            case '/':
               return ExpressionType.Divide;
            default:
               throw new ArgumentException ();
         }
      }

      static void Sample2 ()
      {

         var input = new Dictionary<string, double>
                        {
                           {"x", 1.0},
                           {"y", 2.0},
                        };

         var expression = "2*(x + 1) + y + 3";

         var result = Parser.Parse (s_parserSample2, expression);

         if (result.IsSuccessful)
         {
            var lambda = Expression.Lambda<Func<IDictionary<string, double>, double>> (
               result.Value,
               s_parameterSample2
               );

            var del = lambda.Compile ();

            Console.WriteLine ("{0} = {1}", expression, del (input));
            foreach (var kv in input)
            {
               Console.WriteLine ("{0} = {1}", kv.Key, kv.Value);
            }
         }
         else
         {
            Console.WriteLine (
               result.ErrorMessage
               );
         }


      }

   }
}
