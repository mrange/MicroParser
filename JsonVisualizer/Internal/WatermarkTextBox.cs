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

namespace JsonVisualizer.Internal
{
    using System.Windows;
    using System.Windows.Controls;

    partial class WatermarkTextBox : TextBox
    {
        static WatermarkTextBox ()
        {
            DefaultStyleKeyProperty.OverrideMetadata (typeof (WatermarkTextBox), new FrameworkPropertyMetadata (typeof (WatermarkTextBox)));
        }

        void SetWatermarkVisibility ()
        {
            IsWatermarkVisible = IsWatermarkEnabled && Text.IsNullOrEmpty ();
        }

        protected override void OnGotFocus (RoutedEventArgs e)
        {
            base.OnGotFocus (e);

            SetWatermarkVisibility ();
        }

        protected override void OnLostFocus (RoutedEventArgs e)
        {
            base.OnLostFocus (e);

            SetWatermarkVisibility ();
        }

        protected override void OnTextChanged (TextChangedEventArgs e)
        {
            base.OnTextChanged (e);

            SetWatermarkVisibility ();
        }
    }
}