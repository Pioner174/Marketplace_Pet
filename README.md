
# 🧩 MarketplaceEvent — микросервисная система событий и аналитики

**MarketplaceEvent** — это распределённая система, моделирующая работу маркетплейса.
Она состоит из сервисов, обменивающихся событиями через **Kafka**, и использует **gRPC** для межсервисных вызовов.

## 🚀 Состав системы

| Сервис                         | Назначение                                                                   | Технологии                                |
| ------------------------------ | ---------------------------------------------------------------------------- | ----------------------------------------- |
| **MarketplaceEvent**           | Основной сервис, принимает и обрабатывает заказы, отправляет события в Kafka | .NET 8, gRPC, Kafka Producer              |
| **MarketplaceEvent.Analytics** | Аналитический сервис, слушает Kafka-топики и логирует заказы                 | .NET 8, BackgroundService, Kafka Consumer |
| **Kafka / Zookeeper**          | Брокер сообщений для обмена событиями                                        | Confluent Kafka                           |
| **Kafka UI**                   | Веб-интерфейс для просмотра сообщений и топиков                              | Provectus Kafka UI                        |

---

## 🏗️ Архитектура

```text
        ┌────────────────────┐
        │ MarketplaceEvent   │
        │ (gRPC API)         │
        │                    │
        │ └─> Kafka Producer │
        └────────┬───────────┘
                 │  (orders-topic)
                 ▼
        ┌────────────────────┐
        │ MarketplaceEvent.  │
        │ Analytics           │
        │ (Kafka Consumer)    │
        └────────────────────┘
```

---

## ⚙️ Технологии

* **.NET 8**
* **ASP.NET Core gRPC**
* **Kafka (Confluent)**
* **Docker + Docker Compose**
* **Kafka UI** для мониторинга

---

## 🛠️ Запуск проекта

### 1. Клонировать репозиторий

```bash
git clone https://github.com/<your_username>/MarketplaceEvent.git
cd MarketplaceEvent
```

### 2. Собрать и запустить контейнеры

```bash
docker compose up --build
```

Docker поднимет следующие контейнеры:

| Контейнер               | Порт         | Назначение                    |
| ----------------------- | ------------ | ----------------------------- |
| `zookeeper`             | 2181         | координация Kafka             |
| `kafka`                 | 9092 / 29092 | брокер сообщений              |
| `kafka-ui`              | 8081         | веб-интерфейс для Kafka       |
| `marketplace-event`     | 5001         | gRPC API для создания заказов |
| `marketplace-analytics` | 5000         | слушатель Kafka, аналитика    |

---

## 🧩 Переменные окружения

| Сервис        | Переменная                | Описание               | Значение по умолчанию |
| ------------- | ------------------------- | ---------------------- | --------------------- |
| **Оба**       | `KAFKA__BOOTSTRAPSERVERS` | Адрес брокера Kafka    | `kafka:29092`         |
| **Analytics** | `ASPNETCORE_URLS`         | Порт веб-прослушивания | `http://+:8080`       |
| **Event**     | `GRPC_PORT`               | Порт gRPC API          | `8080`                |

---

## 📦 Структура репозитория

```
MarketplaceEvent/
│
├── MarketplaceEvent/                      # gRPC API
│   ├── Services/
│   │   └── OrderService.cs                # gRPC-обработка заказов
│   ├── Protos/
│   │   └── order.proto                    # контракт gRPC
│   ├── Program.cs
│   └── MarketplaceEvent.csproj
│
├── MarketplaceEvent.Analytics/            # аналитика
│   ├── Services/
│   │   └── OrderConsumerService.cs        # Kafka Consumer
│   ├── Program.cs
│   └── MarketplaceEvent.Analytics.csproj
│
├── docker-compose.yml                     # общий запуск системы
├── README.md                              # этот файл
└── ...
```

---

## 🧠 Проверка работы

### 1. Проверить состояние контейнеров

```bash
docker ps
```

### 2. Проверить Kafka UI

Открыть [http://localhost:8081](http://localhost:8081)
→ Убедиться, что есть топик `orders-topic`.

### 3. Отправить тестовый заказ

Через gRPC-клиент (например, BloomRPC / gRPCurl):

```bash
grpcurl -plaintext -d '{"orderId":"123", "product":"Book", "buyer":"Alice", "amount":59.99}' \
  localhost:5001 MarketplaceEvent.OrderService/CreateOrder
```

### 4. Проверить логи Analytics

```bash
docker logs -f marketplace-analytics
```

Ты должен увидеть:

```
🟢 MarketplaceEvent.Analytics слушает 'orders-topic'
📥 Получен заказ: {"orderId":"123","product":"Book","buyer":"Alice","amount":59.99}
```

---

## 🔧 Полезные команды

```bash
# Подключение MarketplaceEvent к сети
docker network connect kafka-net MarketplaceEvent

# Полная пересборка
docker compose build --no-cache

# Очистка окружения
docker compose down -v

# Просмотр логов Analytics
docker logs -f marketplace-analytics

# Просмотр топиков
docker exec -it kafka kafka-topics --list --bootstrap-server kafka:29092
```

---

## 💡 Troubleshooting

| Проблема                      | Решение                                                                      |
| ----------------------------- | ---------------------------------------------------------------------------- |
| `1/1 brokers are down`        | Kafka не успела запуститься — подожди 10–20 секунд или добавь ожидание в код |
| `Не видит Kafka из Analytics` | Проверь, что переменная `KAFKA__BOOTSTRAPSERVERS` корректна                  |
| Изменения не применяются      | Используй `docker compose build --no-cache`                                  |
| Пустые логи                   | Проверь, что `OrderConsumerService` зарегистрирован в `Program.cs`           |

---