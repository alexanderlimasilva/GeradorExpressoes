using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeradorExpressoes
{
    class Intermediate
    {
        public string expr;     // subexpression string
        public string oper;     // the operator used to create this expression

        public Intermediate(string expr, string oper)
        {
            this.expr = expr;
            this.oper = oper;
        }

    }
}
