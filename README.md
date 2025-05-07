# 🧪 Microservices Sample with .NET 7, RabbitMQ, PostgreSQL, Ocelot API Gateway
A lightweight microservices demo using modern .NET technologies, showcasing event-driven architecture, API Gateway routing, and service decoupling.

---

## 🚀 Technologies Used

- **.NET 7**
- **PostgreSQL** via [Npgsql](https://www.npgsql.org/)
- **MassTransit** with **RabbitMQ**
- **Entity Framework Core**
- **Ocelot API Gateway**
- **Swagger** via SwaggerForOcelot
- **RESTful API** + Event-driven messaging (Pub/Sub)

---
## architecture

```
MicroservicesSample3.1/
│
├── ApiGateway/
│   ├── ocelot.json
│   ├── appsettings.json
│   └── Program.cs
│
├── OrderService/
│   ├── Consumers/
│   │   ├── ProductCreatedConsumer.cs
│   │   ├── ProductUpdatedConsumer.cs
│   │   └── ProductDeletedConsumer.cs
│   ├── Controllers/
│   │   └── OrdersController.cs
│   ├── Data/
│   │   └── OrderDbContext.cs
│   ├── Models/
│   │   ├── Order.cs
│   │   └── ProductReadModel.cs (Read only local copy of product db)
│   └── appsettings.json
│
├── ProductService/
│   ├── Consumers/
│   │   ├── OrderCreatedConsumer.cs
│   │   ├── OrderUpdatedConsumer.cs
│   │   └── OrderDeletedConsumer.cs
│   ├── Controllers/
│   │   └── ProductsController.cs
│   ├── Data/
│   │   └── ProductDbContext.cs
│   ├── Models/
│   │   └── Product.cs
│   └── appsettings.json
│
├── Shared.Messages/
│   ├── ProductCreated.cs
│   ├── ProductUpdated.cs
│   ├── ProductDeleted.cs
│   ├── OrderCreated.cs
│   ├── OrderUpdated.cs
│   └── OrderDeleted.cs

```
---
## 🧩 Key Features

### ✅ Product Microservice : https://localhost:5001
- Full CRUD operations on products.
- Publishes:
  - `ProductCreated`
  - `ProductUpdated`
  - `ProductDeleted`
- Consumes:
  - `OrderCreated` (reduces stock in product table)
  - `OrderUpdated` (adjusts stock difference)
  - `OrderDeleted` (restores stock)
- When a product is being created, a read only copy is created in order db by publishing `ProductCreated` event  
- Reduces quantity when an order is created.
- Handles rollback on `OrderDeleted` and `OrderUpdated`.

### ✅ Order Microservice : https://localhost:5002
- Full CRUD operations on orders.
- Makes use of local read only product table for product quantity validation and pricing.
- Maintains a **read-only** copy of products via event sync.
- Publishes:
  - `OrderCreated` (reduces stock)
  - `OrderUpdated` (adjusts stock difference)
  - `OrderDeleted` (restores stock)
- Consumes:
  - `ProductCreated` (create an entry in local read only copy of product table in orderdb)
  - `ProductUpdated` (update local read only copy of product table in orderdb)
  - `ProductDeleted` (delete local read only copy of product table in orderdb)

### ✅ API Gateway : https://localhost:9000
- Single entry point for client requests.
- Routes requests using **Ocelot**.
- Combines Swagger docs using **SwaggerForOcelot**.

---

## 🔄 Communication Flow

- `OrderService` publishes `OrderCreated` → `ProductService` reduces quantity.
- `ProductService` publishes `ProductCreated/Updated/Deleted` → `OrderService` updates its local read-only product table.
- `OrderService` does **not directly depend** on the Product DB — all data is synced via messaging.

---

## 🧪 Sample API Endpoints

### 📦 Products
```
GET    /products
GET    /products/{id}
POST   /products
PUT    /products/{id}
DELETE /products/{id}
```

### 📬 Orders
```
GET    /orders
GET    /orders/{id}
POST   /orders
PUT    /orders/{id}
DELETE /orders/{id}
```
---

## ✅ Running the Project
1. Make sure RabbitMQ is running locally (localhost:5672).
2. Run ProductService
3. Run OrderService
4. Run ApiGateway
5. Test via Postman or Swagger UI through the Gateway

--- 
## 📈 Order Flow Example
1. Client sends a POST /api/orders to the API Gateway.
2. Gateway routes it to OrderService.
3. OrderService:
    - Checks read-only Product info.
    - Reduces quantity and saves order.
    - Publishes OrderCreated event.
4. ProductService consumes the event and updates the product stock.
