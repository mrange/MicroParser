using System;
using System.Globalization;
using System.Windows.Data;

namespace Bindings.Internal
{
   sealed class ExpressionValueConverter : IValueConverter
   {
      public readonly Func<object[], double> Converter;

      public ExpressionValueConverter(Func<object[], double> converter)
      {
         Converter = converter;
      }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter (new[] {value});
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

   sealed class ExpressionMultiValueConverter : IMultiValueConverter
   {
      public readonly Func<object[], double> Converter;

      public ExpressionMultiValueConverter (Func<object[], double> converter)
      {
         Converter = converter;
      }

      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter (values);
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

}
