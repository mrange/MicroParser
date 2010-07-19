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
            Assert.IsTrue(pResult.IsSuccessful);
            Assert.AreEqual(3, pResult.Value);
         }

         {
            var pResult = Parser.Parse(p_double, "a");
            Assert.IsFalse(pResult.IsSuccessful);
         }

         {
            var pResult = Parser.Parse(p_double.KeepLeft (Parser.EndOfStream ()), "3,14");
            Assert.IsFalse (pResult.IsSuccessful);
         }
      }
   }
}
