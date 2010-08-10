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
   using System.Collections.Generic;
   using System.Linq;

   static partial class Extensions
   {
      // ParserReply.State

      public static bool IsSuccessful (this ParserReply.State state)
      {
         return state == ParserReply.State.Successful;
      }

      public static bool HasConsistentState (this ParserReply.State state)
      {
         return
            (state & ParserReply.State.FatalError_StateIsNotRestored)
               == 0;
      }

      public static bool HasFatalError (this ParserReply.State state)
      {
         return state >= ParserReply.State.FatalError;
      }

      public static bool HasError (this ParserReply.State state)
      {
         return state >= ParserReply.State.Error;
      }

      public static bool HasNonFatalError (this ParserReply.State state)
      {
         return state >= ParserReply.State.Error && state < ParserReply.State.FatalError;
      }

      // IParserErrorMessage

      public static IEnumerable<IParserErrorMessage> DeepTraverse (this IParserErrorMessage value)
      {
         if (value == null)
         {
            yield break;
         }

         var stack = new Stack<IParserErrorMessage> ();
         stack.Push (value);


         while (stack.Count > 0)
         {
            var pop = stack.Pop ();

            var parserErrorMessageGroup = pop as ParserErrorMessage_Group;

            if (parserErrorMessageGroup != null && parserErrorMessageGroup.Group != null)
            {
               foreach (var parserErrorMessage in parserErrorMessageGroup.Group)
               {
                  stack.Push (parserErrorMessage);
               }
            }
            else if (pop != null)
            {
               yield return pop;
            }
         }

      }

      public static IParserErrorMessage Append (this IParserErrorMessage left, IParserErrorMessage right)
      {
         return new ParserErrorMessage_Group (
            left.DeepTraverse ().Concat (right.DeepTraverse ()).ToArray ()
            );
      }

      // CharSatisfy

#if !MICRO_PARSER_SUPPRESS_EXTENSIONS_OR
      public static CharSatisfy Or (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (second.ErrorMessage),
            (c, i) => first.Satisfy (c, i) || second.Satisfy (c, i)
            );
      }
#endif

#if !MICRO_PARSER_SUPPRESS_EXTENSIONS_EXCEPT
      static IParserErrorMessage ExpectedToUnexpected (
         IParserErrorMessage parserErrorMessage
         )
      {
         var parserErrorMessageExpected = parserErrorMessage as ParserErrorMessage_Expected;
         return parserErrorMessageExpected != null
            ? new ParserErrorMessage_Unexpected (parserErrorMessageExpected.Expected)
            : parserErrorMessage
            ;
      }

      public static CharSatisfy Except (this CharSatisfy first, CharSatisfy second)
      {
         return new CharSatisfy (
            first.ErrorMessage.Append (ExpectedToUnexpected (second.ErrorMessage)),
            (c, i) => first.Satisfy (c, i) && !second.Satisfy (c, i)
            );
      }
#endif
   }
}