using System.Collections.Generic;
using System.Windows;
using MicroParser;

namespace SilverlightDynamicJson
{
   sealed class DynamicDependencyObject : DependencyObject
   {
      static readonly IDictionary<string, DependencyProperty> s_properties = 
         new Dictionary<string, DependencyProperty> ();

      public DynamicDependencyObject (MicroTuple<object, object>[] values)
      {
         lock (s_properties)
         {
            foreach (var value in values)
            {
               SetNamedValueImpl (value.Item1, value.Item2);
            }
         }
      }

      private void SetNamedValueImpl(object key, object value)
      {
         var name = key.ToString();
         DependencyProperty property;
         if (!s_properties.TryGetValue(name, out property))
         {
            property = DependencyProperty.Register(
               name,
               typeof(object),
               typeof(DynamicDependencyObject),
               new PropertyMetadata(null)
               );
            s_properties[name] = property;
         }

         SetValue (property, value);
      }

      public object GetNamedValue (object name)
      {
         lock (s_properties)
         {
            return GetValue(s_properties[(name ?? string.Empty).ToString()]);
         }
      }

      public void SetNamedValue(object name, object value)
      {
         lock (s_properties)
         {
            SetNamedValueImpl(name, value);
         }
      }
   }
}