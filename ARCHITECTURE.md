# Async Messaging Architecture — Chinese Auction Microservices

## Task 4.1 — Why RabbitMQ?

| Feature | RabbitMQ | Kafka | Azure Service Bus | NATS |
|---|---|---|---|---|
| Learning curve | Low | High | Medium | Low |
| Message routing | Flexible (topic/fanout/direct exchanges) | Partition-based | Queues + Topics | Subject-based |
| Delivery guarantee | At-least-once | At-least-once / exactly-once | At-least-once | At-most-once (core) |
| Message ordering | Per-queue | Per-partition | Per session | No guarantee |
| Management UI | Built-in (port 15672) | Needs external tool | Azure Portal | NATS Surveyor |
| Best for | Task queues, sagas, RPC | Event streaming, high throughput | Azure-native apps | Low-latency IoT |
| Docker image size | ~180MB | ~600MB+ | N/A (cloud only) | ~20MB |

**We chose RabbitMQ** because:
1. It is taught in class and well-documented
2. Topic exchanges map naturally to saga choreography routing keys
3. Built-in management UI at `http://localhost:15672` makes it easy to inspect queues during demos
4. Lightweight — fits well in a local Docker Compose setup
5. `at-least-once` delivery with manual ack/nack is exactly what we need for the saga

---

## Task 4.2 — Saga Choreography Flow

```
Client
  │
  ▼
POST /api/purchases/checkout
  │
  ▼
OrderService
  ├─ Sets IsDraft=false, Status=Pending
  └─ Publishes ──► [saga exchange] ──► routing key: order.placed
                                              │
                                              ▼
                                     InventoryService
                                       ├─ Atomic Redis DECR
                                       ├─ qty >= 0 → Publishes ──► inventory.reserved
                                       └─ qty < 0  → Rollback  ──► inventory.rejected
                                              │
                              ┌───────────────┴───────────────┐
                              ▼                               ▼
                     inventory.reserved             inventory.rejected
                              │                               │
                              ▼                               ▼
                       OrderService                    OrderService
                       Status=Confirmed                Status=Cancelled
                       Publishes ──►                   Publishes ──►
                       order.confirmed                 order.cancelled
                              │                               │
                              └───────────────┬───────────────┘
                                              ▼
                                    NotificationService
                                    Logs ✅ or ❌ to user
```

### Exchanges & Queues

| Routing Key | Publisher | Consumer Queue | Consumer |
|---|---|---|---|
| `order.placed` | OrderService | `inventory.order.placed` | InventoryService |
| `inventory.reserved` | InventoryService | `order.inventory.reserved` | OrderService |
| `inventory.rejected` | InventoryService | `order.inventory.rejected` | OrderService |
| `order.confirmed` | OrderService | `notification.order.confirmed` | NotificationService |
| `order.cancelled` | OrderService | `notification.order.cancelled` | NotificationService |

---

## Task 4.3 — Failure Path Demo

### Happy Path (stock available):
```bash
# 1. Set inventory for gift 1 to 5 units
curl -X PUT http://localhost:5050/api/inventory/1/quantity -H "Content-Type: application/json" -d "5"

# 2. Add to basket
curl -X POST http://localhost:5050/api/purchases/basket \
  -H "Content-Type: application/json" \
  -d '{"giftId": 1, "userId": 42}'

# 3. Checkout (triggers saga)
curl -X POST http://localhost:5050/api/purchases/checkout \
  -H "Content-Type: application/json" \
  -d '[1]'

# 4. Check status
curl http://localhost:5050/api/purchases/1
# Expected: "status": "Confirmed"
```

### Failure Path (out of stock):
```bash
# 1. Set inventory to 0
curl -X PUT http://localhost:5050/api/inventory/1/quantity -H "Content-Type: application/json" -d "0"

# 2. Add to basket and checkout
curl -X POST http://localhost:5050/api/purchases/basket \
  -H "Content-Type: application/json" \
  -d '{"giftId": 1, "userId": 42}'

curl -X POST http://localhost:5050/api/purchases/checkout \
  -H "Content-Type: application/json" \
  -d '[2]'

# 4. Check status
curl http://localhost:5050/api/purchases/2
# Expected: "status": "Cancelled"
```

### Expected Logs (failure path):
```
[OrderService]      Publishing OrderPlaced PurchaseId=2 GiftId=1
[InventoryService]  Received OrderPlaced PurchaseId=2 GiftId=1
[InventoryService]  Out of stock GiftId=1, publishing InventoryRejected
[OrderService]      Order CANCELLED PurchaseId=2 Reason=Out of stock
[NotificationService] ❌ ORDER CANCELLED — PurchaseId=2 GiftId=1 UserId=42 Reason=Out of stock
```

---

## Task 4.4 — Redis Cache-Aside (ProductCatalogService)

### Strategy
- **Read**: Check Redis first → MISS → query MongoDB → store in Redis (TTL 10 min)
- **Write/Update/Delete**: Invalidate the specific key AND the `all` list key

### Cache Keys
| Key | Content | TTL |
|---|---|---|
| `gift:catalog:all` | All gifts list | 10 min |
| `gift:catalog:all:{category}` | Filtered gifts list | 10 min |
| `gift:catalog:{id}` | Single gift by ID | 10 min |

### Expected Logs
```
# First request (MISS):
[ProductCatalog] CACHE MISS key=all
[ProductCatalog] CACHE SET key=all ttl=00:10:00

# Second request (HIT):
[ProductCatalog] CACHE HIT key=all

# After update:
[ProductCatalog] CACHE INVALIDATED key=abc123
[ProductCatalog] CACHE INVALIDATED key=all
```

---

## Idempotency

Consumers are idempotent via two mechanisms:
1. **InventoryService**: Stores `processed:inventory:{MessageId}` in Redis with 24h TTL — duplicate messages are skipped
2. **OrderService**: Checks `purchase.Status == "Pending"` before processing — already-processed orders are skipped

This handles RabbitMQ's **at-least-once** delivery guarantee safely.
