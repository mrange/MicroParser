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
#pragma once
// ----------------------------------------------------------------------------------------------
#include <functional>
#include <utility>
#include <string>
// ----------------------------------------------------------------------------------------------
#pragma push_macro ("min")
#pragma push_macro ("max")
// ----------------------------------------------------------------------------------------------
#undef min
#undef max
// ----------------------------------------------------------------------------------------------
namespace nano_parser
{
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // Typedefs and constants
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   std::size_t const       parser_max_count = 10000      ;

   typedef wchar_t         parser_char                   ;
   typedef std::wstring    parser_string                 ;
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // parser_input_range:
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   struct parser_input_range
   {
      parser_input_range (
         parser_char const *  i     ,
         std::size_t          is    ,
         std::size_t          pos   
         )
         :  input       (i)
         ,  input_size  (is)
         ,  position    (pos)
      {
      }

      parser_input_range (parser_input_range && pir);

      std::size_t          get_size () const throw ()
      {
         return 
            input && position < input_size
               ?  input_size - position
               :  0u;
      }

      parser_string        get_string () const
      {
         auto size = get_size ();

         return size > 0u
            ?  parser_string (input + position, size)
            :  parser_string ()
            ;
      }

      parser_char const *  input                         ;
      std::size_t          input_size                    ;

      std::size_t          position                      ;
   };
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // parser_state:
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   namespace parser_state_advance_state
   {
      enum type
      {
         successful                                = 00  ,
         error                                     = 10  ,
         error__end_of_stream                      = 11  ,
         error__satisfy_failed                     = 12  ,
         error__end_of_stream__position_changed    = 23  ,
         error__satisfy_failed__position_changed   = 24  ,
      };
   }

   struct parser_state_advance_result
   {
      parser_state_advance_result (
         parser_state_advance_state::type    s ,
         parser_input_range                  v
         )
         :  state (s)
         ,  value (v)
      {
      }

      parser_state_advance_result (
         parser_state_advance_result &&   psar
         );

      parser_state_advance_state::type    state ;
      parser_input_range                  value ;
   };

   // -------------------------------------------------------------------------------------------

   struct parser_state
   {
      parser_state (parser_input_range const & pir) throw ()
         :  m_input (pir)
      {
      }

      parser_state (parser_state && ps);

      bool is_at_end_of_stream () const throw ()
      {
         return m_input.get_size () == 0u;
      }

      template<typename TSatisyFunction>
      parser_state_advance_result advance (
         TSatisyFunction         satisfy                                         ,
         std::size_t             min_count                  = 0u                 ,
         std::size_t             max_count                  = parser_max_count
         )
      {
         auto create_error = [&] (parser_state_advance_state::type error)
            {
               return parser_state_advance_result (
                  error    ,
                  m_input
                  );
            };

         if (m_input.position + min_count >= m_input.input_size + 1)
         {
            return create_error (parser_state_advance_state::error__end_of_stream);
         }

         auto initial_position   = m_input.position                  ;

         auto length             = std::min (max_count, m_input.get_size ());

         for (auto iter = 0u; iter < length; ++iter)
         {
            auto current_char = *(m_input.input + m_input.position);

            if (!satisfy (current_char, iter))
            {
               if (iter < min_count)
               {
                  return iter == 0
                     ?  create_error (parser_state_advance_state::error__satisfy_failed)
                     :  create_error (parser_state_advance_state::error__satisfy_failed__position_changed)
                     ;
               }

               return parser_state_advance_result (
                  parser_state_advance_state::successful                ,
                  parser_input_range (m_input.input, initial_position, iter)
                  );
            }

            ++m_input.position;
         }

         return parser_state_advance_result (
            parser_state_advance_state::successful                ,
            parser_input_range (m_input.input, initial_position, m_input.position - initial_position)
            );
      }

      template<typename TSatisyFunction>
      parser_state_advance_state::type skip_advance (
         TSatisyFunction         satisfy                                         ,
         std::size_t             min_count                  = 0u                 ,
         std::size_t             max_count                  = parser_max_count
         )
      {
         return advance (satisfy, min_count, max_count).state;
      }

   private:
      parser_state               (parser_state const &);
      parser_state& operator ==  (parser_state const &);

      parser_input_range m_input;
   };
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // parser_reply:
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   template<typename TValue>
   struct parser_reply
   {
      parser_reply (parser_state & ps, TValue const & value)
         :  m_state (ps)
         ,  m_value (value)
      {
      }

      parser_reply (parser_state & ps, parser_string const & message)
         :  m_state (ps)
         ,  m_value (TValue ())
         ,  m_message (message)
      {
      }

      parser_reply (parser_reply && pr);

      parser_state get_state () const throw ()
      {
         return m_state;
      }

      TValue get_value () const
      {
         return m_value;
      }

      parser_string get_message () const
      {
         return m_message;
      }

   private:
      parser_state   m_state  ;
      TValue         m_value  ;
      parser_string  m_message;
   };

   template<typename TValue, typename TValueCreator>
   parser_reply<TValue> make_reply (
      parser_state &                      ps       ,
      parser_state_advance_result const & result   ,
      TValueCreator                       creator
      )
   {
      switch (result.state)
      {
      case parser_state_advance_state::successful:
         return parser_reply<TValue> (ps, creator ());
      default:
         // TODO:
         return parser_reply<TValue> (ps, L"Failure");
      }


   }
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // parser_functor:
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   template<typename TValue>
   struct parser_functor
   {
      typedef std::tr1::function<parser_reply<TValue> (parser_state &)> function;

      parser_functor (function f)
         :  m_function (f)
      {
      }

      parser_functor (parser_functor && pf);

      function const & get () const throw ()
      {
         return m_function;
      }

   private:
      function m_function;
   };
   // -------------------------------------------------------------------------------------------

   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
   // parser combinators:
   // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

   // p_return: creates a parser that successfully returns 'value'
   template<typename TValue>
   parser_functor<TValue> p_return (TValue const & value)
   {
      return [=] (parser_state & ps) {return parser_reply<TValue> (ps, value);};
   }

   // p_fail: creates a parser that fails with 'message'
   template<typename TValue>
   parser_functor<TValue> p_fail (parser_string const & message)
   {
      return [=] (parser_state & ps) {return parser_reply<TValue> (ps, message);};
   }

   // p_satisy_string: creates a parser that succeeds if all characters matches 
   // 'satisfy' ([] (parser_char ch, std::size_t i) -> bool) and then returns 
   // the range that matches the satisfy. 
   // Otherwise fails with 'satisfy_failure_message'
   template<typename TSatisyFunction>
   parser_functor<parser_input_range> p_satisy_string (
      TSatisyFunction         satisfy                                         ,
      parser_string const &   satisfy_failure_message                         ,
      std::size_t             min_count                  = 0u                 ,
      std::size_t             max_count                  = parser_max_count
      )
   {
      return [=] (parser_state & ps) -> parser_reply<parser_input_range>
         {
            auto result = ps.advance (satisfy, min_count, max_count);
             
            return make_reply<parser_input_range> (ps, result, [=] {return result.value;});
         };

   }
   // -------------------------------------------------------------------------------------------

   // -------------------------------------------------------------------------------------------
}
// ----------------------------------------------------------------------------------------------
#pragma pop_macro ("max")
#pragma pop_macro ("min")
// ----------------------------------------------------------------------------------------------
