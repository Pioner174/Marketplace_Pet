using MarketplaceEvent.Analytics.Services;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("������� �������");
builder.Services.AddGrpc();
Console.WriteLine("OrderConsumerService �����������");
try
{
    builder.Services.AddHostedService<OrderConsumerService>();
    
}
catch (Exception ex)
{
    Console.WriteLine($"OrderConsumerService {ex.Message}");
}

var app = builder.Build();

app.MapGet("/", () => "MarketplaceEvent.Analytics ������� Kafka");

app.Run();
