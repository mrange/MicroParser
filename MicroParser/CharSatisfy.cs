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
namespace MicroParser
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Reflection;
   using System.Reflection.Emit;

   using Internal;

   sealed partial class CharSatisfy
   {
      public delegate bool Function (char ch, int index);

      public readonly IParserErrorMessage ErrorMessage;
      public readonly Function Satisfy;

      public static implicit operator CharSatisfy (char ch)
      {
         return new CharSatisfy (
            new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (ch)), 
            (c, i) => ch == c
            );
      }

      public CharSatisfy (IParserErrorMessage errorMessage, Function satisfy)
      {
         ErrorMessage = errorMessage;
         Satisfy = satisfy;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      ErrorMessage,
                   }.ToString ();
      }
#endif

      class SatisfyFunctions
      {
         // ReSharper disable MemberHidesStaticFromOuterClass
         // ReSharper disable MemberCanBeMadeStatic.Local

         // These methods should be kept non static as that reduces delegate overhead

         public bool AnyChar (char ch, int index)
         {
            return true;
         }

         public bool WhiteSpace (char ch, int index)
         {
            return char.IsWhiteSpace (ch);
         }

         public bool Digit (char ch, int index)
         {
            return char.IsDigit (ch);
         }

         public bool Letter (char ch, int index)
         {
            return char.IsLetter (ch);
         }
         // ReSharper restore MemberCanBeMadeStatic.Local
         // ReSharper restore MemberHidesStaticFromOuterClass
      }

      static readonly SatisfyFunctions s_satisfyFunctions   = new SatisfyFunctions ();

      public static readonly CharSatisfy AnyChar      = new CharSatisfy (ParserErrorMessages.Expected_Any         , s_satisfyFunctions.AnyChar     );
      public static readonly CharSatisfy WhiteSpace   = new CharSatisfy (ParserErrorMessages.Expected_WhiteSpace  , s_satisfyFunctions.WhiteSpace  );
      public static readonly CharSatisfy Digit        = new CharSatisfy (ParserErrorMessages.Expected_Digit       , s_satisfyFunctions.Digit       );
      public static readonly CharSatisfy Letter       = new CharSatisfy (ParserErrorMessages.Expected_Letter      , s_satisfyFunctions.Letter      );

      public static readonly CharSatisfy LineBreak    = new CharSatisfy (ParserErrorMessages.Expected_LineBreak   , CreateSatisfyFunctionForAnyOfOrNoneOf ("\r\n", true));

#if !MICRO_PARSER_SUPPRESS_CHAR_SATISFY_COMPOSITES
      public static readonly CharSatisfy LineBreakOrWhiteSpace  = LineBreak.Or (WhiteSpace);
      public static readonly CharSatisfy LetterOrDigit          = Letter.Or (Digit);
#endif

#if MICRO_PARSER_NET35
      static Function CreateSatisfyFunctionForAnyOfOrNoneOf (
         string match,
         bool matchResult
         )
      {
         if (!match.Any (ch => ch > 255))
         {
            var boolMap = Enumerable.Repeat (!matchResult, 256).ToArray ();
            foreach (var c in match)
            {
               boolMap[c] = matchResult;
            }

            return (c, i) => ((c & 0xFF00) == 0) && boolMap[c & 0xFF];
         }

         var hashSet = new HashSet<char>(match);
         return (c, i) => hashSet.Contains (c) ? matchResult : !matchResult;
      }
#else
      static Function CreateSatisfyFunctionForAnyOfOrNoneOf (
         string match,
         bool matchResult
         )
      {
         var parameter0 = Expression.Parameter (typeof (char), "ch");
         var parameter1 = Expression.Parameter (typeof (int), "index");

         var resultVariable = Expression.Variable (typeof (bool), "result");

         var switchStatement = Expression.Switch (
            parameter0,
            Expression.Assign (resultVariable, Expression.Constant (!matchResult)),
            Expression.SwitchCase (
               Expression.Assign (resultVariable, Expression.Constant (matchResult)),
               match.Select (ch => Expression.Constant (ch)).ToArray ()
               ));

         var body = Expression.Block (
            new[] { resultVariable },
            switchStatement,
            resultVariable
            );

         var lambda = Expression.Lambda<Function>(
            body,
            parameter0,
            parameter1
            );

         return lambda.Compile ();
      }
#endif

      static CharSatisfy CreateSatisfyForAnyOfOrNoneOf (
         string match,
         Func<char, IParserErrorMessage> action,
         bool matchResult
         )
      {
         if (string.IsNullOrEmpty (match))
         {
            throw new ArgumentNullException ("match");
         }

         var errorMessages = match
            .Select (action)
            .ToArray ()
            ;

         return new CharSatisfy (
            new ParserErrorMessage_Group (errorMessages),
            CreateSatisfyFunctionForAnyOfOrNoneOf (match, matchResult)
            );
      }

      public static CharSatisfy CreateSatisfyForAnyOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (x)),
            true
            );
      }

      public static CharSatisfy CreateSatisfyForNoneOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Unexpected (Strings.CharSatisfy.FormatChar_1.FormatString (x)),
            false
            );
      }
   }
}