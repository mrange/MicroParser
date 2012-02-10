
#define MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE

#define MICRO_PARSER_SUPPRESS_PARSER_CHAIN
#define MICRO_PARSER_SUPPRESS_PARSER_COMBINE
#define MICRO_PARSER_SUPPRESS_PARSER_EXCEPT
#define MICRO_PARSER_SUPPRESS_PARSER_FAIL
#define MICRO_PARSER_SUPPRESS_PARSER_FAIL_WITH_EXPECTED

#define MICRO_PARSER_SUPPRESS_CHAR_PARSER_MANY_CHAR_SATISFY_2
#define MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_NEW_LINE
#define MICRO_PARSER_SUPPRESS_CHAR_PARSER_SKIP_NONE_OF

#define MICRO_PARSER_SUPPRESS_CHAR_SATISFY_COMPOSITES

#define MICRO_PARSER_SUPPRESS_EXTENSIONS_EXCEPT
#define MICRO_PARSER_SUPPRESS_EXTENSIONS_OR



namespace Include
{
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
         var parserErrorMessage = new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatWith (toSkip));

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
               ParserErrorMessages.Expected_HexDigit,
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
}
namespace Include
{
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
            new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatWith (ch)), 
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

#if WINDOWS_PHONE
         // Windows Phone is basically .NET35 but lacks the HashSet class.
         // Approximate with Dictionary<>
         var dictionary = match.ToDictionary (v => v, v => true);
         return (c, i) => dictionary.ContainsKey (c) ? matchResult : !matchResult;
#else
         var hashSet = new HashSet<char> (match);
         return (c, i) => hashSet.Contains (c) ? matchResult : !matchResult;
#endif
      }
#else
      static Function CreateSatisfyFunctionForAnyOfOrNoneOf (
         string match,
         bool matchResult
         )
      {
         // For input string "Test" this generates the equivalent code to
         // Func<char, int> d = (ch, index) => 
         // {
         //    bool result;
         //    switch (ch)
         //    {
         //       case 'T':
         //       case 'e':
         //       case 's':
         //       case 't':
         //          result = matchResult;
         //          break;
         //       default:
         //          result = !matchResult;
         //          break;
         //    }
         //    return result;
         // }

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
         if (match.IsNullOrEmpty ())
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
            x => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatWith (x)),
            true
            );
      }

      public static CharSatisfy CreateSatisfyForNoneOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Unexpected (Strings.CharSatisfy.FormatChar_1.FormatWith (x)),
            false
            );
      }
   }
}
}
namespace Include
{
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
}
namespace Include
{
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
   using System.Collections.Generic;
   using System.Linq;

   static partial class Extensions
   {
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

      // CharSatisfy

#if !MICRO_PARSER_SUPPRESS_EXTENSIONS_OR
      public static CharSatisfy Or (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (second.ErrorMessage),
            (c, i) => first.Satisfy (c, i) || second.Satisfy (c, i)
            );
      }
#endif

#if !MICRO_PARSER_SUPPRESS_EXTENSIONS_EXCEPT
      static IParserErrorMessage ExpectedToUnexpected (
         IParserErrorMessage parserErrorMessage
         )
      {
         var parserErrorMessageExpected = parserErrorMessage as ParserErrorMessage_Expected;
         return parserErrorMessageExpected != null
            ? new ParserErrorMessage_Unexpected (parserErrorMessageExpected.Expected)
            : parserErrorMessage
            ;
      }

      public static CharSatisfy Except (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (ExpectedToUnexpected (second.ErrorMessage)),
            (c, i) => first.Satisfy (c, i) && !second.Satisfy (c, i)
            );
      }
#endif
   }
}
}
namespace Include
{
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
   using System.Text;

   static partial class Extensions
   {

      // System.String

      public static string FormatWith (this string format, params object[] args)
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
   }
}
}
namespace Include
{
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

   public partial class Extensions
   {
      
   }

   public partial class Optional
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

   public partial class ParserReply
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

#if MICRO_PARSER_NET35
   public partial class Tuple
   {

   }

   public partial struct Tuple<TValue1, TValue2>
   {

   }

   public partial struct Tuple<TValue1, TValue2, TValue3>
   {

   }
#endif

#endif
}
}
namespace Include
{
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
}
namespace Include
{
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
   using Internal;

   sealed partial class Parser<TValue>
   {
      // ParserState is basically a string with a position
      // ParserReply contains the updated state and the result of the parser
      // operation depending on if the operation was successful
      public delegate ParserReply<TValue> Function (ParserState state);

      public readonly Function Execute;

      public Parser (Function function)
      {
         if (function == null)
         {
            throw new ArgumentNullException ("function");
         }

         Execute = function;
      }

      public static implicit operator Parser<TValue> (Function function)
      {
         return new Parser<TValue> (function);
      }
   }

   static partial class Parser
   {
      public static ParserResult<TValue> Parse<TValue> (Parser<TValue> parserFunction, string text)
      {
         var parseResult = parserFunction.Execute (
            ParserState.Create (
               text ?? Strings.Empty,
               suppressParserErrorMessageOperations:true
               ));

         if (!parseResult.State.IsSuccessful ())
         {
            var parseResultWithErrorInfo = parserFunction.Execute (
               ParserState.Create (
                  text ?? Strings.Empty
                  ));

            var errorResult = parseResultWithErrorInfo
               .ParserErrorMessage
               .DeepTraverse ()
               .GroupBy (msg => msg.Description)
               .Select (messages =>
                        Strings.Parser.ErrorMessage_2.FormatWith (
                           messages.Key,
                           messages.Distinct ().Select (message => message.Value.ToString ()).Concatenate (", ")
                           ))
               .Concatenate (", ");

            var subString = new SubString ( 
                     text,
                     parseResultWithErrorInfo.ParserState.InternalPosition
                  );

            var completeErrorResult =
               "Pos: {0} ('{1}') - {2}".FormatWith (
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

#if !MICRO_PARSER_SUPPRESS_PARSER_REDIRECT
      public static ParserFunctionRedirect<TValue> Redirect<TValue> ()
      {
         return new ParserFunctionRedirect<TValue> ();
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_RETURN
      public static Parser<TValue> Return<TValue> (TValue value)
      {
         Parser<TValue>.Function function = state => ParserReply<TValue>.Success (state, value);
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_FAIL
      public static Parser<TValue> Fail<TValue>(string message)
      {
         var parserErrorMessageMessage = new ParserErrorMessage_Message (message);
         Parser<TValue>.Function function = state => ParserReply<TValue>.Failure (ParserReply.State.Error, state, parserErrorMessageMessage);
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_FAIL_WITH_EXPECTED
      public static Parser<TValue> FailWithExpected<TValue>(this Parser<TValue> parser, string message)
      {
         var parserErrorMessageMessage = new ParserErrorMessage_Expected (message);
         Parser<TValue>.Function function = 
            state => 
               {
                  var reply = parser.Execute (state);
                  if (reply.State.HasError ())
                  {
                     return ParserReply<TValue>.Failure(
                        ParserReply.State.Error_Expected | reply.State & ParserReply.State.FatalError_Mask, 
                        state, 
                        parserErrorMessageMessage
                        );
                  }
                  return reply;
                  
               };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_DEBUG_BREAK
      public static Parser<TValue> DebugBreak<TValue> (this Parser<TValue> parser)
      {
         Parser<TValue>.Function function =
            state =>
               {
                  Debug.Assert (false);
                  return parser.Execute (state);
               };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_END_OF_STREAM
      public static Parser<Empty> EndOfStream ()
      {
         Parser<Empty>.Function function = state =>
                state.EndOfStream
                   ? ParserReply<Empty>.Success (state, Empty.Value)
                   : ParserReply<Empty>.Failure (
                      ParserReply.State.Error_Expected,
                      state,
                      ParserErrorMessages.Expected_EndOfStream
                      );
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_COMBINE
      public static Parser<TValue2> Combine<TValue, TValue2>(this Parser<TValue> firstParser, Func<TValue, Parser<TValue2>> second)
      {
         Parser<TValue2>.Function function = state =>
                   {
                      var firstResult = firstParser.Execute (state);
                      if (firstResult.State.HasError ())
                      {
                         return firstResult.Failure<TValue2> ();
                      }

                      var secondParser = second (firstResult.Value);
                      var secondResult = secondParser.Execute (state);
                      return secondResult;
                   };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_MAP
      public static Parser<TValue2> Map<TValue1, TValue2> (this Parser<TValue1> firstParser, Func<TValue1, TValue2> mapper)
      {
         Parser<TValue2>.Function function = state =>
         {
            var firstResult = firstParser.Execute (state);

            if (firstResult.State.HasError ())
            {
               return firstResult.Failure<TValue2> ();
            }

            return firstResult.Success (mapper (firstResult.Value));
         };
         return function;
      }

      public static Parser<TValue2> Map<TValue1, TValue2> (this Parser<TValue1> firstParser, TValue2 value2)
      {
         return firstParser.Map (ignore => value2);
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_CHAIN
      public static Parser<TValue1> Chain<TValue1, TValue2>(
         this Parser<TValue1> parser,
         Parser<TValue2> separator,
         Func<TValue1, TValue2, TValue1, TValue1> combiner
         )
      {
         Parser<TValue1>.Function function = state =>
            {
               var result = parser.Execute (state);
               if (result.State.HasError ())
               {
                  return result;
               }

               var accu = result.Value;

               ParserReply<TValue2> separatorResult;

               while ((separatorResult = separator.Execute (state)).State.IsSuccessful ())
               {
                  var trailingResult = parser.Execute (state);

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
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_ARRAY
      public static Parser<TValue[]> Array<TValue> (
         this Parser<TValue> parser,
         Parser<Empty> separator,
         bool allowTrailingSeparator = false,
         int minCount = 0,
         int maxCount = int.MaxValue
         )
      {
         VerifyMinAndMaxCount (minCount, maxCount);

         Parser<TValue[]>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result = new List<TValue> (Math.Max (minCount, 16));

            // Collect required

            for (var iter = 0; iter < minCount; ++iter)
            {
               if (result.Count > 0)
               {
                  var separatorResult = separator.Execute (state);

                  if (separatorResult.State.HasError ())
                  {
                     return separatorResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
               }

               var parserResult = parser.Execute (state);

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
                  var separatorResult = separator.Execute (state);

                  if (separatorResult.State.HasFatalError ())
                  {
                     return separatorResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
                  else if (separatorResult.State.HasError ())
                  {
                     break;
                  }

               }               

               var parserResult = parser.Execute (state);

               if (!allowTrailingSeparator && result.Count > 0)
               {
                  // If a separator has been consumed we need to fail on failures
                  if (parserResult.State.HasError())
                  {
                     return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
               }
               else
               {
                  // If a separator has not been consumed we only need to fail on fatal errors
                  if (parserResult.State.HasFatalError ())
                  {
                     return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
                  }
                  else if (parserResult.State.HasError ())
                  {
                     break;
                  }
               }

                result.Add (parserResult.Value);
            }

            return ParserReply<TValue[]>.Success (state, result.ToArray ());
         };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_MANY
      public static Parser<TValue[]> Many<TValue> (
         this Parser<TValue> parser, 
         int minCount = 0, 
         int maxCount = int.MaxValue
         )
      {
         VerifyMinAndMaxCount (minCount, maxCount);

         Parser<TValue[]>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result = new List<TValue> (Math.Max (minCount, 16));

            // Collect required

            for (var iter = 0; iter < minCount; ++iter)
            {
               var parserResult = parser.Execute (state);

               if (parserResult.State.HasError ())
               {
                  return parserResult.Failure<TValue[]> ().VerifyConsistency (initialPosition);
               }

               result.Add (parserResult.Value);
            }

            // Collect optional

            for (var iter = minCount; iter < maxCount; ++iter)
            {
               var parserResult = parser.Execute (state);

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
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_SWITCH
      public enum SwitchCharacterBehavior
      {
         Consume  ,
         Leave    ,
      }

      public struct SwitchCase<TValue>
      {
         public readonly string           Case;  
         public readonly Parser<TValue>   Parser;
         public readonly string           Expected;

         public SwitchCase (string @case, Parser<TValue> parser, string expected) : this()
         {
            Case     = @case     ?? "";
            Parser   = parser    ;
            Expected = expected  ?? "";
         }
      }
      
      public static SwitchCase<TValue> Case<TValue> (
         string @case,
         Parser<TValue> parser,
         string expected = null
         )
      {
         return new SwitchCase<TValue>(@case, parser, expected);
      }

      public static Parser<TValue> Switch<TValue> (
         SwitchCharacterBehavior switchCharacterBehavior,
         params SwitchCase<TValue>[] cases
         )
      {
         if (cases == null)
         {
            throw new ArgumentNullException ("cases");
         }

         if (cases.Length == 0)
         {
            throw new ArgumentOutOfRangeException ("cases", Strings.Parser.Verify_AtLeastOneParserFunctions);
         }

         var caseDictionary = cases
            .SelectMany ((@case, i) => @case.Case.Select (c => Tuple.Create (c, i)))
            .ToDictionary (kv => kv.Item1, kv => kv.Item2);

         var errorMessages = cases
            .SelectMany(
               (@case, i) => @case.Expected.IsNullOrEmpty()
                                ? @case
                                     .Case
                                     .Select(ch => Strings.CharSatisfy.FormatChar_1.FormatWith(ch))
                                : new[] { @case.Expected })
            .Distinct ()
            .Select (message => new ParserErrorMessage_Expected (message))            
            .ToArray();

         var errorMessage = new ParserErrorMessage_Group (
            errorMessages
            );

         Parser<TValue>.Function function = state =>
                  {
                     var initialPosition = state.Position;

                     var peeked = state.PeekChar ();

                     if (peeked == null)
                     {
                        return ParserReply<TValue>.Failure (
                           ParserReply.State.Error_Unexpected,
                           state,
                           ParserErrorMessages.Unexpected_Eos
                           );
                     }

                     var peekedValue = peeked.Value;

                     int index;
                     if (!caseDictionary.TryGetValue (peekedValue, out index))
                     {
                        return ParserReply<TValue>.Failure (
                           ParserReply.State.Error_Expected,
                           state,
                           errorMessage
                           );                        
                     }

                     if (switchCharacterBehavior == SwitchCharacterBehavior.Consume)
                     {
                        // Intentionally ignores result as SkipAdvance can't fail 
                        // in this situation (we know ParserState has at least one character left)
                        state.SkipAdvance (1);
                     }

                     return cases[index].Parser.Execute(
                        state
                        )
                        .VerifyConsistency(initialPosition);

                  };

         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_CHOICE
      public static Parser<TValue> Choice<TValue> (
         params Parser<TValue>[] parserFunctions
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

         Parser<TValue>.Function function = state =>
                   {
                      var suppressParserErrorMessageOperations = state.SuppressParserErrorMessageOperations;

                      var potentialErrors =
                         !suppressParserErrorMessageOperations
                           ?  new List<IParserErrorMessage> (parserFunctions.Length)
                           :  null
                           ;

                      foreach (var parserFunction in parserFunctions)
                      {
                         var result = parserFunction.Execute (state);

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
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_KEEP_LEFT
      public static Parser<TValue1> KeepLeft<TValue1, TValue2> (
         this Parser<TValue1> firstParser, 
         Parser<TValue2> secondParser
         )
      {
         Parser<TValue1>.Function function = state =>
                   {
                      var initialPosition = state.Position;

                      var firstResult = firstParser.Execute (state);

                      if (firstResult.State.HasError ())
                      {
                         return firstResult;
                      }

                      var secondResult = secondParser.Execute (state);

                      if (secondResult.State.HasError ())
                      {
                         return secondResult.Failure<TValue1> ().VerifyConsistency (initialPosition);
                      }

                      return firstResult.Success (secondResult.ParserState);
                   };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_KEEP_RIGHT
      public static Parser<TValue2> KeepRight<TValue1, TValue2> (
         this Parser<TValue1> firstParser, 
         Parser<TValue2> secondParser
         )
      {
         Parser<TValue2>.Function function = state =>
                   {
                      var firstResult = firstParser.Execute (state);

                      if (firstResult.State.HasError ())
                      {
                         return firstResult.Failure<TValue2> ();
                      }

                      return secondParser.Execute (state);
                   };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_ATTEMPT
      public static Parser<TValue> Attempt<TValue>(
         this Parser<TValue> firstParser
         )
      {
         Parser<TValue>.Function function = state =>
                   {
                      var backupPosition = state.InternalPosition;

                      var firstResult = firstParser.Execute (state);

                      if (!firstResult.State.HasConsistentState ())
                      {
                         ParserState.RestorePosition (state, backupPosition);

                         return ParserReply<TValue>.Failure (
                            ParserReply.State.Error_StateIsRestored, 
                            state, 
                            firstResult.ParserErrorMessage
                            );
                      }
#if DEBUG
                      else
                      {
                         Debug.Assert (backupPosition == state.InternalPosition);
                      }
#endif

                      return firstResult;
                   };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_OPT
      public static Parser<Optional<TValue>> Opt<TValue> (
         this Parser<TValue> firstParser
         )
      {
         Parser<Optional<TValue>>.Function function = state =>
         {
            var firstResult = firstParser.Execute (state);

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
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_BETWEEN
      public static Parser<TValue> Between<TValue> (
         this Parser<TValue> middleParser,
         Parser<Empty> preludeParser,
         Parser<Empty> epilogueParser
         )
      {
         Parser<TValue>.Function function = state =>
                   {
                      var initialPosition = state.Position;

                      var preludeResult = preludeParser.Execute (state);
                      if (preludeResult.State.HasError ())
                      {
                         return preludeResult.Failure<TValue> ();
                      }

                      var middleResult = middleParser.Execute (state);
                      if (middleResult.State.HasError ())
                      {
                         return middleResult.VerifyConsistency (initialPosition);
                      }

                      var epilogueResult = epilogueParser.Execute (state);
                      if (epilogueResult.State.HasError ())
                      {
                         return epilogueResult.Failure<TValue> ().VerifyConsistency (initialPosition);
                      }

                      return middleResult.Success (epilogueResult.ParserState);
                   };
         return function;
      }
#endif

#if !MICRO_PARSER_SUPPRESS_PARSER_EXCEPT
      public static Parser<TValue> Except<TValue> (
         this Parser<TValue> parser,
         Parser<Empty> exceptParser
         )
      {
         Parser<TValue>.Function function = state =>
                   {
                      var exceptResult = exceptParser.Execute (state);

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

                      return parser.Execute (state);
                   };
         return function;
      }
#endif

      internal static void VerifyMinAndMaxCount (int minCount, int maxCount)
      {
         if (minCount > maxCount)
         {
            throw new ArgumentOutOfRangeException ("minCount", Strings.Parser.Verify_MinCountAndMaxCount);
         }
      }
   }
}
}
namespace Include
{
// ----------------------------------------------------------------------------------------------
// Copyright (c) M�rten R�nge.
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
      public readonly static IParserErrorMessage Message_TODO           = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Todo);
      public readonly static IParserErrorMessage Message_Unknown        = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Unknown);

      public readonly static IParserErrorMessage Expected_EndOfStream   = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Eos);
      public readonly static IParserErrorMessage Expected_Digit         = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Digit);
      public readonly static IParserErrorMessage Expected_HexDigit      = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.HexDigit);
      public readonly static IParserErrorMessage Expected_WhiteSpace    = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.WhiteSpace);
      public readonly static IParserErrorMessage Expected_Choice        = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Choice);
      public readonly static IParserErrorMessage Expected_Any           = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Any);
      public readonly static IParserErrorMessage Expected_Letter        = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Letter);
      public readonly static IParserErrorMessage Expected_LineBreak     = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.LineBreak);

      public readonly static IParserErrorMessage Unexpected_Eos         = new ParserErrorMessage_Unexpected (Strings.ParserErrorMessages.Eos);
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
         return Strings.ParserErrorMessages.Message_1.FormatWith (Message);
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
         return Strings.ParserErrorMessages.Expected_1.FormatWith (Expected);
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
         return Strings.ParserErrorMessages.Unexpected_1.FormatWith (Unexpected);
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
         return Strings.ParserErrorMessages.Group_1.FormatWith (Group.Select (message => message.ToString ()).Concatenate (Strings.CommaSeparator));
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
}
namespace Include
{
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
      public readonly Parser<TValue> Parser;
      public Parser<TValue> ParserRedirect;

      public ParserFunctionRedirect ()
      {
         Parser<TValue> .Function function = state => ParserRedirect.Execute (state);
         Parser = function;
      }
      
   }
}
}
namespace Include
{
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
#if !MICRO_PARSER_SUPPRESS_PARSER_GROUP_2
      public static Parser<Tuple<TValue1, TValue2>> Group<TValue1, TValue2> (
            Parser<TValue1> parser1
         ,  Parser<TValue2> parser2
         )
      {
         Parser<Tuple<TValue1, TValue2>>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1.Execute (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2.Execute (state);

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
         return function;
      }
#endif
#if !MICRO_PARSER_SUPPRESS_PARSER_GROUP_3
      public static Parser<Tuple<TValue1, TValue2, TValue3>> Group<TValue1, TValue2, TValue3> (
            Parser<TValue1> parser1
         ,  Parser<TValue2> parser2
         ,  Parser<TValue3> parser3
         )
      {
         Parser<Tuple<TValue1, TValue2, TValue3>>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1.Execute (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2.Execute (state);

            if (result2.State.HasError ())
            {
               return result2.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result3 = parser3.Execute (state);

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
         return function;
      }
#endif


   }
}
}
namespace Include
{
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
         Successful                     = 00,
         Error                          = 10,
         Error_Message                  = 11,
         Error_Expected                 = 12,
         Error_Unexpected               = 13,
         Error_Group                    = 14,
         Error_StateIsRestored          = 15,
         Error_Mask                     = 0x0000FFFF,
         FatalError                     = 0x00010000,
         FatalError_Mask                = 0x7FFF0000,
         FatalError_Terminate           = 0x00010000,
         FatalError_StateIsNotRestored  = 0x00020000,
      }
      // ReSharper restore InconsistentNaming

      static ParserReply<TValue> CreateParserReplyFailure<TValue>(ParserState.AdvanceResult advanceResult, ParserState state, IParserErrorMessage parserErrorMessage)
      {
         switch (advanceResult)
         {
            case ParserState.AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState.AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (State.Error, state, parserErrorMessage);
            case ParserState.AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (State.FatalError_StateIsNotRestored | State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState.AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (State.FatalError_StateIsNotRestored | State.Error, state, parserErrorMessage);
            case ParserState.AdvanceResult.Error:
            default:
               return ParserReply<TValue>.Failure (State.Error, state, ParserErrorMessages.Message_Unknown);
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
            ?  ParserReply<TValue>.Success (state, valueCreator ())
            :  CreateParserReplyFailure<TValue>(advanceResult, state, parserErrorMessage)
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
            && ParserState.InternalPosition - initialPosition.Position > 0
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
}
namespace Include
{
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
}
namespace Include
{
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
         Successful                             = 00,
         Error                                  = 10,
         Error_EndOfStream                      = 11,
         Error_SatisfyFailed                    = 12,
         Error_EndOfStream_PostionChanged       = 23,
         Error_SatisfyFailed_PositionChanged    = 24,
      }
      // ReSharper restore InconsistentNaming

      readonly string m_text;
      int m_position;

      public readonly bool SuppressParserErrorMessageOperations;

      ParserState (int position, string text, bool suppressParserErrorMessageOperations)
      {
         m_position = Math.Max (position, 0);
         m_text = text ?? String.Empty;
         SuppressParserErrorMessageOperations = suppressParserErrorMessageOperations;
      }

      internal int InternalPosition
      {
         get
         {
            return m_position;
         }
      }

      public string Text
      {
         get
         {
            return m_text;
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

      public char? PeekChar ()
      {
         if (EndOfStream)
         {
            return null;
         }

         return m_text[m_position];
      }

      public AdvanceResult Advance (
         ref SubString subString,
         CharSatisfy.Function satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert (minCount <= maxCount);

         var localSatisfy = satisfy ?? CharSatisfy.AnyChar.Satisfy;

         subString.Value = m_text;
         subString.Position = m_position;

         /*
          * This optimization is very tempting to do, but this will give the wrong error message
          * The optimization only saves time at the end of stream so it was removed
         if (m_position + minCount >= m_text.Length + 1)
         {
            return AdvanceResult.Error_EndOfStream;
         }
         */ 

         var length = Math.Min (maxCount, m_text.Length - m_position);
         for (var iter = 0; iter < length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy (c, iter))
            {
               if (iter < minCount)
               {
                  return subString.Position == m_position
                            ? AdvanceResult.Error_SatisfyFailed
                            : AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               subString.Length = m_position - subString.Position;

               return AdvanceResult.Successful;
            }

            ++m_position;
         }

         subString.Length = m_position - subString.Position;

         if (length < minCount)
         {
            return subString.Position == m_position
                      ? AdvanceResult.Error_SatisfyFailed
                      : AdvanceResult.Error_SatisfyFailed_PositionChanged
               ;
         }

         return AdvanceResult.Successful;
      }

      public AdvanceResult SkipAdvance (int count)
      {
         if (m_position + count >= m_text.Length + 1)
         {
            return AdvanceResult.Error_EndOfStream;
         }

         m_position += count;

         return AdvanceResult.Successful;
      }

      public AdvanceResult SkipAdvance (
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
         if (parserState == null)
         {
            return null;
         }

         return new ParserState (
            parserState.m_position, 
            parserState.m_text, 
            parserState.SuppressParserErrorMessageOperations
            );
      }

      public static void Restore (ParserState parserState, ParserState clone)
      {
         if (parserState == null)
         {
            return;
         }

         if (clone == null)
         {
            return;
         }

         parserState.m_position = clone.m_position;
      }

       internal static void RestorePosition (ParserState parserState, int position)
      {
          if (parserState == null)
          {
              return;
          }

          parserState.m_position = position;
      }

   }
}
}
namespace Include
{
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
      public const string CommaSeparator  = ", ";
      public const string Empty           = "";

      public static class Parser
      {
         public const string ErrorMessage_2                    = "{0} : {1}";
         public const string Verify_AtLeastOneParserFunctions  = "cases should contain at least 1 item";
         public const string Verify_MinCountAndMaxCount        = "minCount need to be less or equal to maxCount";
      }

      public static class CharSatisfy
      {
         public const string FormatChar_1 = "'{0}'";
      }

      public static class ParserErrorMessages
      {
         public const string Message_1    = "Message:{0}";
         public const string Expected_1   = "Expected:{0}";
         public const string Unexpected_1 = "Unexpected:{0}";
         public const string Group_1      = "Group:{0}";

         [Obsolete]
         public const string Todo         = "TODO:";
         public const string Unexpected   = "unexpected ";
         public const string Unknown      = "unknown error";
         public const string Eos          = "end of stream";
         public const string WhiteSpace   = "whitespace";
         public const string Digit        = "digit";
         public const string HexDigit     = "hexdigit";
         public const string Letter       = "letter";
         public const string Any          = "any";
         public const string LineBreak    = "linebreak";

         public const string Choice       = "multiple choices";
         public const string Message      = "message";
         public const string Group        = "group";
         public const string Expected     = "expected";

      }

   }
}
}
namespace Include
{
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
   using System.Text;

   partial struct SubString : IEquatable<SubString>
   {
      public string Value;
      public int Position;
      public int Length;

      public static implicit operator SubString (string s)
      {
         return new SubString (s);
      }

      public SubString (string value, int position, int length)
      {
         Value = value;
         Position = position;
         Length = length;
      }

      public SubString (string value, int position = 0)
         :  this (value, position, (value ?? Strings.Empty).Length - position)
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
            return Value ?? Strings.Empty;
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

      public static bool operator == (SubString left, SubString right)
      {
         return left.Equals (right);
      }

      public static bool operator != (SubString left, SubString right)
      {
         return !(left == right);
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
}
namespace Include
{
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
#if MICRO_PARSER_NET35
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
}
