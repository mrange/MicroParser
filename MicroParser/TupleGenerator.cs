namespace MicroParser
{
   public static class MicroTuple
   {
      public static MicroTuple<TValue1, TValue2> Create<TValue1, TValue2> (
            TValue1 value1
         ,  TValue2 value2
         )
      {
         return new MicroTuple<TValue1, TValue2>
            {
               Item1 = value1 ,
               Item2 = value2 ,
            };
      }
      public static MicroTuple<TValue1, TValue2, TValue3> Create<TValue1, TValue2, TValue3> (
            TValue1 value1
         ,  TValue2 value2
         ,  TValue3 value3
         )
      {
         return new MicroTuple<TValue1, TValue2, TValue3>
            {
               Item1 = value1 ,
               Item2 = value2 ,
               Item3 = value3 ,
            };
      }
   }
   public struct MicroTuple<TValue1, TValue2>
   {
      public TValue1 Item1;
      public TValue2 Item2;

      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
         }.ToString ();
      }
   }

   public struct MicroTuple<TValue1, TValue2, TValue3>
   {
      public TValue1 Item1;
      public TValue2 Item2;
      public TValue3 Item3;

      public override string ToString ()
      {
         return new 
         {
            Item1,
            Item2,
            Item3,
         }.ToString ();
      }
   }

}
