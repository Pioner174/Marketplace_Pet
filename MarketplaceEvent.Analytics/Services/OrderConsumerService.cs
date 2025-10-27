using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

namespace MarketplaceEvent.Analytics.Services
{
    public class OrderConsumerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("KAFKA__BOOTSTRAPSERVERS") ?? "kafka:29092",
                GroupId = "analytics-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("orders-topic");

            Console.WriteLine("🟢 MarketplaceEvent.Analytics слушает 'orders-topic'");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    Console.WriteLine($"📥 Получен заказ: {result.Message.Value}");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"⚠️ Ошибка потребления: {e.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    // Нормальное завершение
                    consumer.Close();
                    break;
                }

                await Task.Delay(100, stoppingToken); // чтобы не перегружать цикл
            }
        }
    }
}
