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
using System.Collections.Generic;
using System.Dynamic;
using MicroParser;

// ReSharper disable InconsistentNaming

namespace FunWithExpandos
{
   public sealed class ExpandoUnserializeError
   {
      public readonly string ErrorMessage;

      public ExpandoUnserializeError (string errorMessage)
      {
         ErrorMessage = errorMessage ?? "<NULL>";
      }

      public override string ToString ()
      {
         return new
                   {
                      ErrorMessage
                   }.ToString ();
      }

   }

   public static class JsonSerializer
   {
      static readonly ParserFunction<object> s_parser;

      static JsonSerializer ()
      {
         // Language spec at www.json.org

         Func<char, ParserFunction<Empty>> p_char = CharParser.SkipChar;
         Func<string, ParserFunction<Empty>> p_str = CharParser.SkipString;

         var p_spaces = CharParser.SkipWhiteSpace ();

         var p_null = p_str ("null").Map (empty => null as dynamic);

         var p_true = p_str ("true").Map (empty => true as dynamic);
         var p_false = p_str ("false").Map (empty => false as dynamic);

         var p_number = CharParser.ParseDouble ().Map (d => d as dynamic);

         var p_array_redirect = Parser.Redirect<dynamic> ();
         var p_object_redirect = Parser.Redirect<dynamic> ();

         var p_string = CharParser.ManyCharSatisfy (CharParser.SatisyAnyChar.Except ('"'))
            .Between (
               p_char ('"'),
               p_char ('"')
               )
            .Map (s => s as dynamic);

         var p_array = p_array_redirect.Function;
         var p_object = p_object_redirect.Function;

         var p_value = Parser.Choice (
            p_string,
            p_number,
            p_object,
            p_array,
            p_true,
            p_false,
            p_null
            )
            .KeepLeft (p_spaces);

         var p_elements = p_value.Array (p_char (',').KeepLeft (p_spaces));

         p_array_redirect.Redirect = p_elements.Between (
            p_char ('[').KeepLeft (p_spaces),
            p_char (']')
            )
            .Map (a => a as dynamic);

         var p_member = Parser.Tuple (
            p_string,
            p_char (':').KeepLeft (p_spaces).KeepRight (p_value)
            );

         var p_members = p_member.Array (p_char (',').KeepLeft (p_spaces));

         p_object_redirect.Redirect =
            p_members
               .Between (
                  p_char ('{').KeepLeft (p_spaces), 
                  p_char ('}')
                  )
               .Map (values =>
                        {
                           IDictionary<string, object> exp = new ExpandoObject ();
                           foreach (var value in values)
                           {
                              exp.Add (value.Item1, value.Item2);
                           }

                           return exp as object;
                        });


         s_parser = p_spaces.KeepRight (p_value);
      }
      

      public static dynamic Unserialize (string str)
      {
         var result = Parser.Parse (s_parser, str);

         return result.IsSuccessful 
            ?  result.Value 
            :  new ExpandoUnserializeError (result.ErrorMessage)
            ;
      }
   }
}
