using System.Text;

namespace IA.RNA.MLP.Projeto
{
    public class Particula
    {
        public double[] Posicao { get; set; } // pesos da RNA
        public double Fitness { get; set; } // erro médio
        public double[] Velocidade { get; set; } // atualização de pesos

        public double[] MelhorPosicao { get; set; } // melhor posição encontrada
        public double MelhorFitness { get; set; } // associado a melhor posição
        
        public Particula(double[] posicao, double fitness, double[] velocidade, double[] melhorPosicao, double melhorFitness)
        {
            this.Posicao = new double[posicao.Length];
            posicao.CopyTo(this.Posicao, 0);

            this.Fitness = fitness;

            this.Velocidade = new double[velocidade.Length];
            velocidade.CopyTo(this.Velocidade, 0);

            this.MelhorPosicao = new double[melhorPosicao.Length];
            melhorPosicao.CopyTo(this.MelhorPosicao, 0);

            this.MelhorFitness = melhorFitness;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            s.AppendLine(" ---[ Particula ] ------------------------------- ");

            s.Append("Posições: ");
            for (int i = 0; i < this.Posicao.Length; i++)
                s.AppendFormat("{0} ", Posicao[i]);
            
            s.AppendLine();

            s.AppendFormat("Fitness: {0}", this.Fitness);

            s.AppendLine();

            s.AppendLine("Velocidade: ");
            for (int i = 0; i < this.Velocidade.Length; i++)
                s.AppendFormat("{0} ", Velocidade[i]);

            s.AppendLine();

            s.AppendLine("Melhores Posições: ");
            for (int i = 0; i < this.MelhorPosicao.Length; i++)
                s.AppendFormat("{0} ", this.MelhorPosicao[i]);
            
            s.AppendFormat("Melhor Fitness: {0}", MelhorFitness);

            return s.ToString();
        }
    }
}