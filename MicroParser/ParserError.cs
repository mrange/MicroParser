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

// ReSharper disable InconsistentNaming

namespace MicroParser
{
   using System;
   using System.Linq;
   using Internal;

   partial interface IParserErrorMessage
   {
      string Description { get; }
      object Value { get; }
   }

   abstract partial class ParserErrorMessage : IParserErrorMessage
   {
      public abstract string Description { get; }
      public abstract object Value { get; }
   }

   static partial class ParserErrorMessages
   {
      [Obsolete]
      public readonly static IParserErrorMessage Message_TODO           = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Todo);
      public readonly static IParserErrorMessage Message_Unknown        = new ParserErrorMessage_Message (Strings.ParserErrorMessages.Unknown);

      public readonly static IParserErrorMessage Expected_EndOfStream   = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Eos);
      public readonly static IParserErrorMessage Expected_Digit         = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Digit);
      public readonly static IParserErrorMessage Expected_HexDigit      = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.HexDigit);
      public readonly static IParserErrorMessage Expected_WhiteSpace    = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.WhiteSpace);
      public readonly static IParserErrorMessage Expected_Choice        = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Choice);
      public readonly static IParserErrorMessage Expected_Any           = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Any);
      public readonly static IParserErrorMessage Expected_Letter        = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.Letter);
      public readonly static IParserErrorMessage Expected_LineBreak     = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.LineBreak);

      public readonly static IParserErrorMessage Unexpected_Eos         = new ParserErrorMessage_Unexpected (Strings.ParserErrorMessages.Eos);
   }


   sealed partial class ParserErrorMessage_Message : ParserErrorMessage
   {
      public readonly string Message;

      public ParserErrorMessage_Message (string message)
      {
         Message = message;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Message_1.FormatWith (Message);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Message; }
      }

      public override object Value
      {
         get { return Message; }
      }
   }

   sealed partial class ParserErrorMessage_Expected : ParserErrorMessage
   {
      public readonly string Expected;

      public ParserErrorMessage_Expected (string expected)
      {
         Expected = expected;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Expected_1.FormatWith (Expected);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Expected; }
      }

      public override object Value
      {
         get { return Expected; }
      }
   }

   sealed partial class ParserErrorMessage_Unexpected : ParserErrorMessage
   {
      public readonly string Unexpected;

      public ParserErrorMessage_Unexpected (string unexpected)
      {
         Unexpected = unexpected;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Unexpected_1.FormatWith (Unexpected);
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Unexpected; }
      }

      public override object Value
      {
         get { return Unexpected; }
      }
   }

   sealed partial class ParserErrorMessage_Group : ParserErrorMessage
   {
      public readonly IParserErrorMessage[] Group;

      public ParserErrorMessage_Group (IParserErrorMessage[] group)
      {
         Group = group;
      }

      public override string ToString ()
      {
         return Strings.ParserErrorMessages.Group_1.FormatWith (Group.Select (message => message.ToString ()).Concatenate (Strings.CommaSeparator));
      }

      public override string Description
      {
         get { return Strings.ParserErrorMessages.Group; }
      }

      public override object Value
      {
         get { return Strings.ParserErrorMessages.Group; }
      }
   }
}