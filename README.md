# 🧪 Microservices Sample with .NET 7, RabbitMQ, PostgreSQL, Ocelot API Gateway
- A lightweight microservices demo using modern .NET technologies, showcasing asynchronous communication, event-driven architecture, API Gateway routing, and service decoupling.
- **using choreography**: services react to events independently.

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
## 🚀 Overview
This solution consists of 4 main projects:
1. **ApiGateway** - Central entry point using Ocelot.
2. **ProductService** - Manages products (CRUD), publishes events.
3. **OrderService** - Manages orders, consumes product events, and publishes order events.
4. **Shared.Messages** - Shared contracts for event-based messaging.
---
## 🚀 Architecture

```
MicroservicesSample3.1/
│
├── ApiGateway/                  # Ocelot API Gateway (Port 9000)
│   ├── ocelot.json
│   ├── appsettings.json
│   └── Program.cs
│
├── OrderService/                  # Order microservice (Port 5002)
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
├── ProductService/                  # Product microservice (Port 5001)
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
├── Shared.Messages/                # Shared message contracts (events)
│   ├── ProductCreated.cs
│   ├── ProductUpdated.cs
│   ├── ProductDeleted.cs
│   ├── OrderCreated.cs
│   ├── OrderUpdated.cs
│   └── OrderDeleted.cs
└── MicroservicesSolution.sln        # Solution file
```
---
## 🧩 Key Features

### ✅ Product Microservice : https://localhost:5001
- Full CRUD operations on products.
- **Publishes**:
  - `ProductCreated`
  - `ProductUpdated`
  - `ProductDeleted`
- **Consumes**:
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
- **Publishes**:
  - `OrderCreated` (reduces stock)
  - `OrderUpdated` (adjusts stock difference)
  - `OrderDeleted` (restores stock)
- **Consumes**: (OrderService syncs product data via events:)
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
## 🌐 API Gateway Route Mappings

The API Gateway routes incoming requests to the appropriate services. Below are the route mappings:

| **Route (Incoming)**        | **Forwards To**                            |
|-----------------------------|--------------------------------------------|
| `GET /products`             | `GET https://localhost:5001/api/products`  |
| `GET /orders/{id}`          | `GET https://localhost:5002/api/orders/{id}`|
| `POST /orders`              | `POST https://localhost:5002/api/orders`   |
| `PUT /products/{id}`        | `PUT https://localhost:5001/api/products/{id}`|
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

---

## Architecture 

## 📦 Event-Driven Data Duplication (CQRS-style)

### 🧠 What is it?
When a product is created or updated in the `ProductService`, an event is published (e.g., `ProductCreated`, `ProductUpdated`). The `OrderService` consumes these events and stores a **local copy** of relevant product data in its own database.

This enables **fast local reads** in the OrderService without relying on real-time HTTP calls to ProductService.

### ⚡ Used For
- Improving **read performance** in services like OrderService
- Avoiding cross-service dependencies during runtime
- Implementing **CQRS (Command Query Responsibility Segregation)** by separating write and read models

### ✅ Pros
- 🔹 **High performance** — Reads are fast and local
- 🔹 **Fully decoupled** — Services don’t call each other at runtime
- 🔹 **Resilient and scalable** — Each service can scale independently. Good for distributed architecture.
      - OrderService remains functional even if ProductService is temporarily unavailable.

### ⚠️ Cons
- 🔸 **Eventually consistent** — Data may not be up-to-date in real time
- 🔸 **More complex** — Requires event syncing, data versioning, and potential conflict resolution

### 🏭 Industry Practice
This is a widely adopted approach in **microservices architecture**, especially in **read-heavy** or **eventually consistent** systems. It’s a core strategy in the **CQRS pattern**, used by teams that prioritize performance and service autonomy over strict real-time consistency.

--- 
## 🚦 Event-Driven Architecture
-This approach is known as event-carried state transfer.
- Follows a choreography-based communication pattern (no central coordinator).
- Maintain a local copy of necessary product data (like price, quantity) in OrderService’s database (through event syncing).
Separate read model in the OrderService (CQRS pattern). Events such as ProductCreated, ProductUpdated, and ProductDeleted update the OrderService's local ProductReadModel.
- This read model is populated through event sourcing using RabbitMQ and MassTransit for asynchronous message passing.
- No direct HTTP calls from OrderService to ProductService for product data.
- Keeps services decoupled and independently scalable.

## ✅ Benefits of This Approach (why Event over HTTP calls?)
- ✔️ Services are decoupled and more modular.
- ✔️ Supports scalability and resilience in a distributed architecture.
- ✔️ OrderService remains functional even if ProductService is temporarily unavailable.
- ✔️ Avoids tight coupling and synchronous dependencies.
- ✔️ HTTP is a synchronous call, tightly coupling the services. Events are asynchronous, making services loosely coupled and more resilient.

## 🧭 Orchestration vs Choreography
| **Feature**                | **Orchestration**                           | **Choreography**                                 |
|----------------------------|---------------------------------------------|--------------------------------------------------|
| **Control**                | Centralized                                | Decentralized (each service reacts to events)    |
| **Flow Management**        | One service controls the flow              | Flow emerges from independent services reacting to events |
| **Example Tool**           | API Gateway, Workflow Engines              | RabbitMQ, Kafka (event-based)                   |
| **Scalability**            | Less scalable due to central bottleneck    | More scalable due to loose coupling             |
| **Complexity Location**    | Logic in orchestrator                      | Logic spread across services                    |
