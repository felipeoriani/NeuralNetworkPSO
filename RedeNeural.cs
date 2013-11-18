using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace IA.RNA.MLP.Projeto
{
    public class RedeNeural
    {
        #region [ Propriedades ]

        public double PesoMaximo { get; set; }
        public double PesoMinimo { get; set; }

        private const double Bias = 1;

        public int QuantidadeEntradas { get; private set; }

        public int[] Estrutura { get; set; }
        
        public double PSigmoid { get; set; }

        public double PTangenteHiperbolica { get; set; }

        public ConfiguracaoPso ConfiguracaoPso { get; private set; }
        
        public int NumeroRnAs { get; private set; }

        public List<double[,]> Pesos { get; private set; }

        public Dictionary<int, double> HistoricoDeFitness { get; private set; }

        public TipoErro TipoErro { get; set; }

        #endregion

        #region [.ctor]

        private RedeNeural(int[] estrutura, int qtdeEntradas = 0, int numeroRNAs = 10, double pesoMaximo = 1, TipoErro tipoErro = TipoErro.ErroMedioClassificacao)
        {
            this.QuantidadeEntradas = qtdeEntradas;
            this.Estrutura = estrutura;

            this.NumeroRnAs = numeroRNAs;

            ConfiguracaoPso = new ConfiguracaoPso();

            PSigmoid = 1;
            PTangenteHiperbolica = 1;

            PesoMaximo = pesoMaximo;
            PesoMinimo = -pesoMaximo;

            this.TipoErro = tipoErro;
        }

        #endregion

        public void TreinarPso(ItemSet set)
        {
            List<Particula> enxame = new List<Particula>();
            
            int dimensoes = 0;

            for (int e = 0; e < Estrutura.Length; e++)
                dimensoes += (((e == 0) ? (QuantidadeEntradas) : (Estrutura[e - 1])) + 1 /*bias*/) * Estrutura[e];

            double[] melhorPosicaoGlobal = new double[dimensoes];
            double melhorFitnessGlobal = double.MaxValue;
            
            // realiza iterações com o conjunto de treinamento
            for (int r = 0; r < this.NumeroRnAs; r++)
            {
                // ------ Processo RNA ---------
                
                // cria um peso candidato para a RNA
                var pesosCandidato = GerarPesos();

                // variável para obtenção do erro médio quadrático destes pesos
                double erroMedio;
                // iteração da RNA para obtenção do erro médio
                Iterar(set, out erroMedio, pesosCandidato);
                
                // ------ Processo PSO ---------
                
                // conversão dos pesos da RNA para um array de posições para a particula
                double[] posicao = pesosCandidato.ConverterListaParaArray();
                
                // o fitness será o erro médio na obtenção da RNA
                double fitness = erroMedio;

                // criação do array de velocidades para o deslocamento de particulas
                double[] velocidade = new double[posicao.Length];

                // preenchimento das velocidades de forma limitada para não ocasionar um estouro
                for (int v = 0; v < velocidade.Length; v++)
                    velocidade[v] = Random.NextDoubleInterval(this.ConfiguracaoPso.MenorVelocidade,
                                                              this.ConfiguracaoPso.MaiorVelocidade);

                // criação da particula
                Particula particula = new Particula(posicao, fitness, velocidade, posicao, fitness);

                // inclusão da particula na lista de particulas (enxame)
                enxame.Add(particula);

                // verifica se o fitness da particula 'r' do enxame (atual) é melhor (menor, problema de minimização) que o melhor fitness global
                if (enxame[r].Fitness < melhorFitnessGlobal)
                {
                    // atualiza o melhor fitness global
                    melhorFitnessGlobal = enxame[r].Fitness;

                    // atualiza a melhor posição global
                    enxame[r].Posicao.CopyTo(melhorPosicaoGlobal, 0);
                }
            }


            HistoricoDeFitness = new Dictionary<int, double>();

            int t = 1;

            while (t <= this.ConfiguracaoPso.NumeroIteracoes)
            {
                double[] novaVelocidade = new double[dimensoes];
                double[] novaPosicao = new double[dimensoes];
                    
                // para cada particula
                for (int i = 0; i < enxame.Count; i++)
                {
                    Particula particulaAtual = enxame[i];
                    
                    
                    // ajusta a velocidade
                    for (int j = 0; j < particulaAtual.Velocidade.Length; j++)
                    {
                        this.ConfiguracaoPso.GerarPesosSocialECognitivo();

                        // calcula a velocidade
                        novaVelocidade[j] = (particulaAtual.Velocidade[j]) +
                                            (this.ConfiguracaoPso.PesoCognitivo * (particulaAtual.MelhorPosicao[j] - particulaAtual.Posicao[j])) +
                                            (this.ConfiguracaoPso.PesoSocial * (melhorPosicaoGlobal[j] - particulaAtual.Posicao[j]));

                        // se a velocidade calculada saiu do intervalo permitido, ajusta para o intervalo (menor/maior) permitido
                        novaVelocidade[j] = this.ConfiguracaoPso.PadronizarVelocidade(novaVelocidade[j]);
                    }

                    novaVelocidade.CopyTo(particulaAtual.Velocidade, 0);

                    // atualiza a posição das particulas
                    for (int j = 0; j < particulaAtual.Posicao.Length; j++)
                    {
                        novaPosicao[j] = particulaAtual.Posicao[j] + particulaAtual.Velocidade[j];

                        // padronizar a posição
                        //novaPosicao[j] = this.ConfiguracaoPso.PadronizarParticula(novaPosicao[j]);
                    }

                    novaPosicao.CopyTo(particulaAtual.Posicao, 0);

                    // variável para obtenção do novo erro médio quadrático destes pesos após reajuste
                    double novoErroMedio;

                    Iterar(set, out novoErroMedio, particulaAtual.Posicao);
                    
                    // atualiza o fitness
                    particulaAtual.Fitness = novoErroMedio;

                    //verifica se a nova particula gerada, é melhor
                    if (novoErroMedio < particulaAtual.MelhorFitness)
                    {
                        // atualiza a posição da particula
                        novaPosicao.CopyTo(particulaAtual.MelhorPosicao, 0);
                        
                        // atualiza o melhor fitness para a particula
                        particulaAtual.MelhorFitness = novoErroMedio;
                    }

                    // verifica se o fitness é melhor que o fitness global
                    if (novoErroMedio < melhorFitnessGlobal) 
                    {
                        // atualiza a melhor posição
                        novaPosicao.CopyTo(melhorPosicaoGlobal, 0);
                        
                        // atualiza melhor fitness
                        melhorFitnessGlobal = novoErroMedio;
                    }
                }
                
                HistoricoDeFitness.Add(t, melhorFitnessGlobal);

                t++;
            }
            // define o melhor peso para a RNA
            Pesos = melhorPosicaoGlobal.ConverterArrayParaRedeNeural(this.Estrutura, this.QuantidadeEntradas);
        }

        public List<double[]> Operar(ItemSet set, out double erro)
        {
            if (Pesos == null)
                throw new Exception("A rede não foi treinada.");
            
            List<double[]> resultados = Iterar(set, out erro, Pesos);
            
            for (int r = 0; r < resultados.Count; r++)
            {
                double maior = resultados[r].Max();

                for (int i = 0; i < resultados[r].Length; i++)
                {
                    resultados[r][i] = resultados[r][i] == maior ? 1 : -1;
                }
            }

            return resultados;
        }

        private List<double[]> Iterar(ItemSet set, out double erroMedio, double[] pesos)
        {
            // reconverte as particulas para a estrutura da RNA
            var novoPesoRna = pesos.ConverterArrayParaRedeNeural(this.Estrutura,
                                                                 this.QuantidadeEntradas);

            return Iterar(set, out erroMedio, novoPesoRna);
        }

        private List<double[]> Iterar(ItemSet set, out double erroMedio, List<double[,]> pesos)
        {
            List<double> errosQuadraticos = new List<double>();

            List<double[]> saidas = new List<double[]>();

            List<double> valores = null;

            int acertos = 0;

            // percorre amostras do set
            for (int amostra = 0; amostra < set.Entradas.Count; amostra++)
            {
                // obtem os valores para a primeira camada
                valores = set.Entradas[amostra];

                // percorre as camadas da estrutura
                for (int camada = 0; camada < Estrutura.Length; camada++)
                {
                    List<double> saidaCamada = new List<double>();

                    // percorre cada neuronio da camada
                    for (int neuronio = 0; neuronio < Estrutura[camada]; neuronio++)
                    {
                        double temp = 0d;

                        // realiza a ponderação de todas os valores para cada neuronio multiplicando ao peso
                        for (int i = 0; i < valores.Count; i++)
                            temp += valores[i]*pesos[camada][i, neuronio];

                        // aplica o limiar de ativação (bias) e um novo peso
                        temp += (Bias*pesos[camada][valores.Count, neuronio]);

                        // aplica a função de ativação (tangente hiperbolica)
                        temp = TangenteHiperbolica(temp);

                        // adiciona o valor do neuronio para a camada de saida
                        saidaCamada.Add(temp);
                    }

                    valores = saidaCamada;
                }


                double maior = valores.Max();
                int indexMaior = Array.IndexOf(valores.ToArray(), maior);
                
                if (indexMaior == Array.IndexOf(set.SaidasBinarias[amostra].ToArray(), set.SaidasBinarias[amostra].Max()))
                {
                    acertos++;
                }

                saidas.Add(valores.ToArray());

                // calcula o erro quadrático
                double erro = ObterErroQuadratico(set.SaidasBinarias[amostra], valores);

                errosQuadraticos.Add(erro);
            }

            int erros = set.Entradas.Count - acertos;

            if (TipoErro == TipoErro.ErroMedioClassificacao)
            {
                // erro médio de classificação
                erroMedio = ((double) erros * 100 / set.Entradas.Count); // / 100;
            }
            else
            {
                // obtém o erro quadrático médio
                erroMedio = errosQuadraticos.Average();    
            }

            return saidas;
        }

        private List<double[,]> GerarPesos()
        {
            List<double[,]> pesos = new List<double[,]>();
            
            // preenchimento (aleatorio) dos pesos
            for (int e = 0; e < Estrutura.Length; e++)
                pesos.Add(e == 0
                              ? GerarPesos(this.QuantidadeEntradas + 1 /* bias */, Estrutura[e])
                              : GerarPesos(Estrutura[e - 1] + 1 /* bias */, Estrutura[e]));

            return pesos;
        }

        private double[,] GerarPesos(int qtdeEntradas, int qtdeNeuronios)
        {
            double[,] result = new double[qtdeEntradas, qtdeNeuronios];

            for (int entrada = 0; entrada < qtdeEntradas; entrada++)
                for (int neuronio = 0; neuronio < qtdeNeuronios; neuronio++)
                    result[entrada, neuronio] = Random.NextDoubleInterval(this.PesoMinimo, this.PesoMaximo);

            return result;
        }

        private double Sigmoid(double valor)
        {
            return 1 / (1 + Math.Exp(-this.PSigmoid * valor));
        }

        private double TangenteHiperbolica(double valor)
        {
            return (Math.Exp(this.PTangenteHiperbolica * valor) - Math.Exp(-this.PTangenteHiperbolica * valor))
                    / (Math.Exp(this.PTangenteHiperbolica * valor) + Math.Exp(-this.PTangenteHiperbolica * valor));
        }

        private double ObterErroQuadratico(List<double> saidasEsperadas, List<double> saidaRede)
        {
            double soma = 0;

            for (int i = 0; i < saidasEsperadas.Count; i++)
                soma += Math.Pow(saidasEsperadas[i] - saidaRede[i], 2);

            return ((double)1 / 2) * soma;
        }

        private Random Random = new Random();

        
        #region [ Factories Methods ]

        public static RedeNeural IrisFactory()
        {
            // estrutura da rede:
            //  - Tamanho do Array: Quantidade de Camadas
            //  - Valores do Array: Quantidade de Neuronios por Camada
            //  - Ultimo valor do Array: Quantidade de Saídas
            int[] estruturaRede = new[] { 4, 3};

            var rna = new RedeNeural(estruturaRede, 4, 10, 3d);
            
            return rna;
        }
        
        public static RedeNeural WineFactory()
        {
            // estrutura da rede:
            //  - Tamanho do Array: Quantidade de Camadas
            //  - Valores do Array: Quantidade de Neuronios por Camada
            //  - Ultimo valor do Array: Quantidade de Saídas
            int[] estruturaRede = new[] { 8, 3 };

            var rna = new RedeNeural(estruturaRede, 13, 10, 1d);

            return rna;
        }

        public static RedeNeural LiverDisorderFactory()
        {
            // estrutura da rede:
            //  - Tamanho do Array: Quantidade de Camadas
            //  - Valores do Array: Quantidade de Neuronios por Camada
            //  - Ultimo valor do Array: Quantidade de Saídas
            int[] estruturaRede = new[] { 8, 2 };

            var rna = new RedeNeural(estruturaRede, 7, 10, 2d);

            
            rna.ConfiguracaoPso.SetIntervaloVelocidade(4d);
            

            
            return rna;
        }

        public static RedeNeural NovaRna(Classificador classificador)
        {
            switch (classificador)
            {
                case Classificador.Iris:
                    return IrisFactory();
                    break;
                case Classificador.Wine:
                    return WineFactory();
                    break;
                case Classificador.LiverDisorder:
                    return LiverDisorderFactory();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("classificador");
            }
        }
        
        #endregion
    }
}