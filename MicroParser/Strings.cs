// ReSharper disable InconsistentNaming

using System;

namespace MicroParser
{
   public static class Strings
   {
      public const string CommaSeparator = ", ";
      public const string Null = "<NULL>";
      public const string Empty = "";

      public static class Parser
      {
         public const string ErrorMessage_2 = "{0} : {1}";
         public const string Verify_AtLeastOneParserFunctions = "parserFunctions should contain at least 1 item";
         public const string Verify_MinCountAndMaxCount = "minCount need to be less or equal to maxCount";
      }

      public static class CharSatisfy
      {
         public const string Expect_2 = "{0} except {1}";
         public const string Or_2 = "{0} or {1}";
         public const string Or = " or ";
         public const string ExpectedChar_1 = "'{0}'";
      }

      public static class ParserErrorMessages
      {

         [Obsolete]
         public const string Todo = "TODO:";
         public const string General = "general";
         public const string Unexpected = "unexpected ";
         public const string Unknown = "unknown error";
         public const string Eos = "end of stream";
         public const string WhiteSpace = "whitespace";
         public const string Digit = "digit";
         public const string Letter = "letter";
         public const string Any = "any";
         public const string Choice = "multiple choices";
         public const string Message = "message";
         public const string Group = "group";
         public const string Expected = "expected";

      }

   }
}
