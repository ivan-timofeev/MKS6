using System.ComponentModel.DataAnnotations;

namespace MKS6_Items.Models.DataTransferObjects.Item;

public record CreateItemDto
(
    [Required] string DisplayName,
    [Required] int Quantity,
    [Required] decimal Price
);
