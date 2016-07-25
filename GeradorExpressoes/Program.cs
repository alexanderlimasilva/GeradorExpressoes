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

            // parametros para o inicio da execução
            int populationSize = 100;
            int iterations = 300000;
            int selectionMethod = 2;
            int functionsSet = 1;
            int geneticMethod = 0;

            //Thread	workerThread = null;
            bool needToStop = false;

            String dataset = "abran";
            String caminho = @"D:\Projetos C\Instancias\";
            String nomeArq = caminho + dataset + ".dat";

            double[,] data = null;

            // leitura e impressoa dos dados do dataset
            data = (new LoadData()).leituraArquivo(nomeArq);

            for (int i = 0; i < data.GetLength(0); i++)
            {
                System.Console.WriteLine(String.Format("{0:N}", data[i, 0]) + ", " + String.Format("{0:N}", data[i, 1]));
            }

            // Criação do objeto solução
            Solution solution = new Solution(data, populationSize, iterations, selectionMethod, functionsSet, geneticMethod, needToStop, dataset);

            // Criação do objeto do thread. Não inicia o thread.
            //workerThread = new Thread(new ThreadStart(solution.SearchSolution));

            // Inicia o thread.
            //workerThread.Start();

            System.Console.WriteLine("Iniciando a busca ...");

            solution.SearchSolution();
        }

    }
}
