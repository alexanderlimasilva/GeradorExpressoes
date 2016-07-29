using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GeradorExpressoes;
using AForge.Genetic;
using AForge.Controls;

namespace GeradorExpressoes
{   
    public class Solution
    {

        double[,] data = null;
        int populationSize;
        int iterations;
        int executions;
        int selectionMethod;
        int functionsSet;
        int geneticMethod;
        bool needToStop;
        bool useFunction = false;
        string dataset;
        string nomearq;

        double sum = 0.0;
        double error = 0.0;
        double pred = 0.0;
        double bestMMRE = 1.0e15;
        int hits = 0;
        int npred = 0;

        public Solution(double[,] ndata, int npopulationSize, int niterations, int nselectionMethod, int nfunctionsSet, int ngeneticMethod, Boolean bneedToStop, string instancia, int nrepeticoes)
        {
            data = ndata;
            populationSize = npopulationSize;
            iterations = niterations;
            selectionMethod = nselectionMethod;
            functionsSet = nfunctionsSet;
            geneticMethod = ngeneticMethod;
            needToStop =  bneedToStop;
            dataset = instancia;
            executions = nrepeticoes;
        }

        // função a verificar
        public double func(double x)
        { 
            return 2.649261 * x; 
        }

        public void SearchSolution()
        {
            WriteData saida = new WriteData();
            
            // create fitness function
            MMREFitness fitness = new MMREFitness(data);
            //SymbolicRegressionFitness fitness = new SymbolicRegressionFitness(data, new double[] { 1, 2, 3, 5, 7, 9});

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

            // iterations ou numero de gerações a serem utilizadas
            int i = 1;

            // solution array
            double[,] solution = new double[data.GetLength(0), 2];
            double[] input = new double[data.GetLength(0)];
            double[] output = new double[data.GetLength(0)];
            //double[] input = new double[1];

            // Alexander - Define o valor de entrada X com base no dataset
            for (int j = 0; j < data.GetLength(0); j++)
            {
                input[j] = data[j, 1];
            }

            // Alexander - Define se saida sera baseada na função ou nos dados existentes no dataset
            for (int j = 0; j < data.GetLength(0); j++)
            {
                if (useFunction)
                   output[j] = func(input[j]);
                else
                   output[j] = data[j, 0];

                System.Console.WriteLine(String.Format("{0:N}", input[j]) + ", " + String.Format("{0:N}", output[j]));
            }

            // loop
            while (!needToStop)
            {
                System.Console.WriteLine("Geração: " + i.ToString());
                
                hits = 0;

                if (i <= 1) {
                   //Grava a população gerada e seu fitness em cada rodada
                   nomearq = "Populacao" + ((geneticMethod == 0) ? "_GP_" : "_GEP_") + dataset + "_" + executions.ToString() + "_" + "Geração_" + i.ToString() + "_";
                   saida.escreveArquivo("Geração: " + i.ToString() + "\r\n" + population.toString(), nomearq, 1);

                   nomearq = "AnalisedeTempo" + ((geneticMethod == 0) ? "_GP_" : "_GEP_") + dataset + "_" + executions.ToString() + "_" + iterations.ToString();
                   saida.escreveArquivo("DataSet" + "\t" + "Metodo" + "\t" + "Geração" + "\t" + "MMRE" + "\r\n", nomearq, 0);
                }

                // run one epoch of genetic algorithm
                population.ActualGeneration = i;
                //population.RunEpoch();
                //population.FindBestChromosome();
                
                try
                {
                    // get best solution
                    population.FindBestChromosome();

                    string bestFunction = population.BestChromosome.ToString().Trim();
                    System.Console.WriteLine("Função: " + RPN2Infix.PostfixToInfix(bestFunction));

                    npred = 0;
                    sum = 0.0;
                    error = 0.0;
                    pred = 0.00;
                    double result = 0.0;
                    double resultgerado = 0.0;
                    
                    // calculate best function
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        double HIT_LEVEL = 0.01;
                        double PROBABLY_ZERO = 1.11E-15;
                        double BIG_NUMBER = 1.0e15;
                        
                        System.Console.WriteLine("Valor de entrada: " + input[j]);

                        resultgerado = PolishGerExpression.Evaluate(bestFunction, input, j);

                        // fitness (atual - estimado) / atual
                        result = Math.Abs((output[j] - resultgerado) / output[j]);

                        if (!(result < BIG_NUMBER))       // *NOT* (input.x >= BIG_NUMBER)
                            result = BIG_NUMBER;

                        else if (result < PROBABLY_ZERO)  // slightly off
                            result = 0.0;

                        if (result <= HIT_LEVEL) hits++;  // whatever!

                        //Somatorio do erro
                        sum += result;   

                        //Somatoria do pred com 25%
                        if ((resultgerado <= output[j] * 1.25) && (resultgerado >= output[j] * 0.75)) npred++; 
  
                        //impressao dos dados gerados e esperados
                        System.Console.WriteLine(" Valor gerado: " + resultgerado + " resultado esperado: " + data[j, 0] + " erro relativo: " + result);
                    }

                    // calculate error MMRE
                    error = sum / data.GetLength(0);
                    
                    //calculate Pred(25%)
                    pred = (((double) npred) / data.GetLength(0));

                    if (error < bestMMRE)
                    {
                        bestMMRE = error;
                        nomearq = "AnalisedeTempo" + ((geneticMethod == 0) ? "_GP_" : "_GEP_") + dataset + "_" + executions.ToString() + "_" + iterations.ToString();
                        saida.escreveArquivo(dataset + "\t" + ((geneticMethod == 0) ? "GP" : "GEP") + "\t" + i.ToString() + "\t" + bestMMRE.ToString() + "\r\n", nomearq, 0);
                    }

                    System.Console.WriteLine("Erro acumulado: " + sum);
                    System.Console.WriteLine("Erro Médio: " + error);
                    System.Console.WriteLine("Hits: " + hits);
                    System.Console.WriteLine("");

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }

                // increase current iteration/geracao
                i++;

                //
                if ((iterations != 0) && (i > iterations))
                    //break;
                    needToStop = true;
                else
                    population.RunEpoch();
            }

            // show solution
            //System.Console.WriteLine(population.BestChromosome.ToString());
            string expressao = population.BestChromosome.ToString().Trim();
            string expressaosubst = PolishGerExpression.SubstituteVariables(expressao, input);
            string expressaosimpl = PolishGerExpression.SimplifyExpression(expressaosubst);

            string resultado = "";
            resultado = "Expressão NPR gerada: " + expressao + "\r\n";
            resultado = resultado + "Expressão formatada 1: " + (RPN2Infix.PostfixToInfix(expressao)) + "\r\n";
            resultado = resultado + "Expressão formatada 2: " + (RPN2Infix.Parse(expressao.Replace(",", "."))).Replace(".", ",") + "\r\n";
            //resultado = resultado + "Expressão NPR com substitução: " + expressaosubst + "\r\n";
            //resultado = resultado + "Expressão com substitução formatada: " + RPN2Infix.PostfixToInfix(expressaosubst) + "\r\n";
            //resultado = resultado + "Expressão NPR com substitução simplificada: " + expressaosimpl + "\r\n";
            resultado = resultado + "Expressão com substitução simplificada formatada: " + RPN2Infix.PostfixToInfix(expressaosimpl) + "\r\n";
            resultado = resultado + "Resultado para a expressão: " + "\r\n";        // +testeexpressao + "\r\n";
            resultado = resultado + "Erro acumulado: " + sum.ToString() + "\r\n";
            resultado = resultado + "Erro Médio: "  + error.ToString()  + "\r\n";
            resultado = resultado + "Pred(25): " + pred.ToString() + "\r\n";
            resultado = resultado + "Hits: " + hits.ToString() + "\r\n";
            resultado = resultado + "Geração: " + population.BestGeneration + "\r\n";

            nomearq = "Resultado" + ((geneticMethod == 0) ? "_GP_" : "_GEP_") + dataset + "_" + executions.ToString() + "_";
            saida.escreveArquivo(resultado, nomearq, 1);

            nomearq = "Experimento" + ((geneticMethod == 0) ? "_GP_" : "_GEP_");
            saida.escreveArquivo(dataset + "\t" + ((geneticMethod == 0) ? "GP" : "GEP") + "\t" + executions.ToString() + "\t" + error.ToString() + "\t" + pred.ToString() + "\r\n", nomearq, 0);
            
            //System.Console.WriteLine("Fim do Programa");
        }

    }

}
