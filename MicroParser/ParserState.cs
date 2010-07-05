using System;
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

      public ParserState_AdvanceResult Advance (
         ref SubString subString,
         CharSatisfyFunction satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         Debug.Assert (minCount <= maxCount);

         var localSatisfy = satisfy ?? CharParser.SatisyAnyChar.Satisfy;

         subString.Value = m_text;
         subString.Position = m_position;

         if (m_position + minCount >= m_text.Length + 1)
         {
            return ParserState_AdvanceResult.Error_EndOfStream;
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
                            ? ParserState_AdvanceResult.Error_SatisfyFailed
                            : ParserState_AdvanceResult.Error_SatisfyFailed_PositionChanged
                     ;
               }

               subString.Length = m_position - subString.Position;

               return ParserState_AdvanceResult.Successful;
            }

            ++m_position;
         }

         subString.Length = m_position - subString.Position;

         return ParserState_AdvanceResult.Successful;
      }

      public ParserState_AdvanceResult SkipAdvance (
         CharSatisfyFunction satisfy,
         int minCount = 1,
         int maxCount = int.MaxValue
         )
      {
         var subString = new SubString ();
         return Advance (ref subString, satisfy, minCount, maxCount);
      }

      public override string ToString ()
      {
         return new
                   {
                      Position,
                      EndOfStream,
                      Current = !EndOfStream ? new string (m_text[m_position], 1) : "End of stream",
                   }.ToString ();
      }

      public static ParserState Create (int position, string text)
      {
         return new ParserState (Math.Max (position, 0), text ?? "");
      }

      public static ParserState Clone (ParserState parserState)
      {
         return new ParserState (parserState.m_position, parserState.m_text);
      }

   }
}