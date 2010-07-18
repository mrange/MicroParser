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
   static partial class Optional
   {
      public static Optional<TValue> Create<TValue> (TValue value)
      {
         return new Optional<TValue> (value);
      }

      public static Optional<TValue> Create<TValue> ()
      {
         return new Optional<TValue> ();
      }
   }

   partial struct Optional<TValue>
   {
      public readonly bool HasValue;
      public readonly TValue Value;

      public Optional (TValue value)
      {
         HasValue = true;
         Value = value;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      HasValue,
                      Value = HasValue ? Value : default (TValue),
                   }.ToString ();
      }
#endif
   }
}