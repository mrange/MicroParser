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
using System.IO;
using System.Reflection;
using FunWithExpandos;

namespace PerformanceTestFunWithExpandos
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
         var json = GetStringResource ("PerformanceTestFunWithExpandos.JSON.txt");

         var dt = DateTime.Now;
         const int Count = 10000;

         for (var iter = 0; iter < Count; ++iter)
         {
            var result = JsonSerializer.Unserialize (json);
         }

         var diff = DateTime.Now - dt;

         Console.WriteLine ("{0} took {1:0.00} secs", Count, diff.TotalSeconds);
      }
   }
}
