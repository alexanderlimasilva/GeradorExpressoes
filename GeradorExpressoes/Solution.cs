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
            MMREFitness fitness = new MMREFitness(data);

            // create gene function
            IGPGene gene = (functionsSet == 0) ? (IGPGene)new SimpleGeneFunction(data.GetLength(0)) : (IGPGene)new ExpressionGeneFunction(data.GetLength(0));

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
            double[,] solution = new double[data.GetLength(0), 2];
            //double[] input = new double[data.GetLength(0)];
            double[] input = new double[1];

            
            // Alexander - rever esta funcao
            // calculate X values to be used with solution function
            //for (int j = 0; j < data.GetLength(0); j++)
            //{
            //    //solution[j, 0] = data[i, 1];
            //    variables[0] = data[i, 0];
            //}

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
                        input[j] = data[j, 1];
                        solution[j, 0] = PolishGerExpression.Evaluate(bestFunction, input, j);
                    }

                    // calculate error
                    double error = 0.0;
                    for (int j = 0, k = data.GetLength(0); j < k; j++)
                    {
                        input[j] = data[j, 1];
                        error += (Math.Abs(PolishGerExpression.Evaluate(bestFunction, input, j) - data[j, 0]) / (data[j, 0]));
                    }

                    error = error / data.GetLength(0);

                }
                catch
                {
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
            string expressao = population.BestChromosome.ToString().Trim();
            string expressaosubst = PolishGerExpression.SubstituteVariables(expressao, input);
            string expressaosimpl = PolishGerExpression.SimplifyExpression(expressaosubst);

            string resultado = "";
            resultado = "Expressão NPR gerada: " + expressao + "\r\n";
            resultado = resultado + "Expressão formatada: " + (RPN2Infix.PostfixToInfix(expressao)) + "\r\n";
            resultado = resultado + "Expressão NPR com substitução: " + expressaosubst + "\r\n";
            resultado = resultado + "Expressão com substitução formatada: " + RPN2Infix.PostfixToInfix(expressaosubst) + "\r\n";
            resultado = resultado + "Expressão NPR com substitução e simplificada: " + expressaosimpl + "\r\n";
            resultado = resultado + "Expressão com substitução e simplificada formatada: " + RPN2Infix.PostfixToInfix(expressaosimpl) + "\r\n";

            //saida.escreveArquivo(population.BestChromosome.ToString().Trim(), dataset);
            //System.Console.WriteLine(RPN2Infix.Parse(population.BestChromosome.ToString().Replace("$","")));
            //System.Console.WriteLine(RPN2Infix.PostfixToInfix(population.BestChromosome.ToString().Replace("$", "").Trim()));
            //saida.escreveArquivo(RPN2Infix.PostfixToInfix(population.BestChromosome.ToString().Replace("$", "")), "Expressao");

            WriteData saida = new WriteData();
            saida.escreveArquivo(resultado, dataset);

            System.Console.WriteLine("Fim do Programa");
        }

    }
}
