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
using MicroParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace Test.MicroParser
{
   [TestClass]
   public class MicroParserTest
   {
      public TestContext TestContext { get; set; }

      [TestMethod]
      public void Test_SimpleParsing ()
      {
         var p_double = CharParser.Double ();

         {
            var pResult = Parser.Parse (p_double, "3.14");
            Assert.IsTrue (pResult.IsSuccessful);
            Assert.AreEqual (3.14, pResult.Value);
         }

         {
            var pResult = Parser.Parse (p_double, "3,14");
            Assert.IsTrue (pResult.IsSuccessful);
            Assert.AreEqual (3, pResult.Value);
         }

         {
            var pResult = Parser.Parse (p_double, "a");
            Assert.IsFalse (pResult.IsSuccessful);
         }

         {
            var pResult = Parser.Parse (p_double.KeepLeft (Parser.EndOfStream ()), "3,14");
            Assert.IsFalse (pResult.IsSuccessful);
         }
      }

      [TestMethod]
      public void Test_HexParsing ()
      {
         var p_hex = CharParser.Hex (minCount: 4, maxCount: 4);
         {
            var pResult = Parser.Parse (p_hex.KeepLeft (Parser.EndOfStream ()), "98AB");
            Assert.IsTrue (pResult.IsSuccessful);
            Assert.AreEqual (0x98ABU, pResult.Value);
         }
      }

      public static string Combine (params SubString[] subStrings)
      {
         var accLength = 0;

         foreach (var subString in subStrings)
         {
            accLength += subString.EffectiveLength;
         }

         var charArray = new char[accLength];

         var index = 0;

         foreach (var subString in subStrings)
         {
            var begin = subString.Begin;
            var end = subString.End;
            var value = subString.Value ?? "";

            for (var iter = begin; iter < end; ++iter)
            {
               charArray[index] = value[iter];
               ++index;
            }
         }

         return new string (charArray);
      }

      [TestMethod]
      public void Test_EscapedString ()
      {
         Func<char, Parser<Empty>> p_char = CharParser.SkipChar;

         const string simpleEscape = "\"\\/bfnrt";
         const string simpleEscapeMap = "\"\\/\b\f\n\r\t";

         var p_simpleEscape = CharParser
            .AnyOf (simpleEscape, minCount: 1, maxCount: 1)
            .Map (ch => new SubString (simpleEscapeMap, simpleEscape.IndexOf (ch[0]), 1));

         var p_unicodeEscape =
            CharParser.SkipChar ('u')
            .KeepRight (
               CharParser
               .Hex (minCount: 4, maxCount: 4)
               .Map (ui => new SubString (new string ((char)ui, 1)))
               );

         var p_escape = Parser.Choice (
            p_simpleEscape,
            p_unicodeEscape
            );

         var p_string = Parser
            .Choice (
               CharParser.NoneOf ("\\\"", minCount: 1),
               CharParser.SkipChar ('\\').KeepRight (p_escape))
            .Many ()
            .Between (
               p_char ('"'),
               p_char ('"')
               )
            .Map (subStrings => Combine (subStrings) as object);

         {
            var parserResult = Parser.Parse (p_string, "\"\"");
            Assert.IsTrue (parserResult.IsSuccessful);
            Assert.AreEqual ("", parserResult.Value);
         }

         {
            var parserResult = Parser.Parse (p_string, "\"Test\"");
            Assert.IsTrue (parserResult.IsSuccessful);
            Assert.AreEqual ("Test", parserResult.Value);
         }

         {
            var parserResult = Parser.Parse (p_string, "\"Row\\r\\n\"");
            Assert.IsTrue (parserResult.IsSuccessful);
            Assert.AreEqual ("Row\r\n", parserResult.Value);
         }

      }
   }
}
