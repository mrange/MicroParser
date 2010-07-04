namespace MicroParser
{
   public delegate ParserReply<TValue> ParserFunction<TValue>(ParserState state);
}