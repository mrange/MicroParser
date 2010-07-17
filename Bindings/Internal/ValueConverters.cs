// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;

namespace Bindings.Internal
{
   sealed class ExpressionValueConverter : IValueConverter
   {
      public readonly Func<object[], double> Converter;

      public ExpressionValueConverter (Func<object[], double> converter)
      {
         Converter = converter;
      }

      public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter (new[] {value});
      }

      public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException ();
      }
   }

   sealed class ExpressionMultiValueConverter : IMultiValueConverter
   {
      public readonly Func<object[], double> Converter;

      public ExpressionMultiValueConverter (Func<object[], double> converter)
      {
         Converter = converter;
      }

      public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
      {
         return Converter (values);
      }

      public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException ();
      }
   }

}
