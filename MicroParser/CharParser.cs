using System;
using System.Linq;

namespace MicroParser
{
   public static class CharParser
   {
      public static class Strings
      {
         public const string Any = "any";
         public const string Digit = "digit";
         public const string Letter = "letter";
         public const string WhiteSpace = "whitespace";
      }
      public static ParserFunction<Empty> SkipString (string toSkip)
      {
         var toSkipNotNull = toSkip ?? string.Empty;
         CharSatisfyFunction satisfy = (c, i) => toSkipNotNull[i] == c;

         return state =>
                   {
                      var advanceResult = state.SkipAdvance (satisfy, maxCount:toSkipNotNull.Length);
                      return Parser.ToParserReply (advanceResult, state, ParserErrorMessageFactory.Expected, toSkipNotNull, Empty.Value);
                   };
      }

      public static ParserFunction<Empty> SkipChar (char c)
      {
         return SkipString (new string (c, 1));
      }

      public static ParserFunction<Empty> SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfyFunction satisfy = (c, i) => char.IsWhiteSpace (c);

         return state =>
         {
            var advanceResult = state.SkipAdvance (satisfy, minCount, maxCount);

            return Parser.ToParserReply (
               advanceResult, 
               state, 
               ParserErrorMessageFactory.Expected, 
               Strings.WhiteSpace, 
               Empty.Value
               );
         };
      }

      public static ParserFunction<char> AnyOf (
         string match
         )
      {
         var matchArray = (match ?? "").ToArray ();

         var satisfy = new CharSatify (
            matchArray
               .Select (x => "'" + x + "'")
               .Concatenate (" or "),
            (c, i) =>
               {
                  foreach (var ch in matchArray)
                  {
                     if (ch == c)
                     {
                        return true;
                     }
                  }

                  return false;
               }
            );

         return CharSatisfy (satisfy);
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
               ParserErrorMessageFactory.Expected,
               satisfy.Expected,
               () => subString[0]
               );
         };
      }

      public static ParserFunction<string> ManyCharSatisfy (
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
               ParserErrorMessageFactory.Expected, 
               satisfy.Expected,
               () => subString.ToString ()
               );
         };
      }

      public static ParserFunction<string> ManyCharSatisfy2 (
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
               ? satisfyFirst.Expected
               : satisfyRest.Expected;

            return Parser.ToParserReply (
               advanceResult,
               state,
               ParserErrorMessageFactory.Expected, 
               expected,
               () => subString.ToString ()
               );
         };
      }

      static ParserFunction<Tuple<uint,int>> ParseUIntImpl (
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
               ParserErrorMessageFactory.Expected,
               Strings.Digit,
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

                     return Tuple.Create (accumulated, newPos - oldPos);
                  }
               );
         };
      }

      public static ParserFunction<uint> ParseUInt (
         int minCount = 1,
         int maxCount = 10
         )
      {
         var uintParser = ParseUIntImpl (minCount, maxCount);

         return state =>
         {
            var uintResult = uintParser (state);

            if (uintResult.State.HasError ())
            {
               return uintResult.Failure<uint>();
            }

            return uintResult.Success (uintResult.Value.Item1);
         };
      }

      public static ParserFunction<int> ParseInt (
         int minCount = 1,
         int maxCount = 11
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         var intParser = Parser.Tuple (
            SkipChar ('-').Opt (),
            ParseUInt ()
            );

         return state =>
         {
            var intResult = intParser (state);

            if (intResult.State.HasError ())
            {
               return intResult.Failure<int>();
            }

            var intValue = (int)intResult.Value.Item2;

            return intResult.Success (intResult.Value.Item1.HasValue ? -intValue : intValue);
         };
      }

      public static ParserFunction<double> ParseDouble (
         int minCount = 1,
         int maxCount = 20
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         var doubleParser = Parser.Tuple (
            ParseInt (),
            SkipChar ('.').KeepRight (ParseUIntImpl ()).Opt ()
            );

         return state =>
         {
            var doubleResult = doubleParser (state);

            if (doubleResult.State.HasError ())
            {
               return doubleResult.Failure<double>();
            }

            var intValue = doubleResult.Value.Item1;

            if (doubleResult.Value.Item2.HasValue)
            {
               var tupleValue = doubleResult.Value.Item2.Value;

               var multiplier = intValue >= 0 ? 1 : -1;

               return doubleResult.Success(intValue + multiplier * tupleValue.Item1 * (Math.Pow(0.1, tupleValue.Item2)));
            }

            return doubleResult.Success ((double) intValue);
         };
      }

      public static CharSatify Or (this CharSatify first, CharSatify second)
      {
         return new CharSatify (
            string.Format (
               "{0} or {1}",
               first.Expected,
               second.Expected),
            (c,i) => first.Satisfy (c,i) || second.Satisfy (c,i)
            );
      }

      public static CharSatify And (this CharSatify first, CharSatify second)
      {
         return new CharSatify (
            string.Format (
               "{0} and {1}",
               first.Expected,
               second.Expected),
            (c, i) => first.Satisfy (c, i) && second.Satisfy (c, i)
            );
      }

      public static CharSatify Except (this CharSatify first, CharSatify second)
      {
         return new CharSatify (
            string.Format (
               "{0} except {1}",
               first.Expected,
               second.Expected),
            (c, i) => first.Satisfy (c, i) && !second.Satisfy (c, i)
            );
      }

      public static readonly CharSatify SatisyAnyChar = new CharSatify (Strings.Any, (c, i) => true);
      public static readonly CharSatify SatisyWhiteSpace = new CharSatify (Strings.WhiteSpace, (c, i) => char.IsWhiteSpace (c));
      public static readonly CharSatify SatisyDigit = new CharSatify (Strings.Digit, (c, i) => char.IsDigit (c));
      public static readonly CharSatify SatisyLetter = new CharSatify (Strings.Letter, (c, i) => char.IsLetter (c));
      public static readonly CharSatify SatisyLetterOrDigit = SatisyLetter.Or (SatisyDigit);
   }
}