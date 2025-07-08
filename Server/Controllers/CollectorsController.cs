using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LendingApp.Server.Data;
using LendingApp.Shared.Models;

namespace LendingApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CollectorsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Collector>>> GetCollectors()
    {
        return await _context.Collectors.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Collector>> GetCollector(Guid id)
    {
        var collector = await _context.Collectors.FindAsync(id);
        return collector is null ? NotFound() : collector;
    }

    [HttpPost]
    public async Task<ActionResult<Collector>> CreateCollector(Collector collector)
    {
        _context.Collectors.Add(collector);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCollector), new { id = collector.Id }, collector);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCollector(Guid id, Collector collector)
    {
        if (id != collector.Id) return BadRequest();
        _context.Entry(collector).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollector(Guid id)
    {
        var collector = await _context.Collectors.FindAsync(id);
        if (collector is null) return NotFound();

        _context.Collectors.Remove(collector);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
