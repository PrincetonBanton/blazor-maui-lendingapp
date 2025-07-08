using Microsoft.EntityFrameworkCore;
using LendingApp.Shared.Models;

namespace LendingApp.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Ledger> Ledgers => Set<Ledger>();
    public DbSet<Collector> Collectors => Set<Collector>();

}
