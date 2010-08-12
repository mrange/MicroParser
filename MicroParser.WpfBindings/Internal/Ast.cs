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
using System.Linq;

#pragma warning disable 659
// ReSharper disable InconsistentNaming

namespace MicroParser.WpfBindings.Internal
{
   interface IAst
   {
   }

   abstract class BaseAst : IAst
   {
      int? m_hashCode;

      public override int GetHashCode ()
      {
         if (m_hashCode == null)
         {
            m_hashCode = CalculateHashCode ();
         }

         return m_hashCode.Value;
      }

      protected abstract int CalculateHashCode ();
   }

   enum VariableModifier
   {
      ElementName,
      TemplatedParent,
   }

   enum BinaryOp
   {
      Add,
      Subtract,
      Multiply,
      Divide,
      Max,
      Min,
   }

   sealed class Ast_Binary : BaseAst, IEquatable<Ast_Binary>
   {
      public readonly IAst Left;
      public readonly BinaryOp Op;
      public readonly IAst Right;

      public Ast_Binary (IAst left, BinaryOp op, IAst right)
      {
         Left = left;
         Right = right;
         Op = op;
      }

      public override string ToString ()
      {
         return new
         {
            NodeType = "Binary",
            Left,
            Op,
            Right,
         }.ToString ();
      }

      protected override int CalculateHashCode ()
      {
         return Left.SafeGetHashCode () ^ Op.GetHashCode () ^ Right.SafeGetHashCode ();
      }

      public override bool Equals (object obj)
      {
         return Equals (obj as Ast_Binary);
      }

      public bool Equals (Ast_Binary other)
      {
         if (other == null)
         {
            return false;
         }

         return
            Left.SafeEquals (other.Left)
            && Op == other.Op
            && Right.SafeEquals (other.Right);
      }

   }

   sealed class Ast_Value : BaseAst, IEquatable<Ast_Value>
   {
      public readonly double Value;

      public Ast_Value (double value)
      {
         Value = value;
      }

      public override string ToString ()
      {
         return new
         {
            NodeType = "Value",
            Value,
         }.ToString ();
      }

      protected override int CalculateHashCode ()
      {
         return Value.GetHashCode ();
      }

      public override bool Equals (object obj)
      {
         return Equals (obj as Ast_Value);
      }

      public bool Equals (Ast_Value other)
      {
         if (other == null)
         {
            return false;
         }

         return Value == other.Value;
      }
   }

   sealed class Ast_Variable : BaseAst, IEquatable<Ast_Variable>
   {
      public readonly Optional<VariableModifier> VariableModifier;
      public readonly SubString Root;
      public readonly SubString[] Names;

      public Ast_Variable (
         Optional<VariableModifier> variableModifier,
         SubString root,
         SubString[] names
         )
      {
         VariableModifier = variableModifier;
         Root = root;
         Names = names;
      }

      public override string ToString ()
      {
         return new
         {
            NodeType = "Variable",
            VariableModifier,
            Root,
            Names = Names.Select (ss => ss.ToString ()).Concatenate (","),
         }.ToString ();
      }

      protected override int CalculateHashCode ()
      {
         return
               Root.GetHashCode ()
            ^ Names
               .Aggregate (
                  0x55555555,
                  (s, v) => s ^ v.GetHashCode ()
                  );
      }

      public override bool Equals (object obj)
      {
         return Equals (obj as Ast_Variable);
      }

      public bool Equals (Ast_Variable other)
      {
         if (other == null)
         {
            return false;
         }

         return
               Root.Equals (other.Root)
            && Names.SequenceEqual (other.Names)
            ;
      }
   }

}
