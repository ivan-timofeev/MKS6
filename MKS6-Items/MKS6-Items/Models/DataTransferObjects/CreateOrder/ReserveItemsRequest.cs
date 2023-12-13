using MKS6_Items.Models.DataTransferObjects.Item;

namespace MKS6_Items.Models.DataTransferObjects.CreateOrder;

public class ReserveItemsRequest
{
    public required Guid TransactionalId { get; init; }
    public required Guid OrderId { get; init; }
    public required IEnumerable<RequestedItemDto> RequestedItems { get; init; }
}
