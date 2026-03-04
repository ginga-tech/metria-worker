# Architecture

## Style

Clean Architecture with strict boundaries.

## Layers

### Domain
- EmailDigest entity
- Period ValueObject
- EmailAddress ValueObject
- Idempotency rules

### Application
- ProcessEmailDigestUseCase
- IEmailSender
- IEmailDispatchRepository
- Retry orchestration

### Infrastructure
- RabbitMQ consumer
- SMTP / SendGrid adapter
- EF Core DbContext
- Migrations
- Logging adapter

### Processor (Host)
- BackgroundService
- DI container
- HealthChecks
- Graceful shutdown

## Dependency Rule

- Domain -> no dependencies
- Application -> depends only on Domain
- Infrastructure -> depends on Application
- Processor -> depends on Application + Infrastructure

## Structure
- src/
  - Metria.EmailWorker.Domain/
    - Entities/
    - ValueObjects/
    - Enums/
    - Exceptions/
  - Metria.EmailWorker.Application/
    - Abstractions/
    - Contracts/
    - Exceptions/
    - Models/
    - UseCases/
    - Validation/
  - Metria.EmailWorker.Infrastructure/
    - Configuration/
    - DependencyInjection/
    - Email/
    - Messaging/
    - Observability/
    - Persistence/
    - Time/
  - Metria.EmailWorker.Processor/
    - Extensions/
    - HealthChecks/
    - HostedServices/
    - Program.cs
- test/
  - Metria.EmailWorker.Application.UnitTests/
  - Metria.EmailWorker.Infrastructure.IntegrationTests/

