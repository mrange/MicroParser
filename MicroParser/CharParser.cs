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

   static partial class CharParser
   {
      public static Parser<Empty> SkipChar (char toSkip)
      {
         return SkipString (new string (toSkip, 1));
      }

      public static Parser<Empty> SkipString (string toSkip)
      {
         var toSkipNotNull = toSkip ?? string.Empty;
         var parserErrorMessage = new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (toSkip));
         CharSatisfy.Function satisfy = (c, i) => toSkipNotNull[i] == c;

         return SkipSatisfy (
            new CharSatisfy (parserErrorMessage, satisfy),
            toSkipNotNull.Length,
            toSkipNotNull.Length);
      }

      public static Parser<Empty> SkipAnyOf (string skipAnyOfThese)
      {
         var sat = CreateSatisfyForAnyOf (skipAnyOfThese);
         return SkipSatisfy (
            sat,
            maxCount:1
            );
      }

      public static Parser<Empty> SkipNoneOf (string skipNoneOfThese)
      {
         var sat = CreateSatisfyForNoneOf (skipNoneOfThese);
         return SkipSatisfy (
            sat,
            maxCount: 1
            );
      }

      public static Parser<Empty> SkipSatisfy (
         CharSatisfy charSatisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         Parser<Empty>.Function function = state =>
         {
            var advanceResult = state.SkipAdvance (charSatisfy.Satisfy, minCount, maxCount);
            return ParserReply.Create (advanceResult, state, charSatisfy.ErrorMessage, Empty.Value);
         };
         return function;
      }

      public static Parser<Empty> SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         return SkipSatisfy (SatisyWhiteSpace, minCount, maxCount);
      }

      public static Parser<Empty> SkipNewLine (
         )
      {
         return SkipChar ('\r').Opt ()
            .KeepRight (SkipChar ('\n'));
      }

      public static Parser<char> AnyOf (
         string match
         )
      {
         var satisfy = CreateSatisfyForAnyOf (match);

         return CharSatisfy (satisfy);
      }

      public static Parser<char> NoneOf (
         string match
         )
      {
         var satisfy = CreateSatisfyForNoneOf (match);

         return CharSatisfy (satisfy);
      }

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

      static CharSatisfy CreateSatisfyForAnyOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (x)),
            true
            );
      }

      static CharSatisfy CreateSatisfyForNoneOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Unexpected (Strings.CharSatisfy.FormatChar_1.Form (x)),
            false
            );
      }

      public static Parser<char> CharSatisfy (
         CharSatisfy satisfy
         )
      {
         Parser<char>.Function function = state =>
         {
            var subString = new SubString ();
            var advanceResult = state.Advance (ref subString, satisfy.Satisfy, 1, 1);

            return ParserReply.Create (
               advanceResult,
               state,
               satisfy.ErrorMessage,
               subString[0]
               );
         };
         return function;
      }

      public static Parser<SubString> ManyCharSatisfy (
         CharSatisfy satisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         Parser<SubString>.Function function = state =>
         {
            var subString = new SubString ();
            var advanceResult = state.Advance (ref subString, satisfy.Satisfy, minCount, maxCount);

            return ParserReply.Create (
               advanceResult,
               state,
               satisfy.ErrorMessage,
               subString
               );
         };
         return function;
      }

      public static Parser<SubString> ManyCharSatisfy2 (
         CharSatisfy satisfyFirst,
         CharSatisfy satisfyRest,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         var first = satisfyFirst.Satisfy;
         var rest = satisfyRest.Satisfy;

         CharSatisfy.Function satisfy = (c, i) => i == 0 ? first (c, i) : rest (c, i);

         Parser<SubString>.Function function = state =>
         {
            var subString = new SubString ();

            var advanceResult = state.Advance (ref subString, satisfy, minCount, maxCount);

            var expected =
               (advanceResult == ParserState.AdvanceResult.Error_EndOfStream_PostionChanged || advanceResult == ParserState.AdvanceResult.Error_SatisfyFailed_PositionChanged)
               ? satisfyRest.ErrorMessage
               : satisfyFirst.ErrorMessage;

            return ParserReply.Create (
               advanceResult,
               state,
               expected,
               subString
               );
         };
         return function;
      }

      partial struct UIntResult
      {
         public uint Value;
         public int ConsumedCharacters;
      }

      static Parser<UIntResult> UIntImpl (
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfy.Function satisfy = (c, i) => char.IsDigit (c);

         Parser<UIntResult>.Function function = state =>
         {
            var subString = new SubString ();

            var oldPos = state.Position;

            var advanceResult = state.Advance (ref subString, satisfy, minCount, maxCount);

            var newPos = state.Position;

            return ParserReply.Create (
               advanceResult,
               state,
               ParserErrorMessages.Expected_Digit,
               () =>
                  {
                     var accumulated = 0u;
                     var length = subString.Length;
                     const uint c0 = (uint) '0';
                     for (var iter = 0; iter < length; ++iter)
                     {
                        var c = subString[iter];
                        accumulated = accumulated*10 + (c - c0);
                     }

                     return new UIntResult
                               {
                                  Value = accumulated,
                                  ConsumedCharacters = newPos.Position - oldPos.Position,
                               };
                  }
               );
         };
         return function;
      }

      static uint? CharToHex (char ch)
      {
         if ('0' <= ch && ch <= '9')
         {
            return (uint?) (ch - '0');
         }
         else if ('A' <= ch && ch <= 'F')
         {
            return (uint?) (ch - 'A' + 0xA);
         }
         else if ('a' <= ch && ch <= 'f')
         {
            return (uint?)(ch - 'a' + 0xA);            
         }
         else
         {
            return null;
         }
      }

      public static Parser<uint> Hex (
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfy.Function satisfy = (c, i) => CharToHex (c) != null;

         Parser<uint>.Function function = state =>
         {
            var subString = new SubString ();

            var advanceResult = state.Advance (ref subString, satisfy, minCount, maxCount);

            return ParserReply.Create (
               advanceResult,
               state,
               ParserErrorMessages.Expected_Digit,
               () =>
               {
                  var accumulated = 0u;
                  var length = subString.Length;
                  for (var iter = 0; iter < length; ++iter)
                  {
                     var c = subString[iter];
                     accumulated = accumulated * 0x10U + CharToHex (c).Value;
                  }

                  return accumulated;
               }
               );
         };
         return function;
      }

      public static Parser<uint> UInt (
         int minCount = 1,
         int maxCount = 10
         )
      {
         var uintParser = UIntImpl (minCount, maxCount);

         Parser<uint>.Function function = state =>
         {
            var uintResult = uintParser.Execute (state);

            if (uintResult.State.HasError ())
            {
               return uintResult.Failure<uint> ();
            }

            return uintResult.Success (uintResult.Value.Value);
         };
         return function;
      }

      public static Parser<int> Int (
         )
      {
         var intParser = Parser.Group (
            SkipChar ('-').Opt (),
            UInt ()
            );

         Parser<int>.Function function = state =>
         {
            var intResult = intParser.Execute (state);

            if (intResult.State.HasError ())
            {
               return intResult.Failure<int> ();
            }

            var intValue = (int)intResult.Value.Item2;

            return intResult.Success (intResult.Value.Item1.HasValue ? -intValue : intValue);
         };
         return function;
      }

      public static Parser<double> Double ()
      {
         var intParser = Int ();
         var fracParser = SkipChar ('.').KeepRight (UIntImpl ());
         var expParser = SkipAnyOf ("eE").KeepRight (Parser.Group (AnyOf ("+-").Opt (), UInt ()));

         var doubleParser = Parser.Group (
            intParser,
            fracParser.Opt (),
            expParser.Opt ()
            );

         Parser<double>.Function function = state =>
         {
            var doubleResult = doubleParser.Execute (state);

            if (doubleResult.State.HasError ())
            {
               return doubleResult.Failure<double> ();
            }

            var value = doubleResult.Value;

            var intValue = value.Item1;

            double doubleValue;

            if (value.Item2.HasValue)
            {
               var uIntResult = value.Item2.Value;

               var multiplier = intValue >= 0 ? 1 : -1;

               doubleValue = intValue + multiplier * uIntResult.Value * (Math.Pow (0.1, uIntResult.ConsumedCharacters));
            }
            else
            {
               doubleValue = intValue;
            }

            if (value.Item3.HasValue)
            {
               var modifier = value.Item3.Value.Item1;

               var multiplier = 
                  modifier.HasValue && modifier.Value == '-'
                  ?  -1.0
                  :  1.0
                  ;

               doubleValue *= Math.Pow (10.0, multiplier*value.Item3.Value.Item2);
            }

            return doubleResult.Success (doubleValue);
         };
         return function;
      }

      // CharSatisfy

      public static CharSatisfy Or (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (second.ErrorMessage),
            (c, i) => first.Satisfy (c, i) || second.Satisfy (c, i)
            );
      }

      static IParserErrorMessage ExpectedToUnexpected (
         IParserErrorMessage parserErrorMessage
         )
      {
         var parserErrorMessageExpected = parserErrorMessage as ParserErrorMessage_Expected;
         return parserErrorMessageExpected != null 
            ?  new ParserErrorMessage_Unexpected (parserErrorMessageExpected.Expected) 
            :  parserErrorMessage
            ;
      }

      public static CharSatisfy Except (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (ExpectedToUnexpected (second.ErrorMessage)), 
            (c, i) => first.Satisfy (c, i) && !second.Satisfy (c, i)
            );
      }

      public static readonly CharSatisfy SatisyAnyChar    = new CharSatisfy (ParserErrorMessages.Expected_Any          , (c, i) => true);
      public static readonly CharSatisfy SatisyWhiteSpace = new CharSatisfy (ParserErrorMessages.Expected_WhiteSpace   , (c, i) => char.IsWhiteSpace (c));
      public static readonly CharSatisfy SatisyDigit      = new CharSatisfy (ParserErrorMessages.Expected_Digit        , (c, i) => char.IsDigit (c));
      public static readonly CharSatisfy SatisyLetter     = new CharSatisfy (ParserErrorMessages.Expected_Letter       , (c, i) => char.IsLetter (c));
      public static readonly CharSatisfy SatisyLineBreak  = new CharSatisfy (ParserErrorMessages.Expected_LineBreak    , (c, i) =>
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

      public static readonly CharSatisfy SatisyLineBreakOrWhiteSpace  = SatisyLineBreak.Or (SatisyWhiteSpace);
      public static readonly CharSatisfy SatisyLetterOrDigit          = SatisyLetter.Or (SatisyDigit);
   }
}