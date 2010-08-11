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
namespace MicroParser
{
   using System;
   using System.Diagnostics;

   partial struct ParserStatePosition
   {
      public readonly int Position;

      public ParserStatePosition (int position)
      {
         Position = position;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      Position,
                   }.ToString ();
      }
#endif

   }

   sealed partial class ParserState
   {
      // ReSharper disable InconsistentNaming
      public enum AdvanceResult
      {
         Successful = 00,
         Error = 10,
         Error_EndOfStream = 11,
         Error_SatisfyFailed = 12,
         Error_EndOfStream_PostionChanged = 23,
         Error_SatisfyFailed_PositionChanged = 24,
      }
      // ReSharper restore InconsistentNaming

      readonly string m_text;
      int m_position;

      public readonly bool SuppressParserErrorMessageOperations;

      ParserState (int position, string text, bool suppressParserErrorMessageOperations)
      {
         m_position = Math.Max (position, 0);
         m_text = text ?? String.Empty;
         SuppressParserErrorMessageOperations = suppressParserErrorMessageOperations;
      }

      internal int InternalPosition
      {
         get
         {
            return m_position;
         }
      }

      public string Text
      {
         get
         {
            return m_text;
         }
      }

      public ParserStatePosition Position
      {
         get
         {
            return new ParserStatePosition (m_position);
         }
      }

      public bool EndOfStream
      {
         get
         {
            return !(m_position < m_text.Length);
         }
      }

      public char? PeekChar ()
      {
         if (EndOfStream)
         {
            return null;
         }

         return m_text[m_position];
      }

      public AdvanceResult Advance (
         ref SubString subString,
         CharSatisfy.Function satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert (minCount <= maxCount);

         var localSatisfy = satisfy ?? CharSatisfy.AnyChar.Satisfy;

         subString.Value = m_text;
         subString.Position = m_position;

         if (m_position + minCount >= m_text.Length + 1)
         {
            return AdvanceResult.Error_EndOfStream;
         }

         var length = Math.Min (maxCount, m_text.Length - m_position);
         for (var iter = 0; iter < length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy (c, iter))
            {
               if (iter < minCount)
               {
                  return subString.Position == m_position
                            ? AdvanceResult.Error_SatisfyFailed
                            : AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               subString.Length = m_position - subString.Position;

               return AdvanceResult.Successful;
            }

            ++m_position;
         }

         subString.Length = m_position - subString.Position;

         return AdvanceResult.Successful;
      }

      public AdvanceResult SkipAdvance (int count)
      {
         if (m_position + count >= m_text.Length + 1)
         {
            return AdvanceResult.Error_EndOfStream;
         }

         m_position += count;

         return AdvanceResult.Successful;
      }

      public AdvanceResult SkipAdvance (
         CharSatisfy.Function satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         var subString = new SubString ();
         return Advance (ref subString, satisfy, minCount, maxCount);
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      Position = m_position,
                      SuppressParserErrorMessageOperations,
                      EndOfStream,
                      Current = !EndOfStream ? new string (m_text[m_position], 1) : Strings.ParserErrorMessages.Eos,
                   }.ToString ();
      }
#endif

      public static ParserState Create (
         string text, 
         int position = 0, 
         bool suppressParserErrorMessageOperations = false
         )
      {
         return new ParserState (
            Math.Max (position, 0), 
            text ?? Strings.Empty,
            suppressParserErrorMessageOperations
            );
      }

      public static ParserState Clone (ParserState parserState)
      {
         if (parserState == null)
         {
            return null;
         }

         return new ParserState (
            parserState.m_position, 
            parserState.m_text, 
            parserState.SuppressParserErrorMessageOperations
            );
      }

   }
}