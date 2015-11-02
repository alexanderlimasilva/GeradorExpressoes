using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace GeradorExpressoes
{
    class LoadData
    {
		StreamReader reader = null;
        private static int maxData = 100;
        private double[,] data = null;

        public double[,] leituraArquivo(String arquivo)
        {
            float[,] tempData = new float[maxData, 2];
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            try
            {
                // abre o arquivo para leitura
                reader = new StreamReader(arquivo);
                string str = null;
                int i = 0;

                // le o arquivo
                while ((i < maxData) && ((str = reader.ReadLine()) != null))
                {
                    string[] strs = str.Split(';');
                    if (strs.Length == 1)
                        strs = str.Split(',');

                    // valor X
                    tempData[i, 0] = float.Parse(strs[0]);
                    tempData[i, 1] = float.Parse(strs[1]);

                    // search for min value
                    if (tempData[i, 0] < minX)
                        minX = tempData[i, 0];

                    // search for max value
                    if (tempData[i, 0] > maxX)
                        maxX = tempData[i, 0];

                    i++;
                }

                // allocate and set data
                data = new double[i, 2];
                Array.Copy(tempData, 0, data, 0, i * 2);
            }
            catch (Exception)
            {
                System.Console.WriteLine("Falha na leitura do arquivo");
            }
            finally
            {
                // close file
                if (reader != null)
                    reader.Close();
            }
            return data;
        }

    }
}
