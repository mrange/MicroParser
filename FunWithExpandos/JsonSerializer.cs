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

// ReSharper disable InconsistentNaming
namespace MicroParser
{
   delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);
   delegate bool CharSatisfyFunction(char ch, int index);

   // ReSharper disable InconsistentNaming
   [Flags]
   enum ParserReply_State
   {
      Successful                       = 00,
      Error                            = 10,
      Error_Message                    = 11,
      Error_Expected                   = 12,
      Error_Unexpected                 = 13,
      Error_Group                      = 14,
      Error_StateIsRestored            = 15,
      FatalError                       = 0x00010000,
      FatalError_Mask                  = 0x7FFF0000,
      FatalError_Terminate             = 0x00010000,
      FatalError_StateIsNotRestored    = 0x00020000,
   }
   // ReSharper restore InconsistentNaming


   // ReSharper disable InconsistentNaming
   enum ParserState_AdvanceResult
   {
      Successful                             = 00,
      Error                                  = 10,
      Error_EndOfStream                      = 11,
      Error_SatisfyFailed                    = 12,
      Error_EndOfStream_PostionChanged       = 23,
      Error_SatisfyFailed_PositionChanged    = 24,
   }
   // ReSharper restore InconsistentNaming


}

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
         stringBuilder.Append (str);
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
