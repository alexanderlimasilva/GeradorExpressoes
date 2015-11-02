using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GeradorExpressoes
{
    class RPN2Infix
    {
        // operator ranking
        private enum Rank { Primary, Unary, Mul, Sum, }
        private static Dictionary<string, Rank> _rank = new Dictionary<string, Rank>()
        {
            { "#", Rank.Unary },                  // unary minus is coded as "#", unary plus is left out
            { "*", Rank.Mul }, { "/", Rank.Mul }, 
            { "^", Rank.Mul }, { "ln", Rank.Mul }, 
            { "square", Rank.Mul }, { "exp", Rank.Mul }, { "sqrt", Rank.Mul },
            { "+", Rank.Sum }, { "-", Rank.Sum }, // binary op
        };

        // base class
        private abstract class Expr
        {
            internal Rank Rank { get; set; }
            internal abstract void Write(StringBuilder sb);
        }

        // literal number
        private class Number : Expr
        {
            private string Value { get; set; }
            internal Number(string value) { Value = value; Rank = Rank.Primary; }
            internal override void Write(StringBuilder sb) { sb.Append(Value); }
        }

        // binary operations
        private class BinExpr : Expr
        {
            private Expr Left { get;  set; }
            private Expr Right { get;  set; }
            private string Op { get; set; }
            
            private BinExpr(Expr left, Expr right, string op) 
            { Left = left; Right = right; Op = op; Rank = _rank[op]; }
            
            static internal Expr Create(Stack<Expr> stack, string op)
            {
                Expr right = NestedExpr.NestedIfNeeded(stack.Pop(), op);
                Expr left = NestedExpr.NestedIfNeeded(stack.Pop(), op);
                return new BinExpr(left, right, op);
            }
            internal override void Write(StringBuilder sb) { Left.Write(sb); sb.Append(Op); Right.Write(sb); }
        }

        // unary operations
        private class UnaryExpr : Expr
        {
            private string Op { get; set; }
            private Expr Expr { get; set; }

            private UnaryExpr(Expr expr, string op) { Expr=expr; Op=op; Rank=_rank[op]; }
            
            static internal Expr Create(Stack<Expr> stack, string op)
            {
                Expr expr = NestedExpr.NestedIfNeeded(stack.Pop(), op);
                return new UnaryExpr(expr, op);
            }
            
            internal override void Write(StringBuilder sb)
            { sb.Append("("); sb.Append(Op == "#" ? "-" : Op); Expr.Write(sb); sb.Append(")");  }
        }

        // nested expression
        private class NestedExpr : Expr
        {
            internal Expr Expr { get; private set; }
            private NestedExpr(Expr expr) { Expr = expr; Rank = Rank.Primary; }
            internal override void Write(StringBuilder sb) { sb.Append("("); Expr.Write(sb); sb.Append(")"); }
            internal static Expr NestedIfNeeded(Expr expr, string op)
            { return expr.Rank > _rank[op] ? new NestedExpr(expr) : expr; }
        }

        // scanner
        private static string _tokenizer = @"\s*(\d+|\S)\s*";
        private static string[] _unary = new string[] { "#" };

        private static bool IsNumber(string token)
        { return string.IsNullOrEmpty(token) || token.Length < 1 ? false : char.IsNumber(token[0]); }
        
        private static bool IsUnary(string token) { return _unary.Contains(token); }
        
        // parser
        private Stack<Expr> Stack { get; set; }
        private IEnumerable<string> Tokens { get; set; }
        
        // initialize
        private RPN2Infix(string input)
        {
            //Tokens = Regex.Matches(input, _tokenizer, RegexOptions.Compiled|RegexOptions.Singleline).Cast<Match>().Select(m=>m.Groups[1].Value);

            Tokens = input.Split(' ');
            Stack = new Stack<Expr>();
        }
        
        // parse
        private string Parse()
        {
            foreach (string token in Tokens)
            {
                if (IsNumber(token)) Stack.Push(new Number(token));
                else if (IsUnary(token)) Stack.Push(UnaryExpr.Create(Stack, token));
                else Stack.Push(BinExpr.Create(Stack, token));
            }

            StringBuilder sb = new StringBuilder();
            Stack.Pop().Write(sb);
            return sb.ToString();
        }

        // public access
        public static string Parse(string input)
        {
            return new RPN2Infix(input).Parse();
        }

        //
        // PostfixToInfix
        //
        public static string PostfixToInfix(string postfix)
        {
            // Assumption: the postfix expression to be processed is space-delimited.
            // Split the individual tokens into an array for processing.
            var postfixTokens = postfix.Split(' ');

            // Create stack for holding intermediate infix expressions
            var stack = new Stack<Intermediate>();

            foreach (string token in postfixTokens)
            {
                if (token == "+" || token == "-")
                {
                    // Get the left and right operands from the stack.
                    // Note that since + and - are lowest precedence operators,
                    // we do not have to add any parentheses to the operands.
                    var rightIntermediate = stack.Pop();
                    var leftIntermediate = stack.Pop();

                    // construct the new intermediate expression by combining the left and right 
                    // expressions using the operator (token).
                    var newExpr = leftIntermediate.expr + token + rightIntermediate.expr;

                    // Push the new intermediate expression on the stack
                    stack.Push(new Intermediate(newExpr, token));
                }
                else if (token == "*" || token == "/")
                {
                    string leftExpr, rightExpr;

                    // Get the intermediate expressions from the stack.  
                    // If an intermediate expression was constructed using a lower precedent
                    // operator (+ or -), we must place parentheses around it to ensure 
                    // the proper order of evaluation.

                    var rightIntermediate = stack.Pop();
                    if (rightIntermediate.oper == "+" || rightIntermediate.oper == "-")
                    {
                        rightExpr = "(" + rightIntermediate.expr + ")";
                    }
                    else
                    {
                        rightExpr = rightIntermediate.expr;
                    }

                    var leftIntermediate = stack.Pop();
                    if (leftIntermediate.oper == "+" || leftIntermediate.oper == "-")
                    {
                        leftExpr = "(" + leftIntermediate.expr + ")";
                    }
                    else
                    {
                        leftExpr = leftIntermediate.expr;
                    }

                    // construct the new intermediate expression by combining the left and right 
                    // using the operator (token).
                    var newExpr = leftExpr + token + rightExpr;

                    // Push the new intermediate expression on the stack
                    stack.Push(new Intermediate(newExpr, token));
                }
                else
                {
                    // Must be a number. Push it on the stack.
                    stack.Push(new Intermediate(token, ""));
                }
            }

            // The loop above leaves the final expression on the top of the stack.
            return stack.Peek().expr;
        }

    }
}
