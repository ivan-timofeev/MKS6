using MKS6_Orders.Models.DataTransferObjects;
using MKS6_Orders.Models.DomainModels;

namespace MKS6_Orders.Services;

public interface IReserveItemsResponseProcessor
{
    void ProcessReserveItemsResponse(ReserveItemsResponse reserveItemsResponse);
}

public class ReserveItemsResponseProcessor : IReserveItemsResponseProcessor
{
    private readonly IOrdersManagementService _ordersManagementService;

    public ReserveItemsResponseProcessor(IOrdersManagementService ordersManagementService)
    {
        _ordersManagementService = ordersManagementService;
    }
    
    public void ProcessReserveItemsResponse(ReserveItemsResponse reserveItemsResponse)
    {
        if (reserveItemsResponse.Status == ReserveItemsResponseStatusEnum.Success)
        {
            _ordersManagementService.UpdateOrderStatus(
                reserveItemsResponse.OrderId,
                OrderStatusEnum.Confirmed,
                "Items reserved.");
        }
        else
        {
            _ordersManagementService.UpdateOrderStatus(
                reserveItemsResponse.OrderId,
                OrderStatusEnum.Cancelled,
                reserveItemsResponse.Message);
        }
    }
}
