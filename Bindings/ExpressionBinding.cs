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
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using Bindings.Internal;
using MicroParser;

#pragma warning disable 659

namespace Bindings
{
   using SLE = System.Linq.Expressions;

   public sealed class ExpressionBinding : MarkupExtension
   {
      static readonly ParserFunction<IAst> s_parser;

      static readonly IDictionary<string, Func<IServiceProvider, object>> s_expressionCache = 
         new Dictionary<string, Func<IServiceProvider, object>> ();

      static ExpressionBinding ()
      {
         s_expressionCache.Add ("", svcPrv => 0.0);

         // ReSharper disable InconsistentNaming
         var p_eos = Parser.EndOfStream ();

         var p_spaces = CharParser.SkipWhiteSpace ();

         Func<string, ParserFunction<Empty>> p_token = token => CharParser.SkipString (token).KeepLeft (p_spaces);

         var p_value = CharParser
            .ParseDouble ()
            .KeepLeft (p_spaces)
            .Map (i => new Ast_Value (i) as IAst);

         var p_identifier = CharParser
            .ManyCharSatisfy2 (
               CharParser.SatisyLetter,
               CharParser.SatisyLetterOrDigit,
               1);

         Func<char, VariableModifier> charToModifier =
            ch =>
               {
                  switch (ch)
                  {
                     case '#':
                        return VariableModifier.ElementName;
                     case '^':
                        return VariableModifier.TemplatedParent;
                     default:
                        throw new ArgumentOutOfRangeException ();
                  }
               };

         var p_modifier = CharParser.AnyOf ("#^").Map (charToModifier);

         var p_variable =
            Parser.Tuple (
               p_modifier.Opt (),
               p_identifier,
               p_token (".").KeepRight (p_identifier).Many ())
               .KeepLeft (p_spaces)
               .Map (tuple => new Ast_Variable (tuple.Item1, tuple.Item2, tuple.Item3) as IAst);

         Func<char, BinaryOp> charToBinOp =
            ch =>
               {
                  switch (ch)
                  {
                     case '+':
                        return BinaryOp.Add;
                     case '-':
                        return BinaryOp.Subtract;
                     case '*':
                        return BinaryOp.Multiply;
                     case '/':
                        return BinaryOp.Divide;
                     case '!':
                        return BinaryOp.Max;
                     case '?':
                        return BinaryOp.Min;
                     default:
                        throw new ArgumentOutOfRangeException ();
                  }
               };

         var p_addOp = CharParser.AnyOf ("+-").KeepLeft (p_spaces).Map (charToBinOp);
         var p_mulOp = CharParser.AnyOf ("*/").KeepLeft (p_spaces).Map (charToBinOp);
         var p_maxOp = CharParser.AnyOf ("!?").KeepLeft (p_spaces).Map (charToBinOp);

         var p_ast_redirect = Parser.Redirect<IAst> ();

         var p_ast = p_ast_redirect.Function;

         var p_term = Parser.Choice (
            p_ast.Between (p_token ("(").KeepLeft (p_spaces), p_token (")").KeepLeft (p_spaces)),
            p_variable,
            p_value
            );

         Func<IAst, BinaryOp, IAst, IAst> makeBinOp = (l, op, r) => new Ast_Binary (l, op, r);

         var p_level0 = p_term.Chain (p_mulOp, makeBinOp);
         var p_level1 = p_level0.Chain (p_addOp, makeBinOp);
         var p_level2 = p_level1.Chain (p_maxOp, makeBinOp);

         p_ast_redirect.Redirect = p_level2;

         s_parser = p_ast.KeepLeft (p_eos);
         // ReSharper restore InconsistentNaming
      }

      public string Expression
      {
         get; set;
      }

      static Func<IServiceProvider, object> MakeBindingCreator (string expression)
      {
         var parseResult = Parser.Parse (s_parser, expression);

         if (!parseResult.IsSuccessful)
         {
            throw new ArgumentException (parseResult.ErrorMessage ?? "Unknown error");
         }

         var collectVariablesAstVisitor = new CollectVariablesAstVisitor ();
         collectVariablesAstVisitor.Visit (parseResult.Value);
         var variables = collectVariablesAstVisitor.Variables;
         var inputParameter = SLE.Expression.Parameter (typeof (object[]), "input");
         var expressionBuilderAstVisitor = new ExpressionBuilderAstVisitor (
            variables,
            inputParameter
            );
         var lambda =
            SLE.Expression.Lambda<Func<object[], double>> (
               expressionBuilderAstVisitor.Visit (parseResult.Value),
               inputParameter
               );

         var func = lambda.Compile ();

         if (variables.Count == 0)
         {
            object value = func (null);
            return svcPrv => value;
         }
         else if (variables.Count == 1)
         {
            var binding = MakeBinding (variables.First ().Key);
            binding.Converter = new ExpressionValueConverter (func);
            return binding.ProvideValue;
         }
         else
         {
            var multiBinding =
               new MultiBinding
                  {
                     Converter = new ExpressionMultiValueConverter (func)
                  };
            foreach (var pair in variables)
            {
               multiBinding.Bindings.Add (MakeBinding (pair.Key));
            }

            return multiBinding.ProvideValue;
         }
      }

      static Binding MakeBinding (Ast_Variable key)
      {
         var names = key.Names.Concatenate (".");

         string path;
         
         if (key.VariableModifier.HasValue)
         {
            switch (key.VariableModifier.Value)
            {
               case VariableModifier.ElementName:
                  path = names;
                  break;
               case VariableModifier.TemplatedParent:
                  path = key.Root + "." + names;
                  break;
               default:
                  throw new ArgumentOutOfRangeException ();
            }
         }
         else
         {
            path = key.Root + "." + names;
         }

         var binding = new Binding (path)
                   {
                      Mode = BindingMode.OneWay,
                   };

         if (key.VariableModifier.HasValue)
         {
            switch (key.VariableModifier.Value)
            {
               case VariableModifier.ElementName:
                  binding.ElementName = key.Root;
                  break;
               case VariableModifier.TemplatedParent:
                  binding.RelativeSource = new RelativeSource (RelativeSourceMode.TemplatedParent);
                  break;
               default:
                  throw new ArgumentOutOfRangeException ();
            }
         }

         return binding;
      }

      public override object ProvideValue (IServiceProvider serviceProvider)
      {
         var expression = Expression ?? "";

         Func<IServiceProvider, object > func;

         lock (s_expressionCache)
         {
            func = s_expressionCache.LookupOrAdd (
               expression,
               () => MakeBindingCreator (expression)
               );
         }

         return func (serviceProvider);
      }
   }
}