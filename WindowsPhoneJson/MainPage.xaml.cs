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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MicroParser.Json;

namespace WindowsPhoneJson
{
   public partial class MainPage
   {
      public MainPage()
      {
         InitializeComponent();

         var obj =
            (Dictionary<string, object>) JsonSerializer.Unserialize(GetStringResource("WindowsPhoneJson.JSON.txt"));

         var books = obj["Books"] as object[];

         if (books != null)
         {
            Books.ItemsSource = books.OfType<Dictionary<string, object>>().Select(book => book["ISBN"]);
         }
      }

      static string GetStringResource(string resourceName)
      {
         using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
         {
            if (resourceStream == null)
            {
               return "";
            }

            using (var streamReader = new StreamReader(resourceStream))
            {
               return streamReader.ReadToEnd();
            }
         }
      }

   }
}