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
using System.Diagnostics;
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
      static readonly Parser<object> s_parser;

      struct StringPart
      {
         readonly int m_item1;
         readonly int m_item2;

         public StringPart (int position, int length)
         {
            m_item1 = position;
            m_item2 = length;
         }

         public StringPart (char ch)
         {
            m_item1 = ch;
            m_item2 = 0;
         }

         public bool IsRange
         {
            get
            {
               return m_item2 > 0;
            }
         }

         public int Position
         {
            get
            {
               Debug.Assert (IsRange);
               return m_item1;
            }
         }

         public int Length
         {
            get
            {
               Debug.Assert (IsRange);
               return m_item2;
            }
         }

         public char Character
         {
            get
            {
               unchecked
               {
                  Debug.Assert (!IsRange);
                  return (char)(m_item1 & 0xFFFF);
               }
            }
         }
      }

      static Parser<object> CombineStringParts (
         this Parser<StringPart[]> parser
         )
      {
         Parser<object>.Function function =
            state =>
            {
               var result = parser.Execute (state);
               if (result.State.HasError ())
               {
                  return result.Failure<object>();
               }

               var text = state.Text;
               var length = 0;

               foreach (var stringPart in result.Value)
               {
                  if (stringPart.IsRange)
                  {
                     length += stringPart.Length;
                  }
                  else
                  {
                     ++length;
                  }
               }

               var charArray = new char[length];

               var position = 0;

               foreach (var stringPart in result.Value)
               {
                  if (stringPart.IsRange)
                  {
                     var begin = stringPart.Position;
                     var end = begin + stringPart.Length;
                     for (var iter = begin; iter < end; ++iter)
                     {
                        charArray[position++] = text[iter];
                     }
                  }
                  else
                  {
                     charArray[position++] = stringPart.Character;
                  }
               }

               object stringValue = new string (charArray);
               return result.Success (stringValue);
            };
         return function;
      }

      static JsonSerializer ()
      {
         // Language spec at www.json.org

         // ReSharper disable InconsistentNaming
         Func<char, Parser<Empty>> p_char    = CharParser.SkipChar;
         Func<string, Parser<Empty>> p_str   = CharParser.SkipString;

         var p_spaces                        = CharParser.SkipWhiteSpace ();

         var p_null     = p_str ("null").Map (empty => null as object);
         var p_true     = p_str ("true").Map (empty => true as object);
         var p_false    = p_str ("false").Map (empty => false as object);
         var p_number   = CharParser.Double ().Map (d => d as object);

         var p_array_redirect    = Parser.Redirect<object>();
         var p_object_redirect   = Parser.Redirect<object>();

         const string simpleEscape     = "\"\\/bfnrt";
         const string simpleEscapeMap  = "\"\\/\b\f\n\r\t";

         var p_simpleEscape = CharParser
            .AnyOf (simpleEscape, minCount: 1, maxCount: 1)
            .Map (ch => new StringPart (simpleEscapeMap[simpleEscape.IndexOf (ch[0])]));

         var p_unicodeEscape = CharParser
            .SkipChar ('u')
            .KeepRight (
               CharParser
               .Hex (minCount: 4, maxCount: 4)
               .Map (ui => new StringPart ((char)ui))
               );

         var p_escape = Parser
            .Choice (
               p_simpleEscape,
               p_unicodeEscape
               );

         var p_string = Parser
            .Choice (
               CharParser.NoneOf ("\\\"", minCount: 1).Map (ss => new StringPart (ss.Position, ss.Length)),
               CharParser.SkipChar ('\\').KeepRight (p_escape))
            .Many ()
            .Between (
               p_char ('"'),
               p_char ('"')
               )
            .CombineStringParts ();

         var p_array    = p_array_redirect.Parser;
         var p_object   = p_object_redirect.Parser;

         var p_value = Parser
            .Choice (
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

         p_array_redirect.ParserRedirect = p_elements
            .Between (
               p_char ('[').KeepLeft (p_spaces),
               p_char (']')
               )
            .Map (objects => objects as object);

         var p_member = Parser
            .Group (
               p_string.KeepLeft (p_spaces),
               p_char (':').KeepLeft (p_spaces).KeepRight (p_value)
               );

         var p_members = p_member
            .Array (p_char (',')
            .KeepLeft (p_spaces));

         p_object_redirect.ParserRedirect = p_members
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
