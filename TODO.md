Create a Cloud Code prompt that generates COMPLETE, DEPLOYABLE code and infra for a new **Delivery vertical slice** added to our existing multi-layer .NET repo. Use **TypeScript AWS CDK** for infra, **.NET 8 Web API** for the service, **PostgreSQL on RDS**, and **Vue 3 + TS** for the delivery UI. We want the work delivered as **9 incremental steps**, each with clear acceptance criteria, commands, and READMEs. Single environment: **dev**.

Global constraints:

- Keep it production-minded but simple: **API Gateway API Keys + Usage Plans** (no full auth yet).
- **One env (dev)** only; centralize config; use **SSM Parameter Store** and (when needed) **Secrets Manager**.
- Prefer **SNS → SQS → Fargate worker** pattern for ingestion durability.
- Frontend hosting: **S3 + CloudFront** (no Amplify).
- Provide sample .http files or curl commands to test each step.
- Provide minimal runbooks and diagrams (ASCII/mermaid) per step.

Directory targets (under repo root):

- `/delivery-service` (.NET 8 Web API: Domain, Application, Infrastructure, API)
- `/infra-cdk` (TypeScript CDK app)
- `/web-delivery` (Vue 3 + TS + Pinia)

–––––
STEP 1 — Service skeleton with PostgreSQL (no messaging yet)
Deliver:

- New `.NET 8 Web API` project using Clean Architecture:
  - **Domain**: `DeliveryPerson`, `DeliveryPersonOrder`
  - **Application**: use cases/services (CRUD DeliveryPerson; list available orders by category; assign delivery person to order with single-writer safety + idempotency token)
  - **Infrastructure (EF Core)**: PostgreSQL repositories; migrations; connection via env/SSM; health checks; structured JSON logging; OpenAPI
  - **API** Controllers:
    - `POST/GET/PUT/DELETE /deliveryPerson`
    - `POST/GET /deliveryOrder` (create from monolith payload, list by category, assign delivery person)
- Switch from any NoSQL concept to **PostgreSQL on RDS**. Provide initial EF Core **migrations** creating normalized tables:
  - `delivery_person (id PK, name, phone, email, created_at, updated_at)`
  - `delivery_order (order_id, customer_id) composite PK; category, created_at, status, delivery_person_id nullable FK; snapshot fields JSONB`
- Dockerfile (multi-stage) and `docker-compose` for local API + a local Postgres (optional), but allow direct use of remote RDS later.

Acceptance:

- `dotnet build/test` passes; `GET /health` returns ok; CRUD + simple queries work locally.

–––––
STEP 2 — CDK: VPC, RDS PostgreSQL, ECS Fargate, API Gateway (routing)
Deliver (in `/infra-cdk` TypeScript):

- **VPC** (2 AZ private subnets), Security Groups
- **RDS PostgreSQL (dev)** in private subnets; parameter group; SG rules; output Secret ARN
- **ECS Fargate** cluster + task + service for the API (private subnets, awsvpc)
- **API Gateway (HTTP API)**:
  - Routes: `/deliveryPerson/*` and `/deliveryOrder/*` → Fargate service (private integration via NLB + VPC Link or equivalent)
  - Optional routes (stubs) for `/order/*`, `/products/*` → HTTP integration to existing monolith public URL
- CloudWatch log groups wired for ECS and API GW
- SSM Parameters for service config (DB endpoint/secret ref, API base URLs)
- Outputs + README with `cdk bootstrap` and `cdk deploy` flow

Acceptance:

- `cdk synth` succeeds; `cdk deploy` provisions VPC, RDS, ECS, API GW; service reachable via API GW; DB connectivity verified by `/health/db`.

–––––
STEP 3 — Messaging: SNS → SQS → Worker to upsert orders into PostgreSQL
Deliver:

- **CDK**: create SNS topic `order-created`, SQS queue with DLQ, subscription; task IAM for `sqs:ReceiveMessage/DeleteMessage`
- In `/delivery-service`: add a lightweight **worker** process (same codebase, new entrypoint) that:
  - Polls SQS, validates message schema, uses idempotency key to upsert `delivery_order`
  - On failure: retries with backoff; poison moves to DLQ
- Schema for SNS message (JSON) with fields: `OrderId, CustomerId, Category, CreatedAt, Snapshot{...}`; document versioning

Acceptance:

- Manual `Publish` to SNS propagates to SQS; worker ingests and row appears in `delivery_order`.

–––––
STEP 4 — Web UI (Vue 3 + TS + Pinia) for delivery selection
Deliver (`/web-delivery`):

- Vite + Vue 3 (Composition API), Pinia, TypeScript
- Pages:
  - Dashboard: list **available** orders, filter by **Category**
  - Assignment screen: claim an order (optimistic update; rollback on 409/422)
- API client with typed models; `.env` for API base URL and API key; retry with exponential backoff; toast/alert on failure
- Basic accessibility (focus order, keyboard nav), code-splitting by route

Acceptance:

- `npm run dev` works; can list orders (seeded from Step 3) and claim an order; API calls succeed with API key header.

–––––
STEP 5 — Static hosting: S3 + CloudFront via CDK
Deliver:

- **CDK** bucket (S3) + **CloudFront** distribution; OAC for S3
- Build and upload artifacts from `/web-delivery/dist` with an `aws s3 sync` script; set correct cache headers
- CORS on API GW to allow the CloudFront origin

Acceptance:

- CloudFront URL serves the Vue app; app communicates to the API.

–––––
STEP 6 — API keys, throttling, usage plans (API Gateway)
Deliver:

- **CDK** configures API GW: API Key(s), **Usage Plan** (throttle + quota), stage association
- Delivery UI injects API key via header; document where to store the key (SSM) and how to rotate

Acceptance:

- Requests without key fail; with key pass; throttling observable under load.

–––––
STEP 7 — Observability: CloudWatch + CloudTrail
Deliver:

- **CDK**: CloudWatch alarms (5xx rate, p95 latency, SQS ApproxAgeOfOldestMessage)
- **CloudTrail** trail for the account/region (S3 bucket, log file validation optional)
- Add structured logs (JSON) and correlation ids in the API and worker

Acceptance:

- Alarms visible; sample alarm can be triggered/cleared; CloudTrail shows events.

–––––
STEP 8 — Parameters/Secrets via SSM/Secrets Manager
Deliver:

- Store DB connection secret in **Secrets Manager**; non-secrets (API base, ARNs) in **SSM Parameter Store**
- App config reads from env or SSM at startup; document overrides
- Provide a `tools/configure-dev.sh` to write initial parameters

Acceptance:

- Service starts with no hard-coded secrets; rotation-ready.

–––––
STEP 9 — (Optional) Realtime UX — WebSockets/SSE (can be skipped)
Deliver:

- Add **SSE endpoint** in API for “order feed” (or WebSocket if preferred)
- UI subscribes to SSE, falls back to short polling
- Keep SNS/SQS as backend ingestion; realtime is **read-only** update channel

Acceptance:

- New orders appear in UI without manual refresh; disabled behind a feature flag.

–––––
Artifacts & docs required across steps:

- READMEs per step with exact commands
- Postman/Insomnia or `.http` request files
- Minimal mermaid diagrams (VPC, API GW routes, SNS→SQS→Worker)
- Rollback guidance (destroy resources) and cost notes
