using System.ComponentModel.DataAnnotations;

namespace MKS6_Orders.Models.DataTransferObjects;

public record ReserveItemsRequest
(
    Guid TransactionalId,
    Guid OrderId,
    IEnumerable<RequestedItemDto> RequestedItems
);