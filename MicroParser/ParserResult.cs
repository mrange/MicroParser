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
   public abstract class BaseParserResult
   {
      public readonly bool IsSuccessful;
      public readonly string Text;
      public readonly int Position;
      public readonly string ErrorMessage;

      public bool EndOfStream
      {
         get
         {
            return !(Position < Text.Length);
         }
      }

      protected BaseParserResult (bool isSuccessful, string text, int position, string errorMessage)
      {
         IsSuccessful = isSuccessful;
         Position = position;
         Text = text;
         ErrorMessage = errorMessage ?? Strings.Empty;
      }

      public override string ToString ()
      {
         if (IsSuccessful)
         {
            return new
                      {
                         IsSuccessful,
                         Position,
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
               Position,
               EndOfStream,
               Current = !EndOfStream ? new string (Text[Position], 1) : Strings.ParserErrorMessages.Eos,
               ErrorMessage,
            }.ToString ();
         }
      }

      protected abstract object GetValue ();
   }

   public sealed class ParserResult<TValue> : BaseParserResult
   {
      public readonly TValue Value;

      public ParserResult (bool isSuccessful, string text, int position, string errorMessage, TValue value)
         :  base (isSuccessful, text, position, errorMessage)
      {
         Value = value;
      }

      protected override object GetValue ()
      {
         return Value;
      }
   }
}