namespace MicroParser
{
   public delegate bool CharSatisfyFunction (char ch, int index);

   public sealed class CharSatify
   {
      public readonly string Expected;
      public readonly CharSatisfyFunction Satisfy;

      public static implicit operator CharSatify (char ch)
      {
         return new CharSatify ("'" + ch + "'", (c, i) => ch == c);
      }

      public CharSatify (string expected, CharSatisfyFunction satisfy)
      {
         Expected = expected;
         Satisfy = satisfy;
      }

      public override string ToString ()
      {
         return new
                   {
                      Expected,
                   }.ToString ();
      }
   }
}