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
#if MICRO_PARSER_MAKE_PUBLIC
   public delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);
   public delegate bool CharSatisfyFunction (char ch, int index);

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


   // ReSharper disable InconsistentNaming
   public enum ParserState_AdvanceResult
   {
      Successful                             = 00,
      Error                                  = 10,
      Error_EndOfStream                      = 11,
      Error_SatisfyFailed                    = 12,
      Error_EndOfStream_PostionChanged       = 23,
      Error_SatisfyFailed_PositionChanged    = 24,
   }
   // ReSharper restore InconsistentNaming

   public partial class CharParser
   {

   }

   public partial class CharSatify
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

   public partial class Parser
   {
      
   }

   public partial class ParserFunctionRedirect<TValue>
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

#else
   delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);
   delegate bool CharSatisfyFunction (char ch, int index);

   // ReSharper disable InconsistentNaming
   [Flags]
   enum ParserReply_State
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


   // ReSharper disable InconsistentNaming
   enum ParserState_AdvanceResult
   {
      Successful = 00,
      Error = 10,
      Error_EndOfStream = 11,
      Error_SatisfyFailed = 12,
      Error_EndOfStream_PostionChanged = 23,
      Error_SatisfyFailed_PositionChanged = 24,
   }
   // ReSharper restore InconsistentNaming
#endif
}
