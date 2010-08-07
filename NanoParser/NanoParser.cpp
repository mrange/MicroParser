// NanoParser.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "nano_parser.hpp"


int main (int argc, char* argv[])
{
   auto parser = nano_parser::p_return<int> (3);

   auto parser2 = nano_parser::p_satisy_string (
      [] (wchar_t ch, std::size_t i) {return true;},
      L"any",
      1
      );

	return 0;
}

