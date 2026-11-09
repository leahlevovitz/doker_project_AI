# 🎁 Chinese Auction: Microservices Platform

> A full **event-driven microservices** back end for an online Chinese auction (raffle) platform, built with **.NET**, **RabbitMQ**, **Redis** and **Docker Compose**, including a **Saga** for distributed orders.

![.NET](https://img.shields.io/badge/.NET-C%23-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Saga-FF6600?logo=rabbitmq&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-Cache_%26_Stock-DC382D?logo=redis&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core-CC2927?logo=microsoftsqlserver&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-47A248?logo=mongodb&logoColor=white)

## 🏗️ Architecture
```
                    Client
                      │
                      ▼
            ┌──────────────────┐
            │  API Gateway     │  YARP reverse proxy
            └────────┬─────────┘
                     │
        ┌────────────┼──────────────┬────────────────┐
        ▼            ▼              ▼                ▼
   ┌─────────┐  ┌──────────┐  ┌───────────┐   ┌──────────────┐
   │   BFF   │  │  Nginx   │  │  Order    │   │ Notification │
   │ (aggr.) │  │  load    │  │  Service  │   │  + Lottery   │
   └─────────┘  │ balancer │  │ SQL+EF    │   └──────────────┘
                └────┬─────┘  └─────┬─────┘           ▲
                     ▼              │ order.placed     │ order status
          ┌────────────────────┐   ▼                   │
          │ Product Catalog ×2 │  RabbitMQ ◄───────────┘
          │ MongoDB + Redis    │   ▲   │
          └────────────────────┘   │   ▼
                            ┌───────────────────┐
                            │ Inventory Service │  atomic Redis DECR
                            └───────────────────┘
```

| Service | Responsibility | Tech |
|---|---|---|
| **API Gateway** | Single entry point, routing | YARP |
| **BFF Service** | Aggregates order + gift data for the UI | ASP.NET Core |
| **Product Catalog** (×2 replicas) | Gifts & donors CRUD, search, caching | MongoDB, Redis, Nginx load balancing |
| **Order Service** | Basket, checkout, revenue, saga orchestration | SQL Server, EF Core, RabbitMQ |
| **Inventory Service** | Stock reservation with atomic operations | Redis, RabbitMQ |
| **Notification Service** | Lottery draw, winners, reports | RabbitMQ consumer |
| **Seq** | Centralized structured logging | Serilog |

## 🔄 Saga: distributed checkout
Checkout spans several services, so it uses **choreography-based Saga** over RabbitMQ topic exchanges:

1. **Order Service** marks the order `Pending` and publishes `order.placed`.
2. **Inventory Service** reserves stock with an **atomic Redis `DECR`**:
   - enough stock → publishes `inventory.reserved` → order becomes **Confirmed**
   - out of stock → rolls back → publishes `inventory.rejected` → order becomes **Cancelled**
3. **Notification Service** reacts to the final order status.

Delivery is **at-least-once** with manual ack/nack. The full design, the broker comparison (RabbitMQ vs Kafka vs Azure Service Bus vs NATS) and happy/failure-path demos are in **[ARCHITECTURE.md](ARCHITECTURE.md)**.

## 🚀 Run it
```bash
docker compose up --build
```
| URL | What |
|---|---|
| http://localhost:5050 | API Gateway |
| http://localhost:15672 | RabbitMQ management UI |
| http://localhost:5341 | Seq logs |

## 💡 Key concepts demonstrated
`Microservices` · `API Gateway` · `BFF pattern` · `Saga (choreography)` · `Event-driven messaging` · `Atomic stock reservation` · `Horizontal scaling + load balancing` · `Distributed caching` · `Centralized logging` · `Docker Compose`

---
👩‍💻 Built by **Leah Levovitz** · [LinkedIn](https://www.linkedin.com/in/leahlevovitz) · [GitHub](https://github.com/leahlevovitz)
