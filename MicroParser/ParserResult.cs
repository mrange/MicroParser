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
                         Current = !EndOfStream ? new string (Text[Position], 1) : Strings.ParserErrorMessages.Eos,
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
               Current = !EndOfStream ? new string (Text[Position], 1) : Strings.ParserErrorMessages.Eos,
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