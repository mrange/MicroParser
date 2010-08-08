﻿// ----------------------------------------------------------------------------------------------
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
      static ParserReply<TValue> CreateParserReplyFailure<TValue>(ParserState_AdvanceResult advanceResult, ParserState state, IParserErrorMessage parserErrorMessage)
      {
         switch (advanceResult)
         {
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

      public static ParserReply<TValue> Create<TValue>(
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         TValue value
         )
      {
         return advanceResult == ParserState_AdvanceResult.Successful 
            ?  ParserReply<TValue>.Success (state, value) 
            :  CreateParserReplyFailure<TValue>(advanceResult, state, parserErrorMessage)
            ;
      }

      public static ParserReply<TValue> Create<TValue>(
         ParserState_AdvanceResult advanceResult,
         ParserState state,
         IParserErrorMessage parserErrorMessage,
         Func<TValue> valueCreator
         )
      {
         return advanceResult == ParserState_AdvanceResult.Successful
            ? ParserReply<TValue>.Success (state, valueCreator ())
            : CreateParserReplyFailure<TValue>(advanceResult, state, parserErrorMessage)
            ;
      }      
   }

   partial struct ParserReply<TValue>
   {
      public readonly ParserReply_State State;
      public readonly ParserState ParserState;
      public readonly IParserErrorMessage ParserErrorMessage;

      public readonly TValue Value;

      ParserReply (ParserReply_State state, ParserState parserState, TValue value, IParserErrorMessage parserErrorMessage)
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
            ParserReply_State.Successful, 
            parserState, 
            value, 
            null
            );
      }

      public static ParserReply<TValue> Failure (
         ParserReply_State state, 
         ParserState parserState, 
         IParserErrorMessage parserErrorMessage
         )
      {
         Debug.Assert (!state.IsSuccessful ());
         Debug.Assert (parserErrorMessage != null);

         return new ParserReply<TValue>(
            state.IsSuccessful () ? ParserReply_State.Error : state, 
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
               ParserReply_State.FatalError_StateIsNotRestored | State,
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
         if (State == ParserReply_State.Successful)
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