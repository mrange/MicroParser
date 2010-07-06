using System.Collections;
using System.Collections.Generic;

namespace MicroParser
{
   public class ImmutableList<TValue> : IEnumerable<TValue>
   {
      public static ImmutableList<TValue> Empty = new ImmutableList<TValue> (default (TValue), null);

      public TValue Head;
      public ImmutableList<TValue> Tail;

      ImmutableList (TValue value, ImmutableList<TValue> tail)
      {
         Head = value;
         Tail = tail;
      }

      public bool IsEmpty
      {
         get
         {
            return Tail != null;
         }
      }

      public static ImmutableList<TValue> Singleton (TValue value)
      {
         return new ImmutableList<TValue> (value, Empty);
      }

      public static ImmutableList<TValue> Cons(TValue value, ImmutableList<TValue> tail)
      {
         return new ImmutableList<TValue>(value, tail);
      }

      public IEnumerator<TValue> GetEnumerator()
      {
         var current = this;

         while (current != null)
         {
            yield return current.Head;
            current = current.Tail;
         }
      }

      IEnumerator IEnumerable.GetEnumerator ()
      {
         return GetEnumerator ();
      }
   }
}