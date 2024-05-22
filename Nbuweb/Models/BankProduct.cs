// Models/BankProduct.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NbuWeb.Models
{
    public class BankProduct
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool FixedTerm { get; set; }
        public int? MaxTermLength { get; set; }
        public bool FlexTerm { get; set; }
        public string InterestPaymentInterval { get; set; }
        public bool CanReinvest { get; set; }

        public ICollection<InterestDetail> InterestDetails { get; set; }
        public ICollection<DepositDetail> DepositDetails { get; set; }
    }
}
