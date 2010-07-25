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

      public SubString (string value, int position, int length)
      {
         Value = value;
         Position = position;
         Length = length;
      }

      public SubString (string value, int position)
         :  this (value, position, (value ?? "").Length - position)
      {

      }

      public SubString (string value)
         : this (value, 0, (value ?? "").Length)
      {

      }

      public int EffectiveLength
      {
         get
         {
            return End - Begin;
         }
      }

      public int Begin
      {
         get
         {
            return Math.Max (Position, 0);
         }
      }

      public int End
      {
         get
         {
            return Math.Min (Position + Length, SafeValue.Length);
         }
      }

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

         var effectiveLength = EffectiveLength;
         var effectiveOtherLength = other.EffectiveLength;

         if (effectiveLength != effectiveOtherLength)
         {
            return false;
         }

         var begin = Begin;
         var otherBegin = other.Begin;

         var end = End;
         var otherEnd = other.End;

         var diff = otherBegin - begin;
 
         for (var iter = begin; iter < end; ++iter)
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
         return SafeValue.Substring (Begin, EffectiveLength);
      }

      public char this[int index]
      {
         get
         {
            var realIndex = Position + index;
            return realIndex > -1 && realIndex < Value.Length 
               ?  Value[Position + index] 
               :  ' '
               ;
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

         var end = End;

         for (var iter = Begin; iter < end; ++iter)
         {
            result = (result * 397) ^ value[iter];
         }

         return result;
      }
   }
}