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
   using MicroParser.Internal;
	partial class Parser
	{
#if !MICRO_PARSER_SUPPRESS_PARSER_GROUP_2
      public static Parser<Tuple<TValue1, TValue2>> Group<TValue1, TValue2> (
            Parser<TValue1> parser1
         ,  Parser<TValue2> parser2
         )
      {
         Parser<Tuple<TValue1, TValue2>>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1.Execute (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2.Execute (state);

            if (result2.State.HasError ())
            {
               return result2.Failure<Tuple<TValue1, TValue2>>().VerifyConsistency (initialPosition);
            }
            return result2.Success (
               Tuple.Create (
                     result1.Value
                  ,  result2.Value
                  ));
         };
         return function;
      }
#endif
#if !MICRO_PARSER_SUPPRESS_PARSER_GROUP_3
      public static Parser<Tuple<TValue1, TValue2, TValue3>> Group<TValue1, TValue2, TValue3> (
            Parser<TValue1> parser1
         ,  Parser<TValue2> parser2
         ,  Parser<TValue3> parser3
         )
      {
         Parser<Tuple<TValue1, TValue2, TValue3>>.Function function = state =>
         {
            var initialPosition = state.Position;

            var result1 = parser1.Execute (state);

            if (result1.State.HasError ())
            {
               return result1.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result2 = parser2.Execute (state);

            if (result2.State.HasError ())
            {
               return result2.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            var result3 = parser3.Execute (state);

            if (result3.State.HasError ())
            {
               return result3.Failure<Tuple<TValue1, TValue2, TValue3>>().VerifyConsistency (initialPosition);
            }
            return result3.Success (
               Tuple.Create (
                     result1.Value
                  ,  result2.Value
                  ,  result3.Value
                  ));
         };
         return function;
      }
#endif


   }
}
