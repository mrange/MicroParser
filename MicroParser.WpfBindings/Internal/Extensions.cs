﻿// ----------------------------------------------------------------------------------------------
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
using System;
using System.Collections.Generic;

namespace MicroParser.WpfBindings.Internal
{
   static class Extensions
   {
      // TValue (ref)

      public static int SafeGetHashCode<TValue> (this TValue value)
         where TValue : class
      {
         return value != null ? value.GetHashCode () : 0x55555555;
      }

      public static bool SafeEquals<TValue> (this TValue left, TValue right)
         where TValue : class
      {
         return left != null && right != null
            ? left.Equals (right)
            : left == null && right == null
            ;
      }

      // System.Collection.Generic.IDictionary<TKey, TValue>

      public static TValue LookupOrAdd<TKey, TValue> (
         this IDictionary<TKey, TValue> dictionary,
         TKey key,
         Func<TValue> valueCreator
         )
      {
         if (dictionary == null)
         {
            return valueCreator ();
         }

         TValue value;

         if (!dictionary.TryGetValue (key, out value))
         {
            value = valueCreator ();
            dictionary.Add (key, value);
         }

         return value;
      }

   }
}