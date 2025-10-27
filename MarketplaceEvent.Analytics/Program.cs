using MarketplaceEvent.Analytics.Services;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("Процесс запущен");
builder.Services.AddGrpc();
Console.WriteLine("OrderConsumerService Запускается");
try
{
    builder.Services.AddHostedService<OrderConsumerService>();
    
}
catch (Exception ex)
{
    Console.WriteLine($"OrderConsumerService {ex.Message}");
}

var app = builder.Build();

app.MapGet("/", () => "MarketplaceEvent.Analytics слушает Kafka");

app.Run();
