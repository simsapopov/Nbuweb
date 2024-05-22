// Models/DepositDetail.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NbuWeb.Models
{
    public class DepositDetail
    {
        [Key]
        public int DepositDetailId { get; set; }
        public int ProductId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public int TermLength { get; set; }
        public decimal InterestRate { get; set; }

        [ForeignKey("ProductId")]
        public BankProduct BankProduct { get; set; }
    }
}
