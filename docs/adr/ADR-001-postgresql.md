# ADR-001: PostgreSQL como banco de dados

## Decisao

Usar PostgreSQL 16 como banco relacional principal.

## Por que

- ACID completo — essencial para transacoes financeiras
- Indice UNIQUE em `idempotency_key` resolve idempotencia sem cache externo
- `NUMERIC(18,2)` para valores monetarios sem erro de ponto flutuante
- Open-source, sem custo de licenca
- Provider EF Core maduro (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- Consultas analiticas eficientes (indices parciais, CTEs, window functions)

## Alternativas descartadas

- **SQL Server** — custo de licenciamento sem beneficio tecnico adicional
- **MongoDB** — sem ACID multi-document transparente; inconsistente para decisoes antifraude

## Consequencias

- Migrations gerenciadas via EF Core
- Sem vendor lock-in (roda em Docker, qualquer cloud)
- Backup e replicacao seguem praticas padrao da comunidade
