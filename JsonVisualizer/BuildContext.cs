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

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace JsonVisualizer
{
    sealed class BuildContext
    {
        const string s_indent = "   ";

        public readonly string Indention;
        public readonly List<Inline> Inlines;
        public LineNo LineNo;

        public BuildContext ()
            : this ("  ", new List<Inline>(), new LineNo ())
        {
        }

        BuildContext (
            string indention,
            List<Inline> inlines,
            LineNo lineNo
            )
        {
            Indention = indention;
            Inlines = inlines;
            LineNo = lineNo;
        }

        public BuildContext Indent ()
        {
            return new BuildContext (Indention + s_indent, Inlines, LineNo);
        }

        public void SetInlines (TextBlock textBlock)
        {
            if (textBlock != null)
            {
                textBlock.Inlines.Clear ();
                textBlock.Inlines.AddRange (Inlines);
            }
        }

    }
}