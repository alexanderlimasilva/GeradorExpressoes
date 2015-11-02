using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Threading;

namespace GeradorExpressoes
{
    class Program
    {

        static void Main(string[] args)
        {
            System.Console.WriteLine("Inicio do Programa");

            int populationSize = 100;
            int iterations = 1000;
            int selectionMethod = 0;
            int functionsSet = 1;
            int geneticMethod = 0;

            Thread	workerThread = null;
            bool needToStop = false;

            String dataset = "abran";
            String caminho = @"D:\Projetos C\Instancias\";
            String nomeArq = caminho + dataset + ".csv";

            double[,] data = null;

            data = (new LoadData()).leituraArquivo(nomeArq);

            for (int i = 0; i < data.GetLength(0); i++)
            {
                System.Console.WriteLine(String.Format("{0:N}", data[i, 0]) + ", " + String.Format("{0:N}", data[i, 1]));
            }

            // Create the thread object. This does not start the thread.
            Solution solution = new Solution(data, populationSize, iterations, selectionMethod, functionsSet, geneticMethod, needToStop, dataset);

            workerThread = new Thread(new ThreadStart(solution.SearchSolution));

            workerThread.Start();
            System.Console.WriteLine("Iniciando a busca ...");
        }

    }
}
