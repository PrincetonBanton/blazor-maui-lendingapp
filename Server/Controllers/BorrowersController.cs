using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Server.Data;
using LendingApp.Shared.Models;

namespace LendingApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowersController : ControllerBase
{
    private readonly AppDbContext _context;

    public BorrowersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Borrower>>> GetBorrowers()
    {
        return await _context.Borrowers.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Borrower>> GetBorrower(Guid id)
    {
        var borrower = await _context.Borrowers.FindAsync(id);
        return borrower is null ? NotFound() : borrower;
    }

    [HttpPost]
    public async Task<ActionResult<Borrower>> CreateBorrower(Borrower borrower)
    {
        _context.Borrowers.Add(borrower);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBorrower), new { id = borrower.Id }, borrower);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBorrower(Guid id, Borrower borrower)
    {
        if (id != borrower.Id) return BadRequest();

        _context.Entry(borrower).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBorrower(Guid id)
    {
        var borrower = await _context.Borrowers.FindAsync(id);
        if (borrower is null) return NotFound();

        _context.Borrowers.Remove(borrower);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
