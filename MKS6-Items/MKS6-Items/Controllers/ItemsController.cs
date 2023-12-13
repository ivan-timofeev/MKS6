using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MKS6_Items.Data;
using MKS6_Items.Models;
using MKS6_Items.Models.DataTransferObjects.Item;

namespace MKS6_Items.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<ItemsController> _logger;
    private readonly IDbContextFactory<ItemsDbContext> _dbContextFactory;

    public ItemsController(
        ILogger<ItemsController> logger,
        IDbContextFactory<ItemsDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    // GET: api/items/
    [HttpGet]
    public IActionResult GetAll()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var items = dbContext.Items.ToArray();

        return Ok(items);
    }

    // POST: api/items/
    [HttpPost]
    public IActionResult CreateNew([FromBody, BindRequired] CreateItemDto createItemDto)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var newItem = new Item
        {
            DisplayName = createItemDto.DisplayName,
            AvailableQuantity = createItemDto.Quantity,
            Price = createItemDto.Price
        };
        dbContext.Items.Add(newItem);
        dbContext.SaveChanges();

        return Ok(newItem.Id);
    }
}
