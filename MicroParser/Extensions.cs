using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MicroParser
{
   static class Extensions
   {

      // System.String

      public static string Form(this string format, params object[] args)
      {
         return string.Format (CultureInfo.InvariantCulture, format, args);
      }

      public static bool IsNullOrEmpty (this string str)
      {
         return string.IsNullOrEmpty (str);
      }

      // IEnumerable<string>

      public static string Concatenate(
         this IEnumerable<string> strings,
         string delimiter = null,
         string prepend = null,
         string append = null
         )
      {
         var first = true;

         var sb = new StringBuilder (prepend ?? Strings.Null);

         var del = delimiter ?? Strings.Null;

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

         sb.Append (append ?? Strings.Null);
         return sb.ToString ();
      }

      // ImmutableList<TValue>

      public static ImmutableList<TValue> Cons<TValue>(this ImmutableList<TValue> immutableList, TValue value)
      {
         return ImmutableList<TValue>.Cons(value, immutableList);
      }

      public static ImmutableList<TValue> Cons<TValue>(this ImmutableList<TValue> immutableList, IEnumerable<TValue> values)
      {
         var accu = immutableList;

         foreach (var value in values)
         {
            accu = immutableList.Cons (value);
         }

         return accu;
      }

      // CharSatisfy

      public static CharSatify Or(this CharSatify first, CharSatify second)
      {
         return new CharSatify(
            Strings.CharSatisfy.Or_2.Form(
               first.Expected,
               second.Expected),
            (c, i) => first.Satisfy(c, i) || second.Satisfy(c, i)
            );
      }

      public static CharSatify Except(this CharSatify first, CharSatify second)
      {
         return new CharSatify(
            Strings.CharSatisfy.Expect_2.Form(
               first.Expected,
               second.Expected),
            (c, i) => first.Satisfy(c, i) && !second.Satisfy(c, i)
            );
      }

      // ParserReply_State

      public static bool IsSuccessful(this ParserReply_State state)
      {
         return state == ParserReply_State.Successful;
      }

      public static bool HasConsistentState(this ParserReply_State state)
      {
         return
            (state & ParserReply_State.FatalError_StateIsNotRestored)
               == 0;
      }

      public static bool HasFatalError(this ParserReply_State state)
      {
         return state >= ParserReply_State.FatalError;
      }

      public static bool HasError(this ParserReply_State state)
      {
         return state >= ParserReply_State.Error;
      }

      public static bool HasNonFatalError(this ParserReply_State state)
      {
         return state >= ParserReply_State.Error && state < ParserReply_State.FatalError;
      }

      // IParserErrorMessage

      public static IParserErrorMessage Append (this IParserErrorMessage left, IParserErrorMessage right)
      {
         var lg = left as ParserErrorMessage_Group;
         var rg = left as ParserErrorMessage_Group;

         if (lg != null && rg != null)
         {
            return new ParserErrorMessage_Group (
               Math.Min (lg.Position, rg.Position), 
               lg.Group.Cons (rg.Group)
               );
         }

         if (lg != null && right != null)
         {
            return new ParserErrorMessage_Group(
               lg.Position,
               lg.Group.Cons(right)
               );
         }

         if (left != null && rg != null)
         {
            return new ParserErrorMessage_Group(
               rg.Position,
               rg.Group.Cons(left)
               );
         }

         if (left != null && right != null)
         {
            return new ParserErrorMessage_Group(
               -1,
               ImmutableList<IParserErrorMessage>.Singleton (left).Cons (right)
               );
         }

         if (left != null)
         {
            return left;
         }

         if (right != null)
         {
            return right;
         }

         return ParserErrorMessages.Unexpected_General;
      }

   }
}