namespace NbuWeb.Models
{
    public class InterestDetail
    {
        public int InterestDetailId { get; set; }
        public int TermLengthInMonths { get; set; }
        public decimal UsdInterestRate { get; set; }
        public decimal EurInterestRate { get; set; }
        public decimal BgnInterestRate { get; set; }
        public decimal GovernmentTaxRate
        {
            get; set;
        }
        public decimal EarlyWithdrawalPenaltyRate { get; set; }
        public int BankProductId { get; set; }

        public BankProduct BankProduct { get; set; }
    }
}
