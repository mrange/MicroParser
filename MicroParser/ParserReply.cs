using System;

namespace MicroParser
{
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

   public struct ParserReply<TValue>
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

      public static ParserReply<TValue> Success (ParserState parserState, TValue value)
      {
         return new ParserReply<TValue> (ParserReply_State.Successful, parserState, value, null);
      }

      public static ParserReply<TValue> Failure (ParserReply_State state, ParserState parserState, IParserErrorMessage parserErrorMessage)
      {
         if (parserErrorMessage == null)
         {
            throw new ArgumentNullException ("parserErrorMessage");
         }

         return new ParserReply<TValue>(state, parserState, default (TValue), parserErrorMessage);
      }

      public ParserReply<TValueTo> Failure<TValueTo> ()
      {
         return ParserReply<TValueTo>.Failure (State, ParserState, ParserErrorMessage);
      }

      public ParserReply<TValue> Success (ParserState parserState)
      {
         return Success (parserState, Value);
      }

      public ParserReply<TValue> Failure (ParserState parserState)
      {
         return Failure (State, parserState, ParserErrorMessage);
      }

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

   }
}