﻿////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
// 
// Copyright (c) 2014 Paul Louth
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec.Token
{
    public class Symbol : Parser<SymbolToken>
    {
        public Symbol(string name)
            :
            base(
                inp => Tok.Lexeme<IEnumerable<ParserChar>>(New.String(name))
                          .Select( str => new SymbolToken(str, inp.First().Location) )
                          .Parse(inp)
            )
        { }
    }

    public class Lexeme<A> : Parser<A>
    {
        public Lexeme(Parser<A> p)
            :
            base( 
                inp=>
                    (from    x in p
                     from    w in New.WhiteSpace()
                     select  x)
                    .Parse(inp)
            )
        { }
    }

    public class OneLineComment : Parser<Unit>
    {
        public OneLineComment(LanguageDef def)
            :
            base(
                inp => (from c in
                            New.Try(
                                (from t in New.String(def.CommentLine)
                                 from d in New.Many(New.Satisfy(ch => ch != '\n',"anything but a newline"))
                                 select d))
                        select Unit.Return())
                       .Parse(inp)
            )
        { }
    }

    public class MultiLineComment : Parser<Unit>
    {
        public MultiLineComment(LanguageDef def)
            :
            base(
                inp => (from open in New.Try( New.String(def.CommentStart) )
                        from incom in 
                            def.NestedComments
                                ? new InCommentMulti(def) as Parser<Unit>
                                : new InCommentSingle(def) as Parser<Unit>
                        select incom)
                        .Parse(inp)
            )
        {
        }
    }

    public class InCommentMulti : Parser<Unit>
    {
        public InCommentMulti(LanguageDef def)
            :
            base(
                inp => (from end in New.Try(New.String(def.CommentEnd))
                        select Unit.Return())
                       .Or
                       (from ml in new MultiLineComment(def)
                        from ic1 in new InCommentMulti(def)
                        select ic1)
                       .Or
                       (from sm in
                            New.SkipMany1<Unit>(
                                from chs in New.NoneOf(def.CommentStartEndDistinctChars.Value)
                                select Unit.Return())
                        from ic2 in new InCommentMulti(def)
                        select ic2)
                       .Or
                       (from oo in New.OneOf(def.CommentStartEndDistinctChars.Value)
                        from ic3 in new InCommentMulti(def)
                        select ic3)
                       .Fail("end of comment")
                       .Parse(inp)
            )
        { }
    }

    public class InCommentSingle : Parser<Unit>
    {
        public InCommentSingle(LanguageDef def)
            :
            base(
                inp => (from end in New.Try(New.String(def.CommentEnd))
                        select Unit.Return())
                       .Or
                       (from sm in
                            New.SkipMany1<Unit>(
                                from chs in New.NoneOf(def.CommentStartEndDistinctChars.Value)
                                select Unit.Return())
                        from ic2 in new InCommentSingle(def)
                        select ic2)
                       .Or
                       (from oo in New.OneOf(def.CommentStartEndDistinctChars.Value)
                        from ic3 in new InCommentSingle(def)
                        select ic3)
                       .Fail("end of comment")
                       .Parse(inp)
            )
        { }    }

    public class WhiteSpace : Parser<Unit>
    {
        public WhiteSpace(LanguageDef def)
            :
            base(
                inp =>
                    def.CommentLine == null && def.CommentStart == null 
                        ? New.SkipMany( New.SimpleSpace().Fail("") ).Parse(inp)
                        : def.CommentLine == null
                            ? New.SkipMany<IEnumerable<ParserChar>>(
                                New.SimpleSpace()
                                    .Or(
                                        Tok.MultiLineComment(def)
                                           .Switch<Unit,IEnumerable<ParserChar>>( _ => new ParserChar[0], "")
                                     ))
                                     .Parse(inp)
                            : def.CommentStart == null
                                ? New.SkipMany<IEnumerable<ParserChar>>(
                                      New.SimpleSpace()
                                         .Or(
                                             Tok.OneLineComment(def)
                                                .Switch<Unit,IEnumerable<ParserChar>>( _ => new ParserChar[0], "")
                                         )).Parse(inp)
                                : New.SkipMany<IEnumerable<ParserChar>>(
                                      New.SimpleSpace()
                                         .Or(
                                             Tok.OneLineComment(def)
                                                .Switch<Unit, IEnumerable<ParserChar>>(_ => new ParserChar[0], "")
                                         )
                                         .Or(
                                            Tok.MultiLineComment(def)
                                               .Switch<Unit, IEnumerable<ParserChar>>(_ => new ParserChar[0], "")
                                         )
                                         .Fail("")
                                      )
                                      .Parse(inp)
            )
        {
        }
    }
}
