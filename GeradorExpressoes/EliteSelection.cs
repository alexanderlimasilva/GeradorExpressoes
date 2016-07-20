// AForge Genetic Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2006-2009
// andrew.kirillov@aforgenet.com
//

namespace GeradorExpressoes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using AForge;
    using AForge.Genetic;

    /// <summary>
    /// Elite selection method.
    /// </summary>
    /// 
    /// <remarks>Elite selection method selects specified amount of
    /// best chromosomes to the next generation.</remarks> 
    /// 
    public class EliteSelection : ISelectionMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EliteSelection"/> class.
        /// </summary>
        public EliteSelection( ) { }

        /// <summary>
        /// Apply selection to the specified population.
        /// </summary>
        /// 
        /// <param name="chromosomes">Population, which should be filtered.</param>
        /// <param name="size">The amount of chromosomes to keep.</param>
        /// 
        /// <remarks>Filters specified population keeping only specified amount of best
        /// chromosomes.</remarks>
        /// 
        public void ApplySelection( List<IChromosome> chromosomes, int size )
        {
            // sort chromosomes
            //chromosomes.Sort( );

            // current population size
            int elitesize = chromosomes.Count;

            // Sorting by fitness
            for (int i = elitesize - 1; i >= 1; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (chromosomes[j].Fitness > chromosomes[j + 1].Fitness)
                    {
                        // faz a ordenacao
                        IChromosome aux = chromosomes[j];
                        chromosomes[j] = chromosomes[j + 1];
                        chromosomes[j + 1] = aux;
                    }
                }
            }

            // remove bad chromosomes
            chromosomes.RemoveRange( size, chromosomes.Count - size );
        }
    }
}
