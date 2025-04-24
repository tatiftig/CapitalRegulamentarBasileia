using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeiroProjeto.REGULAMENTAR
{

namespace StandardizedApproachRegulatoryCapitalForForeignExchangeCalculation

{

 

    public class ForeignExchangeRisk

    {

        // Propriedades que realmente precisam ser armazenadas

        public double RiskWeightedAsset { get; set; }

        public double CapitalRequirement { get; set; }

    };

 

        public class Exposure

    {

        public int CurrencyId { get; set; }

        public double Value { get; set; }

        public int LocationId { get; set; }

    };

 

    public class ForeignExchangeRiskCalculation

    {

        // Classe interna para representar exposições

        // Método principal de cálculo

        public ForeignExchangeRisk GetForeignExchangeRisk(List<Exposure> exposures, double rwaFactor, double minExcessForeignExchangeFactor, double minBetweenNetForeignExchangeFactor)

 

        {

            double foreignExchangeExposureFactor = GetForeignExchangeExposureFactor(double minExcessForeignExchangeFactor, double minBetweenNetForeignExchangeFactor,
                double patrimoniodeReferencia);

            double foreignExchangeExposure = GetForeignExchangeExposure(exposures, minExcessForeignExchangeFactor, minBetweenNetForeignExchangeFactor);

 

            double capitalRequirement =

                foreignExchangeExposurefactor * foreignExchangeExposure

            ;

 

            ForeignExchangeRisk foreignExchangeRisk = new ForeignExchangeRisk()

            {

                RiskWeightedAsset = capitalRequirement / rwaFactor,

                CapitalRequirement = capitalRequirement

            };

 

            return foreignExchangeRisk;

        }

 

        private static double GetForeignExchangeExposure(List<Exposure> exposures, double minExcessForeignExchangeFactor, double minBetweenNetForeignExchangeFactor)

        {

            double sumNetForeignExchangeExposure = GetSumNetForeignExchangeExposure(exposures);

            double minExcessForeignExchangeExposure = GetMinExcessForeignExchangeExposure(exposures);

            double minBetweenNetForeignExchangeExposure = GetMinBetweenNetForeignExchangeExposure(exposures);

 

            double foreignExchangeExposure = sumNetForeignExchangeExposure +

                        minExcessForeignExchangeFactor * minExcessForeignExchangeExposure +

                        minBetweenNetForeignExchangeFactor * minBetweenNetForeignExchangeExposure;

 

            return foreignExchangeExposure;

        }

 

        // equivale a Exp1 = min {soma do ABS do excesso de exposição comprada VERSUS soma do ABS do excesso de exposição vendida}

        private static double GetSumNetForeignExchangeExposure(List<Exposure> exposures)

        {

            // Agrupa as operações por moeda e por "sinal" da operação (positiva versus negativa)

            // double sumNetForeignExchangeExposure = 0;

 

            // criar lista de moedas

            // List<int> currencies = exposures.Select(s => s.CurrencyId).Distinct().ToList();

 

            // foreach (int currency in currencies)

            // {

            //     sumNetForeignExchangeExposure +=

            //         exposures

            //         .Where(w => w.CurrencyId == currency)

            //         .Sum(s => s.Value)

            //     ;

            // };

 

            double result = exposures

                .GroupBy(g => g.CurrencyId)

                .Select(g => new

                {

                    Currency = g.Key,

                    // Value = Math.Abs(g.Where(w => w.CurrencyId == g.Key) .Sum(s => s.Value)),

                    Value = Math.Abs(g.Sum(s => s.Value)),

                })

                .Sum(s => s.Value)

            ;

 

            // Retorna a soma total das exposições líquidas absolutas

            return result;

        }

 

        // equivale a Exp2 = min {soma do ABS do excesso de exposição comprada VERSUS soma do ABS do excesso de exposição vendida}

        private static double GetMinExcessForeignExchangeExposure(List<Exposure> exposures)

        {

            // Declara Dicionário chamado groupedExposures com chave int e duas saídas como valor.

            Dictionary<int , (double TotalPositive, double TotalNegative)> groupedExposures = new Dictionary<int, (double, double)>();

 

            // preenche o dicionário agrupando as exposições por CurrencyId e armazenando os totais positivos e negativos.

            foreach (Exposure exposure in exposures)

            {

                int currencyId = exposure.CurrencyId;

                double value = exposure.Value;

 

                if (!groupedExposures.ContainsKey(currencyId))

                {

                    groupedExposures[currencyId] = (0, 0);

                }

 

                if (value > 0)

                {

                    groupedExposures[currencyId] = (groupedExposures[currencyId].TotalPositive + value, groupedExposures[currencyId].TotalNegative);

                }

                else if (value < 0)

                {

                    groupedExposures[currencyId] = (groupedExposures[currencyId].TotalPositive, groupedExposures[currencyId].TotalNegative + value);

                }

            }

 

            // Calcula o excesso de exposição comprada para "cada moeda i"

            double excessBought = 0;

            foreach (KeyValuePair<int, (double TotalPositive, double TotalNegative)> entry in groupedExposures)

            {

                if (Math.Abs(entry.Value.TotalPositive) > Math.Abs(entry.Value.TotalNegative))

                {

                    excessBought += Math.Abs(entry.Value.TotalPositive) - Math.Abs(entry.Value.TotalNegative);

                }

            }

 

            // Calcula o excesso de exposição vendida para "cada moeda i"

            double excessSold = 0;

            foreach (KeyValuePair<int, (double TotalPositive, double TotalNegative)> entry in groupedExposures)

            {

                if (Math.Abs(entry.Value.TotalNegative) > Math.Abs(entry.Value.TotalPositive))

                {

                    excessSold += Math.Abs(entry.Value.TotalNegative) - Math.Abs(entry.Value.TotalPositive);

                }

            }

 

            // Calcula o mínimo entre os excessos

            double minExcessForeignExchangeExposure = Math.Min(excessBought, excessSold);

 

            return minExcessForeignExchangeExposure;

 

        }

 

        // Exp3 = min {soma da exposição líquida absoluta para cada moeda no Brasil VERSUS soma da exposição líquida absoluta para cada moeda no exterior}

        private static double GetMinBetweenNetForeignExchangeExposure(List<Exposure> exposures)

 

        {

            // Declara o dicionário "groupedExposures" = estrutura de dados que armazena pares chave-valor

            Dictionary<(int CurrencyId, int LocationId), double>

            groupedExposures = new Dictionary<(int CurrencyId, int LocationId), double>();

 

            // Preenche o Dictionary com as exposições por moeda e localização (obtém netExposure para cada moeda e localização)  

            foreach (Exposure exposure in exposures)

            {

                (int, int) key = (exposure.CurrencyId, exposure.LocationId);

 

                if (!groupedExposures.ContainsKey(key))

                {

                    groupedExposures[key] = 0.0;

                }

                groupedExposures[key] += exposure.Value;

            }

 

            // Transforma os valores do dicionário em lista de valores absolutos

            foreach (KeyValuePair<(int CurrencyId, int LocationId), double> valuePair in groupedExposures.ToList())

            {

                groupedExposures[valuePair.Key] = Math.Abs(valuePair.Value);

            }

 

            // Inicializa as variáveis para armazenar as somas dos valores absolutos para cada localidade

            double sumTradedInBrazil = 0;

            double sumTradedAbroad = 0;

 

            // Itera sobre as entradas do dicionário groupedExposures

            foreach (KeyValuePair<(int CurrencyId, int LocationId), double> pair in groupedExposures)

            {

                // a sintaxe abaixo define/extrai/deixa claro a que estamos nos referindo quando citamos as variáveis "locationId" e "value"

                int locationId = pair.Key.LocationId;

                double value = pair.Value;

 

                // Soma os valores absolutos para cada localidade

                if (locationId == 1)

                {

                    sumTradedInBrazil += value;

                }

                else if (locationId == 2)

                {

                    sumTradedAbroad += value;

                }

            }

 

            // Encontra o menor valor dentre as localidades

            double minBetweenNetForeignExchangeExposure = Math.Min(sumTradedInBrazil, sumTradedAbroad);

 
 
    return minBetweenNetForeignExchangeExposure;

        }

 

        // método para cálculo do foreignExchangeExposurefactor (F", na Circular3641/13 )

        private static double GetforeignExchangeExposurefactor(List<Exposure> exposures, double minExcessForeignExchangeFactor, double minBetweenNetForeignExchangeFactor,

                double patrimoniodeReferencia)

        {

            // chama o método GetForeignExchangeExposure ara calculo do foreignExchangeExposure que será usado adiante

            double foreignExchangeExposure = GetForeignExchangeExposure(exposures, minExcessForeignExchangeFactor, minBetweenNetForeignExchangeFactor);

 

            // Calcula a expressão expression

            double expression = 0.22 + 8.5 * Math.Pow(foreignExchangeExposure / patrimoniodeReferencia, 2);

 

            // Calcula o mínimo entre a expressão e 1

            double foreignExchangeExposureFactor = Math.Min(expression, 1);

 

            return foreignExchangeExposureFactor;

        }

 

    }

 

};

