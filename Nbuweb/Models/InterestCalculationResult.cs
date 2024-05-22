namespace NbuWeb.Models
{
    public class InterestCalculationResult
    {
        public int Month { get; set; }
        public decimal Interest { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal InterestBeforeTax { get; set; }
        public decimal TotalReinvested { get; set; }
    }
}
