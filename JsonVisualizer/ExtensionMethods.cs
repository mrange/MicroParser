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

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace JsonVisualizer
{
    static class ExtensionMethods
    {
        public static TFreezable FreezeIt<TFreezable> (this TFreezable freezable)
            where TFreezable : Freezable
        {
            if (freezable != null && freezable.CanFreeze && !freezable.IsFrozen)
            {
                freezable.Freeze ();
            }
            return freezable;
        }


        public static TInline ColorIt<TInline>(this TInline inline, Brush brush)
            where TInline : Inline
        {
            if (inline != null)
            {
                inline.Foreground = brush;
            }

            return inline;
        }
    }
}