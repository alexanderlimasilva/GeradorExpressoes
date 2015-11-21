// AForge Core Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2007-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Collections.Generic;

    // Quick and dirty implementation of polish expression evaluator

    /// <summary>
    /// Evaluator of expressions written in reverse polish notation.
    /// </summary>
    /// 
    /// <remarks><para>The class evaluates expressions writen in reverse postfix polish notation.</para>
    /// 
    /// <para>The list of supported functuins is:</para>
    /// <list type="bullet">
    /// <item><b>Arithmetic functions</b>: +, -, *, /;</item>
    /// <item><b>square</b> - square;</item>
    /// <item><b>pow/^</b> - pow;</item>
    /// <item><b>ln</b> - natural logarithm;</item>
    /// <item><b>exp</b> - exponent;</item>
    /// <item><b>sqrt</b> - square root.</item>
    /// </list>
    /// 
    /// <para>Arguments for these functions could be as usual constants, written as numbers, as variables,
    /// writen as $&lt;var_number&gt; (<b>$2</b>, for example). The variable number is zero based index
    /// of variables array.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // expression written in polish notation
    /// string expression = " $20 / 3 $1 * +";
    /// // variables for the expression
    /// double[] vars = new double[] { 3, 4 };
    /// // expression evaluation
    /// double result = PolishExpression.Evaluate( expression, vars );
    /// </code>
    /// </remarks>
    /// 
    public static class PolishGerExpression
    {
        
        /// <summary>
        /// Evaluates specified expression.
        /// </summary>
        ///
        /// <param name="expression">Expression written in postfix polish notation.</param>
        /// <param name="variables">Variables for the expression.</param>
        /// 
        /// <returns>Evaluated value of the expression.</returns>
        /// 
        /// <exception cref="ArgumentException">Unsupported function is used in the expression.</exception>
        /// <exception cref="ArgumentException">Incorrect postfix polish expression.</exception>
        ///
        public static double Evaluate( string expression, double[] variables, int indice)
        {
            // split expression to separate tokens, which represent functions ans variables
            string[] tokens = expression.Trim( ).Split( ' ' );
            // arguments stack
            Stack arguments = new Stack( );

            // walk through all tokens
            foreach ( string token in tokens )
            {
                // check for token type
                if ( char.IsDigit( token[0] ) )
                {
                    // the token in numeric argument
                    arguments.Push( double.Parse( token ) );

                }
                else if ( token[0] == '$' )
                {
                    // the token is variable in $ format
                    int index = int.Parse(token.Substring(1));

                    if ( variables == null )
                            throw new ArgumentNullException( "variables", "Cannot reference variables if none are supplied." );
                    if ( index >= variables.Length )
                            throw new ArgumentOutOfRangeException( "variable number", index, "Cannot reference variables beyond the " + variables.Length + " supplied." );

                    arguments.Push(variables[index]);
                    //arguments.Push(variables[int.Parse(token.Substring(1))]);
                
                }
                else if (token[0] == 'X')
                {
                    // the token is variable
                    int index = indice;
                    arguments.Push(variables[index]);
                }
                //else if (token[0] == 'C')
                //{
                //    // the token is a numeric constant in C format
                //    arguments.Push(double.Parse(token.Substring(1)));
                //}
                else
                {
                    // each function has at least one argument, so let's get the top one
                    // argument from stack
                    double v = (double) arguments.Pop( );

                    // check for function
                    switch ( token )
                    {
                        case "+":			// addition
                            arguments.Push( (double) arguments.Pop( ) + v );
                            break;

                        case "-":			// subtraction
                            arguments.Push( (double) arguments.Pop( ) - v );
                            break;

                        case "*":			// multiplication
                            arguments.Push( (double) arguments.Pop( ) * v );
                            break;

                        case "/":			// division
                            arguments.Push( (double) arguments.Pop( ) / v );
                            break;

                        case "^":			// potencia
                            arguments.Push( Math.Pow((double) arguments.Pop(), v) );
                            break;

                        case "square":		// square
                            arguments.Push( Math.Pow( v , 2) );
                            break;

                        case "ln":			// natural logarithm
                            arguments.Push( Math.Log( v ) );
                            break;

                        case "exp":			// exponent
                            arguments.Push( Math.Exp( v ) );
                            break;

                        case "sqrt":		// square root
                            arguments.Push( Math.Sqrt( v ) );
                            break;

                        default:
                            // throw exception informing about undefined function
                            throw new ArgumentException( "Unsupported function: " + token );
                    }
                }
            }

            // check stack size
            if ( arguments.Count != 1 )
            {
                throw new ArgumentException( "Incorrect expression." );
            }

            // return the only value from stack
            return (double) arguments.Pop( );
        }

        /// <summary>
        /// Performs (partial) variable-substitution in the specified expression.
        /// </summary>
        /// 
        /// <remarks><para>If an insufficient number of variables are supplied, this will
        /// substitute values for the supplied variables (0 through m), and renumber the
        /// remaining variables from m+1 through n to 0 through n-m-1</para>
        /// 
        /// <code>
        /// string expression = PolishExpression.SubstituteVariables( "$0 $1 +", new double[] { 3 } );
        /// // expression is now "3 $0 +".
        /// </code>
        /// </remarks>
        ///
        /// <param name="expression">Expression written in postfix polish notation.</param>
        /// <param name="variables">Variables for the expression.</param>
        /// 
        /// <returns>Expression with variable substitution performed.</returns>
        /// 
        public static string SubstituteVariables( string expression, double[] variables )
        {
            string[] tokens = expression.Trim().Split(' ');

            StringBuilder ret = new StringBuilder( );

            int length = variables == null ? 0 : variables.Length;

            // walk through all tokens
            foreach ( string token in tokens )
            {
                if ( token[0] == '$' )
                {
                    // the token is variable
                    //arguments.Push( variables[int.Parse( token.Substring( 1 ) )] );
                    int index = Int32.Parse(token.Substring(1));

                    if (index < length)
                        ret.Append(variables[index]);
                    else
                        ret.Append("$" + (index - length));
                    //ret.Append('x');
                }
                 else
                    ret.Append( token );

                ret.Append( ' ' );
            }
            ret.Remove( ret.Length - 1, 1 );

            return ret.ToString( );
        }

        /// <summary>
        /// Simplifies the specified expression as much as possible without performing variable substitution.
        /// </summary>
        /// <remarks><code>
        /// string expression = PolishExpression.SimplifyExpression( "$0 3 5 + *" );
        /// // expression is now "$0 8 *".
        /// </code>
        /// </remarks>
        ///
        /// <param name="expression">Expression written in postfix polish notation.</param>
        /// 
        /// <returns>Simplified expression.</returns>
        /// 
        /// <exception cref="ArgumentException">Unsupported function is used in the expression.</exception>
        /// <exception cref="ArgumentException">Incorrect postfix polish expression.</exception>
        ///
        public static string SimplifyExpression(string expression)
        {
            string[] tokens = SplitExpression(expression);

            Stack simplified = new Stack();
            foreach (string token in tokens)
            {
                switch (GetTokenType(token))
                {

                    case TokenType.UnaryFunction:
                        if (simplified.Count < 1) throw new ArgumentException("Incorrect expression: Too few arguments for function " + token);
                        if (simplified.Peek() is double)
                            simplified.Push(ExecuteUnaryFunction(token, (double)simplified.Pop()));
                        else
                            simplified.Push(token);
                        break;

                    case TokenType.BinaryFunction:
                        if (simplified.Count < 2) throw new ArgumentException("Incorrect expression: Too few arguments for function " + token);
                        object arg2 = simplified.Pop();
                        if (simplified.Peek() is double && arg2 is double)
                            simplified.Push(ExecuteBinaryFunction(token, (double)simplified.Pop(), (double)arg2));
                        else
                        {
                            simplified.Push(arg2);
                            simplified.Push(token);
                        }
                        break;

                    case TokenType.Number:
                        simplified.Push(Double.Parse(token));
                        break;

                    case TokenType.Variable:
                        simplified.Push(token);
                        break;
                }
            }

            StringBuilder ret = new StringBuilder();
            foreach (object token in simplified)
            {
                ret.Insert(0, token).Insert(0, ' ');
            }
            ret.Remove(0, 1);
            return ret.ToString();
        }

        /// <summary>
        /// Split expression into tokens, which represent values, functions, and variables
        /// </summary>
        /// <param name="expression">The RPN expression to parse</param>
        /// <returns>An array of tokens</returns>
        private static string[] SplitExpression(string expression)
        {
            return expression.Trim().Split(' ');
        }

        /// <summary>
        /// Enumera os tipos de token
        /// </summary>
        enum TokenType { Number, Variable, BinaryFunction, UnaryFunction };

        /// <summary>
        /// Verifica o tipo de token
        /// </summary>
        /// <param name="token">Token a ser verificado</param>
        /// <returns>Retorna to tipo de token</returns>
        private static TokenType GetTokenType(string token)
        {
            if (token[0] == '$') return TokenType.Variable;
            if (token[0] == 'X') return TokenType.Variable;
            switch (token)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                    return TokenType.BinaryFunction;
                case "square":
                case "ln":
                case "exp":
                case "sqrt":
                    return TokenType.UnaryFunction;
            }
            double temp;
            if (Double.TryParse(token, out temp)) { return TokenType.Number; }
            throw new ArgumentException("Unsupported postfix expression: " + token);
        }

        private static double ExecuteUnaryFunction(string function, double arg)
        {
            // check for function
            switch (function)
            {
                case "square":		// square
                    return Math.Pow(arg, 2);

                case "ln":			// natural logarithm
                    return Math.Log(arg);

                case "exp":			// exponent
                    return Math.Exp(arg);

                case "sqrt":		// square root
                    return Math.Sqrt(arg);

                default:
                    // throw exception informing about undefined function
                    throw new ArgumentException("Unsupported function: " + function);
            }
        }

        private static double ExecuteBinaryFunction(string function, double arg1, double arg2)
        {

            // check for function
            switch (function)
            {

                case "+":			// addition
                    return arg1 + arg2;

                case "-":			// subtraction
                    return arg1 - arg2;

                case "*":			// multiplication
                    return arg1 * arg2;

                case "/":			// division
                    return arg1 / arg2;

                case "^":			// pow
                    return Math.Pow(arg1, arg2);

                default:
                    // throw exception informing about undefined function
                    throw new ArgumentException("Unsupported function: " + function);
            }

        }

    }

}
