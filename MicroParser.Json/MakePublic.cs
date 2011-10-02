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
#if MICRO_PARSER_JSON_MAKE_PUBLIC
namespace MicroParser.Json
{
   using System;
   using System.Collections.Generic;
   using System.Dynamic;

   public partial class JsonUnserializeError
   {

   }

   public partial class JsonSerializer
   {
       static partial void TransformObject(Tuple<string, object>[] properties, ref object result)
       {
           IDictionary<string, object> expando = new ExpandoObject();
           foreach (var p in properties)
           {
               expando[p.Item1 ?? ""] = p.Item2;
           }
           result = expando;
       }
   }
}
#endif
