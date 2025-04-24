using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public static partial class StandardizedApproachRegulatoryCapitalForDrcCalculation
    {
        public class StandardizedApproachRegulatoryCapitalForDrcOutput
        {
            public decimal DrcNsecCorporate { get; set; }
            public decimal DrcNsecSovereign { get; set; }
            public decimal DrcNsecLocalGovernmentAndMunicipality { get; set; }
            public decimal DrcSec { get; set; }
            public decimal DrcCtp { get; set; }
            public decimal RiskWeightedAsset { get; set; }
            public decimal CapitalRequirement { get; set; }
        }

        public class Exposure
        {
            public int IssuerCategoryId { get; set; }
            public int TenorInBusinessDay { get; set; }
            public decimal Result { get; set; }
            public decimal RiskWeight { get; set; }
            public decimal LossGivenDefault { get; set; }
        }

        public class CapitalRequirementForDrcNsec
        {
            public int issuerCategoryId { get; set; }
            public decimal result { get; set; }
        }

        public class StandardizedApproachRegulatoryCapitalForDrcInput
        {
            public List<Exposure> exposures { get; set; }
            public decimal factorForTotalCapitalRequirement { get; set; }
        }

        public static StandardizedApproachRegulatoryCapitalForDrcOutput GetStandardizedApproachRegulatoryCapitalForDrcCalculation(StandardizedApproachRegulatoryCapitalForDrcInput standardizedApproachRegulatoryCapitalForDrcInput)
        {
            // preencher a lista de categorias do emissor

            List<int> issuerCategorieIds = standardizedApproachRegulatoryCapitalForDrcInput.exposures.Where(w => w.IssuerCategoryId != 0).Select(s => s.IssuerCategoryId).Distinct().ToList();


            // criar a lista de capital para DRC-NSEC

            List<CapitalRequirementForDrcNsec> capitalRequirementForDrcNsecs = new List<CapitalRequirementForDrcNsec>();

            // para cada categoria do emissor

            foreach (int issuerCategoryId in issuerCategorieIds)
            {
                // declarar as variáveis de Net JTD (Jump-to-Default)
                decimal netLongJtd = 0;
                decimal netShortJtd = 0;
                decimal netLongJtdByRw = 0;
                decimal netShortJtdByRw = 0;

                // preencher a lista de exposições

                List<Exposure> exposures = standardizedApproachRegulatoryCapitalForDrcInput.exposures.Where(w => w.IssuerCategoryId == issuerCategoryId).ToList();

                // para cada exposição

                foreach (Exposure exposure in exposures)
                {
                    decimal tenorInCalendarDay = (decimal)exposure.TenorInBusinessDay * (decimal)30 / (decimal)21;

                    decimal maturityWeighting = (tenorInCalendarDay <= 90 ? 0.25M : (tenorInCalendarDay <= 360 ? tenorInCalendarDay / 360 : 1));

                    decimal adjust = 0;
                    decimal netJtd = Math.Max(exposure.LossGivenDefault * exposure.Result + adjust, 0) * maturityWeighting;
                    if (exposure.Result > 0)
                    {
                        netLongJtd += netJtd;
                        netLongJtdByRw += netJtd * exposure.RiskWeight;
                    }
                    else
                    {
                        netShortJtd += Math.Abs(netJtd);
                        netShortJtdByRw += netJtd * exposure.RiskWeight;
                    }
                }

                // calcular HBR (Hedge Benefit Ratio)

                decimal hedgeBenefitRatio = netLongJtd / (netLongJtd + netShortJtd);

                // calcular o capital desta categoria do emissor
                capitalRequirementForDrcNsecs.Add(new CapitalRequirementForDrcNsec()
                {
                    issuerCategoryId = issuerCategoryId,
                    result = Math.Max(netLongJtdByRw - hedgeBenefitRatio * netShortJtdByRw, 0),
                });
            }

            // somar os resultados de capital
            decimal capitalRequirementForDrcNsecCorporate = capitalRequirementForDrcNsecs.Where(w => w.issuerCategoryId == IssuerCategory.CORPORATE).Sum(s => s.result);

            decimal capitalRequirementForDrcNsecSovereign = capitalRequirementForDrcNsecs.Where(w => w.issuerCategoryId == IssuerCategory.SOVEREIGN).Sum(s => s.result);

            decimal capitalRequirementForDrcNsecLocalGovernmentAndMunicipality = capitalRequirementForDrcNsecs.Where(w => w.issuerCategoryId == IssuerCategory.LOCAL_GOVERNMENT_AND_MINUCIPALITY).Sum(s => s.result);

            decimal capitalRequirementForDrcSec = 0;

            decimal capitalRequirementForDrcCtp = 0;

            decimal capitalRequirement =
               capitalRequirementForDrcNsecCorporate
               + capitalRequirementForDrcNsecSovereign
               + capitalRequirementForDrcNsecLocalGovernmentAndMunicipality
               + capitalRequirementForDrcSec
               + capitalRequirementForDrcCtp
           ;

            // calcular o RWA

            decimal riskWeightedAsset = capitalRequirement / standardizedApproachRegulatoryCapitalForDrcInput.factorForTotalCapitalRequirement;

            // preencher a saída do Capital Regulamentar

            StandardizedApproachRegulatoryCapitalForDrcOutput standardizedApproachRegulatoryCapitalForDrcOutput = new StandardizedApproachRegulatoryCapitalForDrcOutput()

            {
                DrcNsecCorporate = capitalRequirementForDrcNsecCorporate,
                DrcNsecSovereign = capitalRequirementForDrcNsecSovereign,
                DrcNsecLocalGovernmentAndMunicipality = capitalRequirementForDrcNsecLocalGovernmentAndMunicipality,
                DrcSec = capitalRequirementForDrcSec,
                DrcCtp = capitalRequirementForDrcCtp,
                RiskWeightedAsset = riskWeightedAsset,
                CapitalRequirement = capitalRequirement,
            };

            // fim

            return standardizedApproachRegulatoryCapitalForDrcOutput;
        }
    }
}
