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
   using System.Linq;

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

      static CharSatisfy CreateSatisfyForAnyOfOrNoneOf (
         string match,
         Func<char, IParserErrorMessage> action,
         bool matchResult)
      {
         var matchArray = (match ?? Strings.Empty).ToArray ();

         var expected = matchArray
            .Select (action)
            .ToArray ()
            ;

         var group = new ParserErrorMessage_Group (expected);

         return new CharSatisfy (
            group,
            (c, i) =>
               {
                  foreach (var ch in matchArray)
                  {
                     if (ch == c)
                     {
                        return matchResult;
                     }
                  }

                  return !matchResult;
               }
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