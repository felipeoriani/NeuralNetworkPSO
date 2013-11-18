using System;

namespace IA.RNA.MLP.Projeto
{
    public class ConfiguracaoPso
    {
        public double MaiorVelocidade { get; set; }
        public double MenorVelocidade { get; set; }

        public double MaiorValorPosicao { get; set; }
        public double MenorValorPosicao { get; set; }

        public double PesoSocial { get; set; }
        public double PesoCognitivo { get; set; }

        public bool RandomizarPesosSocialCognitivo { get; set; }

        public int NumeroIteracoes { get; set; }

        public ConfiguracaoPso()
        {
            SetIntervaloVelocidade(0.5d);
            SetIntervaloPosicao(0.3d);

            PesoCognitivo = 0.5d;
            PesoSocial = 0.5d;

            NumeroIteracoes = 100;

            RandomizarPesosSocialCognitivo = true;
        }

        public double PadronizarVelocidade(double velocidade)
        {
            if (velocidade < MenorVelocidade)
                return MenorVelocidade;

            if (velocidade > MaiorVelocidade)
                return MaiorVelocidade;

            return velocidade;
        }

        public double PadronizarParticula(double valorPosicao)
        {
            if (valorPosicao < MenorValorPosicao)
                return MenorValorPosicao;

            if (valorPosicao > MaiorValorPosicao)
                return MaiorValorPosicao;

            return valorPosicao;
        }

        public void SetIntervaloPosicao(double valor)
        {
            this.MaiorValorPosicao = valor;
            this.MenorValorPosicao = -valor;
        }

        public void SetIntervaloVelocidade(double valor)
        {
            this.MaiorVelocidade = valor;
            this.MenorVelocidade = -valor;
        }

        public void GerarPesosSocialECognitivo()
        {
            if (RandomizarPesosSocialCognitivo)
            {
                this.PesoCognitivo = random.NextDouble();
                this.PesoSocial = 1 - this.PesoCognitivo;
            }
        }
        
        private static Random random = new Random();
    }
}