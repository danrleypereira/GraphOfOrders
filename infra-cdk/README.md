# Delivery Service Infrastructure (Step 2)

This CDK project deploys the AWS infrastructure for the Delivery Service vertical slice architecture.

## Architecture Overview

The infrastructure includes:

### VPC & Networking
- **VPC** with 2 Availability Zones (us-east-2a, us-east-2b)
- **Public Subnets** (10.0.0.0/24, 10.0.1.0/24) with Internet Gateway
- **Private Subnets** (10.0.2.0/24, 10.0.3.0/24) with NAT Gateway for outbound internet access
- **Security Groups** for RDS, ECS, and ALB with least-privilege access

### Database
- **RDS PostgreSQL 15.4** (db.t3.micro) in private subnets
- **Database credentials** stored in AWS Secrets Manager
- **Automated backups** (7 days retention)
- **Performance Insights** enabled
- **Database endpoint** stored in SSM Parameter Store

### Compute
- **ECS Fargate Cluster** for containerized workloads
- **Task Definition** with 256 CPU / 512 MB memory
- **ECS Service** with 1 desired instance in private subnets
- **Application Load Balancer** (internal) for traffic distribution
- **Health checks** on `/health` endpoint

### API Gateway
- **HTTP API Gateway** with CORS enabled
- **VPC Link** for secure integration with internal ALB
- **Routes**:
  - `ANY /deliveryPerson/{proxy+}` → ALB
  - `ANY /deliveryOrder/{proxy+}` → ALB
  - `GET /health` → ALB
- **API endpoint** stored in SSM Parameter Store

### Monitoring & Configuration
- **CloudWatch Log Groups** for ECS and API Gateway
- **SSM Parameters** for service configuration
- **Cost-optimized** settings for development environment

## Prerequisites

1. **AWS CLI** configured with appropriate credentials
2. **Node.js** (v18+) and npm installed
3. **AWS CDK** CLI installed: `npm install -g aws-cdk`

## Deployment Commands

### 1. Install Dependencies
```bash
cd infra-cdk
npm install
```

### 2. Bootstrap CDK (First time only)
```bash
# Bootstrap CDK in your AWS account/region
npm run bootstrap

# Or manually specify account/region
cdk bootstrap aws://ACCOUNT-ID/REGION
```

### 3. Synthesize CloudFormation Template
```bash
# Generate CloudFormation template
npm run synth

# Review the generated template
cat cdk.out/DeliveryInfraStack-dev.template.json
```

### 4. Deploy Infrastructure
```bash
# Deploy with confirmation prompts
npm run deploy

# Deploy without prompts (CI/CD)
cdk deploy --require-approval never
```

### 5. Verify Deployment
```bash
# Check stack status
aws cloudformation describe-stacks --stack-name DeliveryInfraStack-dev

# Verify resources
aws ecs list-clusters
aws rds describe-db-instances --db-instance-identifier delivery-db-dev
aws apigatewayv2 get-apis
```

## SSM Parameters Created

After deployment, the following parameters are available:

```bash
# Database configuration
/delivery/dev/database/endpoint
/delivery/dev/database/secret-arn

# API configuration
/delivery/dev/api/base-url

# Infrastructure references
/delivery/dev/vpc/id
/delivery/dev/ecs/cluster-arn
```

### Retrieve Parameters
```bash
# Get all delivery service parameters
aws ssm get-parameters-by-path --path "/delivery/dev/" --recursive

# Get API Gateway URL
aws ssm get-parameter --name "/delivery/dev/api/base-url" --query "Parameter.Value" --output text
```


## Health Check Endpoints

Once deployed, test connectivity:

```bash
# Get API Gateway URL
API_URL=$(aws ssm get-parameter --name "/delivery/dev/api/base-url" --query "Parameter.Value" --output text)

# Test health endpoint (should return 200 when ECS service is healthy)
curl $API_URL/health

# Test delivery endpoints (placeholder until Step 1 service is deployed)
curl $API_URL/deliveryPerson
curl $API_URL/deliveryOrder
```

## Cost Considerations

**Development Environment Optimizations:**
- Single NAT Gateway (shared across AZs)
- RDS db.t3.micro instance
- ECS Fargate with minimal CPU/memory
- 7-day log retention
- Single AZ RDS (no Multi-AZ)

## Cleanup

To destroy all resources:

```bash
# Destroy the stack
npm run destroy

# Confirm deletion
cdk destroy --force
```

## Troubleshooting

### Useful Commands

```bash
# View CDK logs
cdk deploy --verbose

# Check ECS service status
aws ecs describe-services --cluster delivery-cluster-dev --services delivery-service-dev

# View RDS instance details
aws rds describe-db-instances --db-instance-identifier delivery-db-dev

# Test VPC connectivity
aws ec2 describe-vpc-endpoints --filters "Name=vpc-id,Values=<VPC-ID>"
```

## Outputs

The stack provides these outputs:
- `DatabaseEndpoint`: RDS PostgreSQL endpoint
- `DatabaseSecretArn`: Secrets Manager ARN for DB credentials
- `LoadBalancerDNS`: Internal ALB DNS name
- `EcsClusterName`: ECS cluster identifier
- `ApiGatewayUrl`: HTTP API Gateway endpoint
- `SsmParameterPrefix`: Prefix for SSM parameters (`/delivery/dev/`)