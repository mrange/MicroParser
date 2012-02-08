﻿// ----------------------------------------------------------------------------------------------
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

// ReSharper disable InconsistentNaming

namespace JsonVisualizer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;
    using JsonVisualizer.Internal;
    using MicroParser.Json;

    public partial class MainWindow
    {
        static readonly CultureInfo s_defaultCulture = CultureInfo.InvariantCulture;

        readonly SolidColorBrush m_error        = new SolidColorBrush (Colors.Red).FreezeIt ();
        readonly SolidColorBrush m_lineNo       = new SolidColorBrush (Colors.DarkGray).FreezeIt ();
        readonly SolidColorBrush m_token        = new SolidColorBrush (Colors.DarkGray).FreezeIt ();
        readonly SolidColorBrush m_string       = new SolidColorBrush (Colors.DarkOrange).FreezeIt ();
        readonly SolidColorBrush m_number       = new SolidColorBrush (Colors.LimeGreen).FreezeIt ();
        readonly SolidColorBrush m_null         = new SolidColorBrush (Colors.DarkViolet).FreezeIt ();
        readonly SolidColorBrush m_bool         = new SolidColorBrush (Colors.DodgerBlue).FreezeIt ();
        readonly SolidColorBrush m_name         = new SolidColorBrush (Colors.Violet).FreezeIt ();


        public MainWindow ()
        {
            InitializeComponent ();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded (object sender, RoutedEventArgs e)
        {
            JsonInput.Focus ();
        }

        void OnJsonChanged (object sender, TextChangedEventArgs e)
        {
            DeferAction (UpdateJson);
        }

        void UpdateJson ()
        {
            var json = JsonInput.Text;

            object result = JsonSerializer.Unserialize (json);

            var error = result as JsonUnserializeError;

            var textBlock = new TextBlock ();

            if (error != null)
            {
                var buildContext = new BuildContext ();
                AppendLine (buildContext, false, new Run (error.ErrorMessage).ColorIt (m_error));
                buildContext.SetInlines (textBlock);
            }
            else
            {
                var buildContext = new BuildContext ();
                BuildInlines (buildContext, false, result);
                buildContext.SetInlines (textBlock);
            }

            JsonOutput.Content = textBlock;
        }

        void BuildInlines (
            BuildContext buildContext,
            bool appendComma,
            object result            
            )
        {
            if (result == null)
            {
                AppendLine (buildContext, appendComma, new Run ("null").ColorIt (m_null));
            }
            else if (result is bool)
            {
                var b = (bool) result;
                AppendLine (buildContext, appendComma, new Run (b ? "true" : "false").ColorIt (m_bool));
            }
            else if (result is double)
            {
                var d = (double) result;
                AppendLine (buildContext, appendComma, new Run (d.ToString (s_defaultCulture)).ColorIt (m_number));
            }
            else if (result is string)
            {
                var s = (string)result;
                AppendLine (buildContext, appendComma, new Run (JsonSerializer.SerializeStringValue (s)).ColorIt (m_string));
            }
            else if (result is IDictionary<string, object>)
            {
                AppendLine (buildContext, false, new Run ("{").ColorIt (m_token));

                var subIndent = buildContext.Indent ();
                var subSubIndent = subIndent.Indent ();
                var dictionary = (IDictionary<string, object>)result;
                foreach (var value in dictionary.Select ((kv, i) => new {kv, i}))
                {
                    AppendLine (
                        subIndent,
                        false,
                        new Run (JsonSerializer.SerializeStringValue (value.kv.Key)).ColorIt (m_name),
                        new Run (" : ").ColorIt (m_token)
                        );
                    BuildInlines (subSubIndent, value.i + 1 < dictionary.Count, value.kv.Value);
                }

                AppendLine (buildContext, appendComma, new Run ("}").ColorIt (m_token));                                
            }
            else if (result is IEnumerable)
            {
                AppendLine (buildContext, false, new Run ("[").ColorIt (m_token));

                var subIndent = buildContext.Indent ();
                var array = (object[])result;
                for (int index = 0; index < array.Length; index++)
                {
                    var value = array[index];
                    BuildInlines (subIndent, index + 1< array.Length, value);
                }

                AppendLine (buildContext, appendComma, new Run ("]").ColorIt (m_token));                
            }
            else
            {
                AppendLine (
                    buildContext,
                    appendComma,
                    new Run (
                        string.Format (
                            s_defaultCulture, 
                            "result is of unknown type: {0}", 
                            result.GetType ()
                            )
                        ).ColorIt (m_error)
                    );
            }
        }

        void AppendLine (
            BuildContext buildContext,
            bool appendComma,
            Inline inline
            )
        {
            ++buildContext.LineNo.Value;
            var inl = buildContext.Inlines;

            inl.Add (new Run (buildContext.LineNo.Value.ToString ("00", s_defaultCulture)).ColorIt (m_lineNo));

            inl.Add (new Run (buildContext.Indention));
            inl.Add (inline);
            if (appendComma)
            {
                inl.Add (new Run (",").ColorIt (m_token));                
            }
            inl.Add (new LineBreak ());
        }

        void AppendLine (
            BuildContext buildContext,
            bool appendComma,
            params Inline[] inlines
            )
        {
            ++buildContext.LineNo.Value;
            var inl = buildContext.Inlines;

            inl.Add (new Run (buildContext.LineNo.Value.ToString ("00", s_defaultCulture)).ColorIt (m_lineNo));

            inl.Add (new Run (buildContext.Indention));
            inl.AddRange (inlines);
            if (appendComma)
            {
                inl.Add (new Run (",").ColorIt (m_token));
            }
            inl.Add (new LineBreak ());
        }

        void DeferAction (Action action)
        {
            if (action != null)
            {
                Dispatcher.BeginInvoke (action, DispatcherPriority.ApplicationIdle);
            }
        }

        void OnClickCopy (object sender, RoutedEventArgs e)
        {
            var textBlock = JsonOutput.Content as TextBlock;
            if (textBlock != null)
            {
                var sb = new StringBuilder (32);

                foreach (var inline in textBlock.Inlines)
                {
                    if (inline is Run)
                    {
                        sb.Append (((Run) inline).Text);
                    }
                    else if (inline is LineBreak)
                    {
                        sb.AppendLine ();
                    }
                    else
                    {
                        // Unrecognized Inline, skipping it
                        Debug.Assert (false);
                    }
                }

                Clipboard.SetText (sb.ToString ());
            }
        }
    }
}
