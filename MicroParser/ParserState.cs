using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MicroParser
{
   // ReSharper disable InconsistentNaming
   public enum ParserState_AdvanceResult
   {
      Successful                                   = 00,
      Error                                        = 10,
      Error_EndOfStream                            = 11,
      Error_SatisfyFailed                          = 12,
      Error_EndOfStream_PostionChanged             = 23,
      Error_SatisfyFailed_PositionChanged          = 24,
   }
   // ReSharper restore InconsistentNaming

   public class ParserState
   {
      int m_position;
      readonly string m_text;

      ParserState (int position, string text)
      {
         m_position = position;
         m_text = text;
      }

      static readonly Func<char, int, bool> s_satisfyAll = (c, i) => true;

      public int Position
      {
         get
         {
            return m_position;
         }
      }

      public bool EndOfStream
      {
         get
         {
            return !(m_position < m_text.Length);
         }
      }

      public ParserState_AdvanceResult Advance (char[] buffer, Func<char,int,bool> satisfy)
      {
         if (m_position + buffer.Length >= m_text.Length + 1)
         {
            return ParserState_AdvanceResult.Error_EndOfStream;
         }

         var localSatisfy = satisfy ?? s_satisfyAll;

         var originalPosition = m_position;

         for (var iter = 0; iter < buffer.Length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy (c, iter))
            {
               return originalPosition == m_position
                         ? ParserState_AdvanceResult.Error_SatisfyFailed
                         : ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged
                  ;
            }

            ++m_position;
            buffer[iter] = c;
         }

         return ParserState_AdvanceResult.Successful;
      }

      public ParserState_AdvanceResult Advance(
         List<char> buffer,
         Func<char, int, bool> satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert (minCount <= maxCount);

         if (buffer.Capacity < minCount)
         {
            buffer.Capacity = minCount;
         }

         var localSatisfy = satisfy ?? s_satisfyAll;

         var originalPosition = m_position;

         if (m_position + minCount >= m_text.Length + 1)
         {
            return ParserState_AdvanceResult.Error_EndOfStream;
         }

         var length = Math.Min (maxCount, m_text.Length - m_position);
         for (var iter = 0; iter < length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy (c, m_position - originalPosition))
            {
               if (iter < minCount)
               {
                  return originalPosition == m_position
                            ? ParserState_AdvanceResult.Error_SatisfyFailed
                            : ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               return ParserState_AdvanceResult.Successful;
            }

            ++m_position;
            buffer.Add(c);
         }

         return ParserState_AdvanceResult.Successful;
      }

      public ParserState_AdvanceResult SkipAdvance(
         Func<char, int, bool> satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert(minCount <= maxCount);

         var localSatisfy = satisfy ?? s_satisfyAll;

         var originalPosition = m_position;

         if (m_position + minCount >= m_text.Length + 1)
         {
            return ParserState_AdvanceResult.Error_EndOfStream;
         }

         var length = Math.Min(maxCount, m_text.Length - m_position);
         for (var iter = 0; iter < length; ++iter)
         {
            var c = m_text[m_position];

            if (!localSatisfy(c, m_position - originalPosition))
            {
               if (iter < minCount)
               {
                  return originalPosition == m_position
                            ? ParserState_AdvanceResult.Error_SatisfyFailed
                            : ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               return ParserState_AdvanceResult.Successful;
            }

            ++m_position;
         }

         return ParserState_AdvanceResult.Successful;
      }

      public override string ToString()
      {
         return new
                   {
                      Position = m_position,
                      Current = m_position < m_text.Length ? new string (m_text[m_position], 1) : "EOS",
                   }.ToString ();
      }

      public static ParserState Create(int position, string text)
      {
         return new ParserState(Math.Max(position, 0), text ?? "");
      }

      public static ParserState Clone(ParserState parserState)
      {
         return new ParserState(parserState.m_position, parserState.m_text);
      }

   }
}