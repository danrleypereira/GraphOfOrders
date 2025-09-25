# Delivery Service API

## Overview
This is a .NET 9 Web API microservice for managing delivery operations, extracted as a vertical slice from the GraphOfOrders monolith. It follows Clean Architecture principles with PostgreSQL as the database.

## Architecture
```
delivery-service/
├── Domain/               # Domain entities
├── Application/          # Business logic and use cases
├── Infrastructure/       # Data access with EF Core
├── Controllers/          # API endpoints
└── Program.cs           # Application configuration
```

## Prerequisites
- .NET 9 SDK
- PostgreSQL 16
- Docker & Docker Compose (optional)

## Configuration

### Database Connection
Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=delivery;Username=postgres;Password=changeme"
  }
}
```

Or use environment variables:
```bash
export DATABASE_CONNECTION_STRING="Host=localhost;Database=delivery;Username=postgres;Password=changeme"
```

## Running the Service

### Using Docker Compose (Recommended)
```bash
# Start PostgreSQL and the API
docker-compose up -d

# View logs
docker-compose logs -f delivery-api

# Stop services
docker-compose down
```

### Local Development
```bash
# Install dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Run the application
dotnet run

# API will be available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

## Database Migrations

### Create a new migration
```bash
dotnet ef migrations add MigrationName
```

### Update database
```bash
dotnet ef database update
```

### Remove last migration
```bash
dotnet ef migrations remove
```

## API Endpoints

### Health Checks
- `GET /health` - Overall health status
- `GET /health/db` - Database connectivity check

### Delivery Person Management
- `GET /deliveryPerson` - Get all delivery persons
- `GET /deliveryPerson/{id}` - Get delivery person by ID
- `GET /deliveryPerson/active` - Get active delivery persons
- `POST /deliveryPerson` - Create new delivery person
- `PUT /deliveryPerson/{id}` - Update delivery person
- `DELETE /deliveryPerson/{id}` - Delete delivery person

### Delivery Order Management
- `POST /deliveryOrder` - Create order from monolith
- `GET /deliveryOrder` - Get available orders (optional category filter)
- `GET /deliveryOrder/{orderId}/{customerId}` - Get specific order
- `POST /deliveryOrder/{orderId}/{customerId}/assign` - Assign delivery person
- `PUT /deliveryOrder/{orderId}/{customerId}/status` - Update order status
- `GET /deliveryOrder/deliveryPerson/{id}` - Get orders by delivery person

## Testing with curl

### Create a delivery person
```bash
curl -X POST http://localhost:8080/deliveryPerson \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "phone": "555-0123",
    "email": "john@example.com",
    "isActive": true
  }'
```

### Create a delivery order
```bash
curl -X POST http://localhost:8080/deliveryOrder \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1001,
    "customerId": 2001,
    "category": "Electronics",
    "idempotencyToken": "unique-token-123",
    "snapshot": {
      "totalAmount": 299.99,
      "items": ["Laptop", "Mouse"]
    }
  }'
```

### Assign delivery person to order
```bash
curl -X POST http://localhost:8080/deliveryOrder/1001/2001/assign \
  -H "Content-Type: application/json" \
  -d '{
    "deliveryPersonId": 1,
    "idempotencyToken": "assign-token-456"
  }'
```

### Get available orders by category
```bash
curl http://localhost:8080/deliveryOrder?category=Electronics
```

## HTTP Test Files

Create a file `test.http`:
```http
### Health Check
GET http://localhost:8080/health

### Create Delivery Person
POST http://localhost:8080/deliveryPerson
Content-Type: application/json

{
  "name": "Jane Smith",
  "phone": "555-9876",
  "email": "jane@example.com",
  "isActive": true
}

### Get Available Orders
GET http://localhost:8080/deliveryOrder

### Assign Delivery Person
POST http://localhost:8080/deliveryOrder/1001/2001/assign
Content-Type: application/json

{
  "deliveryPersonId": 1,
  "idempotencyToken": "unique-assignment-token"
}
```

## Features
- ✅ Vertical slice architecture
- ✅ PostgreSQL with EF Core 9
- ✅ Idempotency support for critical operations
- ✅ Health checks (self & database)
- ✅ Structured JSON logging with Serilog
- ✅ Docker support
- ✅ OpenAPI/Swagger documentation
- ✅ CORS enabled
- ✅ Automatic migrations in development

## Next Steps (from TODO.md)
- Step 2: AWS CDK infrastructure (VPC, RDS, ECS Fargate, API Gateway)
- Step 3: SNS → SQS → Worker pattern for order ingestion
- Step 4: Vue.js frontend
- Step 5: S3 + CloudFront hosting
- Step 6: API Gateway keys and usage plans
- Step 7: CloudWatch and CloudTrail observability
- Step 8: SSM Parameter Store and Secrets Manager
- Step 9: Real-time updates with SSE/WebSockets

## Troubleshooting

### Database connection issues
1. Check PostgreSQL is running: `docker ps`
2. Verify connection string
3. Check firewall/network settings
4. Review logs: `docker-compose logs postgres`

### Migration failures
1. Ensure database exists
2. Check for pending migrations: `dotnet ef migrations list`
3. Reset database if needed: `dotnet ef database drop`

### Port conflicts
Change ports in `docker-compose.yml` or `launchSettings.json`

## License
See LICENSE in the root repository.