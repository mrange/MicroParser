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
using System;
using MicroParser;

// ReSharper disable InconsistentNaming

namespace SampleParsers
{
   class Program
   {
      static void Main (string[] args)
      {
         // Int () is a builtin parser for ints
         ParserFunction<int> p_int = CharParser.Int ();

         ParserFunction<SubString> p_identifier = CharParser
            .ManyCharSatisfy2 (                  // Creates a string parser
               CharParser.SatisyLetter,         // A test function applied to the 
                                                // first character
               CharParser.SatisyLetterOrDigit,  // A test function applied to the
                                                // rest of the characters
               minCount: 1                      // We require the identifier to be 
                                                // at least 1 character long
               );

         ParserFunction<Empty> p_spaces     = CharParser.SkipWhiteSpace ();
         ParserFunction<Empty> p_assignment = CharParser.SkipChar ('=');

         ParserFunction<MicroParser.Tuple<SubString, int>> p_parser = Parser.Group (
            p_identifier.KeepLeft (p_spaces),
            p_assignment.KeepRight (p_spaces).KeepRight (p_int));

         ParserResult<MicroParser.Tuple<SubString,int>> result = Parser.Parse (
            p_parser,
            "AnIdentifier = 3"
            );

         if (result.IsSuccessful)
         {
            Console.WriteLine (
               "{0} = {1}", 
               result.Value.Item1, 
               result.Value.Item2
               );
         }
         else
         {
            Console.WriteLine (
               result.ErrorMessage 
               );
         }

         Console.ReadKey ();
      }
   }
}
