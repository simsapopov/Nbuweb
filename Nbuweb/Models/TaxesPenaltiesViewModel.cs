namespace NbuWeb.Models
{
    public class TaxesPenaltiesViewModel
    {
        public decimal TotalInterest { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string CurrencySymbol { get; set; }
    }
}
