namespace MicroParser
{
   public delegate bool CharSatisfyFunction (char ch, int index);

   public sealed class CharSatify
   {
      public readonly IParserErrorMessage Expected;
      public readonly CharSatisfyFunction Satisfy;

      public static implicit operator CharSatify (char ch)
      {
         return new CharSatify (
            new ParserErrorMessage_Expected (Strings.CharSatisfy.ExpectedChar_1.Form (ch)), 
            (c, i) => ch == c
            );
      }

      public CharSatify(IParserErrorMessage expected, CharSatisfyFunction satisfy)
      {
         Expected = expected;
         Satisfy = satisfy;
      }

      public override string ToString()
      {
         return new
                   {
                      Expected,
                   }.ToString ();
      }
   }
}