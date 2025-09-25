I have an existing .NET layered monolithic project running on a Debian server in Digital Ocean. I need to create a vertical slice microservice for the delivery person domain that will be deployed separately on AWS.

Please generate the complete code for:

1. **Delivery Person Microservice (.NET Web API)**:

   - Complete new .NET Web API project with clean architecture
   - All layers implemented from scratch (no migrations from existing project):
     - Domain entities (DeliveryPerson, DeliveryPersonOrder)
     - Application services and interfaces
     - Infrastructure layer with DynamoDB integration
     - API controllers for delivery management
   - DeliveryPersonOrder entity structure:
     - Primary key: composite of OrderId and CustomerId
     - Aggregate key based on Category
     - Supports one delivery person per order
     - Include order details received from monolith
   - RESTful endpoints:
     - `/deliveryPerson` - CRUD operations for delivery persons
     - `/deliveryOrder` - manage delivery orders and assignments
   - SNS message consumer to receive order creation notifications from monolith
   - DynamoDB repository pattern implementation
   - Proper dependency injection and configuration
   - Docker configuration for Fargate deployment

2. **AWS Infrastructure (CDK in TypeScript)**:

   - DynamoDB tables:
     - DeliveryPerson table with proper indexes
     - DeliveryPersonOrder table with composite primary key and GSI on category
   - ECS Fargate service for the delivery microservice
   - SNS topic and subscription for order notifications
   - API Gateway with routing:
     - `/deliveryPerson/*` and `/deliveryOrder/*` → Fargate service
     - `/order/*`, `/products/*`, etc. → Digital Ocean monolith
   - CloudWatch logging and monitoring setup
   - CloudTrail for audit logging
   - Amplify hosting for Vue.js application
   - VPC with proper security groups
   - IAM roles and policies with least privilege
   - Environment variables and secrets management

3. **Vue.js Frontend with TypeScript (Amplify-ready)**:

   - Vue 3 with Composition API and TypeScript
   - Pinia for state management
   - Delivery person management interface:
     - Dashboard showing available orders by category
     - Delivery person assignment interface
     - Order tracking and status updates
     - Real-time notifications for new orders
   - Performance optimizations:
     - Lazy loading and code splitting
     - Virtual scrolling for order lists
     - Optimistic updates with proper error handling
   - API integration:
     - Services to communicate with API Gateway
     - Proper error handling and retry logic
     - Real-time updates using AWS AppSync or polling
   - Amplify configuration files for deployment

4. **Integration Components**:

   - SNS message format specification for order creation events
   - API Gateway configuration with proper CORS and authentication
   - DynamoDB access patterns and query optimization
   - Error handling and dead letter queues for SNS processing
   - Monitoring and alerting setup with CloudWatch alarms

5. **Configuration and Deployment**:
   - Environment-specific configurations (dev, staging, prod)
   - Dockerfile optimized for Fargate
   - CDK deployment scripts and configuration
   - Amplify deployment configuration
   - Local development setup with LocalStack or DynamoDB Local

Requirements:

- No CI/CD implementation needed
- Focus on production-ready code with proper logging and monitoring
- Implement proper security practices
- Include comprehensive error handling
- Optimize for DynamoDB access patterns
- Ensure scalability for high-volume order processing

Please provide complete, deployable code with clear documentation for setup and deployment procedures.
