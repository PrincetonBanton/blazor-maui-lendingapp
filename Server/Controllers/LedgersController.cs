using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Server.Data;
using LendingApp.Shared.Models;

namespace LendingApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgersController : ControllerBase
{
    private readonly AppDbContext _context;

    public LedgersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ledger>>> GetLedgers()
    {
        return await _context.Ledgers.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Ledger>> GetLedger(Guid id)
    {
        var ledger = await _context.Ledgers.FindAsync(id);
        return ledger is null ? NotFound() : ledger;
    }

    [HttpPost]
    public async Task<ActionResult<Ledger>> CreateLedger(Ledger ledger)
    {
        _context.Ledgers.Add(ledger);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLedger), new { id = ledger.Id }, ledger);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLedger(Guid id, Ledger ledger)
    {
        if (id != ledger.Id) return BadRequest();

        _context.Entry(ledger).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLedger(Guid id)
    {
        var ledger = await _context.Ledgers.FindAsync(id);
        if (ledger is null) return NotFound();

        _context.Ledgers.Remove(ledger);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("pay")]
    public async Task<IActionResult> MarkPayment([FromBody] PaymentRequest request)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == request.LoanId);
        if (loan == null)
            return NotFound("Loan not found");

        if (loan.PaymentsMade >= loan.ExpectedPayments)
            return BadRequest("All payments already made.");

        // Increment loan state
        loan.PaymentsMade += 1;
        loan.RemainingBalance -= loan.InstallmentAmount;

        // Create ledger record without CollectorName
        var ledger = new Ledger
        {
            LoanId = loan.Id,
            CollectorId = request.CollectorId ?? Guid.Empty, // Optional fallback
            PaymentDate = request.PaymentDate,
            PaymentAmount = loan.InstallmentAmount,
            PaymentNumber = loan.PaymentsMade,
            Remarks = "Payment recorded via UI"
        };

        _context.Ledgers.Add(ledger);
        await _context.SaveChangesAsync();

        return Ok();
    }


    public class PaymentRequest
    {
        public Guid LoanId { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid? CollectorId { get; set; } // Add this
    }
}
