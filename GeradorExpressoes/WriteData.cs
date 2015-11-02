using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GeradorExpressoes
{
    class WriteData
    {

        public void escreveArquivo(String texto, string arquivo)
        {

            String nomeArq = @"D:\Projetos C\Instancias\resultado_" + arquivo + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

            //Declaração do método StreamWriter passando o caminho e nome do arquivo que deve ser salvo
            StreamWriter writer = new StreamWriter(nomeArq);

            writer.Write(texto);

            //Fechando o arquivo
            writer.Close();

            //Limpando a referencia dele da memória
            writer.Dispose();
        }

    }
}
