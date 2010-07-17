using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FunWithExpandos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace TestFunWithExpandos
{
   [TestClass]
   public class TestJsonSerializer
   {
      public TestContext TestContext { get; set; }

      static readonly Tuple<string, object>[] s_primitiveTest =
         new[]
            {
               Tuple.Create ("\"\""             , ""           as object),
               Tuple.Create ("\"Test\""         , "Test"       as object),

               Tuple.Create ("3"                , 3.0          as object),
               Tuple.Create ("3.14"             , 3.14         as object),
               Tuple.Create ("3e3"              , 3e3          as object),
               Tuple.Create ("3.14E-4"          , 3.14E-4      as object),
               Tuple.Create ("2.7182828e+4"     , 2.7182828e+4 as object),

               Tuple.Create ("true"             , true         as object),
               Tuple.Create ("false"            , false        as object),

               Tuple.Create ("null"             , null         as object),

            };

      new static bool Equals(dynamic expected, dynamic value)
      {
         if (expected is double && value != null)
         {
            return Math.Round(expected - value, 10) < double.Epsilon;
         }
         else if (expected != null && value != null)
         {
            return expected.Equals(value);
         }
         else
         {
            return expected == null && value == null;
         }
      }

      [TestMethod]
      public void Test_SimpleExpressions()
      {

         foreach (var primitiveTest in s_primitiveTest)
         {
            var value = JsonSerializer.Unserialize(primitiveTest.Item1);
            bool result = Equals(primitiveTest.Item2, value);
            Assert.IsTrue (result);
         }
      }

      [TestMethod]
      public void Test_ArrayExpressions()
      {
         var array0 = (dynamic[])JsonSerializer.Unserialize("[]");

         Assert.AreEqual (0, array0.Length);

         var array1 = (dynamic[])JsonSerializer.Unserialize("[1]");

         Assert.AreEqual(1, array1.Length);
         Assert.IsTrue(Equals(1.0, array1[0]));

         var array3 = (dynamic[])JsonSerializer.Unserialize("[1, 3.14, \"Test\"]");

         Assert.AreEqual(3, array3.Length);
         Assert.IsTrue(Equals(1.0, array3[0]));
         Assert.IsTrue(Equals(3.14, array3[1]));
         Assert.IsTrue(Equals("Test", array3[2]));

      }

      static int NumberOfProperties (dynamic dyn)
      {
         var dic = dyn as IDictionary<string, object>;
         return dic != null ? dic.Count : 0;
      }

      [TestMethod]
      public void Test_ObjectExpressions()
      {
         var object0 = JsonSerializer.Unserialize("{}");

         Assert.AreEqual(0, NumberOfProperties(object0));

         var object1 = JsonSerializer.Unserialize("{\"Test\":1}");

         Assert.AreEqual(1, NumberOfProperties(object1));
         Assert.IsTrue(Equals(1.0, object1.Test));


         var object2 = JsonSerializer.Unserialize("{\"Test\":1, \"Test2\": \"Tjo\"}");

         Assert.AreEqual(2, NumberOfProperties(object2));
         Assert.IsTrue(Equals(1.0, object2.Test));
         Assert.IsTrue(Equals("Tjo", object2.Test2));
      }

      [TestMethod]
      public void Test_ComplexExpressions()
      {
         var json = GetStringResource ();

         var object0 = JsonSerializer.Unserialize(json);

         var glossary = object0.glossary;

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


         Assert.AreEqual("example glossary", glossary_Title);

         Assert.AreEqual("S", glossary_GlossDiv_Title);


         Assert.AreEqual("SGML", glossary_GlossDiv_GlossList_GlossEntry_ID);
         Assert.AreEqual("SGML", glossary_GlossDiv_GlossList_GlossEntry_SortAs);
         Assert.AreEqual("Standard Generalized Markup Language", glossary_GlossDiv_GlossList_GlossEntry_GlossTerm);
         Assert.AreEqual("SGML", glossary_GlossDiv_GlossList_GlossEntry_Acronym);
         Assert.AreEqual("ISO 8879:1986", glossary_GlossDiv_GlossList_GlossEntry_Abbrev);
         Assert.AreEqual("markup", glossary_GlossDiv_GlossList_GlossEntry_GlossSee);

         Assert.AreEqual("A meta-markup language, used to create markup languages such as DocBook.", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_para);

         Assert.AreEqual(2, glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso.Length);
         Assert.AreEqual("GML", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso[0]);
         Assert.AreEqual("XML", glossary_GlossDiv_GlossList_GlossEntry_GlossDef_GlossSeeAlso[1]);


      }

      static string GetStringResource ()
      {
         using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestFunWithExpandos.JSON.txt"))
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
