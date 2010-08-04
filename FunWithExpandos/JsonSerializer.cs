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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Text;
using MicroParser;

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

         // ReSharper disable InconsistentNaming
         Func<char, ParserFunction<Empty>> p_char = CharParser.SkipChar;
         Func<string, ParserFunction<Empty>> p_str = CharParser.SkipString;
         var p_spaces = CharParser.SkipWhiteSpace ();

         var p_null = p_str ("null").Map (null as object);
         var p_true = p_str ("true").Map (true as object);
         var p_false = p_str ("false").Map (false as object);

         var p_number = CharParser.Double ().Map (d => d as object);

         var p_simpleEscape = CharParser
            .AnyOf ("\"\\/bfnrt")
            .Map (ch =>
               {
                  switch (ch)
                  {
                     case 'b':
                        return '\b';
                     case 'f':
                        return '\f';
                     case 'n':
                        return '\n';
                     case 'r':
                        return '\r';
                     case 't':
                        return '\t';
                     default:
                        return ch;
                  }
               });

         var p_unicodeEscape = CharParser
            .Hex (minCount:4, maxCount:4)
            .Map (ui => (char)ui);

         var p_escape = Parser.Choice (
            p_simpleEscape,
            p_unicodeEscape
            );

         var p_string = Parser
            .Choice (
               CharParser.NoneOf ("\\\""),
               CharParser.SkipChar ('\\').KeepRight (p_escape))
            .Many ()
            .Between (
               p_char ('"'),
               p_char ('"')
               )
            .Map (cs => new string (cs) as object);

         var p_array_redirect = Parser.Redirect<object> ();
         var p_object_redirect = Parser.Redirect<object> ();

         var p_array = p_array_redirect.Parser;
         var p_object = p_object_redirect.Parser;

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

         p_array_redirect.ParserRedirect = p_elements.Between (
            p_char ('[').KeepLeft (p_spaces),
            p_char (']')
            )
            .Map (objects => objects as object);

         var p_member = Parser.Group (
            p_string.KeepLeft (p_spaces),
            p_char (':').KeepLeft (p_spaces).KeepRight (p_value)
            );

         var p_members = p_member.Array (p_char (',').KeepLeft (p_spaces));

         p_object_redirect.ParserRedirect =
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
                              exp.Add (value.Item1.ToString (), value.Item2);
                           }

                           return exp as object;
                        });

         s_parser = p_spaces.KeepRight (p_value);

         // ReSharper restore InconsistentNaming
      }
      
      public static dynamic Unserialize (string str)
      {
         var result = Parser.Parse (s_parser, str);

         return result.IsSuccessful 
            ?  result.Value 
            :  new ExpandoUnserializeError (result.ErrorMessage)
            ;
      }

      static readonly CultureInfo s_cultureInfo = CultureInfo.InvariantCulture;

      static void SerializeImpl (StringBuilder stringBuilder, object dyn)
      {
         if (dyn is double)
         {
            stringBuilder.Append (((double) dyn).ToString (s_cultureInfo));
         }
         else if (dyn is int)
         {
            stringBuilder.Append (((int)dyn).ToString (s_cultureInfo));
         }
         else if (dyn is string)
         {
            SerializeString (stringBuilder, (string)dyn);
         }
         else if (dyn is bool)
         {
            if ((bool)dyn)
            {
               stringBuilder.Append ("true");
            }
            else
            {
               stringBuilder.Append ("false");
            }
         }
         else if (dyn is ExpandoObject)
         {
            var expandoObject = (ExpandoObject)dyn;
            IDictionary<string, object> dictionary = expandoObject;

            stringBuilder.Append ('{');

            var first = true;

            foreach (var kv in dictionary)
            {
               if (first)
               {
                  first = false;
               }
               else
               {
                  stringBuilder.Append (',');
               }

               SerializeString (stringBuilder, kv.Key);
               stringBuilder.Append (':');
               SerializeImpl (stringBuilder, kv.Value);
            }

            stringBuilder.Append ('}');
         }
         else if (dyn is IEnumerable)
         {
            var enumerable = (IEnumerable) dyn;

            stringBuilder.Append ('[');

            var first = true;

            foreach (var obj in enumerable)
            {
               if (first)
               {
                  first = false;
               }
               else
               {
                  stringBuilder.Append (',');
               }

               SerializeImpl (stringBuilder, obj);
            }

            stringBuilder.Append (']');
         }
         else
         {
            stringBuilder.Append ("null");
         }




      }

      static void SerializeString (StringBuilder stringBuilder, string str)
      {
         stringBuilder.Append ('"');
         foreach (var ch in str)
         {
            switch (ch)
            {
               case '\b':
                  stringBuilder.Append ("\\b");
                  break;
               case '\f':
                  stringBuilder.Append ("\\f");
                  break;
               case '\n':
                  stringBuilder.Append ("\\n");
                  break;
               case '\r':
                  stringBuilder.Append ("\\r");
                  break;
               case '\t':
                  stringBuilder.Append ("\\t");
                  break;
               default:
                  stringBuilder.Append (ch);
                  break;
            }
         }
         stringBuilder.Append ('"');
      }

      public static string Serialize (object dyn)
      {
         var sb = new StringBuilder (32);

         SerializeImpl (sb, dyn);

         return sb.ToString ();
      }

   }
}
