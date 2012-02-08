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

// ############################################################################
// #                                                                          #
// #        ---==>  T H I S  F I L E  I S   G E N E R A T E D  <==---         #
// #                                                                          #
// # This means that any edits to the .cs file will be lost when its          #
// # regenerated. Changes should instead be applied to the corresponding      #
// # template file (.tt)                                                      #
// ############################################################################




// ReSharper disable InconsistentNaming
// ReSharper disable InvocationIsSkipped
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantAssignment
// ReSharper disable RedundantUsingDirective

namespace JsonVisualizer.Internal
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media;

    // ------------------------------------------------------------------------
    // WatermarkTextBox
    // ------------------------------------------------------------------------
    partial class WatermarkTextBox
    {
        #region Uninteresting generated code
        public static readonly DependencyProperty IsWatermarkEnabledProperty = DependencyProperty.Register (
            "IsWatermarkEnabled",
            typeof (bool),
            typeof (WatermarkTextBox),
            new FrameworkPropertyMetadata (
                true,
                FrameworkPropertyMetadataOptions.None,
                Changed_IsWatermarkEnabled,
                Coerce_IsWatermarkEnabled          
            ));

        static void Changed_IsWatermarkEnabled (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance != null)
            {
                var oldValue = (bool)eventArgs.OldValue;
                var newValue = (bool)eventArgs.NewValue;

                instance.Changed_IsWatermarkEnabled (oldValue, newValue);
            }
        }

        static object Coerce_IsWatermarkEnabled (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance == null)
            {
                return basevalue;
            }
            var oldValue = (bool)basevalue;
            var newValue = oldValue;

            instance.Coerce_IsWatermarkEnabled (oldValue, ref newValue);


            return newValue;
        }

        public static readonly DependencyProperty IsWatermarkVisibleProperty = DependencyProperty.Register (
            "IsWatermarkVisible",
            typeof (bool),
            typeof (WatermarkTextBox),
            new FrameworkPropertyMetadata (
                true,
                FrameworkPropertyMetadataOptions.None,
                Changed_IsWatermarkVisible,
                Coerce_IsWatermarkVisible          
            ));

        static void Changed_IsWatermarkVisible (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance != null)
            {
                var oldValue = (bool)eventArgs.OldValue;
                var newValue = (bool)eventArgs.NewValue;

                instance.Changed_IsWatermarkVisible (oldValue, newValue);
            }
        }

        static object Coerce_IsWatermarkVisible (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance == null)
            {
                return basevalue;
            }
            var oldValue = (bool)basevalue;
            var newValue = oldValue;

            instance.Coerce_IsWatermarkVisible (oldValue, ref newValue);


            return newValue;
        }

        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register (
            "WatermarkText",
            typeof (string),
            typeof (WatermarkTextBox),
            new FrameworkPropertyMetadata (
                "Enter text here...",
                FrameworkPropertyMetadataOptions.None,
                Changed_WatermarkText,
                Coerce_WatermarkText          
            ));

        static void Changed_WatermarkText (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance != null)
            {
                var oldValue = (string)eventArgs.OldValue;
                var newValue = (string)eventArgs.NewValue;

                instance.Changed_WatermarkText (oldValue, newValue);
            }
        }

        static object Coerce_WatermarkText (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance == null)
            {
                return basevalue;
            }
            var oldValue = (string)basevalue;
            var newValue = oldValue;

            instance.Coerce_WatermarkText (oldValue, ref newValue);


            return newValue;
        }

        public static readonly DependencyProperty WatermarkBrushProperty = DependencyProperty.Register (
            "WatermarkBrush",
            typeof (Brush),
            typeof (WatermarkTextBox),
            new FrameworkPropertyMetadata (
                default (Brush),
                FrameworkPropertyMetadataOptions.None,
                Changed_WatermarkBrush,
                Coerce_WatermarkBrush          
            ));

        static void Changed_WatermarkBrush (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance != null)
            {
                var oldValue = (Brush)eventArgs.OldValue;
                var newValue = (Brush)eventArgs.NewValue;

                instance.Changed_WatermarkBrush (oldValue, newValue);
            }
        }

        static object Coerce_WatermarkBrush (DependencyObject dependencyObject, object basevalue)
        {
            var instance = dependencyObject as WatermarkTextBox;
            if (instance == null)
            {
                return basevalue;
            }
            var oldValue = (Brush)basevalue;
            var newValue = oldValue;

            instance.Coerce_WatermarkBrush (oldValue, ref newValue);


            return newValue;
        }

        #endregion

        // --------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------
        public WatermarkTextBox ()
        {
            CoerceValue (IsWatermarkEnabledProperty);
            CoerceValue (IsWatermarkVisibleProperty);
            CoerceValue (WatermarkTextProperty);
            CoerceValue (WatermarkBrushProperty);
            Constructed__WatermarkTextBox ();
        }
        // --------------------------------------------------------------------
        partial void Constructed__WatermarkTextBox ();
        // --------------------------------------------------------------------

        // --------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------

           
        // --------------------------------------------------------------------
        public bool IsWatermarkEnabled
        {
            get
            {
                return (bool)GetValue (IsWatermarkEnabledProperty);
            }
            set
            {
                if (IsWatermarkEnabled != value)
                {
                    SetValue (IsWatermarkEnabledProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_IsWatermarkEnabled (bool oldValue, bool newValue);
        partial void Coerce_IsWatermarkEnabled (bool value, ref bool coercedValue);
        // --------------------------------------------------------------------


           
        // --------------------------------------------------------------------
        public bool IsWatermarkVisible
        {
            get
            {
                return (bool)GetValue (IsWatermarkVisibleProperty);
            }
            set
            {
                if (IsWatermarkVisible != value)
                {
                    SetValue (IsWatermarkVisibleProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_IsWatermarkVisible (bool oldValue, bool newValue);
        partial void Coerce_IsWatermarkVisible (bool value, ref bool coercedValue);
        // --------------------------------------------------------------------


           
        // --------------------------------------------------------------------
        public string WatermarkText
        {
            get
            {
                return (string)GetValue (WatermarkTextProperty);
            }
            set
            {
                if (WatermarkText != value)
                {
                    SetValue (WatermarkTextProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_WatermarkText (string oldValue, string newValue);
        partial void Coerce_WatermarkText (string value, ref string coercedValue);
        // --------------------------------------------------------------------


           
        // --------------------------------------------------------------------
        public Brush WatermarkBrush
        {
            get
            {
                return (Brush)GetValue (WatermarkBrushProperty);
            }
            set
            {
                if (WatermarkBrush != value)
                {
                    SetValue (WatermarkBrushProperty, value);
                }
            }
        }
        // --------------------------------------------------------------------
        partial void Changed_WatermarkBrush (Brush oldValue, Brush newValue);
        partial void Coerce_WatermarkBrush (Brush value, ref Brush coercedValue);
        // --------------------------------------------------------------------


    }
    // ------------------------------------------------------------------------

}

