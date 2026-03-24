# Architecture Decision Records

## ADR-001 — Microservices over Monolith
**Decision:** Build as microservices
**Reason:** Independent deployment, team scalability, failure isolation

## ADR-002 — RabbitMQ over Direct HTTP
**Decision:** Async events via RabbitMQ
**Reason:** Decoupling, resilience, no tight dependency between services

## ADR-003 — Code First EF Core
**Decision:** Code First migrations
**Reason:** Code is source of truth, version-controlled schema changes

## ADR-004 — Choreography Saga over Orchestration
**Decision:** Event-based compensation
**Reason:** No single point of failure, simpler deployment

## ADR-005 — decimal for all money fields
**Decision:** decimal not double or float
**Reason:** Binary floating point causes rounding errors with money

## ADR-006 — GUID primary keys
**Decision:** Guid.NewGuid() not auto-increment int
**Reason:** Non-sequential, non-guessable, works across distributed systems