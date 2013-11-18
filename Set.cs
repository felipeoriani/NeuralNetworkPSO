using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IA.RNA.MLP.Projeto
{
    public class ItemSet
    {
        public List<List<double>> Entradas { get; set; }
        public List<double> Saidas { get; set; }
        public List<List<double>> SaidasBinarias { get; set; }

        public ItemSet() { }

        public ItemSet(List<List<double>> entradas, List<double> saidas)
        {
            Entradas = entradas;
            Saidas = saidas;

            SaidasBinarias = new List<List<double>>();

            var saidasDistintas = saidas.Distinct().ToList();

            for (int i = 0; i < saidas.Count; i++)
            {
                double[] s = new double[saidasDistintas.Count];

                for (int j = 0; j < saidasDistintas.Count; j++)
                    s[j] = -1d;

                s[saidasDistintas.IndexOf(saidas[i])] = 1d;

                SaidasBinarias.Add(s.ToList());
            }
        }
    }

    public class Set
    {
        public ItemSet Treinamento { get; set; }
        public ItemSet Teste { get; set; }

        public int QtdeAmostrasTreinamento { get; set; }

        public Set(ItemSet treinamento, ItemSet teste)
        {
            Treinamento = treinamento;
            Teste = teste;

            QtdeAmostrasTreinamento = treinamento.Entradas.Count;
        }

        public static List<Set> ObterSets(string caminhoArquivo, int porcentagemTeste, int colunaSupervisionada)
        {
            // define uma cultura
            var cultureInfo = System.Globalization.CultureInfo.InvariantCulture;

            // obtem todas as linhas do arquivo
            List<string> arquivo = File.ReadAllLines(caminhoArquivo).Where(x => !string.IsNullOrEmpty(x)).ToList();
            
            // reordena de forma aleatoria a lista, para não manter um conjunto específico como teste
            //arquivo.SortearPosicoes();

            // transforma os dados em resultados
            var valores = new List<List<double>>();

            foreach (var valor in arquivo)
                valores.Add(valor.Split(',').Select(x => double.Parse(x, cultureInfo)).ToList());
            
            var saidasPossiveis = valores.ObterColuna(colunaSupervisionada).Distinct().ToList();

            saidasPossiveis.Sort();
            
            var valoresOrganizados = new List<List<double>>();

            while (valores.Count > 0)
            {
                for (int i = 0; i < saidasPossiveis.Count(); i++)
                {
                    var vTemp = valores.FirstOrDefault(x => x[colunaSupervisionada] == saidasPossiveis[i]);
                    if (vTemp == null)
                        continue;

                    valoresOrganizados.Add(vTemp);

                    valores.Remove(vTemp);
                }
            }

            valores = new List<List<double>>(valoresOrganizados);
            
            // normaliza todos os valores
            valores = valores.Normalizar(-1, 1);

            // Lista de valores esperados
            var saidas = valores.ObterColuna(colunaSupervisionada);

            // remove a coluna de valores esperados, mantendo apenas as entradas
            var entradas = valores.RemoverColuna(colunaSupervisionada);
            
            var sets = new List<Set>();

            int totalTestes = (int)(valores.Count * (porcentagemTeste / 100d));

            for (int i = 0; i < 10; i++)
            {
                int indiceInicio = (int)(totalTestes * i);
                int indiceFim = (int)((totalTestes * i) + totalTestes);

                var entradaTeste = entradas.Where((amostra, indice) => (indice >= indiceInicio && indice < indiceFim)).ToList();
                var entradaTreinamento = entradas.Where((amostra, indice) => !(indice >= indiceInicio && indice < indiceFim)).ToList();

                var saidaTeste = saidas.Where((amostra, indice) => (indice >= indiceInicio && indice < indiceFim)).ToList(); 
                var saidaTreinamento = saidas.Where((amostra, indice) => !(indice >= indiceInicio && indice < indiceFim)).ToList();

                var itemSetTreinamento = new ItemSet(entradaTreinamento, saidaTreinamento);
                var itemSetTeste = new ItemSet(entradaTeste, saidaTeste);

                sets.Add(new Set(itemSetTreinamento, itemSetTeste));
            }

            return sets;
        }
    }
}