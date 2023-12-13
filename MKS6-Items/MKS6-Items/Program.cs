using Microsoft.EntityFrameworkCore;
using MKS6_Items.BackgroundServices;
using MKS6_Items.Data;
using MKS6_Items.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration["ItemsSqlConnectionString"];
builder.Services.AddDbContextFactory<ItemsDbContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton<IOrdersMicroserviceApiClient, OrdersMicroserviceApiClient>();
builder.Services.AddTransient<IReserveItemsRequestProcessor, ReserveItemsRequestProcessor>();
builder.Services.AddHostedService<ReserveItemsRequestProcessingBackgroundService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();