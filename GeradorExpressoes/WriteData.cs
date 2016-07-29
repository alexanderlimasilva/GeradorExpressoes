using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GeradorExpressoes
{
    class WriteData
    {

        public void escreveArquivo(String texto, String arquivo, int adicionaDataHora)
        {

            String nomeArq = @"D:\Projetos C\Resultados\" + arquivo;

            if (adicionaDataHora == 1) { 
               nomeArq += DateTime.Now.ToString("yyyyMMdd_HHmmss"); 
            }
            
            nomeArq += ".txt";

            //Declaração do método StreamWriter passando o caminho e nome do arquivo que deve ser salvo
            StreamWriter writer = new StreamWriter(nomeArq, true);

            writer.Write(texto);

            //Fechando o arquivo
            writer.Close();

            //Limpando a referencia dele da memória
            writer.Dispose();
        }
    }
}
