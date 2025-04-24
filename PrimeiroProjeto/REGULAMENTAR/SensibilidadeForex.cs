using System;
using System.Collections.Generic;
using System.Linq;

<<<<<<< HEAD
<<<<<<< HEAD
namespace CalculoDeRisco.REGULAMENTAR
=======
namespace PrimeiroProjeto.REGULAMENTAR
>>>>>>> 79ac04b387940018f32fb09d4cbb53c86586d7a1
=======
namespace PrimeiroProjeto.REGULAMENTAR
>>>>>>> 79ac04b387940018f32fb09d4cbb53c86586d7a1
{
    internal class SensibilidadeForex
    {
        // Estrutura para representar um instrumento financeiro
        public class InstrumentoFinanceiro
        {
            public string Moeda { get; set; } // Moeda do fluxo de caixa
            public List<FluxoDeCaixa> FluxosDeCaixa { get; set; } // Lista de fluxos de caixa
            public double TaxaDeCambioAtual { get; set; } // Taxa de câmbio atual em relação à moeda nacional
            public double TaxaDeDesconto { get; set; } // Taxa de desconto para os fluxos de caixa
        }

        // Estrutura para representar um fluxo de caixa
        public class FluxoDeCaixa
        {
            public double Valor { get; set; } // Valor do fluxo de caixa
            public DateTime Data { get; set; } // Data do fluxo de caixa
        }

        // Estrutura para representar uma categoria de risco FX
        public class CategoriaRiscoFX
        {
            public string Moeda { get; set; } // Moeda estrangeira
            public double PonderadorDeRisco { get; set; } // Ponderador de risco (RW_j)
        }

        // Método principal para calcular a componente Delta do risco FX
        public static (double, double, double) CalcularDeltaFX(
            List<InstrumentoFinanceiro> instrumentos,
            List<CategoriaRiscoFX> categoriasRisco,
            string moedaNacional)
        {
            // Dicionário para armazenar as sensibilidades consolidadas (NS_j) por moeda
            var sensibilidadesConsolidadas = new Dictionary<string, double>();

            foreach (var instrumento in instrumentos)
            {
                // Calcula o valor de mercado atual do instrumento
                double valorMercadoAtual = CalcularValorDeMercado(instrumento);

                // Aplica o choque de +1% na taxa de câmbio
                double taxaDeCambioChocada = instrumento.TaxaDeCambioAtual * 1.01;

                // Recalcula o valor de mercado com a taxa de câmbio chocada
                double valorMercadoChocado = CalcularValorDeMercado(instrumento, taxaDeCambioChocada);

                // Calcula a sensibilidade individual (S_i,k)
                double sensibilidade = (valorMercadoChocado - valorMercadoAtual) / 0.01;

                // Consolida a sensibilidade para a moeda do instrumento
                if (!sensibilidadesConsolidadas.ContainsKey(instrumento.Moeda))
                {
                    sensibilidadesConsolidadas[instrumento.Moeda] = 0.0;
                }
                sensibilidadesConsolidadas[instrumento.Moeda] += sensibilidade;
            }

            // Calcula as sensibilidades ponderadas (WS) para cada moeda
            var sensibilidadesPonderadas = new Dictionary<string, double>();
            foreach (var categoria in categoriasRisco)
            {
                if (sensibilidadesConsolidadas.ContainsKey(categoria.Moeda))
                {
                    sensibilidadesPonderadas[categoria.Moeda] =
                        sensibilidadesConsolidadas[categoria.Moeda] * categoria.PonderadorDeRisco;
                }
            }

            // Calcula os requerimentos de capital para os cenários de correlação
            double deltaMedia = CalcularRequerimentoDeCapital(sensibilidadesPonderadas, 0.6); // Correlação média
            double deltaAlta = CalcularRequerimentoDeCapital(sensibilidadesPonderadas, 0.75); // Correlação alta
            double deltaBaixa = CalcularRequerimentoDeCapital(sensibilidadesPonderadas, 0.25); // Correlação baixa

            return (deltaMedia, deltaAlta, deltaBaixa);
        }

        // Método para calcular o valor de mercado de um instrumento financeiro
        private static double CalcularValorDeMercado(InstrumentoFinanceiro instrumento, double? taxaDeCambio = null)
        {
            double taxaDeCambioUsada = taxaDeCambio ?? instrumento.TaxaDeCambioAtual;
            double valorDeMercado = 0.0;

            foreach (var fluxo in instrumento.FluxosDeCaixa)
            {
                // Desconta o fluxo de caixa para o valor presente
                double valorPresente = fluxo.Valor / Math.Pow(1 + instrumento.TaxaDeDesconto, (fluxo.Data - DateTime.Now).TotalDays / 365.0);
                valorDeMercado += valorPresente * taxaDeCambioUsada;
            }

            return valorDeMercado;
        }

        // Método para calcular o requerimento de capital com base nas sensibilidades ponderadas e correlação
        private static double CalcularRequerimentoDeCapital(Dictionary<string, double> sensibilidadesPonderadas, double correlacao)
        {
            double somaQuadrados = 0.0;
            double somaProdutos = 0.0;

            var moedas = sensibilidadesPonderadas.Keys.ToList();
            for (int i = 0; i < moedas.Count; i++)
            {
                somaQuadrados += Math.Pow(sensibilidadesPonderadas[moedas[i]], 2);

                for (int j = i + 1; j < moedas.Count; j++)
                {
                    somaProdutos += 2 * correlacao * sensibilidadesPonderadas[moedas[i]] * sensibilidadesPonderadas[moedas[j]];
                }
            }

            return Math.Sqrt(somaQuadrados + somaProdutos);
        }
    }
}
