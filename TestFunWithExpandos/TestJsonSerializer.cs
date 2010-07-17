using System;
using System.Collections.Generic;
using FunWithExpandos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
         const string s =
            "[\r\n" +
            "   1,\r\n" +
            "   {},\r\n" +
            "   [1,3,4],\r\n" +
            "   {\"X\":[1,3,4]},\r\n" +
            "]\r\n";

         var object0 = JsonSerializer.Unserialize(s);
      }

   }
}
