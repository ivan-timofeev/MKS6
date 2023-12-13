using Microsoft.EntityFrameworkCore;
using MKS6_Orders.Models.DomainModels;

namespace MKS6_Orders.Data;

public sealed class OrdersDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatusHistoryItem> OrderStatusHistoryItems { get; set; }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
        Orders = Set<Order>();
        OrderStatusHistoryItems = Set<OrderStatusHistoryItem>();

        Database.EnsureCreated();
    }
}
