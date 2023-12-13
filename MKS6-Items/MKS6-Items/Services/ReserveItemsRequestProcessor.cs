using System.Data;
using MKS6_Items.Data;
using MKS6_Items.Models.DataTransferObjects.CreateOrder;
using Microsoft.EntityFrameworkCore;

namespace MKS6_Items.Services;

public interface IReserveItemsRequestProcessor
{
    void ProcessReserveItemsRequest(ReserveItemsRequest reserveItemsRequest);
}

public class ReserveItemsRequestProcessor : IReserveItemsRequestProcessor
{
    private readonly IDbContextFactory<ItemsDbContext> _dbContextFactory;
    private readonly IOrdersMicroserviceApiClient _ordersMicroserviceApiClient;

    public ReserveItemsRequestProcessor(
        IDbContextFactory<ItemsDbContext> dbContextFactory,
        IOrdersMicroserviceApiClient ordersMicroserviceApiClient)
    {
        _dbContextFactory = dbContextFactory;
        _ordersMicroserviceApiClient = ordersMicroserviceApiClient;
    }
    
    public void ProcessReserveItemsRequest(ReserveItemsRequest reserveItemsRequest)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable);
        var isTransactionCommitted = false;

        try
        {
            var requestedItemsIds = reserveItemsRequest
                .RequestedItems
                .Select(i => i.ItemId)
                .ToArray();

            var foundItems = dbContext
                .Items
                .Where(i => requestedItemsIds.Contains(i.Id))
                .ToArray();

            if (foundItems.Length != requestedItemsIds.Length)
            {
                MakeErrorResponse(
                    reserveItemsRequest.OrderId,
                    "One or more of requested items not found.");
                return;
            }

            foreach (var requestedItem in reserveItemsRequest.RequestedItems)
            {
                var foundItem = foundItems
                    .Where(i => i.Id == requestedItem.ItemId)
                    .Single();

                if (foundItem.AvailableQuantity < requestedItem.RequestedQuantity)
                {
                    MakeErrorResponse(
                        reserveItemsRequest.OrderId,
                        "One or more requested items are not enough in stock");
                    return;
                }

                foundItem.AvailableQuantity -= requestedItem.RequestedQuantity;
            }

            dbContext.SaveChanges();
            transaction.Commit();

            isTransactionCommitted = true;
        }
        finally
        {
            if (!isTransactionCommitted)
                transaction.Rollback();
        }

        if (isTransactionCommitted)
        {
            MakeSuccessResponse(reserveItemsRequest.OrderId);
        }
    }

    private void MakeErrorResponse(Guid orderId, string message)
    {
        var response = new ReserveItemsResponse
        {
            TransactionalId = Guid.NewGuid(),
            OrderId = orderId,
            Status = ReserveItemsResponseStatusEnum.Error,
            Message = message
        };

        _ordersMicroserviceApiClient.MakeResponseForReserveItemsRequest(response);
    }

    private void MakeSuccessResponse(Guid orderId)
    {
        var response = new ReserveItemsResponse
        {
            TransactionalId = Guid.NewGuid(),
            OrderId = orderId,
            Status = ReserveItemsResponseStatusEnum.Success
        };

        _ordersMicroserviceApiClient.MakeResponseForReserveItemsRequest(response);
    }
}
