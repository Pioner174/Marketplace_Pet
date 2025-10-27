using MarketplaceEvent;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

var app = builder.Build();

// Swagger можно оставить — он будет работать по HTTP/1.1
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// gRPC сервис
app.MapGrpcService<OrderServiceImpl>();
app.MapGet("/", () => "✅ gRPC Marketplace API запущен на HTTP/2 (порт 8080)");

app.Run();
