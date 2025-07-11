using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Server.Data;
using LendingApp.Shared.Models;

namespace LendingApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoansController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
    {
        return await _context.Loans.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Loan>> GetLoan(Guid id)
    {
        var loan = await _context.Loans.FindAsync(id);
        return loan is null ? NotFound() : loan;
    }

    [HttpPost]
    public async Task<ActionResult<Loan>> CreateLoan(Loan loan)
    {
        ApplyLoanCalculations(loan);

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLoan(Guid id, Loan loan)
    {
        if (id != loan.Id) return BadRequest();

        ApplyLoanCalculations(loan);
        _context.Entry(loan).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/payment-update")]
    public async Task<IActionResult> UpdateLoanPayment(Guid id, Loan updatedLoan)
    {
        if (id != updatedLoan.Id)
            return BadRequest();

        var existingLoan = await _context.Loans.FindAsync(id);
        if (existingLoan == null)
            return NotFound();

        // Only update specific fields related to payments
        existingLoan.RemainingBalance = updatedLoan.RemainingBalance;
        existingLoan.PaymentsMade = updatedLoan.PaymentsMade;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoan(Guid id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan is null) return NotFound();

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] LoanStatus newStatus)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null) return NotFound();

        if (loan.Status != newStatus)
        {
            if (newStatus == LoanStatus.Approved && loan.Status == LoanStatus.Pending)
            {
                var approvalDate = DateTime.Now;
                loan.ApprovalDate = approvalDate;
                loan.StartDate = approvalDate.AddMonths(1);
                loan.EndDate = loan.StartDate?.AddMonths(loan.TermInMonths);

                ApplyLoanCalculations(loan);
            }

            loan.Status = newStatus;

            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    //Updated Loan Calculation
    private void ApplyLoanCalculations(Loan loan)
    {
        int paymentsPerMonth = loan.PaymentFrequency switch
        {
            PaymentFrequency.Daily => 30,
            PaymentFrequency.Weekly => 4,
            PaymentFrequency.Monthly => 1,
            _ => 1
        };

        loan.ExpectedPayments = paymentsPerMonth * loan.TermInMonths;

        double totalInterest = loan.PrincipalAmount * (loan.MonthlyInterestRate / 100.0) * loan.TermInMonths;
        loan.TotalAmount = loan.PrincipalAmount + totalInterest;

        loan.InstallmentAmount = loan.TotalAmount / loan.ExpectedPayments;
        loan.RemainingBalance = loan.TotalAmount;
        loan.PaymentsMade = 0;
    }

    [HttpPut("{loanId}/assign-collector")]
    public async Task<IActionResult> AssignCollector(Guid loanId, [FromBody] Guid collectorId)
    {
        var loan = await _context.Loans.FindAsync(loanId);
        if (loan == null)
            return NotFound();

        loan.CollectorId = collectorId;
        await _context.SaveChangesAsync();

        return NoContent();
    }

}
