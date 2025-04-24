namespace StandardizedApproachRegulatoryCapitalForExposurePJUR1Calculation
{

    public class FixedRateExposureRisk
    {
        public double RiskWeightedAsset { get; set; }
        public double CapitalRequirement { get; set; }
    };

    public class FixedRateExposureRiskCalculation
    {
        private double StandardVaRMedio { get; set; }
        private double StandardVaR { get; set; }
        private double StandardStressedVaRMedio { get; set; }
        private double StandardStressedVaR { get; set; }
        public double RiskWeightedAsset { get; private set; }

        /// Método do Calculo Principal da PJUR1: Requerimento de Capital = F * {max[VaRmédio, VaR] + max[stressedVaRmédio, StressedVaR]}
        public FixedRateExposureRisk GetFixedRateExposureRisk(double rwaFactor)
        {
            // Calcula o máximo entre VaR médio e VaR atual
            double maxStandardVaR = Math.Max(StandardVaRMedio, StandardVaR);

            // Calcula o máximo entre StressedVaR médio e StressedVaR atual
            double maxStandardStressedVaR = Math.Max(StandardStressedVaRMedio, StandardStressedVaR);

            // principal cálculo: F * {max[VaRmédio, VaR] + max[stressedVaRmédio, StressedVaR]}
            double capitalRequirement = rwaFactor * (maxStandardVaR + maxStandardStressedVaR);

            FixedRateExposureRisk fixedRateExposureRisk = new FixedRateExposureRisk()
            {
                RiskWeightedAsset = capitalRequirement / rwaFactor,
                CapitalRequirement = capitalRequirement
            };

            return fixedRateExposureRisk;
        }

        /// <summary>
        /// Calcula a matriz de correlação (ρi,j) utilizada no VaR Padrão.
        /// </summary>
        /// <param name="tenors">Array com os prazos dos vértices em dias úteis</param>
        /// <param name="baseParamForCorrelation">Parâmetro-base "rho" para o cálculo das correlações</param>
        /// <param name="decayFactor">Fator de decaimento "k" da correlação</param>
        /// <returns>Matriz de correlação entre os vértices</returns>
        public double[,] GetCorrelationMatrix(double[] tenors, double baseParamForCorrelation, double decayFactor)
        {
            int n = tenors.Length;
            double[,] correlationMatrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double maxValue = Math.Max(tenors[i], tenors[j]);
                    double minValue = Math.Min(tenors[i], tenors[j]);
                    double partition = maxValue / minValue;
                    double exponent = Math.Pow(partition, decayFactor);
                    correlationMatrix[i, j] = baseParamForCorrelation + Math.Pow(1 - baseParamForCorrelation, exponent);
                }
            }
            return correlationMatrix;
        }


        /// <summary>
        /// Calcula o VaR para cada tenor baseado na fórmula: constante * (tenor[i]/252) * volatility[i] * vmtm[i] * √(settlementDays)
        /// </summary>
        /// <param name="tenors">Lista com os prazos dos vértices em dias úteis</param>
        /// <param name="volatilities">Lista com as volatilidades para cada tenor</param>
        /// <param name="marketValues">Lista com os valores de mercado para cada tenor</param>
        /// <param name="constant">Fator Normal Padrão = 2,33</param>
        /// <param name="settlementDays">Número de dias necessários para liquidação definido pelo Banco Central do Brasil</param>
        /// <returns>Lista com os valores de VaR calculados para cada tenor</returns>
        public double[] GetTenorVar(double[] tenors, double[] volatilities, double[] marketValues, double constant, double settlementDays)
        {
            double[] tenorVar = new double[tenors.Length];
            double sqrtSettlementDays = Math.Sqrt(settlementDays);

            for (int i = 0; i < tenors.Length; i++)
            {
                // VaR[i] = constante * (tenor[i]/252) * volatility[i] * vmtm[i] * √(settlementDays)
                tenorVar[i] = constant * (tenors[i] / 252) * volatilities[i] * marketValues[i] * sqrtSettlementDays;
            }
            return tenorVar;
        }

        /// <summary>
        /// Calcula o Valor em Risco Padrão do conjunto das exposições (VaRt Padrão).
        /// </summary>
        /// <param name="tenorVar">Matriz de valores em risco individuais por vértice</param>
        /// <param name="correlationMatrix">Matriz de correlação (ρi,j)</param>
        /// <returns>Valor do VaR Padrão "standardVar"</returns>
        public double GetStandardVar(double[,] tenorVar, double[,] correlationMatrix)
        {
            //validação:
            if (tenorVar == null || correlationMatrix == null)
                throw new ArgumentNullException("As matrizes não podem ser nulas");

            int n = tenorVar.GetLength(0);
            double varSum = 0;

            // VaRt Padrão = √(∑∑(VaRi,t * VaRj,t * ρi,j))
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    varSum += tenorVar[i, i] * tenorVar[j, j] * correlationMatrix[i, j];
                }
            }

            double standardVar = Math.Sqrt(varSum);
            return standardVar;
        }


        /// <summary>
        /// Calcula a matriz de correlação estressada (ρS i,j) utilizada no sVaR Padrão.
        /// </summary>
        /// <param name="tenors">Array com os prazos dos vértices em dias úteis</param>
        /// <param name="baseParamForStressedCorrelation">Parâmetro-base "rhoS" para o cálculo das correlações estressadas</param>
        /// <param name="stressedDecayFactor">Fator de decaimento "kS" para correlações estressadas</param>
        /// <returns>Matriz de correlação estressada entre os vértices</returns>
        public double[,] GetStressedCorrelationMatrix(double[] tenors, double baseParamForStressedCorrelation, double stressedDecayFactor)
        {
            int n = tenors.Length;
            double[,] stressedCorrelationMatrix = new double[n, n];

            // ρS i,j = ρS^(|Pi - Pj|) * kS
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double maxValue = Math.Max(tenors[i], tenors[j]);
                    double minValue = Math.Min(tenors[i], tenors[j]);
                    double partition = maxValue / minValue;
                    double exponent = Math.Pow(partition, stressedDecayFactor);
                    stressedCorrelationMatrix[i, j] = baseParamForStressedCorrelation + Math.Pow(1 - baseParamForStressedCorrelation, exponent);
                }
            }
            return stressedCorrelationMatrix;
        }

        /// <summary>
        /// Calcula StressedVaR para cada tenor baseado na fórmula: constante * (tenor[i]/252) * volatility[i] * vmtm[i] * √(settlementDays)
        /// </summary>
        /// <param name="tenors">Lista com os prazos dos vértices em dias úteis</param>
        /// <param name="stressedVolatilities">Lista com as stressedVolatilities para cada tenor</param>
        /// <param name="marketValues">Lista com os valores de mercado para cada tenor</param>
        /// <param name="constant">Fator Normal Padrão = 2,33</param>
        /// <param name="settlementDays">Número de dias necessários para liquidação definido pelo Banco Central do Brasil</param>
        /// <returns>Lista com os valores de VaR calculados para cada tenor</returns>
        public double[] GetTenorStressedVar(double[] tenors, double[] stressedVolatilities, double[] marketValues, double constant, double settlementDays)
        {
            double[] tenorStressedVar = new double[tenors.Length];
            double sqrtSettlementDays = Math.Sqrt(settlementDays);

            for (int i = 0; i < tenors.Length; i++)
            {
                // VaR[i] = constante * (tenor[i]/252) * volatility[i] * vmtm[i] * √(settlementDays)
                tenorStressedVar[i] = constant * (tenors[i] / 252) * stressedVolatilities[i] * marketValues[i] * sqrtSettlementDays;
            }
            return tenorStressedVar;
        }

        /// <summary>
        /// Calcula o Valor em Risco Estressado Padrão do conjunto das exposições (SVaRt Padrão).
        /// </summary>
        /// <param name="tenorStressedVar">SVar individuais por vértice</param>
        /// <param name="correlationMatrix">Matriz de correlação (ρi,j)</param>
        /// <returns>Valor do VaR Padrão "standardVar"</returns>
        public double GetStandardStressedVar(double[,] tenorStressedVar, double[,] correlationMatrix)
        {
            //validação:
            if (tenorStressedVar == null || correlationMatrix == null)
                throw new ArgumentNullException("As matrizes não podem ser nulas");

            int n = tenorStressedVar.GetLength(0);
            double StressedVarSum = 0;

            // VaRt Padrão = √(∑∑(VaRi,t * VaRj,t * ρi,j))
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    StressedVarSum += tenorStressedVar[i, i] * tenorStressedVar[j, j] * correlationMatrix[i, j];
                }
            }

            double standardStressedVar = Math.Sqrt(StressedVarSum);
            return standardStressedVar;
        }
    }
}





