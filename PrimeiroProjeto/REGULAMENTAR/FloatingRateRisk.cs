using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculoDeRisco.REGULAMENTAR
{
    internal class FloatingRateRisk
    {
        // Fator F = 8%
        private const double F = 0.08;

        // Fatores Yi por vértice
        private static readonly double[] Yi = { 0.0, 0.0015, 0.0030, 0.0040, 0.0080, 0.0150, 0.0290, 0.0420, 0.0560, 0.0680, 0.1350 };

        // Fatores Wj por zona
        private static readonly double[] Wj = { 0.4, 0.3, 0.3 };

        // Vértices em dias úteis
        private static readonly int[] Vertices = { 1, 21, 42, 63, 126, 252, 504, 756, 1008, 1260, 2520 };

        // Método principal para calcular o RWA
        public double CalculateRWAJUR2(List<CashFlow> cashFlows, double mext)
        {
            // Inicializa variáveis para os cálculos
            double totalRWA = 0.0;

            // Agrupa os fluxos de caixa por moeda
            var groupedByCurrency = cashFlows.GroupBy(cf => cf.Currency);

            foreach (var currencyGroup in groupedByCurrency)
            {
                // Passo 1: Alocar fluxos de caixa nos vértices
                double[] ELi = AllocateCashFlowsToVertices(currencyGroup.ToList());

                // Passo 2: Aplicar fatores Yi
                for (int i = 0; i < ELi.Length; i++)
                {
                    ELi[i] *= (1 + Yi[i]);
                }

                // Passo 3: Calcular descasamento vertical (DVi)
                double[] DVi = CalculateVerticalMismatch(ELi);

                // Passo 4: Calcular descasamento horizontal dentro das zonas (DHZj)
                double[] DHZj = CalculateHorizontalMismatchWithinZones(ELi);

                // Passo 5: Calcular descasamento horizontal entre zonas (DHE)
                double DHE = CalculateHorizontalMismatchBetweenZones(ELi);

                // Soma quádrupla para a moeda atual
                double sum = ELi.Sum() + DVi.Sum() + DHZj.Sum() + DHE;

                // Adiciona o RWA da moeda ao total
                totalRWA += F * mext * Math.Sqrt(sum);
            }

            return totalRWA;
        }

        private double[] AllocateCashFlowsToVertices(List<CashFlow> cashFlows)
        {
            double[] ELi = new double[Vertices.Length];

            foreach (var cashFlow in cashFlows)
            {
                int maturity = cashFlow.MaturityInBusinessDays;

                if (maturity <= Vertices[0])
                {
                    ELi[0] += cashFlow.Value;
                }
                else if (maturity >= Vertices[^1])
                {
                    ELi[^1] += cashFlow.Value * (double)maturity / Vertices[^1];
                }
                else
                {
                    for (int i = 0; i < Vertices.Length - 1; i++)
                    {
                        if (maturity > Vertices[i] && maturity <= Vertices[i + 1])
                        {
                            double fraction = (double)(maturity - Vertices[i]) / (Vertices[i + 1] - Vertices[i]);
                            ELi[i] += cashFlow.Value * (1 - fraction);
                            ELi[i + 1] += cashFlow.Value * fraction;
                            break;
                        }
                    }
                }
            }

            return ELi;
        }

        private double[] CalculateVerticalMismatch(double[] ELi)
        {
            double[] DVi = new double[ELi.Length];

            for (int i = 0; i < ELi.Length; i++)
            {
                double positive = Math.Max(0, ELi[i]);
                double negative = Math.Max(0, -ELi[i]);
                DVi[i] = 0.1 * Math.Min(positive, negative);
            }

            return DVi;
        }

        private double[] CalculateHorizontalMismatchWithinZones(double[] ELi)
        {
            double[] DHZj = new double[Wj.Length];

            // Zonas de vencimento
            int[][] zones = {
                new int[] { 0, 1, 2, 3, 4 }, // Zona 1: P1 a P5
                new int[] { 5, 6, 7 },       // Zona 2: P6 a P8
                new int[] { 8, 9, 10 }       // Zona 3: P9 a P11
            };

            for (int j = 0; j < zones.Length; j++)
            {
                double positiveSum = 0.0;
                double negativeSum = 0.0;

                foreach (int i in zones[j])
                {
                    if (ELi[i] > 0)
                        positiveSum += ELi[i];
                    else
                        negativeSum += Math.Abs(ELi[i]);
                }

                DHZj[j] = Math.Min(positiveSum, negativeSum) * Wj[j];
            }

            return DHZj;
        }

        private double CalculateHorizontalMismatchBetweenZones(double[] ELi)
        {
            // Zonas de vencimento
            int[][] zones = {
                new int[] { 0, 1, 2, 3, 4 }, // Zona 1: P1 a P5
                new int[] { 5, 6, 7 },       // Zona 2: P6 a P8
                new int[] { 8, 9, 10 }       // Zona 3: P9 a P11
            };

            double[] zoneTotals = new double[zones.Length];

            for (int j = 0; j < zones.Length; j++)
            {
                foreach (int i in zones[j])
                {
                    zoneTotals[j] += ELi[i];
                }
            }

            double mismatch = 0.0;

            // Entre Zona 1 e Zona 2
            if (zoneTotals[0] * zoneTotals[1] < 0)
                mismatch += 0.4 * Math.Min(Math.Abs(zoneTotals[0]), Math.Abs(zoneTotals[1]));

            // Entre Zona 2 e Zona 3
            if (zoneTotals[1] * zoneTotals[2] < 0)
                mismatch += 0.4 * Math.Min(Math.Abs(zoneTotals[1]), Math.Abs(zoneTotals[2]));

            // Entre Zona 1 e Zona 3
            if (zoneTotals[0] * zoneTotals[2] < 0)
                mismatch += 1.0 * Math.Min(Math.Abs(zoneTotals[0]), Math.Abs(zoneTotals[2]));

            return mismatch;
        }
    }

    public class CashFlow
    {
        public string Currency { get; set; } // Moeda do fluxo de caixa
        public double Value { get; set; } // Valor do fluxo de caixa
        public int MaturityInBusinessDays { get; set; } // Vencimento em dias úteis
    }
}
