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
using System.Collections.Generic;
using System.Text;

namespace TestParser
{
   static class Extensions
   {
      public static bool IsNullOrEmpty (this string str)
      {
         return string.IsNullOrEmpty (str);
      }

      public static string Concatenate (
         this IEnumerable<string> strings,
         string delimiter = null,
         string prepend = null,
         string append = null
         )
      {
         var first = true;

         var sb = new StringBuilder (prepend ?? "");

         var del = delimiter ?? "";

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

         sb.Append (append ?? "");
         return sb.ToString ();
      }

      public static TValue Lookup<TKey, TValue> (
         this IDictionary<TKey, TValue> dictionary,
         TKey key,
         TValue defaultValue
         )
      {
         TValue value;
         return dictionary.TryGetValue (key, out value)
                   ? value
                   : defaultValue;
      }

   }
}