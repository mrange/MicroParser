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
   using Internal;

   sealed partial class CharSatify
   {
      public readonly IParserErrorMessage Expected;
      public readonly CharSatisfyFunction Satisfy;

      public static implicit operator CharSatify (char ch)
      {
         return new CharSatify (
            new ParserErrorMessage_Expected (Strings.CharSatisfy.ExpectedChar_1.Form (ch)), 
            (c, i) => ch == c
            );
      }

      public CharSatify (IParserErrorMessage expected, CharSatisfyFunction satisfy)
      {
         Expected = expected;
         Satisfy = satisfy;
      }

#if !SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      Expected,
                   }.ToString ();
      }
#endif
   }
}