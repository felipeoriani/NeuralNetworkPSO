using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace IA.RNA.MLP.Projeto
{
    class Program
    {
        static void Main(string[] args)
        {
            var iris = ObterSetsIris();
            var wine = ObterSetsWine();
            var liverDisorder = ObterSetsLiverDisorder();
            
            bool comThreads = false;

            if (comThreads)
            {
                List<Thread> threads = new List<Thread>
                    {
                        new Thread(() => OperarSet(iris, Classificador.Iris)),
                        new Thread(() => OperarSet(wine, Classificador.Wine)),
                        new Thread(() => OperarSet(liverDisorder, Classificador.LiverDisorder))
                    };

                foreach (Thread thread in threads)
                    thread.Start();

                foreach (Thread thread in threads)
                    thread.Join();
            }
            else
            {
                OperarSet(iris, Classificador.Iris);
                OperarSet(wine, Classificador.Wine);
                OperarSet(liverDisorder, Classificador.LiverDisorder);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Fim...");
            Console.ReadKey();
        }

        public static void OperarSet(List<Set> sets, Classificador classificador)
        {
            int iterador = 0;

            double porcentagemTotal = 0;

            foreach (var set in sets)
            {
                iterador++;

                string titulo = string.Empty;

                RedeNeural redeNeural = RedeNeural.NovaRna(classificador);

                redeNeural.TreinarPso(set.Treinamento);

                double erro;
                List<double[]> saidas = redeNeural.Operar(set.Teste, out erro);

                int acertos = 0;
                int erros = 0;

                for (int i = 0; i < saidas.Count; i++)
                {
                    /*Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("TESTE\t");
                    for (int j = 0; j < set.Teste.SaidasBinarias[i].Count; j++)
                        Console.Write(set.Teste.SaidasBinarias[i][j] + "\t");
                    */
                    double maior = saidas[i].Max();

                    if (Array.IndexOf(saidas[i], maior) ==
                        Array.IndexOf(set.Teste.SaidasBinarias[i].ToArray(), set.Teste.SaidasBinarias[i].Max()))
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.Write("\tOK");
                        acertos++;
                    }
                    else
                    {
                        //Console.ForegroundColor = ConsoleColor.Red;
                        //Console.Write("\tERRO");
                        erros++;
                    }

                    //Console.WriteLine();

                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.Write("SAIDA\t");

                    //for (int j = 0; j < saidas[i].Count(); j++)
                    //    Console.Write(saidas[i][j] + "\t");

                    //Console.WriteLine();
                    //Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine("----------------------------------------------");
                }

                
                
                switch (classificador)
                {
                    case Classificador.Iris:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        titulo = "Iris";
                        break;
                    case Classificador.Wine:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        titulo = "Wine";
                        break;
                    case Classificador.LiverDisorder:
                        Console.ForegroundColor = ConsoleColor.Green;
                        titulo = "Liver Disorder";
                        break;
                }

                Console.WriteLine();
                Console.WriteLine(" {0} - {1} de {2} - Acertos: {3}  Erros: {4}", titulo.ToUpper(), iterador,
                                    sets.Count, acertos, erros);
                Console.WriteLine(" Erro Médio: {0}", erro.ToString("0.00"));
                Console.WriteLine();

                porcentagemTotal += erro;

                /*
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Acertos: {0}", acertos);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Erros: {0}", erros);

                Console.WriteLine();
                Console.ReadKey();*/
                
                List<DataPoint> pontos = redeNeural.HistoricoDeFitness.Select(x => new DataPoint(x.Key, x.Value)).ToList();


                string nomeArquivo = string.Format("{0} - {1} de {2}.jpg", titulo.ToUpper(), iterador, sets.Count);
                
                GerarGrafico(pontos, Path.Combine(Path.Combine(path, "Graficos"), nomeArquivo),
                             Path.GetFileNameWithoutExtension(nomeArquivo), acertos, erros, redeNeural, set);
                
            }

            porcentagemTotal = porcentagemTotal / sets.Count;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Erro Médio Geral: {0}%", porcentagemTotal.ToString("0.00"));
            Console.WriteLine();

        }

        protected static void GerarGrafico(IList<DataPoint> series, string caminhoArquivo, string titutlo, int acertos, int erros, RedeNeural rna, Set set)
        {
            using (var grafico = new Chart())
            {
                grafico.ChartAreas.Add(new ChartArea());

                grafico.AntiAliasing = AntiAliasingStyles.All;
                grafico.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
                grafico.Width = 800;
                grafico.Height = 650;

                int totalAmostras = acertos + erros;
                double porcentagemAcertos = (acertos * 100) / totalAmostras;

                grafico.Titles.Add(titutlo);
                grafico.Titles.Add(string.Format("Evolução do Melhor Fitness do algoritmo PSO com {0} iterações", rna.ConfiguracaoPso.NumeroIteracoes));

                string estruturaRna = string.Empty;
                for (int i = 0; i < rna.Estrutura.Length; i++)
                {
                    if (i == rna.Estrutura.Length - 1)
                        estruturaRna += string.Format("{0} Neurônios Camada de Saída.", rna.Estrutura[i]);
                    else
                        estruturaRna += string.Format("{0} Neurônios Camada Oculta -> ", rna.Estrutura[i]);
                }
                grafico.Titles.Add(string.Format("Estrutura da RNA: {0}", estruturaRna));
                grafico.Titles.Add(string.Format("Total Amostras - Treinamento: {0} - Testes: {1}", set.Treinamento.Saidas.Count, set.Teste.Saidas.Count));
                grafico.Titles.Add(string.Format("{0}% de Aproveitamento (Acertos: {1} - Erros: {2})", porcentagemAcertos.ToString("0.00"), acertos, erros));
                
                var s = new Series();
                s.ChartType = SeriesChartType.Line;
                s.Color = System.Drawing.Color.OrangeRed;
                s.BorderWidth = 2;
                s.Legend = "Fitness";
                s.LegendText = "Fitness";

                foreach (var pontos in series) 
                    s.Points.Add(pontos);

                grafico.Series.Add(s);

                if (File.Exists(caminhoArquivo)) 
                    File.Delete(caminhoArquivo);

                grafico.SaveImage(caminhoArquivo, ChartImageFormat.Png);
            }
        }


        public static List<Set> ObterSetsIris()
        {
            return Set.ObterSets(Path.Combine(path, "iris.data.txt"), 10, 4);
        }

        public static List<Set> ObterSetsWine()
        {
            return Set.ObterSets(Path.Combine(path, "wine.data.txt"), 10, 0);
        }

        public static List<Set> ObterSetsLiverDisorder()
        {
            return Set.ObterSets(Path.Combine(path, "liver-disorder-bupa.data.txt"), 10, 6);
        }

        private const string path = @"C:\Projeto RNA GA IE\data";
    }

}

