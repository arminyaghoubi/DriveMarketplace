using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public DbSet<Auction> Auctions { get; set; }

    public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
