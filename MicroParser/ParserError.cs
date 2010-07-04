using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroParser
{
   public interface IParserErrorMessage
   {
      IParserErrorMessage Next { get; set; }
   }

   public static class ParserErrorMessageFactory
   {
      public static readonly Func<string, IParserErrorMessage> Message = message => new ParserErrorMessage_Message(message);
      public static readonly Func<string, IParserErrorMessage> Expected = expected => new ParserErrorMessage_Expected(expected);
      public static readonly Func<string, IParserErrorMessage> Unexpected = unexpected => new ParserErrorMessage_Unexpected(unexpected);
      public static readonly Func<int, IParserErrorMessage, IParserErrorMessage> Group = (position, group) => new ParserErrorMessage_Group(position, group);
   }

   public abstract class ParserErrorMessage : IParserErrorMessage
   {
      public IParserErrorMessage Next { get; set; }

      public static IEnumerable<IParserErrorMessage> Traverse(IParserErrorMessage parserErrorMessage)
      {
         while (parserErrorMessage != null)
         {
            yield return parserErrorMessage;
            parserErrorMessage = parserErrorMessage.Next;
         }
      }
   }

   public sealed class ParserErrorMessage_Message : ParserErrorMessage
   {
      public string Message;

      public ParserErrorMessage_Message(string message)
      {
         Message = message;
      }

      public override string ToString()
      {
         return new
                   {
                      Message,
                   }.ToString();
      }
   }

   public sealed class ParserErrorMessage_Expected : ParserErrorMessage
   {
      public string Expected;

      public ParserErrorMessage_Expected(string expected)
      {
         Expected = expected;
      }

      public override string ToString()
      {
         return new
         {
            Expected,
         }.ToString();
      }
   }

   public sealed class ParserErrorMessage_Unexpected : ParserErrorMessage
   {
      public string Unexpected;

      public ParserErrorMessage_Unexpected(string unexpected)
      {
         Unexpected = unexpected;
      }

      public override string ToString()
      {
         return new
         {
            Unexpected,
         }.ToString();
      }
   }

   public sealed class ParserErrorMessage_Group : ParserErrorMessage
   {
      public int Position;
      public IParserErrorMessage Group;

      public ParserErrorMessage_Group(int position, IParserErrorMessage @group)
      {
         Position = position;
         Group = group;
      }

      public override string ToString()
      {
         return new
                   {
                      Position,
                      Group = Traverse(Group).Select(message => message.ToString()).Concatenate(","),
                   }.ToString();
      }
   }
}