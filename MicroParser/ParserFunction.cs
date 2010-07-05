namespace MicroParser
{
   public delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);

   public sealed class ParserFunctionRedirect<TValue>
   {
      public readonly ParserFunction<TValue> Function;
      public ParserFunction<TValue> Redirect;

      public ParserFunctionRedirect ()
      {
         Function = state => Redirect (state);
      }
      
   }
}