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
#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_CHAR
      public static Parser<Empty> SkipChar (char toSkip)
      {
         return SkipString (new string (toSkip, 1));
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_STRING
      public static Parser<Empty> SkipString (string toSkip)
      {
         if (toSkip.IsNullOrEmpty ())
         {
            throw new ArgumentNullException ("toSkip");
         }

         CharSatisfy.Function satisfy = (c, i) => c == toSkip[i];
         var parserErrorMessage = new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (toSkip));

         return SkipSatisfy (
            new CharSatisfy (parserErrorMessage, satisfy),
            toSkip.Length,
            toSkip.Length);
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_ANY_OF
      public static Parser<Empty> SkipAnyOf (string skipAnyOfThese)
      {
         var sat = CharSatisfy.CreateSatisfyForAnyOf (skipAnyOfThese);
         return SkipSatisfy (
            sat,
            maxCount:1
            );
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_NONE_OF
      public static Parser<Empty> SkipNoneOf (string skipNoneOfThese)
      {
         var sat = CharSatisfy.CreateSatisfyForNoneOf (skipNoneOfThese);
         return SkipSatisfy (
            sat,
            maxCount: 1
            );
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_SATISFY
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
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_WHITE_SPACE
      public static Parser<Empty> SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         return SkipSatisfy (CharSatisfy.WhiteSpace, minCount, maxCount);
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_NEW_LINE
      public static Parser<Empty> SkipNewLine (
         )
      {
         return SkipChar ('\r').Opt ()
            .KeepRight (SkipChar ('\n'));
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_ANY_OF
      public static Parser<SubString> AnyOf (
         string match,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         var satisfy = CharSatisfy.CreateSatisfyForAnyOf (match);

         return ManyCharSatisfy (satisfy, minCount, maxCount);
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_NONE_OF
      public static Parser<SubString> NoneOf (
         string match,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         var satisfy = CharSatisfy.CreateSatisfyForNoneOf (match);

         return ManyCharSatisfy (satisfy, minCount, maxCount);
      }
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_MANY_CHAR_SATISFY
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
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_MANY_CHAR_SATISFY_2
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
#endif

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

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_HEX
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

      [CLSCompliant (false)]
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
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_UINT
      [CLSCompliant (false)]
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
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_INT
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
#endif

#if !MICRO_PARSER_SUPPRESS_CHAR_PARSER_DOUBLE
      public static Parser<double> Double ()
      {
         var intParser = Int ();
         var fracParser = SkipChar ('.').KeepRight (UIntImpl ());
         var expParser = SkipAnyOf ("eE").KeepRight (Parser.Group (AnyOf ("+-", maxCount:1), UInt ()));

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
                  modifier[0] == '-'
                  ?  -1.0
                  :  1.0
                  ;

               doubleValue *= Math.Pow (10.0, multiplier*value.Item3.Value.Item2);
            }

            return doubleResult.Success (doubleValue);
         };
         return function;
      }
#endif
   }
}