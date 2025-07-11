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

    [HttpGet("loan/{loanId}")]
    public async Task<ActionResult<IEnumerable<Ledger>>> GetLedgersForLoan(Guid loanId)
    {
        var ledgers = await _context.Ledgers
            .Where(l => l.LoanId == loanId)
            .OrderBy(l => l.PaymentNumber)
            .ToListAsync();

        return ledgers;
    }
    [HttpPost]
    public async Task<ActionResult<Ledger>> PostLedger(Ledger ledger)
    {
        _context.Ledgers.Add(ledger);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLedgersForLoan), new { loanId = ledger.LoanId }, ledger);
    }

}
