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
using MicroParser;

namespace SilverlightDynamicJson
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

         var p_null = p_str ("null").Map (empty => null as object);

         var p_true = p_str ("true").Map (empty => true as object);
         var p_false = p_str ("false").Map (empty => false as object);

         var p_number = CharParser.Double ().Map (d => d as object);

         var p_array_redirect = Parser.Redirect<object>();
         var p_object_redirect = Parser.Redirect<object>();

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
            .Hex (minCount: 4, maxCount: 4)
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

         var p_elements = p_value
            .Array (p_char (',')
            .KeepLeft (p_spaces))
            .Map (objects => new ObservableCollection<object> (objects));

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
               .Map (values => new DynamicDependencyObject (values) as object);

         s_parser = p_spaces.KeepRight (p_value);

         // ReSharper restore InconsistentNaming
      }

      public static object Unserialize (string str)
      {
         var result = Parser.Parse (s_parser, str);

         return result.IsSuccessful
            ? result.Value
            : new ExpandoUnserializeError (result.ErrorMessage)
            ;
      }
   }
}
