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
namespace MicroParser.Internal
{
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;
   using System.Text;

   static partial class Extensions
   {

      // System.String

      public static string Form (this string format, params object[] args)
      {
         return string.Format (CultureInfo.InvariantCulture, format, args);
      }

      public static bool IsNullOrEmpty (this string str)
      {
         return string.IsNullOrEmpty (str);
      }

      // IEnumerable<string>

      public static string Concatenate (
         this IEnumerable<string> strings,
         string delimiter = null,
         string prepend = null,
         string append = null
         )
      {
         var first = true;

         var sb = new StringBuilder (prepend ?? Strings.Empty);

         var del = delimiter ?? Strings.Empty;

         foreach (var value in strings)
         {
            if (first)
            {
               first = false;
            }
            else
            {
               sb.Append (del);
            }
            sb.Append (value);
         }

         sb.Append (append ?? Strings.Empty);
         return sb.ToString ();
      }

      // ParserReply_State

      public static bool IsSuccessful (this ParserReply_State state)
      {
         return state == ParserReply_State.Successful;
      }

      public static bool HasConsistentState (this ParserReply_State state)
      {
         return
            (state & ParserReply_State.FatalError_StateIsNotRestored)
               == 0;
      }

      public static bool HasFatalError (this ParserReply_State state)
      {
         return state >= ParserReply_State.FatalError;
      }

      public static bool HasError (this ParserReply_State state)
      {
         return state >= ParserReply_State.Error;
      }

      public static bool HasNonFatalError (this ParserReply_State state)
      {
         return state >= ParserReply_State.Error && state < ParserReply_State.FatalError;
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

   }
}