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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace MicroParser.WpfBindings.Internal
{
   abstract class AstVisitor<TValue>
   {
      public TValue Visit (IAst ast)
      {
         var astBinary = ast as Ast_Binary;
         if (astBinary != null)
         {
            return OnBinary (astBinary);
         }

         var astValue = ast as Ast_Value;
         if (astValue != null)
         {
            return OnValue (astValue);
         }

         var astVariable = ast as Ast_Variable;
         if (astVariable != null)
         {
            return OnVariable (astVariable);
         }

         Debug.Assert (false);
         return default (TValue);
      }

      protected abstract TValue OnBinary (Ast_Binary astBinary);
      protected abstract TValue OnValue (Ast_Value astValue);
      protected abstract TValue OnVariable (Ast_Variable astVariable);
   }

   sealed class CollectVariablesAstVisitor : AstVisitor<IAst>
   {
      public readonly Dictionary<Ast_Variable, int> Variables = new Dictionary<Ast_Variable, int> ();

      protected override IAst OnBinary (Ast_Binary astBinary)
      {
         var left = Visit (astBinary.Left);
         var right = Visit (astBinary.Right);
         return astBinary;
      }

      protected override IAst OnValue (Ast_Value astValue)
      {
         return astValue;
      }

      protected override IAst OnVariable (Ast_Variable astVariable)
      {
         if (!Variables.ContainsKey (astVariable))
         {
            Variables[astVariable] = Variables.Count;
         }

         return astVariable;
      }
   }

   sealed class ExpressionBuilderAstVisitor : AstVisitor<Expression>
   {
      static MethodInfo GetMethod<TValue> (Expression<Func<TValue>> expression)
      {
         return ((MethodCallExpression)expression.Body).Method;
      }

      static readonly MethodInfo s_minMethod = GetMethod (() => Math.Min (0.0, 0.0));
      static readonly MethodInfo s_maxMethod = GetMethod (() => Math.Max (0.0, 0.0));

      readonly Dictionary<Ast_Variable, int> m_variables;
      readonly ParameterExpression m_inputParameter;

      public ExpressionBuilderAstVisitor (
         Dictionary<Ast_Variable, int> variables,
         ParameterExpression inputParameter
         )
      {
         m_variables = variables;
         m_inputParameter = inputParameter;
      }

      protected override Expression OnBinary (Ast_Binary astBinary)
      {
         var left = Visit (astBinary.Left);
         var right = Visit (astBinary.Right);

         switch (astBinary.Op)
         {
            case BinaryOp.Add:
               return Expression.Add (left, right);
            case BinaryOp.Subtract:
               return Expression.Subtract (left, right);
            case BinaryOp.Multiply:
               return Expression.Multiply (left, right);
            case BinaryOp.Divide:
               return Expression.Divide (left, right);
            case BinaryOp.Max:
               return Expression.Call (s_maxMethod, left, right);
            case BinaryOp.Min:
               return Expression.Call (s_minMethod, left, right);
            default:
               throw new ArgumentOutOfRangeException ();
         }
      }

      protected override Expression OnValue (Ast_Value astValue)
      {
         return Expression.Constant (astValue.Value);
      }

      protected override Expression OnVariable (Ast_Variable astVariable)
      {
         var index = m_variables[astVariable];

         var value = Expression.ArrayIndex (m_inputParameter, Expression.Constant (index));

         return Expression.Condition (
            Expression.TypeIs (value, typeof (double)),
            Expression.Convert (value, typeof (double)),
            Expression.Constant (0.0));
      }
   }

}
