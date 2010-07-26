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
using System.IO;
using System.Reflection;

namespace SilverlightDynamicJson
{
   public partial class MainPage
   {
      public MainPage()
      {
         InitializeComponent();

         var obj = JsonSerializer.Unserialize(GetStringResource());

         var books = obj.Books;

         LB.ItemsSource = books;

      }

      static string GetStringResource()
      {
         using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SilverlightDynamicJson.JSON.txt"))
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
