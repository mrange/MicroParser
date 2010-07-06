namespace MicroParser
{
   public static class Optional
   {
      public static Optional<TValue> Create<TValue> (TValue value)
      {
         return new Optional<TValue> (value);
      }

      public static Optional<TValue> Create<TValue> ()
      {
         return new Optional<TValue> ();
      }
   }

   public struct Optional<TValue>
   {
      public readonly bool HasValue;
      public readonly TValue Value;

      public Optional (TValue value)
      {
         HasValue = true;
         Value = value;
      }

      public override string ToString ()
      {
         return new
                   {
                      HasValue,
                      Value = HasValue ? Value : default (TValue),
                   }.ToString ();
      }

   }
}