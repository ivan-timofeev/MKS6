using Microsoft.EntityFrameworkCore;
using MKS6_Orders.Data;
using MKS6_Orders.Models.DomainModels;
using MKS6_Orders.Services;

namespace MKS6_Orders.BackgroundServices;

public class StartProcessingOfCreatedOrdersBackgroundService : BackgroundService
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;
    private readonly IOrdersManagementService _ordersManagementService;

    public StartProcessingOfCreatedOrdersBackgroundService(
        IDbContextFactory<OrdersDbContext> dbContextFactory,
        IOrdersManagementService ordersManagementService)
    {
        _dbContextFactory = dbContextFactory;
        _ordersManagementService = ordersManagementService;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Task.CompletedTask;
            
                ExecuteInternal();
                Thread.Sleep(millisecondsTimeout: 60_000);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private void ExecuteInternal()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var orders = dbContext
            .Orders
            .Include(o => o.OrderStatusHistory)
            .Where(o => o.OrderStatusHistory.Count == 1)
            .Where(o => o.OrderStatusHistory.Single().OrderStatus == OrderStatusEnum.Created)
            .Select(o => o.Id)
            .ToArray();

        foreach (var orderId in orders)
        {
            _ordersManagementService.MakeRequestToReserveOrderItems(orderId);
        }
    }
}
