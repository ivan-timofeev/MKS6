namespace MKS6_Items.Models.DataTransferObjects.Item;

public class ItemDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public required int AvailableQuantity { get; init; }
    public required decimal Price { get; init; }
}
