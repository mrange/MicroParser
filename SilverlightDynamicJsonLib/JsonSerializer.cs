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
using System.Collections.ObjectModel;
using SilverlightDynamicJsonLib;

namespace MicroParser.Json
{
   public partial class JsonUnserializeError
   {

   }

   public partial class JsonSerializer
   {
      static partial void TransformObjects(object[] objects, ref object result)
      {
         result = new ObservableCollection<object>(objects);
      }

      static partial void TransformObject(Tuple<string, object>[] properties, ref object result)
      {
         result = new DynamicDependencyObject(properties);
      }
   }
}
