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



namespace MicroParser
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Linq;
   using System.Reflection;
   using System.Reflection.Emit;

   using Internal;

   sealed partial class CharSatisfy
   {
      public delegate bool Function (char ch, int index);

      public readonly IParserErrorMessage ErrorMessage;
      public readonly Function Satisfy;

      static CharSatisfy ()
      {
         var assemblyName = new AssemblyName ("MicroParser.Dynamic");

         s_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
         s_moduleBuilder = s_assemblyBuilder.DefineDynamicModule ("MicroParser.Dynamic.Module");
      }

      public static implicit operator CharSatisfy (char ch)
      {
         return new CharSatisfy (
            new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (ch)), 
            (c, i) => ch == c
            );
      }

      public CharSatisfy (IParserErrorMessage errorMessage, Function satisfy)
      {
         ErrorMessage = errorMessage;
         Satisfy = satisfy;
      }

#if !MICRO_PARSER_SUPPRESS_ANONYMOUS_TYPE
      public override string ToString ()
      {
         return new
                   {
                      ErrorMessage,
                   }.ToString ();
      }
#endif

      static readonly object s_lock = new object ();
      static readonly AssemblyBuilder s_assemblyBuilder;
      static readonly ModuleBuilder s_moduleBuilder;

      static readonly IDictionary<string, Function> s_anyOf = new Dictionary<string, Function> ();
      static readonly IDictionary<string, Function> s_noneOf = new Dictionary<string, Function>();

      public static readonly CharSatisfy AnyChar = new CharSatisfy (ParserErrorMessages.Expected_Any, (c, i) => true);
      public static readonly CharSatisfy WhiteSpace = new CharSatisfy (ParserErrorMessages.Expected_WhiteSpace   , (c, i) => Char.IsWhiteSpace (c));
      public static readonly CharSatisfy Digit      = new CharSatisfy (ParserErrorMessages.Expected_Digit        , (c, i) => Char.IsDigit (c));
      public static readonly CharSatisfy Letter     = new CharSatisfy (ParserErrorMessages.Expected_Letter       , (c, i) => Char.IsLetter (c));

      public static readonly CharSatisfy LineBreak  = new CharSatisfy (ParserErrorMessages.Expected_LineBreak    , (c, i) =>
         {
            switch (c)
            {
               case '\r':
               case '\n':
                  return true;
               default:
                  return false;
            }
         });

#if !MICRO_PARSER_SUPPRESS_CHAR_SATISFY_COMPOSITES
      public static readonly CharSatisfy LineBreakOrWhiteSpace  = LineBreak.Or (WhiteSpace);
      public static readonly CharSatisfy LetterOrDigit          = Letter.Or (Digit);
#endif

      static Function CreateSatisfyFromString (
         string match,
         bool matchResult
         )
      {
         lock (s_lock)
         {
            var dic = matchResult ? s_anyOf : s_noneOf;

            Function func;
            if (!dic.TryGetValue (match, out func))
            {
               var typeBuilder = s_moduleBuilder.DefineType (
                  "Satisfy_{0}_{1}".FormatString (
                     matchResult ? "AnyOf" : "NoneOf",
                     match
                     ));

               var methodBuilder = typeBuilder.DefineMethod (
                  "Execute",
                  MethodAttributes.Public,
                  CallingConventions.Standard,
                  typeof (bool),
                  new[] {typeof (char), typeof (int)}
                  );

               var ilGenerator = methodBuilder.GetILGenerator ();

               for (var iter = 0; iter < match.Length; ++iter)
               {
                  var ch = match[iter];

                  ilGenerator.Emit (OpCodes.Ldarg_1);
                  ilGenerator.Emit (OpCodes.Ldc_I4, ch);
                  ilGenerator.Emit (OpCodes.Ceq);
                  if (iter != 0)
                  {
                     ilGenerator.Emit (OpCodes.Or);
                  }
               }

               if (!matchResult)
               {
                  ilGenerator.Emit (OpCodes.Ldc_I4_0);
                  ilGenerator.Emit (OpCodes.Ceq);
               }

               ilGenerator.Emit (OpCodes.Ret);

               var type = typeBuilder.CreateType ();
               var instance = Activator.CreateInstance (type);
               func = (Function) Delegate.CreateDelegate (typeof (Function), instance, "Execute");
               dic[match] = func;
            }

            return func;
         }

      }

      static Function CreateSatisfyFunctionForAnyOfOrNoneOf (
         string match,
         bool matchResult
         )
      {
         if (match.Length < 4)
         {
            return CreateSatisfyFromString (match, matchResult);
         }
         if (!match.Any (ch => ch > 255))
         {
            var boolMap = Enumerable.Repeat (!matchResult, 256).ToArray ();
            foreach (var c in match)
            {
               boolMap[c] = matchResult;
            }

            return (c, i) => ((c & 0xFF00) == 0) && boolMap[c & 0xFF];
         }
         if (match.Length < 16)
         {
            return CreateSatisfyFromString (match, matchResult);
         }

         var hashSet = new HashSet<char>(match);
         return (c, i) => hashSet.Contains (c) ? matchResult : !matchResult;
      }

      static CharSatisfy CreateSatisfyForAnyOfOrNoneOf (
         string match,
         Func<char, IParserErrorMessage> action,
         bool matchResult
         )
      {
         if (string.IsNullOrEmpty (match))
         {
            throw new ArgumentNullException ("match");
         }

         var errorMessages = match
            .Select (action)
            .ToArray ()
            ;

         return new CharSatisfy (
            new ParserErrorMessage_Group (errorMessages),
            CreateSatisfyFunctionForAnyOfOrNoneOf (match, matchResult)
            );
      }

      public static CharSatisfy CreateSatisfyForAnyOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Expected (Strings.CharSatisfy.FormatChar_1.FormatString (x)),
            true
            );
      }

      public static CharSatisfy CreateSatisfyForNoneOf (string match)
      {
         return CreateSatisfyForAnyOfOrNoneOf (
            match,
            x => new ParserErrorMessage_Unexpected (Strings.CharSatisfy.FormatChar_1.FormatString (x)),
            false
            );
      }
   }
}