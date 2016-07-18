// AForge Genetic Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2006-2011
// contacts@aforgenet.com
//

namespace GeradorExpressoes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using AForge;
    using AForge.Genetic;
    using GeradorExpressoes;

    /// <summary>
    /// Population of chromosomes.
    /// </summary>
    /// 
    /// <remarks><para>The class represents population - collection of individuals (chromosomes)
    /// and provides functionality for common population's life cycle - population growing
    /// with help of genetic operators and selection of chromosomes to new generation
    /// with help of selection algorithm. The class may work with any type of chromosomes
    /// implementing <see cref="IChromosome"/> interface, use any type of fitness functions
    /// implementing <see cref="IFitnessFunction"/> interface and use any type of selection
    /// algorithms implementing <see cref="ISelectionMethod"/> interface.</para>
    /// </remarks>
    /// 
    public class Population
    {
        private IFitnessFunction fitnessFunction;
        private ISelectionMethod selectionMethod;
        private List<IChromosome> population = new List<IChromosome>( );
        private int			size;
        private int         tournamentSize = 5;
        private double		randomSelectionPortion = 0.0;
        private bool        autoShuffling = false;

        // population parameters
        private double		crossoverRate	= 0.75;
        private double		mutationRate	= 0.50;

        // random number generator
        private static ThreadSafeRandom rand = new ThreadSafeRandom( );

        //
        private double      maxNodes   = 0;
        private double		fitnessMax = 0;
        private double		fitnessSum = 0;
        private double		fitnessAvg = 0;
        private IChromosome	bestChromosome = null;
        private int         bestGeneration = 0;
        private int         actualGeneration = 0;

        /// <summary>
        /// Crossover rate, [0.1, 1].
        /// </summary>
        /// 
        /// <remarks><para>The value determines the amount of chromosomes which participate
        /// in crossover.</para>
        /// 
        /// <para>Default value is set to <b>0.75</b>.</para>
        /// </remarks>
        /// 
        public double CrossoverRate
        {
            get { return crossoverRate; }
            set
            {
                crossoverRate = Math.Max( 0.1, Math.Min( 1.0, value ) );
            }
        }

        /// <summary>
        /// Mutation rate, [0.1, 1].
        /// </summary>
        /// 
        /// <remarks><para>The value determines the amount of chromosomes which participate
        /// in mutation.</para>
        /// 
        /// <para>Defaul value is set to <b>0.1</b>.</para></remarks>
        /// 
        public double MutationRate
        {
            get { return mutationRate; }
            set
            {
                mutationRate = Math.Max( 0.1, Math.Min( 1.0, value ) );
            }
        }

        /// <summary>
        /// Random selection portion, [0, 0.9].
        /// </summary>
        /// 
        /// <remarks><para>The value determines the amount of chromosomes which will be
        /// randomly generated for the new population. The property controls the amount
        /// of chromosomes, which are selected to a new population using
        /// <see cref="SelectionMethod">selection operator</see>, and amount of random
        /// chromosomes added to the new population.</para>
        /// 
        /// <para>Default value is set to <b>0</b>.</para></remarks>
        /// 
        public double RandomSelectionPortion
        {
            get { return randomSelectionPortion; }
            set
            {
                randomSelectionPortion = Math.Max( 0, Math.Min( 0.9, value ) );
            }
        }

        /// <summary>
        /// Determines of auto shuffling is on or off.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies if automatic shuffling needs to be done
        /// on each <see cref="RunEpoch">epoch</see> by calling <see cref="Shuffle"/>
        /// method.</para>
        /// 
        /// <para>Default value is set to <see langword="false"/>.</para></remarks>
        /// 
        public bool AutoShuffling
        {
            get { return autoShuffling; }
            set { autoShuffling = value; }
        }

        /// <summary>
        /// Selection method to use with the population.
        /// </summary>
        /// 
        /// <remarks><para>The property sets selection method which is used to select
        /// population members for a new population - filter population after reproduction
        /// was done with operators like crossover and mutations.</para></remarks>
        /// 
        public ISelectionMethod SelectionMethod
        {
            get { return selectionMethod; }
            set { selectionMethod = value; }
        }

        /// <summary>
        /// Fitness function to apply to the population.
        /// </summary>
        /// 
        /// <remarks><para>The property sets fitness function, which is used to evaluate
        /// usefulness of population's chromosomes. Setting new fitness function causes recalculation
        /// of fitness values for all population's members and new best member will be found.</para>
        /// </remarks>
        /// 
        public IFitnessFunction FitnessFunction
        {
            get { return fitnessFunction; }
            set
            {
                fitnessFunction = value;

                foreach ( IChromosome member in population )
                {
                    member.Evaluate( fitnessFunction );
                }

                FindBestChromosome( );
            }
        }

        /// <summary>
        /// Maximum fitness of the population.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps maximum fitness of chromosomes currently existing
        /// in the population.</para>
        /// 
        /// <para><note>The property is recalculate only after <see cref="Selection">selection</see>
        /// or <see cref="Migrate">migration</see> was done.</note></para>
        /// </remarks>
        /// 
        public double FitnessMax
        {
            get { return fitnessMax; }
        }

        /// <summary>
        /// Summary fitness of the population.
        /// </summary>
        ///
        /// <remarks><para>The property keeps summary fitness of all chromosome existing in the
        /// population.</para>
        /// 
        /// <para><note>The property is recalculate only after <see cref="Selection">selection</see>
        /// or <see cref="Migrate">migration</see> was done.</note></para>
        /// </remarks>
        ///
        public double FitnessSum
        {
            get { return fitnessSum; }
        }

        /// <summary>
        /// Average fitness of the population.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps average fitness of all chromosome existing in the
        /// population.</para>
        /// 
        /// <para><note>The property is recalculate only after <see cref="Selection">selection</see>
        /// or <see cref="Migrate">migration</see> was done.</note></para>
        /// </remarks>
        ///
        public double FitnessAvg
        {
            get { return fitnessAvg; }
        }

        /// <summary>
        /// Best chromosome of the population.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps the best chromosome existing in the population
        /// or <see langword="null"/> if all chromosomes have 0 fitness.</para>
        /// 
        /// <para><note>The property is recalculate only after <see cref="Selection">selection</see>
        /// or <see cref="Migrate">migration</see> was done.</note></para>
        /// </remarks>
        /// 
        public IChromosome BestChromosome
        {
            get { return bestChromosome; }
        }

        /// <summary>
        /// Size of the population.
        /// </summary>
        /// 
        /// <remarks>The property keeps initial (minimal) size of population.
        /// Population always returns to this size after selection operator was applied,
        /// which happens after <see cref="Selection"/> or <see cref="RunEpoch"/> methods
        /// call.</remarks>
        /// 
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Generation of the population.
        /// </summary>
        /// 
        /// <remarks>The property keeps the generation of the best chromosome.</remarks>
        /// 
        public int BestGeneration
        {
            get { return bestGeneration; }
        }

        /// <summary>
        /// Generation of the population.
        /// </summary>
        /// 
        /// <remarks>The property keeps the generation of the best chromosome.</remarks>
        /// 
        public int ActualGeneration
        {
            get { return actualGeneration; }
            set
            {
                actualGeneration = value;
            }
        }

        /// <summary>
        /// Get chromosome with specified index.
        /// </summary>
        /// 
        /// <param name="index">Chromosome's index to retrieve.</param>
        /// 
        /// <remarks>Allows to access individuals of the population.</remarks>
        /// 
        public IChromosome this[int index]
        {
            get { return population[index]; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Population"/> class.
        /// </summary>
        /// 
        /// <param name="size">Initial size of population.</param>
        /// <param name="ancestor">Ancestor chromosome to use for population creatioin.</param>
        /// <param name="fitnessFunction">Fitness function to use for calculating
        /// chromosome's fitness values.</param>
        /// <param name="selectionMethod">Selection algorithm to use for selection
        /// chromosome's to new generation.</param>
        /// 
        /// <remarks>Creates new population of specified size. The specified ancestor
        /// becomes first member of the population and is used to create other members
        /// with same parameters, which were used for ancestor's creation.</remarks>
        /// 
        /// <exception cref="ArgumentException">Too small population's size was specified. The
        /// exception is thrown in the case if <paramref name="size"/> is smaller than 2.</exception>
        ///
        public Population( int size,
                           IChromosome ancestor,
                           IFitnessFunction fitnessFunction,
                           ISelectionMethod selectionMethod )
        {
            if ( size < 2 )
                throw new ArgumentException( "Too small population's size was specified." );

            this.fitnessFunction = fitnessFunction;
            this.selectionMethod = selectionMethod;
            this.size = size;

            // add ancestor to the population
            ancestor.Evaluate( fitnessFunction );
            population.Add( ancestor.Clone( ) );
            // add more chromosomes to the population
            for ( int i = 1; i < size; i++ )
            {
                // create new chromosome
                IChromosome c = ancestor.CreateNew( );
                // calculate it's fitness
                c.Evaluate( fitnessFunction );
                // add it to population
                population.Add( c );
            }
        }

        /// <summary>
        /// Regenerate population.
        /// </summary>
        /// 
        /// <remarks>The method regenerates population filling it with random chromosomes.</remarks>
        /// 
        public void Regenerate( )
        {
            IChromosome ancestor = population[0];

            // clear population
            population.Clear( );
            // add chromosomes to the population
            for ( int i = 0; i < size; i++ )
            {
                // create new chromosome
                IChromosome c = ancestor.CreateNew( );
                // calculate it's fitness
                c.Evaluate( fitnessFunction );
                // add it to population
                population.Add( c );
            }
        }

        /// <summary>
        /// Do crossover in the population.
        /// </summary>
        /// 
        /// <remarks>The method walks through the population and performs crossover operator
        /// taking each two chromosomes in the order of their presence in the population.
        /// The total amount of paired chromosomes is determined by
        /// <see cref="CrossoverRate">crossover rate</see>.</remarks>
        /// 
        public virtual void Crossover( )
        {
            // new population, initially empty
            List<IChromosome> newPopulation = new List<IChromosome>();
            
            // crossover
            for ( int i = 1; i < size; i += 2 )
            {
                // generate next random number and check if we need to do crossover
                //if (rand.NextDouble() <= crossoverRate)
                //{
                    // faz o torneio para escolher os ancestrais
                    IChromosome c1 = tournamentSelection(population); //population[i - 1].Clone();
                    IChromosome c2 = tournamentSelection(population); //population[i].Clone();

                    // do crossover
                    c1.Crossover(c2);

                    // calculate fitness of these two offsprings
                    c1.Evaluate(fitnessFunction);
                    c2.Evaluate(fitnessFunction);

                    // add two new offsprings to the population
                    //population.Add( c1 );
                    //population.Add( c2 );

                    // add two new offsprings to the new population
                    newPopulation.Add(c1);
                    newPopulation.Add(c2);
                //}
                //else
                //{
                //    // clone both ancestors
                //    newPopulation.Add(population[i - 1].Clone());
                //    newPopulation.Add(population[i].Clone());
                //}
            }

            // empty current population
            population.Clear();

            // move elements from new to current population
            population.AddRange(newPopulation);
        }

        /// <summary>
        /// Do mutation in the population.
        /// </summary>
        /// 
        /// <remarks>The method walks through the population and performs mutation operator
        /// taking each chromosome one by one. The total amount of mutated chromosomes is
        /// determined by <see cref="MutationRate">mutation rate</see>.</remarks>
        /// 
        public virtual void Mutate( )
        {
            // new population, initially empty
            List<IChromosome> newPopulation = new List<IChromosome>();
            
            // mutate
            for ( int i = 0; i < size; i++ )
            {
                // generate next random number and check if we need to do mutation
                if ( rand.NextDouble( ) <= mutationRate )
                {
                    // clone the chromosome
                    IChromosome c = population[i].Clone( );
                    // mutate it
                    c.Mutate( );
                    // calculate fitness of the mutant
                    c.Evaluate( fitnessFunction );
                    // add mutant to the population
                    population.Add( c );

                    // add mutant to the new population
                    //newPopulation.Add(c);
                }
                //else
                //{
                //    // clone both ancestors
                //    newPopulation.Add(population[i].Clone());
                //}
            }

            //// empty current population
            //population.Clear();

            //// move elements from new to current population
            //population.AddRange(newPopulation);
        }

        /// <summary>
        /// Do selection.
        /// </summary>
        /// 
        /// <remarks>The method applies selection operator to the current population. Using
        /// specified selection algorithm it selects members to the new generation from current
        /// generates and adds certain amount of random members, if is required
        /// (see <see cref="RandomSelectionPortion"/>).</remarks>
        /// 
        public virtual void Selection( )
        {
            // amount of random chromosomes in the new population
            int randomAmount = (int) ( randomSelectionPortion * size );

            // do selection
            selectionMethod.ApplySelection( population, size - randomAmount );

            // add random chromosomes
            if ( randomAmount > 0 )
            {
                IChromosome ancestor = population[0];

                for ( int i = 0; i < randomAmount; i++ )
                {
                    // create new chromosome
                    IChromosome c = ancestor.CreateNew( );
                    // calculate it's fitness
                    c.Evaluate( fitnessFunction );
                    // add it to population
                    population.Add( c );
                }
            }

            FindBestChromosome( );
        }

        /// <summary>
        /// Run one epoch of the population.
        /// </summary>
        /// 
        /// <remarks>The method runs one epoch of the population, doing crossover, mutation
        /// and selection by calling <see cref="Crossover"/>, <see cref="Mutate"/> and
        /// <see cref="Selection"/>.</remarks>
        /// 
        public void RunEpoch( )
        {
           //SortbyFitness();
            Crossover( );
            Mutate( );
            //Selection( );

            if ( autoShuffling )
                Shuffle( );

        }

        /// <summary>
        /// create instance of random generator
        /// </summary>
        protected static IRandomNumberGenerator generator = new UniformGenerator(new Range(0, 1));

        /// <summary>
        /// Novo metodo para escolher um cromossomo para o crossover por torneio
        /// </summary>
        /// <param name="chromosomes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public IChromosome tournamentSelection(List<IChromosome> chromosomes)
        {
            // new population, initially empty
            List<IChromosome> newTempPopulation = new List<IChromosome>();
            
            // size of current population
            int currentSize = chromosomes.Count;

            // Iniciando o cromossomo
            IChromosome cromo = BestChromosome;

            // calculate summary fitness of current population
            double fitnessSum = 0;
            foreach (IChromosome c in chromosomes)
            {
                fitnessSum += c.Fitness;
            }

            //// create wheel ranges
            //double[] rangeMax = new double[currentSize];
            //double s = 0;
            //int k = currentSize - 1; // 0;

            //SortbyFitness();

            //foreach (IChromosome c in chromosomes)
            //{
            //    // cumulative normalized fitness
            //    s += (c.Fitness / fitnessSum);
            //    rangeMax[k--] = s;
            //}

            //int randomID = (int)Math.Round(generator.Next() * currentSize);
            //IChromosome c = chromosomes[randomID].Clone( );

            // select chromosomes from old population to the new temp population
            for (int j = 0; j < tournamentSize; j++)
            {
                //// get wheel value
                //double wheelValue = rand.NextDouble();

                //// find the chromosome for the wheel value
                //for (int i = 0; i < currentSize; i++)
                //{
                //    if (wheelValue >= rangeMax[i])
                //    {
                //        // add the chromosome to the new population
                //        //cromo = ((IChromosome)chromosomes[i]).Clone();
                //        newTempPopulation.Add(((IChromosome)chromosomes[i]).Clone());
                //        break;
                //    }
                //}

                int randomID = (int)Math.Round(generator.Next() * currentSize);

                if (randomID < 0) { randomID = 0; }
                if (randomID >= currentSize) { randomID = currentSize - 1; }

                newTempPopulation.Add(chromosomes[randomID].Clone());
            }

            //// Sorting
            //for (int i = tournamentSize - 1; i >= 1; i--)
            //{
            //    for (int j = 0; j < i; j++)
            //    {
            //        if (newTempPopulation[j].Fitness > newTempPopulation[j + 1].Fitness)
            //        {
            //            // faz a ordenacao
            //            IChromosome aux = newTempPopulation[j];
            //            newTempPopulation[j] = newTempPopulation[j + 1];
            //            newTempPopulation[j + 1] = aux;
            //        }
            //    }
            //}

            //cromo = newTempPopulation[0];

            // Find best chromosome in the temp population
            cromo = newTempPopulation[0];
            double fitMax = newTempPopulation[0].Fitness;

            for (int i = 1; i < tournamentSize; i++)
            {
                double fitness = newTempPopulation[i].Fitness;

                // check for min
                if (fitness < fitMax)
                {
                    fitMax = fitness;
                    cromo = newTempPopulation[i];
                }
            }

            return cromo;
        }

        /// <summary>
        /// Shuffle randomly current population.
        /// </summary>
        /// 
        /// <remarks><para>Population shuffling may be useful in cases when selection
        /// operator results in not random order of chromosomes (for example, after elite
        /// selection population may be ordered in ascending/descending order).</para></remarks>
        /// 
        public void Shuffle( )
        {
            // current population size
            int size = population.Count;
            // create temporary copy of the population
            List<IChromosome> tempPopulation = population.GetRange( 0, size );
            // clear current population and refill it randomly
            population.Clear( );

            while ( size > 0 )
            {
                int i = rand.Next( size );

                population.Add( tempPopulation[i] );
                tempPopulation.RemoveAt( i );

                size--;
            }
        }

        /// <summary>
        /// Sort the current population in descending order by his fitness value.
        /// </summary>
        /// 
        /// <remarks><para>Population may be ordered in descending order by his fitness value.</para></remarks>
        /// 
        public void SortbyFitness()
        {
            // current population size
            int size = population.Count;
            // Sorting
            for (int i= size - 1; i >= 1; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (population[j].Fitness > population[j+1].Fitness)
                    {
                        // faz a ordenacao
                        IChromosome aux = population[j];
                        population[j] = population[j + 1];
                        population[j + 1] = aux;
                    }
                }
            }
        }

        /// <summary>
        /// Add chromosome to the population.
        /// </summary>
        /// 
        /// <param name="chromosome">Chromosome to add to the population.</param>
        /// 
        /// <remarks><para>The method adds specified chromosome to the current population.
        /// Manual adding of chromosome maybe useful, when it is required to add some initialized
        /// chromosomes instead of random.</para>
        /// 
        /// <para><note>Adding chromosome manually should be done very carefully, since it
        /// may break the population. The manually added chromosome must have the same type
        /// and initialization parameters as the ancestor passed to constructor.</note></para>
        /// </remarks>
        /// 
        public void AddChromosome( IChromosome chromosome )
        {
            chromosome.Evaluate( fitnessFunction );
            population.Add( chromosome );
        }

        /// <summary>
        /// Perform migration between two populations.
        /// </summary>
        /// 
        /// <param name="anotherPopulation">Population to do migration with.</param>
        /// <param name="numberOfMigrants">Number of chromosomes from each population to migrate.</param>
        /// <param name="migrantsSelector">Selection algorithm used to select chromosomes to migrate.</param>
        /// 
        /// <remarks><para>The method performs migration between two populations - current and the
        /// <paramref name="anotherPopulation">specified one</paramref>. During migration
        /// <paramref name="numberOfMigrants">specified number</paramref> of chromosomes is choosen from
        /// each population using <paramref name="migrantsSelector">specified selection algorithms</paramref>
        /// and put into another population replacing worst members there.</para></remarks>
        /// 
        public void Migrate( Population anotherPopulation, int numberOfMigrants, ISelectionMethod migrantsSelector )
        {
            int currentSize = this.size;
            int anotherSize = anotherPopulation.Size;

            // create copy of current population
            List<IChromosome> currentCopy = new List<IChromosome>( );

            for ( int i = 0; i < currentSize; i++ )
            {
                currentCopy.Add( population[i].Clone( ) );
            }

            // create copy of another population
            List<IChromosome> anotherCopy = new List<IChromosome>( );

            for ( int i = 0; i < anotherSize; i++ )
            {
                anotherCopy.Add( anotherPopulation.population[i].Clone( ) );
            }

            // apply selection to both populations' copies - select members to migrate
            migrantsSelector.ApplySelection( currentCopy, numberOfMigrants );
            migrantsSelector.ApplySelection( anotherCopy, numberOfMigrants );

            // sort original populations, so the best chromosomes are in the beginning
            population.Sort( );
            anotherPopulation.population.Sort( );

            // remove worst chromosomes from both populations to free space for new members
            population.RemoveRange( currentSize - numberOfMigrants, numberOfMigrants );
            anotherPopulation.population.RemoveRange( anotherSize - numberOfMigrants, numberOfMigrants );

            // put migrants to corresponding populations
            population.AddRange( anotherCopy );
            anotherPopulation.population.AddRange( currentCopy );

            // find best chromosomes in each population
            FindBestChromosome( );
            anotherPopulation.FindBestChromosome( );
        }

        /// <summary>
        /// Resize population to the new specified size.
        /// </summary>
        /// 
        /// <param name="newPopulationSize">New size of population.</param>
        /// 
        /// <remarks><para>The method does resizing of population. In the case if population
        /// should grow, it just adds missing number of random members. In the case if
        /// population should get smaller, the <see cref="SelectionMethod">population's
        /// selection method</see> is used to reduce the population.</para></remarks>
        /// 
        /// <exception cref="ArgumentException">Too small population's size was specified. The
        /// exception is thrown in the case if <paramref name="newPopulationSize"/> is smaller than 2.</exception>
        /// 
        public void Resize( int newPopulationSize )
        {
            Resize( newPopulationSize, selectionMethod );
        }

        /// <summary>
        /// Resize population to the new specified size.
        /// </summary>
        /// 
        /// <param name="newPopulationSize">New size of population.</param>
        /// <param name="membersSelector">Selection algorithm to use in the case
        /// if population should get smaller.</param>
        /// 
        /// <remarks><para>The method does resizing of population. In the case if population
        /// should grow, it just adds missing number of random members. In the case if
        /// population should get smaller, the specified selection method is used to
        /// reduce the population.</para></remarks>
        /// 
        /// <exception cref="ArgumentException">Too small population's size was specified. The
        /// exception is thrown in the case if <paramref name="newPopulationSize"/> is smaller than 2.</exception>
        ///
        public void Resize( int newPopulationSize, ISelectionMethod membersSelector )
        {
            if ( newPopulationSize < 2 )
                throw new ArgumentException( "Too small new population's size was specified." );

            if ( newPopulationSize > size )
            {
                // population is growing, so add new rundom members

                // Note: we use population.Count here instead of "size" because
                // population may be bigger already after crossover/mutation. So
                // we just keep those members instead of adding random member.
                int toAdd = newPopulationSize - population.Count;

                for ( int i = 0; i < toAdd; i++ )
                {
                    // create new chromosome
                    IChromosome c = population[0].CreateNew( );
                    // calculate it's fitness
                    c.Evaluate( fitnessFunction );
                    // add it to population
                    population.Add( c );
                }
            }
            else
            {
                // do selection
                membersSelector.ApplySelection( population, newPopulationSize );
            }

            size = newPopulationSize;
        }

        // Find best chromosome in the population so far
        //public void FindBestChromosome()
        //{
        //    bestChromosome = population[0];
        //    fitnessMax = bestChromosome.Fitness;
        //    fitnessSum = fitnessMax;

        //    for (int i = 1; i < size; i++)
        //    {
        //        double fitness = population[i].Fitness;

        //        // accumulate summary value
        //        fitnessSum += fitness;

        //        // check for max
        //        if (fitness < fitnessMax)
        //        {
        //            fitnessMax = fitness;
        //            bestChromosome = population[i];
        //        }
        //    }
        //    fitnessAvg = fitnessSum / size;
        //}

        // Find best chromosome in the population so far
        public void FindBestChromosome()
        {
            string[] tokens;

            // size of current population
            int currentSize = population.Count;

            if (bestChromosome == null)
            {
                bestChromosome = population[0];
                fitnessMax = population[0].Fitness;
                fitnessSum = fitnessMax;

                tokens = population[0].ToString().Trim().Split(' ');
                maxNodes = tokens.Length;
            }

            for (int i = 1; i < currentSize; i++)
            {
                double fitness = population[i].Fitness;

                tokens = population[i].ToString().Trim().Split(' ');
                double totNodes = tokens.Length;

                // accumulate summary value
                fitnessSum += fitness;

                // check for min or min nodes
                if ((fitness < fitnessMax) || ((fitness == fitnessMax) && (totNodes < maxNodes)))
                {
                   fitnessMax = fitness;
                   bestChromosome = population[i];
                   maxNodes = totNodes;
                   bestGeneration = actualGeneration;
                }

            }

            fitnessAvg = fitnessSum / size;
        }

        /// <summary>
        /// Print the population so far
        /// </summary>
        /// 
        /// <remarks><para>Print the population and fitness value</para></remarks>
        /// 
        public String toString( ) {

            IChromosome	c1 = null;
            string populacao = "";

            for (int i = 0; i < size; i++)
            {
                c1 = population[i];
                //System.Console.WriteLine("Chromosome " + (i + 1) + ": " + RPN2Infix.PostfixToInfix(c1.ToString().Trim()) + " Fitness: " + population[i].Fitness.ToString());
                populacao += "Chromosome " + (i + 1) + ": " + "\t" + RPN2Infix.PostfixToInfix(c1.ToString().Trim()) + "\r\n" + "Fitness: " + "\t" + population[i].Fitness.ToString() + "\r\n";
            }

            return populacao;
        }
        
    }
}