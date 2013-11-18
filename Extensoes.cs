using System;
using System.Collections.Generic;
using System.Linq;

namespace IA.RNA.MLP.Projeto
{
    public static class Extensoes
    {

        public static T[] ObterLinha<T>(this T[,] matriz, int linha)
        {
            var t = new List<T>();

            for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                t.Add(matriz[linha, coluna]);

            return t.ToArray();
        }

        public static T[] ObterColuna<T>(this T[,] matriz, int coluna)
        {
            var t = new List<T>();

            for (int linha = 0; linha < matriz.GetLength(0); linha++)
                t.Add(matriz[linha, coluna]);

            return t.ToArray();
        }

        public static List<T> ObterColuna<T>(this List<List<T>> matriz, int coluna)
        {
            var t = new List<T>();
            
            for (int linha = 0; linha < matriz.Count(); linha++)
                t.Add(matriz.ElementAt(linha).ElementAt(coluna));

            return t;
        }

        public static List<T> ObterLinha<T>(this List<List<T>> matriz, int linha)
        {
            var t = new List<T>();

            for (int coluna = 0; coluna < matriz.ElementAt(linha).Count(); coluna++)
                t.Add(matriz.ElementAt(linha).ElementAt(coluna));

            return t;
        }

        public static List<List<T>> RemoverColuna<T>(this List<List<T>> matriz, int coluna)
        {
            var lista = matriz.ToList();

            for (int linha = 0; linha < lista.Count; linha++)
            {
                var temp = lista[linha].ToList();
                temp.RemoveAt(coluna);

                lista[linha] = temp;
            }

            return lista;
        }

        public static void SortearPosicoes<T>(this IList<T> lista)
        {
            Random random = new Random();
            int n = lista.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T valor = lista[k];
                lista[k] = lista[n];
                lista[n] = valor;
            }
        }

        public static double[,] Normalizar(double[,] dados)
        {
            double[,] valores = (double[,]) dados.Clone();

            for (int c = 0; c < valores.GetLength(1); c++)
            {
                double[] coluna = valores.ObterColuna(c);

                double maior = coluna.Max();
                double menor = coluna.Min();

                double divisor = maior - menor;

                for (int linha = 0; linha < valores.GetLength(0); linha++)
                {
                    valores[linha, c] = (valores[linha, c] - menor)/divisor;
                }
            }

            return valores;
        }

        public static List<List<double>> Normalizar(this List<List<double>> dados)
        {
            var valores = new List<List<double>>(dados);

            for (int c = 0; c < valores.First().Count; c++)
            {
                var coluna = valores.ObterColuna(c);

                double maior = coluna.Max();
                double menor = coluna.Min();

                double divisor = maior - menor;

                for (int linha = 0; linha < valores.Count; linha++)
                {
                    valores[linha][c] = (valores[linha][c] - menor) / divisor;
                }
            }

            return valores;
        }

        public static List<List<double>> Normalizar(this List<List<double>> dados, double escalaMinima, double escalaMaxima) 
        {
            var valores = new List<List<double>>(dados);
            
            for (int c = 0; c < valores.First().Count; c++)
            {
                var coluna = valores.ObterColuna(c);

                double maior = coluna.Max();
                double menor = coluna.Min();

                double valorIntervalo = maior - menor;
                double escanaIntervalo = escalaMaxima - escalaMinima;
                
                for (int linha = 0; linha < valores.Count; linha++)
                {
                    valores[linha][c] = ((escanaIntervalo * (valores[linha][c] - menor)) / valorIntervalo) + escalaMinima;
                }
            }

            return valores;
        }

        public static double[,] ConverterParaMatriz(this List<List<double>> dados)
        {
            var linha = dados.Count;
            var coluna = dados[0].Count;
            var resultado = new double[linha, coluna];
            for (int l = 0; l != linha; l++)
                for (int c = 0; c != coluna; c++)
                    resultado[l, c] = dados[l][c];

            return resultado;
        }
        
        public static double NextDoubleInterval(this Random random, double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }
        
        public static double[] ConverterParaArray(this double[,] matriz)
        {
            return matriz.Cast<double>().ToArray();
        }
        
        public static double[,] ConverterParaMatrix(this double[] array, int linha, int coluna)
        {
            if (array.Length != linha * coluna)
            {
                throw new ArgumentException("Tamanho inválido");
            }

            double[,] resultado = new double[linha, coluna];
            
            // BlockCopy uses byte lengths: a double is 8 bytes
            Buffer.BlockCopy(array, 0, resultado, 0, array.Length * sizeof(double));

            return resultado;
        }

        public static double[] ConverterListaParaArray(this List<double[,]> dados)
        {
            List<double> resultado = new List<double>();

            foreach (var item in dados)
                resultado.AddRange(item.ConverterParaArray());
            
            return resultado.ToArray();
        }

        public static List<double[,]> ConverterArrayParaRedeNeural(this double[] array, int[] estrutura, int qtdEntradas)
        {
            List<double[,]> resultado = new List<double[,]>();

            double[] arrayTemporario = (double[]) array.Clone();
            
            for (int i = 0; i < estrutura.Length; i++)
            {
                int entradas = (i == 0 ? qtdEntradas : estrutura[i - 1]) + 1;
                int saidas = estrutura[i];
                
                double[,] temp = new double[entradas, saidas];

                var arrayLocal = arrayTemporario.Where((valor, indice) => indice < (entradas * saidas)).ToArray();

                var matrizLocal = arrayLocal.ConverterParaMatrix(entradas, saidas);

                resultado.Add(matrizLocal);

                arrayTemporario = arrayTemporario.Where((valor, indice) => indice >= ((entradas * saidas))).ToArray();
            }   

            return resultado;
        }

    }
}