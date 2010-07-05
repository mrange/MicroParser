using System.Collections.Generic;
using System.Text;

namespace MicroParser
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