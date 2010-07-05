using System;
using System.Collections.Generic;

namespace MicroParser
{
   public static class Parser
   {

      public static ParserFunction<TValue> Return<TValue>(TValue value)
      {
         return state => ParserReply<TValue>.Success (state, value);
      }

      public static ParserFunction<TValue> Fail<TValue>(string message)
      {
         return state => ParserReply<TValue>.Failure (ParserReply_State.Error, state, new ParserErrorMessage_Message (message));
      }

      public static ParserFunction<Empty> EndOfStream ()
      {
         return state =>
                state.EndOfStream
                   ? ParserReply<Empty>.Success (state, Empty.Value)
                   : ParserReply<Empty>.Failure (
                      ParserReply_State.Error_Expected, 
                      state,
                      new ParserErrorMessage_Expected ("End of stream")
                      );
      }

      public static ParserFunction<TValue2> Combine<TValue, TValue2>(this ParserFunction<TValue> firstParser, Func<TValue, ParserFunction<TValue2>> second)
      {
         return state =>
                   {
                      var firstResult = firstParser (state);
                      if (firstResult.State >= ParserReply_State.Error)
                      {
                         return firstResult.Failure<TValue2>();
                      }

                      var secondParser = second (firstResult.Value);
                      var secondResult = secondParser (state);
                      return secondResult;
                   };
      }

      public static ParserFunction<TValue2> Map<TValue1, TValue2>(this ParserFunction<TValue1> firstParser, Func<TValue1, TValue2> mapper)
      {
         return state =>
         {
            var firstResult = firstParser (state);

            if (firstResult.State != ParserReply_State.Successful)
            {
               return firstResult.Failure<TValue2>();
            }

            return ParserReply<TValue2>.Success (firstResult.ParserState, mapper (firstResult.Value));
         };
      }

      public static ParserFunction<TValue> Choice<TValue>(
         params ParserFunction<TValue>[] parserFunctions
         )
      {
         if (parserFunctions == null)
         {
            throw new ArgumentNullException ("parserFunctions");
         }
         if (parserFunctions.Length == 0)
         {
            throw new ArgumentOutOfRangeException ("parserFunctions", "parserFunctions should contain at least 1 item");
         }

         return state =>
                   {
                      var pos = state.Position;

                      var potentialErrors = new List<IParserErrorMessage> (parserFunctions.Length);

                      foreach (var parserFunction in parserFunctions)
                      {
                         var result = parserFunction (state);

                         if (result.State == ParserReply_State.Successful)
                         {
                            return result;
                         }
                         else if (result.State >= ParserReply_State.FatalError)
                         {
                            return result;
                         }
                         else
                         {
                            potentialErrors.Add (result.ParserErrorMessage);
                         }
                      }

                      var topGroup = new ParserErrorMessage_Group (pos, null);

                      foreach (var potentialError in potentialErrors)
                      {
                         var group = potentialError as ParserErrorMessage_Group;
                         if (group != null)
                         {
                            foreach (var groupMember in ParserErrorMessage.Traverse (group.Group))
                            {
                              topGroup.Append (groupMember);
                            }
                         }
                         else
                         {
                            topGroup.Append (potentialError);
                         }
                      }

                      return ParserReply<TValue>.Failure (ParserReply_State.Error_Group, state, topGroup);
                   };
      }

      public static ParserFunction<TValue1> KeepLeft<TValue1, TValue2>(this ParserFunction<TValue1> firstParser, ParserFunction<TValue2> secondParser)
      {
         return state =>
                   {
                      var firstResult = firstParser (state);

                      if (firstResult.State >= ParserReply_State.Error)
                      {
                         return firstResult;
                      }

                      var secondResult = secondParser (state);

                      if (secondResult.State >= ParserReply_State.Error)
                      {
                         return secondResult.Failure<TValue1>();
                      }

                      return firstResult.Success (secondResult.ParserState);
                   };
      }

      public static ParserFunction<TValue2> KeepRight<TValue1, TValue2>(this ParserFunction<TValue1> firstParser, ParserFunction<TValue2> secondParser)
      {
         return state =>
                   {
                      var firstResult = firstParser (state);

                      if (firstResult.State >= ParserReply_State.Error)
                      {
                         return firstResult.Failure<TValue2>();
                      }

                      return secondParser (state);
                   };
      }

      public static ParserFunction<Tuple<TValue1, TValue2>> Tuple2<TValue1, TValue2>(ParserFunction<TValue1> firstParser, ParserFunction<TValue2> secondParser)
      {
         return state =>
         {
            var firstResult = firstParser (state);

            if (firstResult.State >= ParserReply_State.Error)
            {
               return firstResult.Failure<Tuple<TValue1, TValue2>>();
            }

            var secondResult = secondParser (state);

            if (secondResult.State >= ParserReply_State.Error)
            {
               return secondResult.Failure<Tuple<TValue1, TValue2>>();
            }


            return ParserReply<Tuple<TValue1, TValue2>>.Success (
               secondResult.ParserState,
               Tuple.Create (
                  firstResult.Value,
                  secondResult.Value
                  ));

         };
      }

      public static ParserFunction<TValue> Attempt<TValue>(this ParserFunction<TValue> firstParser)
      {
         return state =>
                   {
                      var clone = ParserState.Clone (state);

                      var firstResult = firstParser (state);

                      if (firstResult.State != ParserReply_State.FatalError_StateIsNotRestored)
                      {
                         return firstResult;
                      }
                      else
                      {
                         return ParserReply<TValue>.Failure (ParserReply_State.Error_StateIsRestored, clone, firstResult.ParserErrorMessage);
                      }
                   };
      }

      public static ParserFunction<TValue> Between<TValue>(
         this ParserFunction<TValue> middleParser,
         ParserFunction<Empty> preludeParser,
         ParserFunction<Empty> epilogueParser
         )
      {
         return state =>
                   {
                      var preludeResult = preludeParser (state);
                      if (preludeResult.State >= ParserReply_State.Error)
                      {
                         return preludeResult.Failure<TValue> ();
                      }

                      var middleResult = middleParser (state);
                      if (middleResult.State >= ParserReply_State.Error)
                      {
                         return middleResult;
                      }

                      var epilogueResult = epilogueParser (state);
                      if (preludeResult.State >= ParserReply_State.Error)
                      {
                         return epilogueResult.Failure<TValue>();
                      }

                      return middleResult;

                   };
      }

      public static ParserFunction<TValue> Except<TValue>(
         this ParserFunction<TValue> parser,
         ParserFunction<Empty> exceptParser
         )
      {
         return state =>
                   {
                      var exceptResult = exceptParser (state);

                      if (exceptResult.State == ParserReply_State.Successful ||
                          exceptResult.State >= ParserReply_State.FatalError)
                      {
                         return exceptResult.Failure<TValue> ();
                      }

                      return parser (state);
                   };
      }

      internal static ParserReply<TValue> ToParserReply<TValue>(
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         Func<string, IParserErrorMessage> parserErrorMessageCreator,
         string errorMessage,
         TValue defaultValue
         )
      {
         switch (advanceResult)
         {
            case ParserState_AdvanceResult.Successful:
               return ParserReply<TValue>.Success (state, defaultValue);
            case ParserState_AdvanceResult.Error:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Unexpected ("Unknown error"));
            case ParserState_AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Message ("End of stream"));
            case ParserState_AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, parserErrorMessageCreator (errorMessage));
            case ParserState_AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Message ("End of stream"));
            case ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error, state, parserErrorMessageCreator (errorMessage));
            default:
               throw new ArgumentOutOfRangeException ();
         }
      }

      internal static ParserReply<TValue> ToParserReply<TValue>(
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         Func<string, IParserErrorMessage> parserErrorMessageCreator,
         string errorMessage,
         Func<TValue> valueCreator
         )
      {
         switch (advanceResult)
         {
            case ParserState_AdvanceResult.Successful:
               return ParserReply<TValue>.Success (state, valueCreator ());
            case ParserState_AdvanceResult.Error:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Unexpected ("Unknown error"));
            case ParserState_AdvanceResult.Error_EndOfStream:
               return ParserReply<TValue>.Failure (ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Unexpected ("End of stream"));
            case ParserState_AdvanceResult.Error_SatisfyFailed:
               return ParserReply<TValue>.Failure (ParserReply_State.Error, state, parserErrorMessageCreator (errorMessage));
            case ParserState_AdvanceResult.Error_EndOfStream_PostionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error_Unexpected, state, new ParserErrorMessage_Message ("End of stream"));
            case ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged:
               return ParserReply<TValue>.Failure (ParserReply_State.FatalError_StateIsNotRestored | ParserReply_State.Error, state, parserErrorMessageCreator (errorMessage));
            default:
               throw new ArgumentOutOfRangeException ();
         }
      }

      internal static void VerifyMinAndMaxCount (int minCount, int maxCount)
      {
         if (minCount > maxCount)
         {
            throw new ArgumentOutOfRangeException ("minCount", "minCount need to be less or equal to maxCount");
         }
      }
   }
}