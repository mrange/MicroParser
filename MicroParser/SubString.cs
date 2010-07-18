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
   using System;
   using System.Diagnostics;

   partial struct SubString : IEquatable<SubString>
   {
      public string Value;
      public int Position;
      public int Length;

      string SafeValue
      {
         get
         {
            return Value ?? "";
         }
      }

      public bool Equals (SubString other)
      {

         var value = SafeValue;
         var otherValue = other.SafeValue;

         var end = Math.Min (Position + Length, value.Length);
         var otherEnd = Math.Min (other.Position + other.Length, otherValue.Length);

         var effectiveLength = end - Position;
         var effectiveOtherLength = otherEnd - other.Position;

         if (effectiveLength != effectiveOtherLength)
         {
            return false;
         }

         var diff = other.Position - Position;
 
         for (var iter = Position; iter < end; ++iter)
         {
            if (value[iter] != otherValue[iter + diff])
            {
               return false;
            }
         }

         return true;
      }

      public override string ToString ()
      {
         return SafeValue.Substring (Position, Length);
      }

      public char this[int index]
      {
         get
         {
            Debug.Assert (Value != null);
            return Value[Position + index];
         }
      }

      public override bool Equals (object obj)
      {
         return obj is SubString && Equals ((SubString) obj);
      }

      public override int GetHashCode ()
      {
         var result = 0x55555555;

         var value = SafeValue;

         var end = Math.Min (Position + Length, value.Length);

         for (var iter = Position; iter < end; ++iter)
         {
            result = (result * 397) ^ value[iter];
         }

         return result;
      }
   }
}