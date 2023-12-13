using Microsoft.EntityFrameworkCore;
using MKS6_Items.Models;

namespace MKS6_Items.Data;

public sealed class ItemsDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    public ItemsDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
        Items = Set<Item>();
    }
}
