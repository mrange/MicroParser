﻿using System;

namespace MicroParser
{
   public sealed class CharSatify
   {
      public readonly string Expected;
      public readonly Func<char, int, bool> Satisfy;

      public CharSatify (string expected, Func<char, int, bool> satisfy)
      {
         Expected = expected;
         Satisfy = satisfy;
      }
   }

   public static class CharParser
   {
      public static ParserFunction<Empty> SkipString(string toSkip)
      {
         var toSkipNotNull = toSkip ?? "";
         Func<char,int, bool> satisfy = (c, i) => toSkipNotNull[i] == c;

         return state =>
                   {
                      var advanceResult = state.SkipAdvance (satisfy, maxCount:toSkipNotNull.Length);
                      return Parser.ToParserReply (advanceResult, state, ParserErrorMessageFactory.Expected, toSkipNotNull, Empty.Value);
                   };
      }

      public static ParserFunction<Empty> SkipChar(char c)
      {
         return SkipString (new string (c, 1));
      }

      public static ParserFunction<Empty> SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         Func<char, int, bool> satisfy = (c, i) => char.IsWhiteSpace(c);

         return state =>
         {
            var advanceResult = state.SkipAdvance(satisfy, minCount, maxCount);
            return Parser.ToParserReply(advanceResult, state, ParserErrorMessageFactory.Expected, "whitespace", Empty.Value);
         };
      }

      public static ParserFunction<string> ManyCharSatisfy(
         CharSatify satisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount(minCount, maxCount);

         return state =>
         {
            var subString = new SubString ();
            var advanceResult = state.Advance(ref subString, satisfy.Satisfy, minCount, maxCount);
            return Parser.ToParserReply(
               advanceResult,
               state,
               ParserErrorMessageFactory.Expected, 
               satisfy.Expected,
               () => subString.ToString ());
         };
      }

      public static ParserFunction<string> ManyCharSatisfy2(
         CharSatify satisfyFirst,
         CharSatify satisfyRest,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount(minCount, maxCount);

         var first = satisfyFirst.Satisfy;
         var rest = satisfyRest.Satisfy;

         Func<char, int, bool> satisfy = (c, i) => i == 0 ? first(c, i) : rest(c, i);

         return state =>
         {
            var subString = new SubString();
            var advanceResult = state.Advance(ref subString, satisfy, minCount, maxCount);
            var expected =
               (advanceResult == ParserState_AdvanceResult.Error_EndOfStream_PostionChanged || advanceResult == ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged)
               ? satisfyFirst.Expected
               : satisfyRest.Expected;

            return Parser.ToParserReply(
               advanceResult,
               state,
               ParserErrorMessageFactory.Expected, 
               expected,
               () => subString.ToString());
         };
      }

      public static ParserFunction<int> ParseInt(
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount(minCount, maxCount);

         Func<char, int, bool> satisfy = (c, i) => char.IsDigit(c);

         return state =>
         {
            var subString = new SubString();
            var advanceResult = state.Advance(ref subString, satisfy, minCount, maxCount);
            return Parser.ToParserReply(
               advanceResult,
               state,
               ParserErrorMessageFactory.Expected, 
               "digit",
               () =>
                  {
                     var accumulated = 0;
                     var length = subString.Length;
                     const int c0 = (int) '0';
                     for (var iter = 0; iter < length; ++iter)
                     {
                        var c = subString[iter];
                        accumulated = accumulated*10 + (c - c0);
                     }

                     return accumulated;
                  });
         };
      }

      public static readonly CharSatify SatisyWhiteSpace = new CharSatify("WhiteSpace", (c, i) => char.IsWhiteSpace(c));
      public static readonly CharSatify SatisyDigit = new CharSatify("digit", (c, i) => char.IsDigit(c));
      public static readonly CharSatify SatisyLetter = new CharSatify("letter", (c, i) => char.IsLetter(c));
      public static readonly CharSatify SatisyLetterOrDigit = new CharSatify("letter or digit", (c, i) => char.IsLetterOrDigit(c));
   }
}