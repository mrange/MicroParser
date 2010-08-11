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
                        Strings.Parser.ErrorMessage_2.FormatString (
                           messages.Key,
                           messages.Distinct ().Select (message => message.Value.ToString ()).Concatenate (", ")
                           ))
               .Concatenate (", ");

            var subString = new SubString ( 
                     text,
                     parseResultWithErrorInfo.ParserState.InternalPosition
                  );

            var completeErrorResult =
               "Pos: {0} ('{1}') - {2}".FormatString (
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
         Consume,
         Leave,
      }

      public static Parser<TValue> Switch<TValue> (
         SwitchCharacterBehavior switchCharacterBehavior,
         params Tuple<string, Parser<TValue>>[] parserFunctions
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

         var dictionary = parserFunctions
            .SelectMany ((tuple, i) => tuple.Item1.Select (c => Tuple.Create (c, i)))
            .ToDictionary (kv => kv.Item1, kv => kv.Item2);

         var errorMessages = dictionary
            .Select (ch => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (ch.Key)))
            .ToArray ();

         var errorMessage = new ParserErrorMessage_Group (
            errorMessages
            );

         Parser<TValue>.Function function = state =>
                  {
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
                     if (!dictionary.TryGetValue (peekedValue, out index))
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

                     return parserFunctions[index].Item2.Execute (
                        state
                        );
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
                      var clone = ParserState.Clone (state);

                      var firstResult = firstParser.Execute (state);

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