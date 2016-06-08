using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AForge;
using AForge.Genetic;
using AForge.Controls;

namespace GeradorExpressoes
{

    class ExpressionGeneFunction : IGPGene
    {
        /// <summary>
        /// Enumeration of supported functions.
        /// </summary>
        protected enum Functions
        {
            /// <summary>
            /// Addition operator.
            /// Arguments 2
            /// </summary>
            Add = 0,
            /// <summary>
            /// Subtraction operator.
            /// Arguments 2
            /// </summary>
            Subtract = 1,
            /// <summary>
            /// Multiplication operator.
            /// Arguments 2
            /// </summary>
            Multiply = 2,
            /// <summary>
            /// Division operator.
            /// Arguments 2
            /// </summary>
            Divide = 3,
            /// <summary>
            /// Pow/^ function.
            /// Arguments 2
            /// </summary>
            Pow = 4,
            /// <summary>
            /// Square function.
            /// Arguments 1
            /// </summary>
            Square = 5,
            /// <summary>
            /// Natural logarithm function.
            /// Arguments 1
            /// </summary>
            Ln = 6,
            /// <summary>
            /// Exponent function.
            /// Arguments 1
            /// </summary>
            Exp = 7,
            /// <summary>
            /// Square root function.
            /// Arguments 1
            /// </summary>
            Sqrt = 8
            /// <summary>
            /// Random numbers function.
            /// Arguments 0
            /// </summary>
            ///Erc = 9
        }

        /// <summary>
        /// Number of different functions supported by the class.
        /// </summary>
        protected const int FunctionsCount = 9; //10

        // gene type
        private GPGeneType	type;
        // total amount of variables in the task which is supposed to be solved
        private int			variablesCount;
        //
        private double		val;
        // arguments count
        private int			argumentsCount = 0;

        /// <summary>
        /// Random number generator for chromosoms generation.
        /// </summary>
        protected static ThreadSafeRandom rand = new ThreadSafeRandom( );

        // create instance of random generator
        protected static IRandomNumberGenerator generator = new UniformGenerator(new Range(0, 1));

        /// <summary>
        /// Gene type.
        /// </summary>
        /// 
        /// <remarks><para>The property represents type of a gene - function, argument, etc.</para>
        /// </remarks>
        /// 
        public GPGeneType GeneType
        {
            get { return type; }
        }

        /// <summary>
        /// Arguments count.
        /// </summary>
        /// 
        /// <remarks><para>Arguments count of a particular function gene.</para></remarks>
        /// 
        public int ArgumentsCount
        {
            get { return argumentsCount; }
        }

        /// <summary>
        /// Maximum arguments count.
        /// </summary>
        /// 
        /// <remarks><para>Maximum arguments count of a function gene supported by the class.
        /// The property may be used by chromosomes' classes to allocate correctly memory for
        /// functions' arguments, for example.</para></remarks>
        /// 
        public int MaxArgumentsCount
        {
            get { return 5; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGeneFunction"/> class.
        /// </summary>
        /// 
        /// <param name="variablesCount">Total amount of variables in the task which is supposed
        /// to be solved.</param>
        /// 
        /// <remarks><para>The constructor creates randomly initialized gene with random type
        /// and value by calling <see cref="Generate( )"/> method.</para></remarks>
        /// 
        public ExpressionGeneFunction( int variablesCount ) : this( variablesCount, true ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGeneFunction"/> class.
        /// </summary>
        /// 
        /// <param name="variablesCount">Total amount of variables in the task which is supposed
        /// to be solved.</param>
        /// <param name="type">Gene type to set.</param>
        /// 
        /// <remarks><para>The constructor creates randomly initialized gene with random
        /// value and preset gene type.</para></remarks>
        /// 
        public ExpressionGeneFunction( int variablesCount, GPGeneType type )
        {
            this.variablesCount = variablesCount;
            // generate the gene value
            Generate( type );
        }

        // Private constructor
        private ExpressionGeneFunction( int variablesCount, bool random )
        {
            this.variablesCount = variablesCount;
            // generate the gene value
            if ( random )
                Generate( );
        }

        /// <summary>
        /// Get string representation of the gene.
        /// </summary>
        /// 
        /// <returns>Returns string representation of the gene.</returns>
        ///
        public override string ToString( )
        {
            if ( type == GPGeneType.Function )
            {
                // get function string representation
                switch ( (Functions) val )
                {
                    case Functions.Add:			// addition
                        return "+";

                    case Functions.Subtract:	// subtraction
                        return "-";

                    case Functions.Multiply:	// multiplication
                        return "*";

                    case Functions.Divide:		// division
                        return "/";

                    case Functions.Pow:			// pow
                        return "^";

                    case Functions.Square:		// square
                        return "square";

                    case Functions.Ln:			// natural logarithm
                        return "ln";

                    case Functions.Exp:			// exponent
                        return "exp";

                    case Functions.Sqrt:		// square root
                        return "sqrt";

                    //case Functions.Erc:		    // constant random numbers
                    //    float randomNumber = generator.Next();
                    //    return randomNumber.ToString();
                    //    //return string.Format("C{0}", randomNumber);
                }
            } 
            else if ( type == GPGeneType.Constant )
            {
                return string.Format("{0}", val);
            }

            // Alexander - Mudado gerador de constantes para ser um argumento ou terminal para evitar erro 
            //if (rand.Next(4) >= 2)
            //{
            //    // ERC - constant random numbers
            //    float randomNumber = generator.Next();
            //    return randomNumber.ToString();
            //    //return string.Format("C{0}", randomNumber);
            //}

            // get argument string representation
            //return string.Format( "${0}", val );
            return string.Format("X");
 
        }

        /// <summary>
        /// Clone the gene.
        /// </summary>
        /// 
        /// <remarks><para>The method clones the chromosome returning the exact copy of it.</para></remarks>
        /// 
        public IGPGene Clone( )
        {
            // create new gene ...
            ExpressionGeneFunction clone = new ExpressionGeneFunction( variablesCount, false );
            // ... with the same type and value
            clone.type = type;
            clone.val  = val;
            clone.argumentsCount = argumentsCount;

            return clone;
        }

        /// <summary>
        /// Randomize gene with random type and value.
        /// </summary>
        /// 
        /// <remarks><para>The method randomizes the gene, setting its type and value randomly.</para></remarks>
        /// 
        public void Generate( )
        {
            // give more chance to function and argument
            Generate((rand.Next(4) == 3) ? ((rand.Next(4) == 3) ? GPGeneType.Constant : GPGeneType.Argument) : GPGeneType.Function);
        }

        /// <summary>
        /// Randomize gene with random value.
        /// </summary>
        /// 
        /// <param name="type">Gene type to set.</param>
        /// 
        /// <remarks><para>The method randomizes a gene, setting its value randomly, but type
        /// is set to the specified one.</para></remarks>
        ///
        public void Generate( GPGeneType type )
        {
            // gene type
            this.type = type;
            // gene value
            if (type == GPGeneType.Constant)
            {
                // se for constante ERC gerar valor aleatorio como argumento
                val = generator.Next();
            }
            else 
            {
              val = rand.Next( ( type == GPGeneType.Function ) ? FunctionsCount : variablesCount );
            }
            // arguments count
            argumentsCount = ((type == GPGeneType.Argument) || (type == GPGeneType.Constant)) ? 0 : (val <= (int)Functions.Pow) ? 2 : 1;
        }
     
        /// <summary>
        /// Creates new gene with random type and value.
        /// </summary>
        /// 
        /// <remarks><para>The method creates new randomly initialized gene .
        /// The method is useful as factory method for those classes, which work with gene's interface,
        /// but not with particular gene class.</para>
        /// </remarks>
        ///
        public IGPGene CreateNew( )
        {
            return new ExpressionGeneFunction(variablesCount);
        }

        /// <summary>
        /// Creates new gene with certain type and random value.
        /// </summary>
        /// 
        /// <param name="type">Gene type to create.</param>
        /// 
        /// <remarks><para>The method creates new gene with specified type, but random value.
        /// The method is useful as factory method for those classes, which work with gene's interface,
        /// but not with particular gene class.</para>
        /// </remarks>
        ///
        public IGPGene CreateNew( GPGeneType type )
        {
            return new ExpressionGeneFunction(variablesCount, type);
        }
    }
}
