MicroParser – a minimal parser combinator library for C# with the focus on light dependencies, small size and reasonable performance for strings that fits in memory

See [MicroParser.pdf](docs/MicroParser.pdf) for an introduction to how to get started with MicroParser

There a quite a few ways to parse text:
1. Write your own parser code
2. Regular Expression
3. FParsec ([http://www.quanttec.com/fparsec/](http://www.quanttec.com/fparsec/))
4. Yacc/Lex and similar
5. FSYacc/FSLex (shipped with F#)
6. boost.spirit ([http://boost-spirit.com/home/](http://boost-spirit.com/home/))
7. MicroParser

MicroParser aims to be small, easily deployable and to build on great ideas from FParsec/Parsec but is implemented in C# instead of F#/Haskell. MicroParser should be a good choice when developers want to write parsers for simple expressions such as:

```
2*(x + 1) + y + 3
```

MicroParser also aims to give decent error reporting although at the time of writing it's not on par with some of the more complete parser frameworks such as FParsec.

MicroParser requires VisualStudio 2010 but is compatible with .NET 3.5.

