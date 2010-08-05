// NanoParser.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "parser.hpp"


int main (int argc, char* argv[])
{
   auto parser = nano_parser::p_return<int> (3);

	return 0;
}

