﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroParser
{

   public static class Parser
   {
      public static ParserResult<TValue> Parse<TValue> (ParserFunction<TValue> parserFunction, string text)
      {
         var parseResult = parserFunction (ParserState.Create (text ?? Strings.Empty));

         if (parseResult.State.IsSuccessful ())
         {
            return new ParserResult<TValue> (
               true,
               text,
               parseResult.ParserState.Position,
               Strings.Empty,
               parseResult.Value
               );
         }
         else
         {
            var errorResult = parseResult
               .ParserErrorMessage
               .DeepTraverse ()
               .Select (msg => new {Type = msg.Description, msg.Value})
               .GroupBy (tuple => tuple.Type)
               .Select (x =>
                       Strings.Parser.ErrorMessage_2.Form (
                           x.Key,
                           x.Distinct ().Select (y => y.Value.ToString ()).Concatenate (", ")
                           ))
               .Concatenate (", ");
               
            return new ParserResult<TValue> (
               false,
               text,
               parseResult.ParserState.Position,
               errorResult,
               default (TValue)
               );
         }
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

      public static ParserFunction<TValue1> Chain<TValue1, TValue2> (
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

      public static ParserFunction<TValue[]> Many<TValue> (
         this ParserFunction<TValue> parser, 
         int minCount = 0, 
         int maxCount = int.MaxValue
         )
      {
         VerifyMinAndMaxCount (minCount, maxCount);

         return state =>
         {
            var result = new List<TValue> (Math.Max (minCount, 16));

            // Collect required

            for (var iter = 0; iter < minCount; ++iter)
            {
               var parserResult = parser (state);

               if (parserResult.State.HasError ())
               {
                  return parserResult.Failure<TValue[]> ();
               }

               result.Add (parserResult.Value);
            }

            // Collect optional

            for (var iter = minCount; iter < maxCount; ++iter)
            {
               var parserResult = parser (state);

               if (parserResult.State.HasFatalError ())
               {
                  return parserResult.Failure<TValue[]> ();
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
                      var firstResult = firstParser (state);

                      if (firstResult.State.HasError ())
                      {
                         return firstResult;
                      }

                      var secondResult = secondParser (state);

                      if (secondResult.State.HasError ())
                      {
                         return secondResult.Failure<TValue1> ();
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

      public static ParserFunction<Tuple<TValue1, TValue2>> Tuple<TValue1, TValue2> (
         ParserFunction<TValue1> firstParser, 
         ParserFunction<TValue2> secondParser
         )
      {
         return state =>
         {
            var firstResult = firstParser (state);

            if (firstResult.State.HasError ())
            {
               return firstResult.Failure<Tuple<TValue1, TValue2>> ();
            }

            var secondResult = secondParser (state);

            if (secondResult.State.HasError ())
            {
               return secondResult.Failure<Tuple<TValue1, TValue2>> ();
            }


            return secondResult.Success (
               System.Tuple.Create (
                  firstResult.Value,
                  secondResult.Value
                  ));
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
                         return ParserReply<TValue>.Failure (ParserReply_State.Error_StateIsRestored, clone, firstResult.ParserErrorMessage);
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
                      var preludeResult = preludeParser (state);
                      if (preludeResult.State.HasError ())
                      {
                         return preludeResult.Failure<TValue> ();
                      }

                      var middleResult = middleParser (state);
                      if (middleResult.State.HasError ())
                      {
                         return middleResult;
                      }

                      var epilogueResult = epilogueParser (state);
                      if (preludeResult.State.HasError ())
                      {
                         return epilogueResult.Failure<TValue> ();
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

      internal static ParserReply<TValue> ToParserReply<TValue> (
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         TValue defaultValue
         )
      {
         switch (advanceResult)
         {
            case ParserState_AdvanceResult.Successful:
               return ParserReply<TValue>.Success (state, defaultValue);
            case ParserState_AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState_AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, parserErrorMessage);
            case ParserState_AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error, state, parserErrorMessage);
            case ParserState_AdvanceResult.Error:
            default:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, ParserErrorMessages.Message_Unknown);
         }
      }

      internal static ParserReply<TValue> ToParserReply<TValue> (
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         Func<TValue> valueCreator
         )
      {
         switch (advanceResult)
         {
            case ParserState_AdvanceResult.Successful:
               return ParserReply<TValue>.Success (state, valueCreator ());
            case ParserState_AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState_AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, parserErrorMessage);
            case ParserState_AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error_Unexpected, state, ParserErrorMessages.Unexpected_Eos);
            case ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error, state, parserErrorMessage);
            case ParserState_AdvanceResult.Error:
            default:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, ParserErrorMessages.Message_Unknown);
         }
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