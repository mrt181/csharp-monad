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

using NUnit.Framework;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    [TestFixture]
    public class TestExpr
    {
        [Test]
        public void ExpressionTests()
        {
            var ten = Eval("2*3+4");

            Assert.IsTrue(ten == 10);

            var fourteen = Eval("2*(3+4)");

            Assert.IsTrue(fourteen == 14);
        }


        public int Eval(string expr)
        {
            var r = NewT.Expr().Parse(expr);
            if (r.Value.Count() == 0)
            {
                return 999;
            }
            else
            {
                return r.Value.First().Item1;
            }
        }
    }


    public class NewT
    {
        public static Expr Expr()
        {
            return new Expr();
        }
        public static Term Term()
        {
            return new Term();
        }
        public static Factor Factor()
        {
            return new Factor();
        }
    }

    public class Expr : Parser<int>
    {
        public Expr()
            :
            base(
                inp => (from t in NewT.Term()
                        from e in
                            New.Choice<int>(
                                from plus in New.Character('+')
                                from expr in NewT.Expr()
                                select expr,
                                New.Return<int>(0)
                                )
                        select t + e)
                       .Parse(inp)
            )
        { }
    }

    public class Term : Parser<int>
    {
        public Term()
            :
            base(
                inp => (from f in NewT.Factor()
                        from t in
                            New.Choice<int>(
                                from mult in New.Character('*')
                                from term in NewT.Term()
                                select term,
                                New.Return<int>(1)
                                )
                        select f * t)
                       .Parse(inp)
            )
        { }
    }

    public class Factor : Parser<int>
    {
        public Factor()
            :
            base(
                inp => (from choice in
                            New.Choice<int>(
                                from d in New.Digit()
                                select Int32.Parse(d.Value.ToString()),
                                from open in New.Character('(')
                                from expr in NewT.Expr()
                                from close in New.Character(')')
                                select expr
                                )
                        select choice)
                        .Parse(inp)

            )
        { }

    }
}
