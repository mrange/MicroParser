using System;
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
         var array = (dynamic[])JsonSerializer.Unserialize ("[1, 3.14, \"Test\"]");

         Assert.IsTrue (Equals (1.0, array[0]));
         Assert.IsTrue(Equals(3.14, array[1]));
         Assert.IsTrue(Equals("Test", array[2]));

      }
   }
}
