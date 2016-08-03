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
            int execucoes = 3;
            int indDataSet = 3;
            int populationSize = 100;
            int iterations = 30000;
            int selectionMethod = 2;
            int functionsSet = 1;
            int geneticMethod = 0;

            //Thread	workerThread = null;
            bool needToStop = false;

            String dataset = "";
            WriteData saida = new WriteData();

            //String nomearq = "Experimento" + ((geneticMethod == 0) ? "_GP_" : "_GEP_");
            //saida.escreveArquivo("DataSet" + "\t" + "Metodo" + "\t" + "Execucao" + "\t" + "MMRE" + "\t" + "PRED(25)" + "\r\n", nomearq, 0);

            //para executar os experimentos com a GP e GEP conforme parametros acima
            for (int k = 0; k <= geneticMethod; k++) 
            {

                String nomearq = "Experimento" + ((k == 0) ? "_GP_" : "_GEP_");
                saida.escreveArquivo("DataSet" + "\t" + "Metodo" + "\t" + "Execucao" + "\t" + "MMRE" + "\t" + "PRED(25)" + "\r\n", nomearq, 0);

                //para definir o dataset a ser utilizado conforme parametros acima
                for (int j = 1; j <= indDataSet; j++)
                {
                    switch (j)
                    {
                        case 1: dataset = "abran"; break;
                        case 2: dataset = "albrecht"; break;
                        case 3: dataset = "bailey"; break;
                        case 4: dataset = "belady"; break;
                        case 5: dataset = "boehm"; break;
                        case 6: dataset = "heiat"; break;
                        case 7: dataset = "jss"; break;
                        case 8: dataset = "kemerer"; break;
                        case 9: dataset = "miyaza"; break;
                        case 10: dataset = "shepp"; break;
                        case 11: dataset = "desharn"; break;
                        case 12: dataset = "kitchen"; break;
                        default: dataset = "teste"; break;
                    }

                    String caminho = @"D:\Projetos C\Instancias\";
                    String nomeArq = caminho + dataset + ".dat";

                    double[,] data = null;

                    // leitura e impressoa dos dados do dataset
                    data = (new LoadData()).leituraArquivo(nomeArq);

                    for (int i = 0; i < data.GetLength(0); i++)
                    {
                        System.Console.WriteLine(String.Format("{0:N}", data[i, 0]) + ", " + String.Format("{0:N}", data[i, 1]));
                    }

                    // quantidade de execuções a serem realizadas definidas nos parametros acima
                    for (int i = 1; i <= execucoes; i++)
                    {
                        // Criação do objeto solução
                        Solution solution = new Solution(data, populationSize, iterations, selectionMethod, functionsSet, k, needToStop, dataset, i);

                        // Criação do objeto do thread. Não inicia o thread.
                        //workerThread = new Thread(new ThreadStart(solution.SearchSolution));

                        // Inicia o thread.
                        //workerThread.Start();

                        System.Console.WriteLine("Iniciando a execução: " + i.ToString() + "\r\n");

                        solution.SearchSolution();
                    }
                }
            }

            System.Console.WriteLine("Fim do Programa");
        }
    }
}
