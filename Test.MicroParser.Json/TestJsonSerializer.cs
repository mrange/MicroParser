// ----------------------------------------------------------------------------------------------
// Copyright (c) M�rten R�nge.
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MicroParser.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace Test.MicroParser.Json
{
   [TestClass]
   public class TestJsonSerializer
   {
      public TestContext TestContext { get; set; }

      static readonly Tuple<string, object, string>[] s_primitiveTest =
         new[]
            {
               Tuple.Create ("\"\""             , ""           as object   , "\"\""             ),
               Tuple.Create ("\"Test\""         , "Test"       as object   , "\"Test\""         ),
               Tuple.Create ("\"\\tNR\\r\\n\""  , "\tNR\r\n"   as object   , "\"\\tNR\\r\\n\""  ),
               Tuple.Create ("\"Test\""         , "Test"       as object   , "\"Test\""         ),
               Tuple.Create ("\"\\u0041\""      , "A"          as object   , "\"A\""            ),

               Tuple.Create ("0"                , 0.0          as object   , "0"                ),
               Tuple.Create ("-0"               , 0.0          as object   , "0"                ),
               Tuple.Create ("0.5"              , 0.5          as object   , "0.5"              ),
               Tuple.Create ("3"                , 3.0          as object   , "3"                ),
               Tuple.Create ("3"                , 3.0          as object   , "3"                ),
               Tuple.Create ("3.14"             , 3.14         as object   , "3.14"             ),
               Tuple.Create ("3e3"              , 3e3          as object   , "3000"             ),
               Tuple.Create ("3.14E-4"          , 3.14E-4      as object   , "0.000314"         ),
               Tuple.Create ("2.7182828e+4"     , 2.7182828e+4 as object   , "27182.828"        ),

               Tuple.Create ("true"             , true         as object   , "true"             ),
               Tuple.Create ("false"            , false        as object   , "false"            ),

               Tuple.Create ("null"             , null         as object   , "null"             ),

            };

      new static bool Equals (dynamic expected, dynamic value)
      {
         if (expected is double && value != null)
         {
            return Math.Round (expected - value, 10) < double.Epsilon;
         }
         else if (expected != null && value != null)
         {
            return expected.Equals (value);
         }
         else
         {
            return expected == null && value == null;
         }
      }

      [TestMethod]
      public void Test_SimpleExpressions ()
      {

         foreach (var primitiveTest in s_primitiveTest)
         {
            // In order to parse simple value expressions we need to wrap them in arrays
            var wrappedInArray = "[" + primitiveTest.Item1 + "]";
            var wrappedValue = JsonSerializer.Unserialize (wrappedInArray);

            var array = wrappedValue as object[];
            Assert.IsNotNull (array);
            Assert.AreEqual(1, array.Length);

            var value = array[0];
            var result = Equals (primitiveTest.Item2, value);
            Assert.IsTrue (result);
         }
      }

      [TestMethod]
      public void Test_ArrayExpressions ()
      {
         var array0 = (dynamic[])JsonSerializer.Unserialize ("[]");

         Assert.AreEqual (0, array0.Length);

         var array1 = (dynamic[])JsonSerializer.Unserialize ("[1]");

         Assert.AreEqual (1, array1.Length);
         Assert.IsTrue (Equals (1.0, array1[0]));

         var array3 = (dynamic[])JsonSerializer.Unserialize ("[1, 3.14, \"Test\"]");

         Assert.AreEqual (3, array3.Length);
         Assert.IsTrue (Equals (1.0, array3[0]));
         Assert.IsTrue (Equals (3.14, array3[1]));
         Assert.IsTrue (Equals ("Test", array3[2]));

      }

      static int NumberOfProperties (object dyn)
      {
         var dic = dyn as IDictionary<string, object>;
         return dic != null ? dic.Count : 0;
      }

      [TestMethod]
      public void Test_ObjectExpressions ()
      {
         dynamic object0 = JsonSerializer.Unserialize ("{}");
         Assert.AreEqual (0, NumberOfProperties (object0));

         dynamic object1 = JsonSerializer.Unserialize ("{\"Test\":1}");
         Assert.AreEqual (1, NumberOfProperties (object1));
         Assert.IsTrue (Equals (1.0, object1.Test));

         dynamic object2 = JsonSerializer.Unserialize ("{\"Test\":1, \"Test2\": \"Tjo\"}");
         Assert.AreEqual (2, NumberOfProperties (object2));
         Assert.IsTrue (Equals (1.0, object2.Test));
         Assert.IsTrue (Equals ("Tjo", object2.Test2));
      }

      [TestMethod]
      public void Test_ComplexExpressions ()
      {
         var json = GetStringResource ("Test.MicroParser.Json.JSON.txt");

         var object0 = JsonSerializer.Unserialize (json);

         VerifyComplexObject ((object) object0);
      }

      static void VerifyComplexObject (dynamic complexObject)
      {
         var glossary = complexObject.glossary;

         var glossary_Title = glossary.title;
         var glossary_GlossDiv = glossary.GlossDiv;

         var glossary_GlossDiv_Title = glossary_GlossDiv.title;
         var glossary_GlossDiv_GlossList = glossary_GlossDiv.GlossList;

         var glossary_GlossDiv_GlossList_GlossEntry   = glossary_GlossDiv_GlossList.GlossEntry;

         var glossary_GlossDiv_GlossList_GlossEntry_ID         = glossary_GlossDiv_GlossList_GlossEntry.ID;
         var glossary_GlossDiv_GlossList_GlossEntry_SortAs     = glossary_GlossDiv_GlossList_GlossEntry.SortAs;
         var glossary_GlossDiv_GlossList_GlossEntry_GlossTerm  = glossary_GlossDiv_GlossList_GlossEntry.GlossTerm;
         var glossary_GlossDiv_GlossList_GlossEntry_Acronym    = glossary_GlossDiv_GlossList_GlossEntry.Acronym;
         var glossary_GlossDiv_GlossList_GlossEntry_Abbrev     = glossary_GlossDiv_GlossList_GlossEntry.Abbrev;
         var glossary_GlossDiv_GlossList_GlossEntry_GlossDef   = glossary_GlossDiv_GlossList_GlossEntry.GlossDef;
         var glossary_GlossDiv_GlossList_GlossEntry_GlossSee   = glossary_GlossDiv_GlossList_GlossEntry.GlossSee;

         var glossary_GlossDiv_GlossList_GlossEntry_GlossDef_para          = glossary_GlossDiv_GlossList_GlossEntry_GlossDef.para;
         var glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso  = glossary_GlossDiv_GlossList_GlossEntry_GlossDef.GlossSeeAlso;


         Assert.AreEqual ("example glossary", glossary_Title);

         Assert.AreEqual ("S", glossary_GlossDiv_Title);


         Assert.AreEqual ("SGML", glossary_GlossDiv_GlossList_GlossEntry_ID);
         Assert.AreEqual ("SGML", glossary_GlossDiv_GlossList_GlossEntry_SortAs);
         Assert.AreEqual ("Standard Generalized Markup Language", glossary_GlossDiv_GlossList_GlossEntry_GlossTerm);
         Assert.AreEqual ("SGML", glossary_GlossDiv_GlossList_GlossEntry_Acronym);
         Assert.AreEqual ("ISO 8879:1986", glossary_GlossDiv_GlossList_GlossEntry_Abbrev);
         Assert.AreEqual ("markup", glossary_GlossDiv_GlossList_GlossEntry_GlossSee);

         Assert.AreEqual ("A meta-markup language, used to create markup languages such as DocBook.", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_para);

         Assert.AreEqual (2, glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso.Length);
         Assert.AreEqual ("GML", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso[0]);
         Assert.AreEqual ("XML", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso[1]);
      }

      [TestMethod]
      public void Test_Performance ()
      {
         var json = GetStringResource ("Test.MicroParser.Json.JSON.txt");
         dynamic object0 = JsonSerializer.Unserialize (json);
         dynamic glossary = object0.glossary;

         var then = DateTime.Now;

         for (var iter = 0; iter < 10000; ++iter)
         {
            var innerObject0 = JsonSerializer.Unserialize (json);
         }

         var diff = DateTime.Now - then;

         Assert.IsTrue (diff.TotalSeconds < 10);
      }

      [TestMethod]
      public void Test_SimpleSerialize ()
      {
         foreach (var primitiveTest in s_primitiveTest)
         {
            var serialize = JsonSerializer.Serialize (primitiveTest.Item2);

            Assert.AreEqual (primitiveTest.Item3, serialize);
         }

      }

      [TestMethod]
      public void Test_ComplexSerialize ()
      {
         var json = GetStringResource ("Test.MicroParser.Json.JSON.txt");

         var object0 = JsonSerializer.Unserialize (json);

         var serialize = JsonSerializer.Serialize (object0);

         var object1 = JsonSerializer.Unserialize (serialize);

         VerifyComplexObject (object1);

      }

      static string GetStringResource (string resourceName)
      {
         using (var resourceStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (resourceName))
         {
            if (resourceStream == null)
            {
               return "";
            }

            using (var streamReader = new StreamReader (resourceStream))
            {
               return streamReader.ReadToEnd ();
            }
         }
      }
   }
}
