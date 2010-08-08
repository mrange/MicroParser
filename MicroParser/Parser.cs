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

   static partial class Parser
   {
      public static ParserResult<TValue> Parse<TValue> (ParserFunction<TValue> parserFunction, string text)
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

      public static ParserFunction<TValue> Return<TValue> (TValue value)
      {
         return state => ParserReply<TValue>.Success (state, value);
      }

      public static ParserFunction<TValue> Fail<TValue> (string message)
      {
         var parserErrorMessageMessage = new ParserErrorMessage_Message (message);
         return state => ParserReply<TValue>.Failure (ParserReply_State.Error, state, parserErrorMessageMessage);
      }

      public static ParserFunction<Empty> EndOfStream ()
      {
         return state =>
                state.EndOfStream
                   ? ParserReply<Empty>.Success (state, Empty.Value)
                   : ParserReply<Empty>.Failure (
                      ParserReply_State.Error_Expected,
                      state,
                      ParserErrorMessages.Expected_EndOfStream
                      );
      }

      public static ParserFunction<TValue2> Combine<TValue, TValue2> (this ParserFunction<TValue> firstParser, Func<TValue, ParserFunction<TValue2>> second)
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

      public static ParserFunction<TValue2> Map<TValue1, TValue2> (this ParserFunction<TValue1> firstParser, Func<TValue1, TValue2> mapper)
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

      public static ParserFunction<TValue2> Map<TValue1, TValue2> (this ParserFunction<TValue1> firstParser, TValue2 value2)
      {
         return firstParser.Map (ignore => value2);
      }

      public static ParserFunction<TValue1> Chain<TValue1, TValue2>(
         this ParserFunction<TValue1> parser,
         ParserFunction<TValue2> separator,
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

      public static ParserFunction<TValue[]> Array<TValue> (
         this ParserFunction<TValue> parser,
         ParserFunction<Empty> separator,
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

      public static ParserFunction<TValue[]> Many<TValue> (
         this ParserFunction<TValue> parser, 
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

      public static ParserFunction<TValue> Choice<TValue> (
         params ParserFunction<TValue>[] parserFunctions
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
                         return ParserReply<TValue>.Failure (ParserReply_State.Error_Group, state, topGroup);
                      }

                      return ParserReply<TValue>.Failure (ParserReply_State.Error_Expected, state, ParserErrorMessages.Expected_Choice);
                   };
      }

      public static ParserFunction<TValue1> KeepLeft<TValue1, TValue2> (
         this ParserFunction<TValue1> firstParser, 
         ParserFunction<TValue2> secondParser
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

      public static ParserFunction<TValue2> KeepRight<TValue1, TValue2> (
         this ParserFunction<TValue1> firstParser, 
         ParserFunction<TValue2> secondParser
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

      public static ParserFunction<TValue> Attempt<TValue> (
         this ParserFunction<TValue> firstParser
         )
      {
         return state =>
                   {
                      var clone = ParserState.Clone (state);

                      var firstResult = firstParser (state);

                      if (!firstResult.State.HasConsistentState ())
                      {
                         return ParserReply<TValue>.Failure (
                            ParserReply_State.Error_StateIsRestored, 
                            clone, 
                            firstResult.ParserErrorMessage
                            );
                      }

                      return firstResult;
                   };
      }

      public static ParserFunction<Optional<TValue>> Opt<TValue> (
         this ParserFunction<TValue> firstParser
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

      public static ParserFunction<TValue> Between<TValue> (
         this ParserFunction<TValue> middleParser,
         ParserFunction<Empty> preludeParser,
         ParserFunction<Empty> epilogueParser
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

      public static ParserFunction<TValue> Except<TValue> (
         this ParserFunction<TValue> parser,
         ParserFunction<Empty> exceptParser
         )
      {
         return state =>
                   {
                      var exceptResult = exceptParser (state);

                      if (exceptResult.State.IsSuccessful ())
                      {
                         return ParserReply<TValue>.Failure (
                            ParserReply_State.Error_Unexpected, 
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