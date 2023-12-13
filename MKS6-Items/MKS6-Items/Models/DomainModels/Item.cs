namespace MKS6_Items.Models;

public class Item
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required int AvailableQuantity { get; set; }
    public required decimal Price { get; set; }
}
