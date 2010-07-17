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
   using System;

   public static class Strings
   {
      public const string CommaSeparator = ", ";
      public const string Empty = "";

      public static class Parser
      {
         public const string ErrorMessage_2 = "{0} : {1}";
         public const string Verify_AtLeastOneParserFunctions = "parserFunctions should contain at least 1 item";
         public const string Verify_MinCountAndMaxCount = "minCount need to be less or equal to maxCount";
      }

      public static class CharSatisfy
      {
         public const string ExpectedChar_1 = "'{0}'";
      }

      public static class ParserErrorMessages
      {

         [Obsolete]
         public const string Todo = "TODO:";
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
