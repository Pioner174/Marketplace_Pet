# 🧩 MarketplaceEvent.Analytics

Сервис аналитики событий **MarketplaceEvent**, который потребляет заказы из Kafka (`orders-topic`), полученные от основного gRPC-сервиса.

## 🚀 Стек

* **.NET 8**
* **ASP.NET Core (Minimal API + BackgroundService)**
* **Kafka (Confluent)**
* **Docker + Docker Compose**

---

## ⚙️ Архитектура

```text
MarketplaceEvent (gRPC)
      │
      ▼
   Kafka (orders-topic)
      │
      ▼
MarketplaceEvent.Analytics (Consumer)
```

Сервис `MarketplaceEvent.Analytics` слушает топик `orders-topic` и обрабатывает входящие сообщения.
Используется `BackgroundService` (`OrderConsumerService`), который подключается к брокеру Kafka через `Confluent.Kafka`.

---

## 🛠️ Запуск локально

### 1. Склонируй репозиторий

```bash
git clone https://github.com/<your_username>/MarketplaceEvent.Analytics.git
cd MarketplaceEvent.Analytics
```

### 2. Запусти Docker-окружение

```bash
docker compose up --build
```

Будет поднято:

* `zookeeper`
* `kafka`
* `kafka-ui` (доступен по адресу [http://localhost:8081](http://localhost:8081))
* `marketplace-analytics` (доступен по [http://localhost:5000](http://localhost:5000))

---

## 🧩 Переменные окружения

| Переменная                | Описание                       | Значение по умолчанию |
| ------------------------- | ------------------------------ | --------------------- |
| `ASPNETCORE_URLS`         | URL, на котором слушает сервис | `http://+:8080`       |
| `KAFKA__BOOTSTRAPSERVERS` | Адрес Kafka брокера            | `kafka:29092`         |

---

## 🔍 Основной код

`Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHostedService<OrderConsumerService>();

var app = builder.Build();

app.MapGet("/", () => "MarketplaceEvent.Analytics слушает Kafka");
app.Run();
```

`OrderConsumerService.cs`:

```csharp
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

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
        }
    }
}
```

---

## 🧠 Отладка

* Проверить, что Kafka запущена:

  ```bash
  docker ps
  ```
* Посмотреть содержимое топика:
  через [Kafka UI](http://localhost:8081)
* Логи `marketplace-analytics`:

  ```bash
  docker logs -f marketplace-analytics
  ```

---

## 📦 Структура проекта

```
MarketplaceEvent.Analytics/
│
├── MarketplaceEvent.Analytics.csproj
├── Program.cs
├── Services/
│   └── OrderConsumerService.cs
├── Dockerfile
└── docker-compose.yml
```

---

## 🧰 Команды для разработки

```bash
# Пересобрать контейнеры
docker compose build --no-cache

# Удалить все контейнеры и volume’ы
docker compose down -v

# Запуск с пересборкой
docker compose up --build
```

---

## 🧪 Проверка работы

1. Отправь сообщение в `orders-topic` через Kafka UI
2. В логах `marketplace-analytics` появится:

   ```
   📥 Получен заказ: {...}
   ```

---