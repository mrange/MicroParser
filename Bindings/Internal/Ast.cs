using System;
using System.Linq;
using MicroParser;

#pragma warning disable 659
// ReSharper disable InconsistentNaming

namespace Bindings.Internal
{
   interface IAst
   {
   }

   abstract class BaseAst : IAst
   {
      int? m_hashCode;

      public override int GetHashCode()
      {
         if (m_hashCode == null)
         {
            m_hashCode = CalculateHashCode();
         }

         return m_hashCode.Value;
      }

      protected abstract int CalculateHashCode();
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

      public Ast_Binary(IAst left, BinaryOp op, IAst right)
      {
         Left = left;
         Right = right;
         Op = op;
      }

      public override string ToString()
      {
         return new
         {
            NodeType = "Binary",
            Left,
            Op,
            Right,
         }.ToString();
      }

      protected override int CalculateHashCode()
      {
         return Left.SafeGetHashCode() ^ Op.GetHashCode() ^ Right.SafeGetHashCode();
      }

      public override bool Equals(object obj)
      {
         return Equals(obj as Ast_Binary);
      }

      public bool Equals(Ast_Binary other)
      {
         if (other == null)
         {
            return false;
         }

         return
            Left.SafeEquals(other.Left)
            && Op == other.Op
            && Right.SafeEquals(other.Right);
      }

   }

   sealed class Ast_Value : BaseAst, IEquatable<Ast_Value>
   {
      public readonly double Value;

      public Ast_Value(double value)
      {
         Value = value;
      }

      public override string ToString()
      {
         return new
         {
            NodeType = "Value",
            Value,
         }.ToString();
      }

      protected override int CalculateHashCode()
      {
         return Value.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         return Equals(obj as Ast_Value);
      }

      public bool Equals(Ast_Value other)
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
      public readonly string Root;
      public readonly string[] Names;

      public Ast_Variable(
         Optional<VariableModifier> variableModifier,
         string root, 
         string[] names
         )
      {
         VariableModifier = variableModifier;
         Root = root ?? "";
         Names = names ?? new string[0];
      }

      public override string ToString()
      {
         return new
         {
            NodeType = "Variable",
            VariableModifier,
            Root,
            Names = Names.Concatenate(","),
         }.ToString();
      }

      protected override int CalculateHashCode()
      {
         return
               Root.SafeGetHashCode()
            ^ Names
               .Aggregate(
                  0x55555555,
                  (s, v) => s ^ v.SafeGetHashCode()
                  );
      }

      public override bool Equals(object obj)
      {
         return Equals(obj as Ast_Variable);
      }

      public bool Equals(Ast_Variable other)
      {
         if (other == null)
         {
            return false;
         }

         return
               Root.SafeEquals(other.Root)
            && Names.SequenceEqual(other.Names)
            ;
      }
   }

}
