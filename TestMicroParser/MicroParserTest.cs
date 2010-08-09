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
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace TestMicroParser
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

      [TestMethod]
      public void Test_EscapedString ()
      {
         Func<char, Parser<Empty>> p_char = CharParser.SkipChar;

         var p_escape = CharParser
            .AnyOf ("\"\\/bfnrt")
            .Map (ch =>
            {
               switch (ch)
               {
                  case 'b':
                     return '\b';
                  case 'f':
                     return '\f';
                  case 'n':
                     return '\n';
                  case 'r':
                     return '\r';
                  case 't':
                     return '\t';
                  default:
                     return ch;
               }
            });

         var p_string = Parser
            .Choice (
               CharParser.NoneOf ("\\\""),
               CharParser.SkipChar ('\\').KeepRight (p_escape))
            .Many ()
            .Between (
               p_char ('"'),
               p_char ('"')
               )
            .Map (cs => new string (cs));

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
