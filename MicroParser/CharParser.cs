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
      public static ParserFunction<Empty> SkipChar (char toSkip)
      {
         return SkipString (new string (toSkip, 1));
      }

      public static ParserFunction<Empty> SkipString (string toSkip)
      {
         var toSkipNotNull = toSkip ?? string.Empty;
         var parserErrorMessage = new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (toSkip));
         CharSatisfyFunction satisfy = (c, i) => toSkipNotNull[i] == c;

         return SkipSatisfy (
            new CharSatify (parserErrorMessage, satisfy),
            toSkipNotNull.Length,
            toSkipNotNull.Length);
      }

      public static ParserFunction<Empty> SkipAnyOf (string skipAnyOfThese)
      {
         var sat = CreateSatisfyForAnyOf (skipAnyOfThese);
         return SkipSatisfy (
            sat,
            maxCount:1
            );
      }

      public static ParserFunction<Empty> SkipNoneOf (string skipNoneOfThese)
      {
         var sat = CreateSatisfyForNoneOf (skipNoneOfThese);
         return SkipSatisfy (
            sat,
            maxCount: 1
            );
      }

      public static ParserFunction<Empty> SkipSatisfy (
         CharSatify charSatify,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var advanceResult = state.SkipAdvance (charSatify.Satisfy, minCount, maxCount);
            return Parser.ToParserReply (advanceResult, state, charSatify.Expected, Empty.Value);
         };
      }

      public static ParserFunction<Empty> SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         return SkipSatisfy (SatisyWhiteSpace, minCount, maxCount);
      }

      public static ParserFunction<Empty> SkipNewLine (
         )
      {
         return Parser.Group (
            SkipChar ('\r').Opt (),
            SkipChar ('\n')
            ).Map (new Empty ());
      }

      public static ParserFunction<char> AnyOf(
         string match
         )
      {
         var satisfy = CreateSatisfyForAnyOf (match);

         return CharSatisfy (satisfy);
      }

      public static ParserFunction<char> NoneOf (
         string match
         )
      {
         var satisfy = CreateSatisfyForNoneOf (match);

         return CharSatisfy (satisfy);
      }

      static CharSatify CreateSatisfyForAnyOfOrNoneOf (
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

         return new CharSatify (
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

      static CharSatify CreateSatisfyForAnyOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (x)),
            true
            );
      }

      static CharSatify CreateSatisfyForNoneOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Unexpected (Strings.CharSatisfy.FormatChar_1.Form (x)),
            false
            );
      }

      public static ParserFunction<char> CharSatisfy (
         CharSatify satisfy
         )
      {
         return state =>
         {
            var subString = new SubString ();
            var advanceResult = state.Advance (ref subString, satisfy.Satisfy, 1, 1);

            return Parser.ToParserReply (
               advanceResult,
               state,
               satisfy.Expected,
               subString[0]
               );
         };
      }

      public static ParserFunction<SubString> ManyCharSatisfy (
         CharSatify satisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var subString = new SubString ();
            var advanceResult = state.Advance (ref subString, satisfy.Satisfy, minCount, maxCount);

            return Parser.ToParserReply (
               advanceResult,
               state,
               satisfy.Expected,
               subString
               );
         };
      }

      public static ParserFunction<SubString> ManyCharSatisfy2 (
         CharSatify satisfyFirst,
         CharSatify satisfyRest,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         var first = satisfyFirst.Satisfy;
         var rest = satisfyRest.Satisfy;

         CharSatisfyFunction satisfy = (c, i) => i == 0 ? first (c, i) : rest (c, i);

         return state =>
         {
            var subString = new SubString ();

            var advanceResult = state.Advance (ref subString, satisfy, minCount, maxCount);

            var expected =
               (advanceResult == ParserState_AdvanceResult.Error_EndOfStream_PostionChanged || advanceResult == ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged)
               ? satisfyRest.Expected
               : satisfyFirst.Expected;

            return Parser.ToParserReply (
               advanceResult,
               state,
               expected,
               subString
               );
         };
      }

      static ParserFunction<Tuple<uint,int>> UIntImpl (
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfyFunction satisfy = (c, i) => char.IsDigit (c);

         return state =>
         {
            var subString = new SubString ();

            var oldPos = state.Position;

            var advanceResult = state.Advance (ref subString, satisfy, minCount, maxCount);

            var newPos = state.Position;

            return Parser.ToParserReply (
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

                     return Tuple.Create (accumulated, newPos.Position - oldPos.Position);
                  }
               );
         };
      }

      public static ParserFunction<uint> UInt ()
      {
         var uintParser = UIntImpl ();

         return state =>
         {
            var uintResult = uintParser (state);

            if (uintResult.State.HasError ())
            {
               return uintResult.Failure<uint> ();
            }

            return uintResult.Success (uintResult.Value.Item1);
         };
      }

      public static ParserFunction<int> Int (
         )
      {
         var intParser = Parser.Group (
            SkipChar ('-').Opt (),
            UInt ()
            );

         return state =>
         {
            var intResult = intParser (state);

            if (intResult.State.HasError ())
            {
               return intResult.Failure<int> ();
            }

            var intValue = (int)intResult.Value.Item2;

            return intResult.Success (intResult.Value.Item1.HasValue ? -intValue : intValue);
         };
      }

      public static ParserFunction<double> Double ()
      {
         var intParser = Int ();
         var fracParser = SkipChar ('.').KeepRight (UIntImpl ());
         var expParser = SkipAnyOf ("eE").KeepRight (Parser.Group (AnyOf ("+-").Opt (), UInt ()));

         var doubleParser = Parser.Group (
            intParser,
            fracParser.Opt (),
            expParser.Opt ()
            );

         return state =>
         {
            var doubleResult = doubleParser (state);

            if (doubleResult.State.HasError ())
            {
               return doubleResult.Failure<double> ();
            }

            var value = doubleResult.Value;

            var intValue = value.Item1;

            double doubleValue;

            if (value.Item2.HasValue)
            {
               var tupleValue = value.Item2.Value;

               var multiplier = intValue >= 0 ? 1 : -1;

               doubleValue = intValue + multiplier * tupleValue.Item1 * (Math.Pow (0.1, tupleValue.Item2));
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
      }

      // CharSatisfy

      public static CharSatify Or (this CharSatify first, CharSatify second)
      {
         return new CharSatify (
            first.Expected.Append (second.Expected),
            (c, i) => first.Satisfy (c, i) || second.Satisfy (c, i)
            );
      }

      public static CharSatify Except (this CharSatify first, CharSatify second)
      {
         return new CharSatify (
            first.Expected.Append (second.Expected), // TODO: Change expected into unexpected
            (c, i) => first.Satisfy (c, i) && !second.Satisfy (c, i)
            );
      }

      public static readonly CharSatify SatisyAnyChar = new CharSatify (ParserErrorMessages.Expected_Any, (c, i) => true);
      public static readonly CharSatify SatisyWhiteSpace = new CharSatify (ParserErrorMessages.Expected_WhiteSpace, (c, i) => char.IsWhiteSpace (c));
      public static readonly CharSatify SatisyDigit = new CharSatify (ParserErrorMessages.Expected_Digit, (c, i) => char.IsDigit (c));
      public static readonly CharSatify SatisyLetter = new CharSatify (ParserErrorMessages.Expected_Letter, (c, i) => char.IsLetter (c));
      public static readonly CharSatify SatisyLineBreak = new CharSatify(ParserErrorMessages.Expected_LineBreak, (c, i) =>
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
      public static readonly CharSatify SatisyLineBreakOrWhiteSpace = SatisyLineBreak.Or (SatisyWhiteSpace);
      public static readonly CharSatify SatisyLetterOrDigit = SatisyLetter.Or (SatisyDigit);
   }
}