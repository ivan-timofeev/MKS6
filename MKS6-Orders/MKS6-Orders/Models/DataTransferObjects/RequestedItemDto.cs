using System.ComponentModel.DataAnnotations;

namespace MKS6_Orders.Models.DataTransferObjects;

public sealed record RequestedItemDto
(
    [Required]
    Guid ItemId,
    
    [Range(1, 100)]
    int RequestedQuantity
);