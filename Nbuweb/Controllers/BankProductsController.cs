using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NbuWeb.Data;
using NbuWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NbuWeb.Controllers
{
    public class BankProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(decimal? amount, int? termLength, string currency)
        {
            var query = _context.BankProducts.Include(p => p.InterestDetails).AsQueryable();

            if (amount.HasValue)
            {
                query = query.Where(p => p.MinAmount <= amount.Value && p.MaxAmount >= amount.Value);
            }

            if (termLength.HasValue)
            {
                query = query.Where(p => p.InterestDetails.Any(d => d.TermLengthInMonths == termLength.Value));
            }

            if (!string.IsNullOrEmpty(currency))
            {
                query = query.Where(p => p.InterestDetails.Any(d =>
                    (currency == "USD" && d.UsdInterestRate > 0) ||
                    (currency == "EUR" && d.EurInterestRate > 0) ||
                    (currency == "BGN" && d.BgnInterestRate > 0)));
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankProduct = await _context.BankProducts
                .Include(p => p.InterestDetails)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (bankProduct == null)
            {
                return NotFound();
            }

            var availableTermLengths = bankProduct.InterestDetails
                .Select(d => d.TermLengthInMonths)
                .OrderBy(t => t)
                .ToList();

            ViewBag.AvailableTermLengths = availableTermLengths;
            ViewBag.CanReinvest = bankProduct.CanReinvest;

            return View(bankProduct);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductType,MinAmount,MaxAmount,FixedTerm,MaxTermLength,FlexTerm,InterestPaymentInterval,CanReinvest")] BankProduct bankProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bankProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bankProduct);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankProduct = await _context.BankProducts.FindAsync(id);
            if (bankProduct == null)
            {
                return NotFound();
            }
            return View(bankProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductType,MinAmount,MaxAmount,FixedTerm,MaxTermLength,FlexTerm,InterestPaymentInterval,CanReinvest")] BankProduct bankProduct)
        {
            if (id != bankProduct.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bankProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankProductExists(bankProduct.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bankProduct);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankProduct = await _context.BankProducts
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (bankProduct == null)
            {
                return NotFound();
            }

            return View(bankProduct);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bankProduct = await _context.BankProducts.FindAsync(id);
            _context.BankProducts.Remove(bankProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BankProductExists(int id)
        {
            return _context.BankProducts.Any(e => e.ProductId == id);
        }

        [HttpPost]
        public async Task<IActionResult> CalculateInterestReinvested(int id, decimal depositAmount, string currency, int termLength, decimal? monthlyDeposit = null)
        {
            var product = await _context.BankProducts
                .Include(p => p.InterestDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Bank product not found.");
            }

            var interestDetail = product.InterestDetails
                .Where(d => d.TermLengthInMonths <= termLength)
                .OrderByDescending(d => d.TermLengthInMonths)
                .FirstOrDefault();

            if (interestDetail == null)
            {
                return NotFound("Interest details not found for the specified term length.");
            }

            decimal interestRate = 0;
            string currencySymbol = "";
            switch (currency)
            {
                case "USD":
                    interestRate = interestDetail.UsdInterestRate;
                    currencySymbol = "$";
                    break;
                case "EUR":
                    interestRate = interestDetail.EurInterestRate;
                    currencySymbol = "€";
                    break;
                case "BGN":
                    interestRate = interestDetail.BgnInterestRate;
                    currencySymbol = "лв";
                    break;
                default:
                    return BadRequest("Invalid currency.");
            }

            decimal totalAmount = depositAmount;
            decimal totalInterestBeforeTax = 0;
            decimal totalInterestAfterTax = 0;
            List<InterestCalculationResult> results = new List<InterestCalculationResult>();
            int paymentInterval = GetPaymentIntervalInMonths(product.InterestPaymentInterval);
            decimal accumulatedInterest = 0;

            for (int month = 1; month <= termLength; month++)
            {
                if (monthlyDeposit.HasValue)
                {
                    totalAmount += monthlyDeposit.Value;
                }
                decimal interest = totalAmount * (interestRate / 100) / 12;
                decimal interestAfterTax = interest * (1 - (interestDetail.GovernmentTaxRate / 100));
                accumulatedInterest += interestAfterTax;

                if (month % paymentInterval == 0)
                {
                    totalAmount += accumulatedInterest; 
                    accumulatedInterest = 0;
                }

                totalInterestBeforeTax += interest;
                totalInterestAfterTax += interestAfterTax;

                results.Add(new InterestCalculationResult
                {
                    Month = month,
                    InterestBeforeTax = interest,
                    Interest = interestAfterTax,
                    TotalAmount = totalAmount,
                    TotalInterestEarned = accumulatedInterest
                });
            }

           
            if (accumulatedInterest > 0)
            {
                totalAmount += accumulatedInterest;
            }

            var viewModel = new CalculationResultViewModel
            {
                Results = results,
                TotalAmount = totalAmount,
                Reinvest = true,
                CurrencySymbol = currencySymbol,
                InterestPaymentInterval = product.InterestPaymentInterval, 
                TotalInterestBeforeTax = totalInterestBeforeTax,
                TotalInterestAfterTax = totalInterestAfterTax,
                TotalTaxAmount = totalInterestBeforeTax - totalInterestAfterTax
            };

            return PartialView("_CalculationResultReinvested", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CalculateInterestNonReinvested(int id, decimal depositAmount, string currency, int termLength, decimal? monthlyDeposit = null)
        {
            var product = await _context.BankProducts
                .Include(p => p.InterestDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Bank product not found.");
            }

            var interestDetail = product.InterestDetails
                .Where(d => d.TermLengthInMonths <= termLength)
                .OrderByDescending(d => d.TermLengthInMonths)
                .FirstOrDefault();

            if (interestDetail == null)
            {
                return NotFound("Interest details not found for the specified term length.");
            }

            decimal interestRate = 0;
            string currencySymbol = "";
            switch (currency)
            {
                case "USD":
                    interestRate = interestDetail.UsdInterestRate;
                    currencySymbol = "$";
                    break;
                case "EUR":
                    interestRate = interestDetail.EurInterestRate;
                    currencySymbol = "€";
                    break;
                case "BGN":
                    interestRate = interestDetail.BgnInterestRate;
                    currencySymbol = "лв";
                    break;
                default:
                    return BadRequest("Invalid currency.");
            }

            decimal totalAmount = depositAmount;
            decimal totalInterestPaid = 0;
            decimal totalInterestBeforeTax = 0;
            decimal totalInterestAfterTax = 0;
            decimal accumulatedInterest = 0;
            List<InterestCalculationResult> results = new List<InterestCalculationResult>();
            int paymentInterval = GetPaymentIntervalInMonths(product.InterestPaymentInterval);

            for (int month = 1; month <= termLength; month++)
            {
                if (monthlyDeposit.HasValue)
                {
                    totalAmount += monthlyDeposit.Value;
                }
                decimal interest = totalAmount * (interestRate / 100) / 12;
                decimal interestAfterTax = interest * (1 - (interestDetail.GovernmentTaxRate / 100));
                accumulatedInterest += interestAfterTax;

                if (month % paymentInterval == 0)
                {
                    totalInterestPaid += accumulatedInterest;
                    results.Add(new InterestCalculationResult
                    {
                        Month = month,
                        InterestBeforeTax = interest,
                        Interest = accumulatedInterest,
                        TotalAmount = totalAmount
                    });
                    accumulatedInterest = 0;
                }
                else
                {
                    results.Add(new InterestCalculationResult
                    {
                        Month = month,
                        InterestBeforeTax = interest,
                        Interest = 0,
                        TotalAmount = totalAmount
                    });
                }

                totalInterestBeforeTax += interest;
                totalInterestAfterTax += interestAfterTax;
            }

           
            if (accumulatedInterest > 0)
            {
                totalInterestPaid += accumulatedInterest;
                results.Last().Interest += accumulatedInterest;
            }

            var viewModel = new CalculationResultViewModel
            {
                Results = results,
                TotalAmount = totalAmount,
                TotalInterestEarned = totalInterestPaid,
                Reinvest = false,
                CurrencySymbol = currencySymbol,
                InterestPaymentInterval = product.InterestPaymentInterval,
                TotalInterestBeforeTax = totalInterestBeforeTax,
                TotalInterestAfterTax = totalInterestAfterTax,
                TotalTaxAmount = totalInterestBeforeTax - totalInterestAfterTax
            };

            return PartialView("_CalculationResultNonReinvested", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CalculateRecurringDeposit(int id, decimal depositAmount, string currency, int termLength, decimal monthlyDeposit, bool reinvest = false)
        {
            var product = await _context.BankProducts
                .Include(p => p.InterestDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Bank product not found.");
            }

            var interestDetails = product.InterestDetails
                .Where(d => d.TermLengthInMonths <= termLength)
                .OrderByDescending(d => d.TermLengthInMonths)
                .FirstOrDefault();

            if (interestDetails == null)
            {
                return NotFound("Interest details not found for the specified term length.");
            }

            decimal interestRate = 0;
            string currencySymbol = "";
            switch (currency)
            {
                case "USD":
                    interestRate = interestDetails.UsdInterestRate;
                    currencySymbol = "$";
                    break;
                case "EUR":
                    interestRate = interestDetails.EurInterestRate;
                    currencySymbol = "€";
                    break;
                case "BGN":
                    interestRate = interestDetails.BgnInterestRate;
                    currencySymbol = "лв";
                    break;
                default:
                    return BadRequest("Invalid currency.");
            }

            decimal totalAmount = depositAmount;
            decimal totalInterestBeforeTax = 0;
            decimal totalInterestAfterTax = 0;
            List<InterestCalculationResult> results = new List<InterestCalculationResult>();
            int paymentInterval = GetPaymentIntervalInMonths(product.InterestPaymentInterval);
            decimal accumulatedInterest = 0;

            for (int month = 1; month <= termLength; month++)
            {
                totalAmount += monthlyDeposit;

                decimal interest = totalAmount * (interestRate / 100) / 12;
                decimal interestAfterTax = interest * (1 - (interestDetails.GovernmentTaxRate / 100));
                accumulatedInterest += interestAfterTax;

                totalInterestBeforeTax += interest;
                totalInterestAfterTax += interestAfterTax;

                if (reinvest && month % paymentInterval == 0)
                {
                    totalAmount += accumulatedInterest;
                    accumulatedInterest = 0;
                }

                results.Add(new InterestCalculationResult
                {
                    Month = month,
                    InterestBeforeTax = interest,
                    Interest = interestAfterTax,
                    TotalAmount = totalAmount,
                    TotalInterestEarned = accumulatedInterest
                });
            }

            if (reinvest && accumulatedInterest > 0)
            {
                totalAmount += accumulatedInterest;
            }

            var viewModel = new CalculationResultViewModel
            {
                Results = results,
                TotalAmount = totalAmount,
                Reinvest = reinvest,
                CurrencySymbol = currencySymbol,
                InterestPaymentInterval = product.InterestPaymentInterval,
                TotalInterestBeforeTax = totalInterestBeforeTax,
                TotalInterestAfterTax = totalInterestAfterTax,
                TotalTaxAmount = totalInterestBeforeTax - totalInterestAfterTax
            };

            if (reinvest)
            {
                return PartialView("_CalculationResultReinvested", viewModel);
            }
            else
            {
                viewModel.TotalInterestEarned = results.Sum(r => r.Interest);
                return PartialView("_CalculationResultNonReinvested", viewModel);
            }
        }



        [HttpPost]
        public async Task<IActionResult> CalculateTaxesPenalties(int id, decimal depositAmount, string currency, int termLength, decimal withdrawalAmount)
        {
            var product = await _context.BankProducts
                .Include(p => p.InterestDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Bank product not found.");
            }

            var interestDetail = product.InterestDetails
                .Where(d => d.TermLengthInMonths <= termLength)
                .OrderByDescending(d => d.TermLengthInMonths)
                .FirstOrDefault();

            if (interestDetail == null)
            {
                return NotFound("Interest details not found for the specified term length.");
            }

            decimal interestRate = 0;
            decimal taxRate = interestDetail.GovernmentTaxRate;
            decimal penaltyRate = interestDetail.EarlyWithdrawalPenaltyRate;
            string currencySymbol = "";
            switch (currency)
            {
                case "USD":
                    interestRate = interestDetail.UsdInterestRate;
                    currencySymbol = "$";
                    break;
                case "EUR":
                    interestRate = interestDetail.EurInterestRate;
                    currencySymbol = "€";
                    break;
                case "BGN":
                    interestRate = interestDetail.BgnInterestRate;
                    currencySymbol = "лв";
                    break;
                default:
                    return BadRequest("Invalid currency.");
            }

            decimal totalInterest = depositAmount * (interestRate / 100) * (termLength / 12m);
            decimal taxAmount = totalInterest * (taxRate / 100);
            decimal penaltyAmount = withdrawalAmount * (penaltyRate / 100);

            return PartialView("_TaxesPenaltiesResult", new TaxesPenaltiesViewModel
            {
                TotalInterest = totalInterest,
                TaxAmount = taxAmount,
                PenaltyAmount = penaltyAmount,
                CurrencySymbol = currencySymbol
            });
        }

        private int GetPaymentIntervalInMonths(string interval)
        {
            return interval.ToLower() switch
            {
                "monthly" => 1,
                "quarterly" => 3,
                "semiannually" => 6,
                "annually" => 12,
                _ => 1,
            };
        }
    }
}
