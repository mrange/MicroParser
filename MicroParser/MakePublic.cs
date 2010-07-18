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
   public delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);
   public delegate bool CharSatisfyFunction(char ch, int index);

   public partial class CharParser
   {

   }

   public partial class CharSatify
   {

   }

   public partial interface IParserErrorMessage
   {

   }

   public partial struct Empty
   {
      
   }

   public static partial class MicroTuple
   {
      
   }

   public partial struct MicroTuple<TValue1, TValue2>
   {
      
   }

   public partial struct MicroTuple<TValue1, TValue2, TValue3>
   {

   }

   public static partial class Optional
   {

   }

   public partial struct Optional<TValue>
   {
      
   }

   public partial class Parser
   {
      
   }

   public partial class ParserFunctionRedirect<TValue>
   {

   }

   public partial struct ParserReply<TValue>
   {

   }

   public partial class BaseParserResult
   {

   }

   public partial class ParserResult<TValue>
   {

   }

   public partial class ParserState
   {

   }

   public partial struct ParserStatePosition
   {

   }

   public partial struct SubString
   {

   }


}
