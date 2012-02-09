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

namespace MicroParser.Json
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Include.MicroParser;

    public sealed partial class JsonUnserializeError
    {
        public readonly string ErrorMessage;
        public readonly int ErrorOffset;

        public JsonUnserializeError (string errorMessage, int errorOffset)
        {
            ErrorMessage = errorMessage ?? "<NULL>";
            ErrorOffset = errorOffset;
        }

        public override string ToString ()
        {
            return new
            {
                ErrorOffset,
                ErrorMessage
            }.ToString ();
        }

    }

    static partial class JsonSerializer
    {
       static partial void TransformObjects (object[] objects, ref object result);
       static partial void TransformObject (Tuple<string, object>[] properties, ref object result);

#if MICRO_PARSER_JSON_NET35
        static IEnumerable<TZipped> Zip<T0, T1, TZipped>(
           this IEnumerable<T0> values0,
           IEnumerable<T1> values1,
           Func<T0, T1, TZipped> zipper
           )
        {
            if (zipper == null)
            {
                throw new ArgumentNullException ("zipper");
            }
            values0 = values0 ?? new T0[0];
            values1 = values1 ?? new T1[0];

            using (var e0 = values0.GetEnumerator ())
            using (var e1 = values1.GetEnumerator ())
            {
                bool moveNext0;
                bool moveNext1;
                while ((moveNext0 = e0.MoveNext () & (moveNext1 = e1.MoveNext ())))
                {
                    yield return zipper (e0.Current, e1.Current);
                }

                if (moveNext0 != moveNext1)
                {
                    throw new ArgumentException ("values0 and values1 must be of same length");
                }
            }

        }
#endif
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

        static Parser<string> CombineStringParts (
           this Parser<StringPart[]> parser
           )
        {
            Parser<string>.Function function =
               state =>
               {
                   var result = parser.Execute (state);
                   if (result.State.HasError ())
                   {
                       return result.Failure<string>();
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

                   var stringValue = new string (charArray);
                   return result.Success (stringValue);
               };
            return function;
        }

        static object TransformObjects (object[] objects)
        {
            object result = objects;
            TransformObjects (objects, ref result);
            return result;
        }

        static object TransformObject (Tuple<string, object>[] properties)
        {
            object result = properties;
            TransformObject (properties, ref result);
            return result;
        }

        static JsonSerializer ()
        {
            // Language spec at www.json.org

            // ReSharper disable InconsistentNaming
            Func<char, Parser<Empty>> p_char = CharParser.SkipChar;
            Func<string, Parser<Empty>> p_str = CharParser.SkipString;

            var p_spaces = CharParser.SkipWhiteSpace ();

            var p_null      = p_str ("null").Map (null as object);
            var p_true      = p_str ("true").Map (true as object);
            var p_false     = p_str ("false").Map (false as object);
            var p_number    = CharParser.Double ().Map (d => d as object);

            const string simpleEscape = "\"\\/bfnrt";
            const string simpleEscapeMap = "\"\\/\b\f\n\r\t";
            Debug.Assert (simpleEscape.Length == simpleEscapeMap.Length);

            var simpleSwitchCases = simpleEscape
               .Zip (
                  simpleEscapeMap,
                  (l, r) => Tuple.Create (l.ToString (s_cultureInfo), Parser.Return (new StringPart (r)))
                  );

            var otherSwitchCases =
               new[]
               {
                  Tuple.Create (
                     "u",
                     CharParser
                        .Hex (minCount: 4, maxCount: 4)
                        .Map (ui => new StringPart ((char) ui)))
               };

            var switchCases = simpleSwitchCases.Concat (otherSwitchCases).ToArray ();

            var p_escape = Parser.Switch (
               Parser.SwitchCharacterBehavior.Consume,
               switchCases
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

            var p_array_redirect = Parser.Redirect<object>();
            var p_object_redirect = Parser.Redirect<object>();

            var p_array = p_array_redirect.Parser;
            var p_object = p_object_redirect.Parser;

            // .Switch is used as we can tell by looking at the first character which parser to use
            var p_value = Parser
               .Switch (
                  Parser.SwitchCharacterBehavior.Leave,
                  Tuple.Create ("\""             , p_string.Map (v => v as object)),
                  Tuple.Create ("0123456789"     , p_number),
                  Tuple.Create ("{"              , p_object),
                  Tuple.Create ("["              , p_array),
                  Tuple.Create ("t"              , p_true),
                  Tuple.Create ("f"              , p_false),
                  Tuple.Create ("n"              , p_null)
                  )
               .KeepLeft (p_spaces);

            var p_elements = p_value
               .Array (p_char (',')
               .KeepLeft (p_spaces))
               .Map (TransformObjects);

            p_array_redirect.ParserRedirect = p_elements
               .Between (
                  p_char ('[').KeepLeft (p_spaces),
                  p_char (']')
                  );

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
               .Map (TransformObject);

            var p_eos = Parser.EndOfStream ();

            // .Switch is used as we can tell by looking at the first character which parser to use
            var p_root = Parser
               .Switch (
                  Parser.SwitchCharacterBehavior.Leave,
                  Tuple.Create ("{", p_object),
                  Tuple.Create ("[", p_array)
                  )
               .KeepLeft (p_spaces);

            s_parser = p_spaces.KeepRight (p_root).KeepLeft (p_spaces).KeepLeft (p_eos);

            // ReSharper restore InconsistentNaming
        }

        public static object Unserialize (string str)
        {
            // TODO: Parser bugs
            // 0123 -> Doesn't generate an error (according to json.org non-zero digits can't start with 0)

            var result = Parser.Parse (s_parser, str);

            return result.IsSuccessful ? result.Value : new JsonUnserializeError (result.ErrorMessage, result.Unconsumed.Begin);
        }

        static readonly CultureInfo s_cultureInfo = CultureInfo.InvariantCulture;

        static void SerializeImpl (StringBuilder stringBuilder, object dyn)
        {
            if (dyn is double)
            {
                stringBuilder.Append (((double)dyn).ToString (s_cultureInfo));
            }
            else if (dyn is int)
            {
                stringBuilder.Append (((int)dyn).ToString (s_cultureInfo));
            }
            else if (dyn is string)
            {
                SerializeStringValue (stringBuilder, (string)dyn);
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
            else if (dyn is IDictionary<string, object>)
            {
                var dictionary = dyn as IDictionary<string, object>;

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

                    SerializeStringValue (stringBuilder, kv.Key);
                    stringBuilder.Append (':');
                    SerializeImpl (stringBuilder, kv.Value);
                }

                stringBuilder.Append ('}');
            }
            else if (dyn is IEnumerable)
            {
                var enumerable = (IEnumerable)dyn;

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

        static void SerializeStringValue (StringBuilder stringBuilder, string str)
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
