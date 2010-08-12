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
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;
using MicroParser.Json;

namespace PerformanceTest.MicroParser.Json
{
   class Program
   {
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

      static void Main (string[] args)
      {
         if (true)
         {
            // cold run
            var json = GetStringResource ("PerformanceTest.MicroParser.Json.JSON.txt");
            JsonSerializer.Unserialize (json);            
         }

         if (true)
         {
            var json = GetStringResource ("PerformanceTest.MicroParser.Json.JSON.txt");

            const int Count = 40000;

            var dt = DateTime.Now;


            for (var iter = 0; iter < Count; ++iter)
            {
               var result = JsonSerializer.Unserialize (json);
            }

            var diff = DateTime.Now - dt;

            Console.WriteLine ("Complex Json {0} times took {1:0.00} secs", Count, diff.TotalSeconds);
         }

         if (true)
         {
            var bigStringBuilder = new StringBuilder ();
            for (var iter = 0; iter < 1000; ++iter)
            {
               bigStringBuilder.AppendLine ("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. ");
            }
            var bigString = bigStringBuilder.ToString ();
            dynamic bigObject = new ExpandoObject ();
            bigObject.BigOne = bigString;
            var json = JsonSerializer.Serialize (bigObject);
            const int Count = 400;

            var dt = DateTime.Now;

            for (var iter = 0; iter < Count; ++iter)
            {
               var result = JsonSerializer.Unserialize (json);
            }

            var diff = DateTime.Now - dt;

            Console.WriteLine ("Big Json {0} times took {1:0.00} secs", Count, diff.TotalSeconds);
         }
      }
   }
}
