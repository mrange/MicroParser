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
namespace MicroParser
{
   using Internal;

   using System;
   using System.Diagnostics;

   // ReSharper disable InconsistentNaming
   [Flags]
   public enum ParserReply_State
   {
      Successful                       = 00,
      Error                            = 10,
      Error_Message                    = 11,
      Error_Expected                   = 12,
      Error_Unexpected                 = 13,
      Error_Group                      = 14,
      Error_StateIsRestored            = 15,
      FatalError                       = 0x00010000,
      FatalError_Mask                  = 0x7FFF0000,
      FatalError_Terminate             = 0x00010000,
      FatalError_StateIsNotRestored    = 0x00020000,
   }
   // ReSharper restore InconsistentNaming

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
         return new ParserReply<TValue> (
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

         return new ParserReply<TValue> (
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
            return new ParserReply<TValue> (
               ParserReply_State.FatalError_StateIsNotRestored | State,
               ParserState,
               default (TValue),
               ParserErrorMessage
               );
         }

         return this;
      }

#if !SUPPRESS_ANONYMOUS_TYPE
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