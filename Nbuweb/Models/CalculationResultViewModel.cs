using System.Collections.Generic;

namespace NbuWeb.Models
{
    public class CalculationResultViewModel
    {
        public List<InterestCalculationResult> Results { get; set; }
        public decimal TotalAmount { get; set; }
        public bool Reinvest { get; set; }
        public string CurrencySymbol { get; set; }
        public string InterestPaymentInterval { get; set; }
        public decimal TotalInterestBeforeTax { get; set; }
        public decimal TotalInterestAfterTax { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalInterestEarned { get; set; }
    }
}
