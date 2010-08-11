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
   using System.Linq;
   using System.Linq.Expressions;

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

      public static readonly CharSatisfy AnyChar    = new CharSatisfy (ParserErrorMessages.Expected_Any          , (c, i) => true);
      public static readonly CharSatisfy WhiteSpace = new CharSatisfy (ParserErrorMessages.Expected_WhiteSpace   , (c, i) => Char.IsWhiteSpace (c));
      public static readonly CharSatisfy Digit      = new CharSatisfy (ParserErrorMessages.Expected_Digit        , (c, i) => Char.IsDigit (c));
      public static readonly CharSatisfy Letter     = new CharSatisfy (ParserErrorMessages.Expected_Letter       , (c, i) => Char.IsLetter (c));

      public static readonly CharSatisfy LineBreak  = new CharSatisfy (ParserErrorMessages.Expected_LineBreak    , (c, i) =>
                                                                                                                            {
                                                                                                                               switch (c)
                                                                                                                               {
                                                                                                                                  case '\r':
                                                                                                                                  case '\n':
                                                                                                                                     return true;
                                                                                                                                  default:
                                                                                                                                     return false;
                                                                                                                               }
                                                                                                                            });

#if !MICRO_PARSER_SUPPRESS_CHAR_SATISFY_COMPOSITES
      public static readonly CharSatisfy LineBreakOrWhiteSpace  = LineBreak.Or (WhiteSpace);
      public static readonly CharSatisfy LetterOrDigit          = Letter.Or (Digit);
#endif

      static Function CreateSatisfyFromString (
         IEnumerable<char> s,
         bool matchResult
         )
      {
         var parameter0 = Expression.Parameter (typeof (char), "ch");
         var parameter1 = Expression.Parameter (typeof (int), "index");

         Func<char, Expression> compareCreator;
         Func<Expression, Expression, Expression> aggregator;
         if (matchResult)
         {
            compareCreator = c => Expression.Equal (Expression.Constant (c), parameter0);
            aggregator = (accu, value) => Expression.OrElse (value, accu);
         }
         else
         {
            compareCreator = c => Expression.NotEqual (Expression.Constant (c), parameter0);
            aggregator = (accu, value) => Expression.AndAlso (value, accu);            
         }
         var comparisons = s
            .Select (compareCreator)
            .ToArray ();

         var body = comparisons
            .Skip (1)
            .Aggregate (
               comparisons[0],
               aggregator
               );

         var lambda = Expression.Lambda<Function> (
            body,
            parameter0,
            parameter1
            );

         return lambda.Compile ();
      }

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

         var group = new ParserErrorMessage_Group (errorMessages);

         if (match.Length < 4)
         {
            return new CharSatisfy (
               group,
               CreateSatisfyFromString (match, matchResult)
               );
         }
         else if (!match.Any (ch => ch > 255))
         {
            var boolMap = Enumerable.Repeat (!matchResult, 256).ToArray ();
            foreach (var c in match)
            {
               boolMap[c] = matchResult;
            }

            return new CharSatisfy (
               group,
               (c, i) => ((c & 0xFF00) == 0) && boolMap[c & 0xFF]
               );
         }
         else if (match.Length < 16)
         {
            return new CharSatisfy (
               group,
               CreateSatisfyFromString (match, matchResult)
               );
         }
         else
         {
            var hashSet = new HashSet<char>(match);
            return new CharSatisfy (
               group,
               (c, i) => hashSet.Contains (c) ? matchResult : !matchResult
               );            
         }

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