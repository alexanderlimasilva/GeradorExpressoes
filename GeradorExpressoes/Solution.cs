using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AForge;
using AForge.Genetic;
using AForge.Controls;

namespace GeradorExpressoes
{   
    public class Solution
    {
        double[,] data = null;

        WriteData saida = new WriteData();

        int populationSize;
        int iterations;
        int selectionMethod;
        int functionsSet;
        int geneticMethod;
        bool needToStop;
        string dataset;

        public Solution(double[,] ndata, int npopulationSize, int niterations, int nselectionMethod, int nfunctionsSet, int ngeneticMethod, Boolean bneedToStop, string instancia)
        {
            data = ndata;
            populationSize = npopulationSize;
            iterations = niterations;
            selectionMethod = nselectionMethod;
            functionsSet = nfunctionsSet;
            geneticMethod = ngeneticMethod;
            needToStop =  bneedToStop;
            dataset = instancia;
        }

        public void SearchSolution()
        {
            // create fitness function
            MMREFitness fitness = new MMREFitness(data, new double[] { 1, 2, 3, 5, 7 });

            // create gene function
            IGPGene gene = (functionsSet == 0) ? (IGPGene)new SimpleGeneFunction(6) : (IGPGene)new ExpressionGeneFunction(9);

            // create population
            Population population = new Population(populationSize, (geneticMethod == 0) ?
                    (IChromosome)new GPTreeChromosome(gene) :
                    (IChromosome)new GEPChromosome(gene, 15),
                fitness,
                (selectionMethod == 0) ? (ISelectionMethod)new EliteSelection() :
                (selectionMethod == 1) ? (ISelectionMethod)new RankSelection() :
                                         (ISelectionMethod)new RouletteWheelSelection()
                );

            // iterations
            int i = 1;

            // solution array
            double[,] solution = new double[50, 2];
            double[] input = new double[6] { 0, 1, 2, 3, 5, 7 };

            // acha menor valor e maior
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            // search for min value
            for (int j = 0; j < 50; j++)
            {
                if (data[i, 0] < minX)
                    minX = (float) data[i, 0];

                // search for max value
                if (data[i, 0] > maxX)
                    maxX = (float) data[i, 0];
            }


            // calculate X values to be used with solution function
            for (int j = 0; j < 50; j++)
            {
                solution[j, 0] = minX + (double) j * (maxX-minX) / 49;
            }

            // loop
            while (!needToStop)
            {
                // run one epoch of genetic algorithm
                population.RunEpoch();

                try
                {
                    // get best solution
                    string bestFunction = population.BestChromosome.ToString();

                    // calculate best function
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        input[0] = solution[j, 0];
                        solution[j, 1] = PolishGerExpression.Evaluate(bestFunction, input);
                    }
                    //chart.UpdateDataSeries("solution", solution);

                    // calculate error
                    double error = 0.0;
                    for (int j = 0, k = data.GetLength(0); j < k; j++)
                    {
                        input[0] = data[j, 0];
                        error += Math.Abs(data[j, 1] - PolishGerExpression.Evaluate(bestFunction, input));
                    }

                }
                catch
                {
                    // remove any solutions from chart in case of any errors
                    //chart.UpdateDataSeries("solution", null);
                    System.Console.WriteLine("Erro");
                }

                // increase current iteration
                i++;

                //
                if ((iterations != 0) && (i > iterations))
                    break;
            }

            // show solution
            //System.Console.WriteLine(population.BestChromosome.ToString());
            saida.escreveArquivo(population.BestChromosome.ToString(), dataset);
            
            // System.Console.WriteLine(RPN2Infix.Parse(population.BestChromosome.ToString().Replace("$","")));
            System.Console.WriteLine(RPN2Infix.PostfixToInfix(population.BestChromosome.ToString().Replace("$", "")));
            //saida.escreveArquivo(RPN2Infix.PostfixToInfix(population.BestChromosome.ToString().Replace("$", "")), "postfix");

            System.Console.WriteLine("Fim do Programa");
        }

    }
}
