﻿


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
      public static Parser<Empty>.Function SkipChar (char toSkip)
      {
         return SkipString (new string (toSkip, 1));
      }

      public static Parser<Empty>.Function SkipString (string toSkip)
      {
         var toSkipNotNull = toSkip ?? string.Empty;
         var parserErrorMessage = new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (toSkip));
         CharSatisfy.Function satisfy = (c, i) => toSkipNotNull[i] == c;

         return SkipSatisfy (
            new CharSatisfy (parserErrorMessage, satisfy),
            toSkipNotNull.Length,
            toSkipNotNull.Length);
      }

      public static Parser<Empty>.Function SkipAnyOf (string skipAnyOfThese)
      {
         var sat = CreateSatisfyForAnyOf (skipAnyOfThese);
         return SkipSatisfy (
            sat,
            maxCount:1
            );
      }

      public static Parser<Empty>.Function SkipNoneOf (string skipNoneOfThese)
      {
         var sat = CreateSatisfyForNoneOf (skipNoneOfThese);
         return SkipSatisfy (
            sat,
            maxCount: 1
            );
      }

      public static Parser<Empty>.Function SkipSatisfy (
         CharSatisfy charSatisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var advanceResult = state.SkipAdvance (charSatisfy.Satisfy, minCount, maxCount);
            return ParserReply.Create (advanceResult, state, charSatisfy.ErrorMessage, Empty.Value);
         };
      }

      public static Parser<Empty>.Function SkipWhiteSpace (
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         return SkipSatisfy (SatisyWhiteSpace, minCount, maxCount);
      }

      public static Parser<Empty>.Function SkipNewLine (
         )
      {
         return SkipChar ('\r').Opt ()
            .KeepRight (SkipChar ('\n'));
      }

      public static Parser<char>.Function AnyOf (
         string match
         )
      {
         var satisfy = CreateSatisfyForAnyOf (match);

         return CharSatisfy (satisfy);
      }

      public static Parser<char>.Function NoneOf (
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

      public static Parser<char>.Function CharSatisfy (
         CharSatisfy satisfy
         )
      {
         return state =>
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
      }

      public static Parser<SubString>.Function ManyCharSatisfy (
         CharSatisfy satisfy,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
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
      }

      public static Parser<SubString>.Function ManyCharSatisfy2 (
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

         return state =>
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
      }

      partial struct UIntResult
      {
         public uint Value;
         public int ConsumedCharacters;
      }

      static Parser<UIntResult>.Function UIntImpl (
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfy.Function satisfy = (c, i) => char.IsDigit (c);

         return state =>
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

      public static Parser<uint>.Function Hex (
         int minCount = 1,
         int maxCount = 10
         )
      {
         Parser.VerifyMinAndMaxCount (minCount, maxCount);

         CharSatisfy.Function satisfy = (c, i) => CharToHex (c) != null;

         return state =>
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
      }

      public static Parser<uint>.Function UInt (
         int minCount = 1,
         int maxCount = 10
         )
      {
         var uintParser = UIntImpl (minCount, maxCount);

         return state =>
         {
            var uintResult = uintParser (state);

            if (uintResult.State.HasError ())
            {
               return uintResult.Failure<uint> ();
            }

            return uintResult.Success (uintResult.Value.Value);
         };
      }

      public static Parser<int>.Function Int (
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

      public static Parser<double>.Function Double ()
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
   using Internal;

   sealed partial class CharSatisfy
   {
      public delegate bool Function (char ch, int index);

      public readonly IParserErrorMessage ErrorMessage;
      public readonly Function Satisfy;

      public static implicit operator CharSatisfy (char ch)
      {
         return new CharSatisfy (
            new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.Form (ch)), 
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
                      Expected,
                   }.ToString ();
      }
#endif
   }
}
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
   partial struct Empty
   {
      public static Empty Value;

      public override string  ToString ()
      {
         return Strings.Empty;
      }
   }

}
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
namespace MicroParser.Internal
{
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;
   using System.Text;

   static partial class Extensions
   {

      // System.String

      public static string Form (this string format, params object[] args)
      {
         return string.Format (CultureInfo.InvariantCulture, format, args);
      }

      public static bool IsNullOrEmpty (this string str)
      {
         return string.IsNullOrEmpty (str);
      }

      // IEnumerable<string>

      public static string Concatenate (
         this IEnumerable<string> strings,
         string delimiter = null,
         string prepend = null,
         string append = null
         )
      {
         var first = true;

         var sb = new StringBuilder (prepend ?? String.Empty);

         var del = delimiter ?? String.Empty;

         foreach (var value in strings)
         {
            if (first)
            {
               first = false;
            }
            else
            {
               sb.Append (del);
            }
            sb.Append (value);
         }

         sb.Append (append ?? String.Empty);
         return sb.ToString ();
      }

      // ParserReply.State

      public static bool IsSuccessful (this ParserReply.State state)
      {
         return state == ParserReply.State.Successful;
      }

      public static bool HasConsistentState (this ParserReply.State state)
      {
         return
            (state & ParserReply.State.FatalError_StateIsNotRestored)
               == 0;
      }

      public static bool HasFatalError (this ParserReply.State state)
      {
         return state >= ParserReply.State.FatalError;
      }

      public static bool HasError (this ParserReply.State state)
      {
         return state >= ParserReply.State.Error;
      }

      public static bool HasNonFatalError (this ParserReply.State state)
      {
         return state >= ParserReply.State.Error && state < ParserReply.State.FatalError;
      }

      // IParserErrorMessage

      public static IEnumerable<IParserErrorMessage> DeepTraverse (this IParserErrorMessage value)
      {
         if (value == null)
         {
            yield break;
         }

         var stack = new Stack<IParserErrorMessage> ();
         stack.Push (value);


         while (stack.Count > 0)
         {
            var pop = stack.Pop ();

            var parserErrorMessageGroup = pop as ParserErrorMessage_Group;

            if (parserErrorMessageGroup != null && parserErrorMessageGroup.Group != null)
            {
               foreach (var parserErrorMessage in parserErrorMessageGroup.Group)
               {
                  stack.Push (parserErrorMessage);
               }
            }
            else if (pop != null)
            {
               yield return pop;
            }
         }

      }

      public static IParserErrorMessage Append (this IParserErrorMessage left, IParserErrorMessage right)
      {
         return new ParserErrorMessage_Group (
            left.DeepTraverse ().Concat (right.DeepTraverse ()).ToArray ()
            );
      }

   }
}
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
#if MICRO_PARSER_MAKE_PUBLIC
   public partial class CharParser
   {

   }

   public partial class CharSatisfy
   {

   }

   public partial interface IParserErrorMessage
   {

   }

   public partial struct Empty
   {
      
   }

   public static partial class Tuple
   {
      
   }

   public partial struct Tuple<TValue1, TValue2>
   {
      
   }

   public partial struct Tuple<TValue1, TValue2, TValue3>
   {

   }

   public static partial class Optional
   {

   }

   public partial struct Optional<TValue>
   {
      
   }

   public partial class Parser<TValue>
   {

   }

   public partial class Parser
   {
      
   }

   public partial class ParserFunctionRedirect<TValue>
   {

   }

   public static partial class ParserReply
   {

   }

   public partial struct ParserReply<TValue>
   {

   }

   public partial class BaseParserResult
   {

   }

   public partial class ParserResult<TValue>
   {

   }

   public partial class ParserState
   {

   }

   public partial struct ParserStatePosition
   {

   }

   public partial struct SubString
   {

   }
#endif
}

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
   static partial class Optional
   {
      public static Optional<TValue> Create<TValue> (TValue value)
      {
         return new Optional<TValue> (value);
      }

      public static Optional<TValue> Create<TValue> ()
      {
         return new Optional<TValue> ();
      }
   }

   partial struct Optional<TValue>
   {
      public readonly bool HasValue;
      public readonly TValue Value;

      public Optional (TValue value)
      {
         HasValue = true;
         Value = value;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      HasValue,
                      Value = HasValue ? Value : default (TValue),
                   }.ToString ();
      }
#endif
   }
}
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
   using Internal;

   static partial class Parser<TValue>
   {
      public delegate ParserReply<TValue> Function (ParserState state);      
   }

   static partial class Parser
   {
      public static ParserResult<TValue> Parse<TValue> (Parser<TValue>.Function parserFunction, string text)
      {
         var parseResult = parserFunction (
            ParserState.Create (
               text ?? Strings.Empty,
               suppressParserErrorMessageOperations:true
               ));

         if (!parseResult.State.IsSuccessful ())
         {
            var parseResultWithErrorInfo = parserFunction (
               ParserState.Create (
                  text ?? Strings.Empty
                  ));

            var errorResult = parseResultWithErrorInfo
               .ParserErrorMessage
               .DeepTraverse ()
               .GroupBy (msg => msg.Description)
               .Select (messages =>
                        Strings.Parser.ErrorMessage_2.Form (
                           messages.Key,
                           messages.Distinct ().Select (message => message.Value.ToString ()).Concatenate (", ")
                           ))
               .Concatenate (", ");

            var subString = new SubString ( 
                     text,
                     parseResultWithErrorInfo.ParserState.InternalPosition
                  );

            var completeErrorResult =
               "Pos: {0} ('{1}') - {2}".Form (
                  subString.Position,
                  subString[0],
                  errorResult
                  );

            return new ParserResult<TValue> (
               false,
               subString,
               completeErrorResult,
               default (TValue)
               );
         }

         return new ParserResult<TValue> (
            true,
            new SubString ( 
                  text,
                  parseResult.ParserState.InternalPosition
               ),
            Strings.Empty,
            parseResult.Value
            );
      }

      public static ParserFunctionRedirect<TValue> Redirect<TValue> ()
      {
         return new ParserFunctionRedirect<TValue> ();
      }

      public static Parser<TValue>.Function Return<TValue> (TValue value)
      {
         return state => ParserReply<TValue>.Success (state, value);
      }

      public static Parser<TValue>.Function Fail<TValue> (string message)
      {
         var parserErrorMessageMessage = new ParserErrorMessage_Message (message);
         return state => ParserReply<TValue>.Failure (ParserReply.State.Error, state, parserErrorMessageMessage);
      }

      public static Parser<Empty>.Function EndOfStream ()
      {
         return state =>
                state.EndOfStream
                   ? ParserReply<Empty>.Success (state, Empty.Value)
                   : ParserReply<Empty>.Failure (
                      ParserReply.State.Error_Expected,
                      state,
                      ParserErrorMessages.Expected_EndOfStream
                      );
      }

      public static Parser<TValue2>.Function Combine<TValue, TValue2>(this Parser<TValue>.Function firstParser, Func<TValue, Parser<TValue2>.Function> second)
      {
         return state =>
                   {
                      var firstResult = firstParser (state);
                      if (firstResult.State.HasError ())
                      {
                         return firstResult.Failure<TValue2> ();
                      }

                      var secondParser = second (firstResult.Value);
                      var secondResult = secondParser (state);
                      return secondResult;
                   };
      }

      public static Parser<TValue2>.Function Map<TValue1, TValue2> (this Parser<TValue1>.Function firstParser, Func<TValue1, TValue2> mapper)
      {
         return state =>
         {
            var firstResult = firstParser (state);

            if (firstResult.State.HasError ())
            {
               return firstResult.Failure<TValue2> ();
            }

            return firstResult.Success (mapper (firstResult.Value));
         };
      }

      public static Parser<TValue2>.Function Map<TValue1, TValue2> (this Parser<TValue1>.Function firstParser, TValue2 value2)
      {
         return firstParser.Map (ignore => value2);
      }

      public static Parser<TValue1>.Function Chain<TValue1, TValue2>(
         this Parser<TValue1>.Function parser,
         Parser<TValue2>.Function separator,
         Func<TValue1, TValue2, TValue1, TValue1> combiner
         )
      {
         return state =>
            {
               var result = parser (state);
               if (result.State.HasError ())
               {
                  return result;
               }

               var accu = result.Value;

               ParserReply<TValue2> separatorResult;

               while ((separatorResult = separator (state)).State.IsSuccessful ())
               {
                  var trailingResult = parser (state);

                  if (trailingResult.State.HasError ())
                  {
                     return trailingResult;
                  }

                  accu = combiner (accu, separatorResult.Value, trailingResult.Value);
               }

               if (separatorResult.State.HasFatalError ())
               {
                  return separatorResult.Failure<TValue1> ();
               }

               return ParserReply<TValue1>.Success (state, accu);
            };
      }

      public static Parser<TValue[]>.Function Array<TValue> (
         this Parser<TValue>.Function parser,
         Parser<Empty>.Function separator,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var initialPosition = state.Position;

            var result = new List<TValue> (Math.Max (minCount, 16));

            // Collect required

            for (var iter = 0; iter < minCount; ++iter)
            {
               if (result.Count > 0)
               {
                  var separatorResult = separator (state);

                  if (separatorResult.State.HasError ())
                  {
                     return separatorResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
               }

               var parserResult = parser (state);

               if (parserResult.State.HasError ())
               {
                  return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
               }

               result.Add (parserResult.Value);
            }

            // Collect optional

            for (var iter = minCount; iter < maxCount; ++iter)
            {
               if (result.Count > 0)
               {
                  var separatorResult = separator (state);

                  if (separatorResult.State.HasFatalError ())
                  {
                     return separatorResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
                  else if (separatorResult.State.HasError ())
                  {
                     break;
                  }

               }

               var parserResult = parser (state);

               if (parserResult.State.HasFatalError ())
               {
                  return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
               }
               else if (parserResult.State.HasError ())
               {
                  break;
               }

               result.Add (parserResult.Value);
            }

            return ParserReply<TValue[]>.Success (state, result.ToArray ());
         };
      }

      public static Parser<TValue[]>.Function Many<TValue> (
         this Parser<TValue>.Function parser, 
         int minCount = 0, 
         int maxCount = int.MaxValue
         )
      {
         VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var initialPosition = state.Position;

            var result = new List<TValue> (Math.Max (minCount, 16));

            // Collect required

            for (var iter = 0; iter < minCount; ++iter)
            {
               var parserResult = parser (state);

               if (parserResult.State.HasError ())
               {
                  return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
               }

               result.Add (parserResult.Value);
            }

            // Collect optional

            for (var iter = minCount; iter < maxCount; ++iter)
            {
               var parserResult = parser (state);

               if (parserResult.State.HasFatalError ())
               {
                  return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
               }
               else if (parserResult.State.HasError ())
               {
                  break;
               }

               result.Add (parserResult.Value);
            }

            return ParserReply<TValue[]>.Success (state, result.ToArray ());
         };
      }

      public static Parser<TValue>.Function Choice<TValue> (
         params Parser<TValue>.Function[] parserFunctions
         )
      {
         if (parserFunctions == null)
         {
            throw new ArgumentNullException ("parserFunctions");
         }

         if (parserFunctions.Length == 0)
         {
            throw new ArgumentOutOfRangeException ("parserFunctions", Strings.Parser.Verify_AtLeastOneParserFunctions);
         }

         return state =>
                   {
                      var suppressParserErrorMessageOperations = state.SuppressParserErrorMessageOperations;

                      var potentialErrors =
                         !suppressParserErrorMessageOperations
                           ?  new List<IParserErrorMessage> (parserFunctions.Length)
                           :  null
                           ;

                      foreach (var parserFunction in parserFunctions)
                      {
                         var result = parserFunction (state);

                         if (result.State.IsSuccessful ())
                         {
                            return result;
                         }
                         else if (result.State.HasFatalError ())
                         {
                            return result;
                         }
                         else if (!suppressParserErrorMessageOperations)
                         {
                            potentialErrors.Add (result.ParserErrorMessage);
                         }
                      }

                      if (!suppressParserErrorMessageOperations)
                      {
                         var topGroup = new ParserErrorMessage_Group (potentialErrors.ToArray ());
                         return ParserReply<TValue>.Failure (ParserReply.State.Error_Group, state, topGroup);
                      }

                      return ParserReply<TValue>.Failure (ParserReply.State.Error_Expected, state, ParserErrorMessages.Expected_Choice);
                   };
      }

      public static Parser<TValue1>.Function KeepLeft<TValue1, TValue2> (
         this Parser<TValue1>.Function firstParser, 
         Parser<TValue2>.Function secondParser
         )
      {
         return state =>
                   {
                      var initialPosition = state.Position;

                      var firstResult = firstParser (state);

                      if (firstResult.State.HasError ())
                      {
                         return firstResult;
                      }

                      var secondResult = secondParser (state);

                      if (secondResult.State.HasError ())
                      {
                         return secondResult.Failure<TValue1> ().VerifyConsistency (initialPosition);
                      }

                      return firstResult.Success (secondResult.ParserState);
                   };
      }

      public static Parser<TValue2>.Function KeepRight<TValue1, TValue2> (
         this Parser<TValue1>.Function firstParser, 
         Parser<TValue2>.Function secondParser
         )
      {
         return state =>
                   {
                      var firstResult = firstParser (state);

                      if (firstResult.State.HasError ())
                      {
                         return firstResult.Failure<TValue2> ();
                      }

                      return secondParser (state);
                   };
      }

      public static Parser<TValue>.Function Attempt<TValue> (
         this Parser<TValue>.Function firstParser
         )
      {
         return state =>
                   {
                      var clone = ParserState.Clone (state);

                      var firstResult = firstParser (state);

                      if (!firstResult.State.HasConsistentState ())
                      {
                         return ParserReply<TValue>.Failure (
                            ParserReply.State.Error_StateIsRestored, 
                            clone, 
                            firstResult.ParserErrorMessage
                            );
                      }

                      return firstResult;
                   };
      }

      public static Parser<Optional<TValue>>.Function Opt<TValue> (
         this Parser<TValue>.Function firstParser
         )
      {
         return state =>
         {
            var firstResult = firstParser (state);

            if (firstResult.State.IsSuccessful ())
            {
               return firstResult.Success (Optional.Create (firstResult.Value));
            }

            if (firstResult.State.HasNonFatalError ())
            {
               return firstResult.Success (Optional.Create<TValue> ());
            }

            return firstResult.Failure<Optional<TValue>> ();
         };
      }

      public static Parser<TValue>.Function Between<TValue> (
         this Parser<TValue>.Function middleParser,
         Parser<Empty>.Function preludeParser,
         Parser<Empty>.Function epilogueParser
         )
      {
         return state =>
                   {
                      var initialPosition = state.Position;

                      var preludeResult = preludeParser (state);
                      if (preludeResult.State.HasError ())
                      {
                         return preludeResult.Failure<TValue> ();
                      }

                      var middleResult = middleParser (state);
                      if (middleResult.State.HasError ())
                      {
                         return middleResult.VerifyConsistency (initialPosition);
                      }

                      var epilogueResult = epilogueParser (state);
                      if (epilogueResult.State.HasError ())
                      {
                         return epilogueResult.Failure<TValue> ().VerifyConsistency (initialPosition);
                      }

                      return middleResult.Success (epilogueResult.ParserState);
                   };
      }

      public static Parser<TValue>.Function Except<TValue> (
         this Parser<TValue>.Function parser,
         Parser<Empty>.Function exceptParser
         )
      {
         return state =>
                   {
                      var exceptResult = exceptParser (state);

                      if (exceptResult.State.IsSuccessful ())
                      {
                         return ParserReply<TValue>.Failure (
                            ParserReply.State.Error_Unexpected, 
                            exceptResult.ParserState, 
                            ParserErrorMessages.Message_TODO
                            );
                      }
                      else if (exceptResult.State.HasFatalError ())
                      {
                         return exceptResult.Failure<TValue> ();
                      }

                      return parser (state);
                   };
      }

      internal static void VerifyMinAndMaxCount (int minCount, int maxCount)
      {
         if (minCount > maxCount)
         {
            throw new ArgumentOutOfRangeException ("minCount", Strings.Parser.Verify_MinCountAndMaxCount);
         }
      }
   }
}
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

// ReSharper disable InconsistentNaming

namespace MicroParser
{
   using System;
   using System.Linq;
   using Internal;

   partial interface IParserErrorMessage
   {
      string Description { get; }
      object Value { get; }
   }

   abstract partial class ParserErrorMessage : IParserErrorMessage
   {
      public abstract string Description { get; }
      public abstract object Value { get; }
   }

   static partial class ParserErrorMessages
   {
      [Obsolete]
      public readonly static IParserErrorMessage Message_TODO = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Todo);
      public readonly static IParserErrorMessage Message_Unknown = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Unknown);

      public readonly static IParserErrorMessage Expected_EndOfStream = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Eos);
      public readonly static IParserErrorMessage Expected_Digit = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Digit);
      public readonly static IParserErrorMessage Expected_WhiteSpace = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.WhiteSpace);
      public readonly static IParserErrorMessage Expected_Choice = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Choice);
      public readonly static IParserErrorMessage Expected_Any = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Any);
      public readonly static IParserErrorMessage Expected_Letter = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Letter);
      public readonly static IParserErrorMessage Expected_LineBreak = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.LineBreak);

      public readonly static IParserErrorMessage Unexpected_Eos = new ParserErrorMessage_Unexpected (Strings.ParserErrorMessages.Eos);
   }


   sealed partial class ParserErrorMessage_Message : ParserErrorMessage
   {
      public readonly string Message;

      public ParserErrorMessage_Message (string message)
      {
         Message = message;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Message_1.Form (Message);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Message; }
      }

      public override object Value
      {
         get { return Message; }
      }
   }

   sealed partial class ParserErrorMessage_Expected : ParserErrorMessage
   {
      public readonly string Expected;

      public ParserErrorMessage_Expected (string expected)
      {
         Expected = expected;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Expected_1.Form (Expected);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Expected; }
      }

      public override object Value
      {
         get { return Expected; }
      }
   }

   sealed partial class ParserErrorMessage_Unexpected : ParserErrorMessage
   {
      public readonly string Unexpected;

      public ParserErrorMessage_Unexpected (string unexpected)
      {
         Unexpected = unexpected;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Unexpected_1.Form (Unexpected);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Unexpected; }
      }

      public override object Value
      {
         get { return Unexpected; }
      }
   }

   sealed partial class ParserErrorMessage_Group : ParserErrorMessage
   {
      public readonly IParserErrorMessage[] Group;

      public ParserErrorMessage_Group (IParserErrorMessage[] group)
      {
         Group = group;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Group_1.Form (Group.Select (message => message.ToString ()).Concatenate (Strings.CommaSeparator));
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Group; }
      }

      public override object Value
      {
         get { return Strings.ParserErrorMessages.Group; }
      }
   }
}
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

   sealed partial class ParserFunctionRedirect<TValue>
   {
      public readonly Parser<TValue>.Function Parser;
      public Parser<TValue>.Function ParserRedirect;

      public ParserFunctionRedirect ()
      {
         Parser = state => ParserRedirect (state);
      }
      
   }
}
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
   using MicroParser.Internal;
	partial class Parser
	{
      public static Parser<Tuple<TValue1, TValue2>>.Function Group<TValue1, TValue2> (
            Parser<TValue1>.Function parser1
         ,  Parser<TValue2>.Function parser2
         )
      {
         return state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1 (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2 (state);

            if (result2.State.HasError ())
            {
               return result2.Failure<Tuple<TValue1, TValue2>>().VerifyConsistency (initialPosition);
            }
            return result2.Success (
               Tuple.Create (
                     result1.Value
                  ,  result2.Value
                  ));
         };
      }
      public static Parser<Tuple<TValue1, TValue2, TValue3>>.Function Group<TValue1, TValue2, TValue3> (
            Parser<TValue1>.Function parser1
         ,  Parser<TValue2>.Function parser2
         ,  Parser<TValue3>.Function parser3
         )
      {
         return state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1 (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2 (state);

            if (result2.State.HasError ())
            {
               return result2.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result3 = parser3 (state);

            if (result3.State.HasError ())
            {
               return result3.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            return result3.Success (
               Tuple.Create (
                     result1.Value
                  ,  result2.Value
                  ,  result3.Value
                  ));
         };
      }


   }
}

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
   using Internal;

   using System;
   using System.Diagnostics;

   static partial class ParserReply
   {
      // ReSharper disable InconsistentNaming
      [Flags]
      public enum State
      {
         Successful = 00,
         Error = 10,
         Error_Message = 11,
         Error_Expected = 12,
         Error_Unexpected = 13,
         Error_Group = 14,
         Error_StateIsRestored = 15,
         FatalError = 0x00010000,
         FatalError_Mask = 0x7FFF0000,
         FatalError_Terminate = 0x00010000,
         FatalError_StateIsNotRestored = 0x00020000,
      }
      // ReSharper restore InconsistentNaming

      static ParserReply<TValue> CreateParserReplyFailure<TValue>(ParserState.AdvanceResult advanceResult, ParserState state, IParserErrorMessage parserErrorMessage)
      {
         switch (advanceResult)
         {
            case ParserState.AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (ParserReply.State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState.AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (ParserReply.State.Error, state, parserErrorMessage);
            case ParserState.AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (ParserReply.State.FatalError_StateIsNotRestored | ParserReply.State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState.AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (ParserReply.State.FatalError_StateIsNotRestored | ParserReply.State.Error, state, parserErrorMessage);
            case ParserState.AdvanceResult.Error:
            default:
               return ParserReply<TValue>.Failure (ParserReply.State.Error, state, ParserErrorMessages.Message_Unknown);
         }
      }

      public static ParserReply<TValue> Create<TValue>(
         ParserState.AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         TValue value
         )
      {
         return advanceResult == ParserState.AdvanceResult.Successful 
            ?  ParserReply<TValue>.Success (state, value) 
            :  CreateParserReplyFailure<TValue>(advanceResult, state, parserErrorMessage)
            ;
      }

      public static ParserReply<TValue> Create<TValue>(
         ParserState.AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         Func<TValue> valueCreator
         )
      {
         return advanceResult == ParserState.AdvanceResult.Successful
            ? ParserReply<TValue>.Success (state, valueCreator ())
            : CreateParserReplyFailure<TValue>(advanceResult, state, parserErrorMessage)
            ;
      }      
   }

   partial struct ParserReply<TValue>
   {
      public readonly ParserReply.State State;
      public readonly ParserState ParserState;
      public readonly IParserErrorMessage ParserErrorMessage;

      public readonly TValue Value;

      ParserReply (ParserReply.State state, ParserState parserState, TValue value, IParserErrorMessage parserErrorMessage)
      {
         State = state;
         ParserState = parserState;
         ParserErrorMessage = parserErrorMessage;
         Value = value;
      }

      public static ParserReply<TValue> Success (
         ParserState parserState, 
         TValue value
         )
      {
         return new ParserReply<TValue>(
            ParserReply.State.Successful, 
            parserState, 
            value, 
            null
            );
      }

      public static ParserReply<TValue> Failure (
         ParserReply.State state, 
         ParserState parserState, 
         IParserErrorMessage parserErrorMessage
         )
      {
         Debug.Assert (!state.IsSuccessful ());
         Debug.Assert (parserErrorMessage != null);

         return new ParserReply<TValue>(
            state.IsSuccessful () ? ParserReply.State.Error : state, 
            parserState, 
            default (TValue), 
            parserErrorMessage
            );
      }

      public ParserReply<TValueTo> Failure<TValueTo> ()
      {
         return ParserReply<TValueTo>.Failure (State, ParserState, ParserErrorMessage);
      }

      public ParserReply<TValue> Success (ParserState parserState)
      {
         return Success (parserState, Value);
      }

      public ParserReply<TValueTo> Success<TValueTo> (TValueTo valueTo)
      {
         return ParserReply<TValueTo>.Success (ParserState, valueTo);
      }

      public ParserReply<TValue> Failure (ParserState parserState)
      {
         return Failure (
            State,
            parserState, 
            ParserErrorMessage
            );
      }

      public ParserReply<TValue> VerifyConsistency (ParserStatePosition initialPosition)
      {
         if (
               State.HasError () 
            && ParserState.InternalPosition - initialPosition.Position > 1
            )
         {
            return new ParserReply<TValue>(
               ParserReply.State.FatalError_StateIsNotRestored | State,
               ParserState,
               default (TValue),
               ParserErrorMessage
               );
         }

         return this;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         if (State == ParserReply.State.Successful)
         {
            return new
            {
               State,
               ParserState,
               Value,
            }.ToString ();
            
         }
         else
         {
            return new
            {
               State,
               ParserState,
               ParserErrorMessage,
            }.ToString ();
         }
      }      
#endif

   }
}
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
   abstract partial class BaseParserResult
   {
      public readonly bool IsSuccessful;
      public readonly SubString Unconsumed;
      public readonly string ErrorMessage;

      public bool EndOfStream
      {
         get
         {
            return !(Unconsumed.Begin < Unconsumed.End);
         }
      }

      protected BaseParserResult (bool isSuccessful, SubString unconsumed, string errorMessage)
      {
         IsSuccessful = isSuccessful;
         Unconsumed = unconsumed;
         ErrorMessage = errorMessage ?? Strings.Empty;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         if (IsSuccessful)
         {
            return new
                      {
                         IsSuccessful,
                         Position = Unconsumed.Begin,
                         EndOfStream,
                         Current = !EndOfStream ? new string (Unconsumed[Unconsumed.Begin], 1) : Strings.ParserErrorMessages.Eos,
                         Value = GetValue (),
                      }.ToString ();
         }
         else
         {

            return new
            {
               IsSuccessful,
               Position = Unconsumed.Begin,
               EndOfStream,
               Current = !EndOfStream ? new string (Unconsumed[Unconsumed.Begin], 1) : Strings.ParserErrorMessages.Eos,
               ErrorMessage,
            }.ToString ();
         }
      }
#endif

      protected abstract object GetValue ();
   }

   sealed partial class ParserResult<TValue> : BaseParserResult
   {
      public readonly TValue Value;

      public ParserResult (bool isSuccessful, SubString subString, string errorMessage, TValue value)
         : base (isSuccessful, subString, errorMessage)
      {
         Value = value;
      }

      protected override object GetValue ()
      {
         return Value;
      }
   }
}
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
   using System.Diagnostics;

   partial struct ParserStatePosition
   {
      public readonly int Position;

      public ParserStatePosition (int position)
      {
         Position = position;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      Position,
                   }.ToString ();
      }
#endif

   }

   sealed partial class ParserState
   {
      // ReSharper disable InconsistentNaming
      public enum AdvanceResult
      {
         Successful = 00,
         Error = 10,
         Error_EndOfStream = 11,
         Error_SatisfyFailed = 12,
         Error_EndOfStream_PostionChanged = 23,
         Error_SatisfyFailed_PositionChanged = 24,
      }
      // ReSharper restore InconsistentNaming

      readonly string m_text;
      int m_position;

      public readonly bool SuppressParserErrorMessageOperations;

      ParserState (int position, string text, bool suppressParserErrorMessageOperations)
      {
         m_position = position;
         m_text = text;
         SuppressParserErrorMessageOperations = suppressParserErrorMessageOperations;
      }

      internal int InternalPosition
      {
         get
         {
            return m_position;
         }
      }

      public ParserStatePosition Position
      {
         get
         {
            return new ParserStatePosition (m_position);
         }
      }

      public bool EndOfStream
      {
         get
         {
            return !(m_position < m_text.Length);
         }
      }

      public ParserState.AdvanceResult Advance (
         ref SubString subString,
         CharSatisfy.Function satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert (minCount <= maxCount);

         var localSatisfy = satisfy ?? CharParser.SatisyAnyChar.Satisfy;

         subString.Value = m_text;
         subString.Position = m_position;

         if (m_position + minCount >= m_text.Length + 1)
         {
            return ParserState.AdvanceResult.Error_EndOfStream;
         }

         var length = Math.Min (maxCount, m_text.Length - m_position);
         for (var iter = 0; iter < length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy (c, iter))
            {
               if (iter < minCount)
               {
                  return subString.Position == m_position
                            ? ParserState.AdvanceResult.Error_SatisfyFailed
                            : ParserState.AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               subString.Length = m_position - subString.Position;

               return ParserState.AdvanceResult.Successful;
            }

            ++m_position;
         }

         subString.Length = m_position - subString.Position;

         return ParserState.AdvanceResult.Successful;
      }

      public ParserState.AdvanceResult SkipAdvance (
         CharSatisfy.Function satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         var subString = new SubString ();
         return Advance (ref subString, satisfy, minCount, maxCount);
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      Position = m_position,
                      SuppressParserErrorMessageOperations,
                      EndOfStream,
                      Current = !EndOfStream ? new string (m_text[m_position], 1) : Strings.ParserErrorMessages.Eos,
                   }.ToString ();
      }
#endif

      public static ParserState Create (
         string text, 
         int position = 0, 
         bool suppressParserErrorMessageOperations = false
         )
      {
         return new ParserState (
            Math.Max (position, 0), 
            text ?? Strings.Empty,
            suppressParserErrorMessageOperations
            );
      }

      public static ParserState Clone (ParserState parserState)
      {
         return new ParserState (
            parserState.m_position, 
            parserState.m_text, 
            parserState.SuppressParserErrorMessageOperations
            );
      }

   }
}
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

// ReSharper disable InconsistentNaming

namespace MicroParser
{
   using System;

   static partial class Strings
   {
      public const string CommaSeparator = ", ";
      public const string Empty = "";

      public static class Parser
      {
         public const string ErrorMessage_2 = "{0} : {1}";
         public const string Verify_AtLeastOneParserFunctions = "parserFunctions should contain at least 1 item";
         public const string Verify_MinCountAndMaxCount = "minCount need to be less or equal to maxCount";
      }

      public static class CharSatisfy
      {
         public const string FormatChar_1 = "'{0}'";
      }

      public static class ParserErrorMessages
      {
         public const string Message_1 = "Message:{0}";
         public const string Expected_1 = "Expected:{0}";
         public const string Unexpected_1 = "Unexpected:{0}";
         public const string Group_1 = "Group:{0}";

         [Obsolete]
         public const string Todo = "TODO:";
         public const string Unexpected = "unexpected ";
         public const string Unknown = "unknown error";
         public const string Eos = "end of stream";
         public const string WhiteSpace = "whitespace";
         public const string Digit = "digit";
         public const string Letter = "letter";
         public const string Any = "any";
         public const string LineBreak = "linebreak";

         public const string Choice = "multiple choices";
         public const string Message = "message";
         public const string Group = "group";
         public const string Expected = "expected";

      }

   }
}

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
   using System.Diagnostics;

   partial struct SubString : IEquatable<SubString>
   {
      public string Value;
      public int Position;
      public int Length;

      public SubString (string value, int position, int length)
      {
         Value = value;
         Position = position;
         Length = length;
      }

      public SubString (string value, int position)
         :  this (value, position, (value ?? "").Length - position)
      {

      }

      public SubString (string value)
         : this (value, 0, (value ?? "").Length)
      {

      }

      public int EffectiveLength
      {
         get
         {
            return End - Begin;
         }
      }

      public int Begin
      {
         get
         {
            return Math.Max (Position, 0);
         }
      }

      public int End
      {
         get
         {
            return Math.Min (Position + Length, SafeValue.Length);
         }
      }

      string SafeValue
      {
         get
         {
            return Value ?? "";
         }
      }

      public bool Equals (SubString other)
      {

         var value = SafeValue;
         var otherValue = other.SafeValue;

         var effectiveLength = EffectiveLength;
         var effectiveOtherLength = other.EffectiveLength;

         if (effectiveLength != effectiveOtherLength)
         {
            return false;
         }

         var begin = Begin;
         var otherBegin = other.Begin;

         var end = End;
         var otherEnd = other.End;

         var diff = otherBegin - begin;
 
         for (var iter = begin; iter < end; ++iter)
         {
            if (value[iter] != otherValue[iter + diff])
            {
               return false;
            }
         }

         return true;
      }

      public override string ToString ()
      {
         return SafeValue.Substring (Begin, EffectiveLength);
      }

      public char this[int index]
      {
         get
         {
            var realIndex = Position + index;
            return realIndex > -1 && realIndex < Value.Length 
               ?  Value[Position + index] 
               :  ' '
               ;
         }
      }

      public override bool Equals (object obj)
      {
         return obj is SubString && Equals ((SubString) obj);
      }

      public override int GetHashCode ()
      {
         var result = 0x55555555;

         var value = SafeValue;

         var end = End;

         for (var iter = Begin; iter < end; ++iter)
         {
            result = (result * 397) ^ value[iter];
         }

         return result;
      }
   }
}
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
#if !MICRO_PARSER_USE_NET4_TUPLE
   static partial class Tuple
   {
      public static Tuple<TValue1, TValue2> Create<TValue1, TValue2> (
            TValue1 value1
         ,  TValue2 value2
         )
      {
         return new Tuple<TValue1, TValue2>
            {
               Item1 = value1 ,
               Item2 = value2 ,
            };
      }
      public static Tuple<TValue1, TValue2, TValue3> Create<TValue1, TValue2, TValue3> (
            TValue1 value1
         ,  TValue2 value2
         ,  TValue3 value3
         )
      {
         return new Tuple<TValue1, TValue2, TValue3>
            {
               Item1 = value1 ,
               Item2 = value2 ,
               Item3 = value3 ,
            };
      }
   }
   partial struct Tuple<TValue1, TValue2>
   {
      public TValue1 Item1;
      public TValue2 Item2;

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
         }.ToString ();
      }
#endif
   }
   partial struct Tuple<TValue1, TValue2, TValue3>
   {
      public TValue1 Item1;
      public TValue2 Item2;
      public TValue3 Item3;

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
            Item3,
         }.ToString ();
      }
#endif
   }
#endif
}

