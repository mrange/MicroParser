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
#if !MICRO_PARSER_USE_NET4_TUPLE
   static partial class Tuple
   {
      public static Tuple<TValue1, TValue2> Create<TValue1, TValue2> (
            TValue1 value1
         ,  TValue2 value2
         )
      {
         return new Tuple<TValue1, TValue2>
            {
               Item1 = value1 ,
               Item2 = value2 ,
            };
      }
      public static Tuple<TValue1, TValue2, TValue3> Create<TValue1, TValue2, TValue3> (
            TValue1 value1
         ,  TValue2 value2
         ,  TValue3 value3
         )
      {
         return new Tuple<TValue1, TValue2, TValue3>
            {
               Item1 = value1 ,
               Item2 = value2 ,
               Item3 = value3 ,
            };
      }
   }
   partial struct Tuple<TValue1, TValue2>
   {
      public TValue1 Item1;
      public TValue2 Item2;

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
         }.ToString ();
      }
#endif
   }
   partial struct Tuple<TValue1, TValue2, TValue3>
   {
      public TValue1 Item1;
      public TValue2 Item2;
      public TValue3 Item3;

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
            Item3,
         }.ToString ();
      }
#endif
   }
#endif
}
