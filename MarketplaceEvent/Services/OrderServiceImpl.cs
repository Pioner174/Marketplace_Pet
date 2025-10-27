using Confluent.Kafka;
using Grpc.Core;
namespace MarketplaceEvent;


public class OrderServiceImpl : OrderService.OrderServiceBase
{
    private readonly IProducer<Null, string> _producer;

    public OrderServiceImpl()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA__BOOTSTRAPSERVERS") ?? "kafka:29092",
            Acks = Acks.All
        };

        int attempts = 0;
        while (true)
        {
            try
            {
                _producer = new ProducerBuilder<Null, string>(config).Build();
                Console.WriteLine("Kafka producer подключен");
                break;
            }
            catch (Exception ex)
            {
                attempts++;
                Console.WriteLine($"Kafka недоступна, попытка {attempts}: {ex.Message}");
                Thread.Sleep(5000);
            }
        }
    }

    public override async Task<OrderReply> CreateOrder(OrderRequest request, ServerCallContext context)
    {
        var message = $"OrderId: {request.OrderId}, Product: {request.Product}, Buyer: {request.Buyer}, Amount: {request.Amount}";
        await _producer.ProduceAsync("orders-topic", new Message<Null, string> { Value = message });
        Console.WriteLine($"[Kafka] Отправлено сообщение: {message}");

        return new OrderReply { Status = "Order created and sent to Kafka" };
    }
}
