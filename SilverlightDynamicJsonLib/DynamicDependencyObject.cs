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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace SilverlightDynamicJsonLib
{
   public sealed partial class DynamicDependencyObject : DependencyObject, IDynamicMetaObjectProvider
   {
      static readonly IDictionary<string, DependencyProperty> s_properties =
         new Dictionary<string, DependencyProperty>();

      public DynamicDependencyObject(Tuple<string, object>[] values)
      {
         lock (s_properties)
         {
            foreach (var value in values)
            {
               SetNamedValueImpl(value.Item1, value.Item2);
            }
         }
      }

      void SetNamedValueImpl(string key, object value)
      {
         var name = key ?? "";
         DependencyProperty property;
         if (!s_properties.TryGetValue(name, out property))
         {
            property = DependencyProperty.Register(
               name,
               typeof (object),
               typeof (DynamicDependencyObject),
               new PropertyMetadata(null)
               );
            s_properties[name] = property;
         }

         SetValue(property, value);
      }

      public object GetNamedValue(object name)
      {
         lock (s_properties)
         {
            return GetValue(s_properties[(name ?? string.Empty).ToString()]);
         }
      }

      public object SetNamedValue(string name, object value)
      {
         lock (s_properties)
         {
            SetNamedValueImpl(name ?? "", value);
            return value;
         }
      }

      public DynamicMetaObject GetMetaObject(Expression parameter)
      {
         return new DynamicDependencyMetaObject(
            parameter,
            this
            );
      }
   }

   sealed partial class DynamicDependencyMetaObject : DynamicMetaObject
   {
      static readonly MethodInfo s_getNamedValue = GetMethodInfo(ddm => ddm.GetNamedValue(null));
      static readonly MethodInfo s_setNamedValue = GetMethodInfo(ddm => ddm.SetNamedValue(null, null));

      static MethodInfo GetMethodInfo(Expression<Action<DynamicDependencyObject>> expression)
      {
         return ((MethodCallExpression) expression.Body).Method;
      }

      public DynamicDependencyMetaObject(Expression expression, object value)
         : base(expression, BindingRestrictions.Empty, value)
      {
      }

      BindingRestrictions GetRestrictions()
      {
         return BindingRestrictions.GetTypeRestriction(Expression, typeof (DynamicDependencyObject));
      }

      public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
      {
         var setMemberExpression = Expression.Call(
            GetSelf(),
            s_setNamedValue,
            Expression.Constant(binder.Name),
            Expression.Convert(value.Expression, typeof (object))
            );

         return new DynamicMetaObject(
            setMemberExpression,
            GetRestrictions()
            );
      }

      public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
      {
         var getMemberExpression = Expression.Call(
            GetSelf(),
            s_getNamedValue,
            Expression.Constant(binder.Name)
            );

         return new DynamicMetaObject(
            getMemberExpression,
            GetRestrictions()
            );
      }

      Expression GetSelf()
      {
         return Expression.Convert(Expression, typeof (DynamicDependencyObject));
      }
   }
}