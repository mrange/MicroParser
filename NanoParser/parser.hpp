#pragma once

#include <functional>
#include <utility>
#include <string>


namespace nano_parser
{
   typedef std::string string;

   struct parser_state
   {
   };

   template<typename TValue>
   struct parser_reply
   {
      parser_reply (parser_state & ps, TValue const & value)
      {
      }

      parser_reply (parser_state & ps, string const & message)
      {
      }
   };

   template<typename TValue>
   struct parser
   {
      typedef std::tr1::function<parser_reply<TValue> (parser_state &)> function;
   };

   template<typename TValue>
   typename parser<TValue>::function p_return (TValue const & value)
   {
      return [=] (parser_state & ps) {return parser_reply<TValue> (ps, value);};
   }

   template<typename TValue>
   typename parser<TValue>::function p_fail (string const & message)
   {
      return [=] (parser_state & ps) {return parser_reply<TValue> (ps, message);};
   }


}