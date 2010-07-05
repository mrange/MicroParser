using System.Diagnostics;

namespace MicroParser
{
   public struct SubString
   {
      public string Value;
      public int Position;
      public int Length;

      public override string ToString()
      {
         return (Value ?? "").Substring(Position, Length);
      }

      public char this[int index]
      {
         get
         {
            Debug.Assert (Value != null);
            return Value[Position + index];
         }
      }

   }
}