using System;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace MicroParser
{
   public interface IParserErrorMessage
   {
   }

   public abstract class ParserErrorMessage : IParserErrorMessage
   {
   }

   public static class ParserErrorMessages
   {
      [Obsolete]
      public readonly static IParserErrorMessage Message_TODO = new ParserErrorMessage_Message(Strings.ParserErrorMessages.Todo);
      public readonly static IParserErrorMessage Message_Unknown = new ParserErrorMessage_Message(Strings.ParserErrorMessages.Unknown);

      public readonly static IParserErrorMessage Expected_EndOfStream = new ParserErrorMessage_Expected(Strings.ParserErrorMessages.Eos);
      public readonly static IParserErrorMessage Expected_Digit = new ParserErrorMessage_Expected(Strings.ParserErrorMessages.Digit);
      public readonly static IParserErrorMessage Expected_WhiteSpace = new ParserErrorMessage_Expected (Strings.ParserErrorMessages.WhiteSpace);
      public readonly static IParserErrorMessage Expected_Choice = new ParserErrorMessage_Expected(Strings.ParserErrorMessages.Choice);
      public readonly static IParserErrorMessage Expected_Any = new ParserErrorMessage_Expected(Strings.ParserErrorMessages.Any);

      public readonly static IParserErrorMessage Unexpected_General = new ParserErrorMessage_Unexpected(Strings.ParserErrorMessages.General);
      public readonly static IParserErrorMessage Unexpected_Eos = new ParserErrorMessage_Unexpected(Strings.ParserErrorMessages.Eos);
   }


   public sealed class ParserErrorMessage_Message : ParserErrorMessage
   {
      public readonly string Message;

      public ParserErrorMessage_Message (string message)
      {
         Message = message;
      }

      public override string ToString ()
      {
         return new
                   {
                      Message,
                   }.ToString ();
      }
   }

   public sealed class ParserErrorMessage_Expected : ParserErrorMessage
   {
      public readonly string Expected;

      public ParserErrorMessage_Expected (string expected)
      {
         Expected = expected;
      }

      public override string ToString ()
      {
         return new
         {
            Expected,
         }.ToString ();
      }
   }

   public sealed class ParserErrorMessage_Unexpected : ParserErrorMessage
   {
      public readonly string Unexpected;

      public ParserErrorMessage_Unexpected (string unexpected)
      {
         Unexpected = unexpected;
      }

      public override string ToString ()
      {
         return new
         {
            Unexpected,
         }.ToString ();
      }
   }

   public sealed class ParserErrorMessage_Group : ParserErrorMessage
   {
      public readonly IParserErrorMessage[] Group;

      public ParserErrorMessage_Group(IParserErrorMessage[] group)
      {
         Group = group;
      }

      public override string ToString ()
      {
         return new
            {
               Group = Group.Select (message => message.ToString ()).Concatenate (Strings.CommaSeparator),
            }.ToString ();
      }
   }
}