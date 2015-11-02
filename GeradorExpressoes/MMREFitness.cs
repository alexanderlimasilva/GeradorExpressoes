using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeradorExpressoes
{
    using System;
    using AForge;
    using AForge.Genetic;

    /// <summary>
    /// Fitness function for symbolic regression problem
    /// </summary>
    /// 
    /// <remarks><para>The fitness function calculates fitness value of
    /// <see cref="GPTreeChromosome">GP</see> and <see cref="GEPChromosome">GEP</see>
    /// chromosomes with the aim of solving symbolic regression problem. The fitness function's
    /// value is computed as:
    /// <code> ((estimated value - atual value) / atual value ) / number of elements</code>
    /// where <b>error</b> equals to the avarage of magnitude of relative error between function values (computed using
    /// the function encoded by chromosome) and input values (function to be approximated).</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    ///	// constants
    ///	double[] constants = new double[5] { 1, 2, 3, 5, 7 };
    ///	// function to be approximated
    ///	double[,] data = dada from dataset;
    ///	// create population
    ///	Population population = new Population( 100,
    ///		new GPTreeChromosome( new SimpleGeneFunction( 1 + constants.Length ) ),
    ///		new SymbolicRegressionFitness( data, constants ),
    ///		new EliteSelection( ) );
    ///	// run one epoch of the population
    ///	population.RunEpoch( );
    /// </code>
    /// </remarks>
    /// 

    public class MMREFitness : IFitnessFunction
    {
        // regression data
        private double[,]	data;
        // varibles
        private double[]	variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolicRegressionFitness"/> class.
        /// </summary>
        /// 
        /// <param name="data">Function to be approximated.</param>
        /// <param name="constants">Array of constants to be used as additional
        /// paramters for genetic expression.</param>
        /// 
        /// <remarks><para>The <paramref name="data"/> parameter defines the function to be approximated and
        /// represents a two dimensional array of (x, y) points.</para>
        /// 
        /// <para>The <paramref name="constants"/> parameter is an array of constants, which can be used as
        /// additional variables for a genetic expression. The actual amount of variables for
        /// genetic expression equals to the amount of constants plus one - the <b>x</b> variable.</para>
        /// </remarks>
        /// 
        public MMREFitness( double[,] data, double[] constants )
        {
            this.data = data;
            // copy constants
            variables = new double[constants.Length + 1];
            Array.Copy( constants, 0, variables, 1, constants.Length );
        }

        /// <summary>
        /// Evaluates chromosome.
        /// </summary>
        /// 
        /// <param name="chromosome">Chromosome to evaluate.</param>
        /// 
        /// <returns>Returns chromosome's fitness value.</returns>
        ///
        /// <remarks>The method calculates fitness value of the specified
        /// chromosome.</remarks>
        ///
        public double Evaluate( IChromosome chromosome )
        {           
            // get function in polish notation
            string function = chromosome.ToString( );

            // go through all the data
            double error = 0.0;
            for ( int i = 0, n = data.GetLength( 0 ); i < n; i++ )
            {
                // put next X value to variables list
                variables[0] = data[i, 0];
                // avoid evaluation errors
                try
                {
                    // evalue the function
                    double y = PolishGerExpression.Evaluate( function, variables );
                    // check for correct numeric value
                    if ( double.IsNaN( y ) )
                        return 0;
                    // get the difference between evaluated Y and real Y
                    // and sum relative error
                    error += Math.Abs( y - data[i, 1]) / (data[i, 1]) ;
                    //System.Console.WriteLine(Math.Abs(y - data[i, 1]) / (data[i, 1]));
                }
                catch
                {
                    return 0;
                }
            }

            // return function average value 
            System.Console.WriteLine(error / data.GetLength(0));
            return (error / data.GetLength ( 0 ) );
        }

        /// <summary>
        /// Translates genotype to phenotype .
        /// </summary>
        /// 
        /// <param name="chromosome">Chromosome, which genoteype should be
        /// translated to phenotype.</param>
        ///
        /// <returns>Returns chromosome's fenotype - the actual solution
        /// encoded by the chromosome.</returns> 
        /// 
        /// <remarks>The method returns string value, which represents approximation
        /// expression written in polish postfix notation.</remarks>
        ///
        public string Translate( IChromosome chromosome )
        {
            // return polish notation for now ...
            return chromosome.ToString( );
        }
    }

}
