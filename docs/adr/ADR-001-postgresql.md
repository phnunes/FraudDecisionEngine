# ADR-001: Uso de PostgreSQL como banco de dados

**Status:** Aceito  
**Data:** 2026-07-13

## Contexto

O FraudDecisionEngine precisa de um banco de dados relacional para persistir transacoes financeiras submetidas para analise de risco. Os requisitos sao:

- Garantias ACID para transacoes financeiras.
- Integridade referencial e constraints (chaves unicas, not null, check).
- Indice unico para implementar idempotencia sem dependencias externas.
- Tipos numericos precisos (decimal/numeric) para valores monetarios.
- Performance adequada para consultas analiticas (historico de transacoes por cliente).
- Compatibilidade com Entity Framework Core.

## Decisao

Adotar **PostgreSQL 16** como banco de dados relacional principal.

## Motivacao

1. **ACID completo**: PostgreSQL oferece isolamento transacional robusto, essencial para um sistema financeiro onde duplicatas ou inconsistencias sao inaceitaveis.

2. **Indice unico para idempotencia**: O indice `ix_transactions_idempotency_key` garante unicidade no nivel do banco, eliminando race conditions sem necessidade de locks distribuidos ou cache externo.

3. **Tipos numericos precisos**: `NUMERIC(18,2)` para valores monetarios evita erros de ponto flutuante. PostgreSQL trata decimais com precisao arbitraria nativamente.

4. **Sem custo de licenca**: Open-source com licenca permissiva. Elimina custos de licenciamento que seriam significativos com SQL Server em producao.

5. **Ecossistema .NET**: O provider `Npgsql.EntityFrameworkCore.PostgreSQL` e maduro, bem mantido e suporta todas as features do EF Core (migrations, LINQ, shadow properties).

6. **Performance para consultas analiticas**: Indices parciais, CTEs, window functions e paralelismo de queries facilitam consultas como "transacoes do cliente nos ultimos N minutos" (usada pela FrequencyRule).

7. **Extensibilidade**: Suporte a JSON/JSONB, tipos customizados e extensoes (caso seja necessario auditar payloads completos futuramente).

## Alternativas consideradas

### SQL Server

- Maturidade excelente e integracao nativa com .NET.
- **Descartado** por custo de licenciamento em producao (Enterprise/Standard) e por nao agregar beneficio tecnico sobre PostgreSQL para este caso de uso.

### MongoDB

- Flexibilidade de schema e escalabilidade horizontal.
- **Descartado** por nao oferecer garantias ACID multi-document nativas de forma transparente. Para um sistema de decisao antifraude, consistencia forte e indispensavel. Alem disso, indice unico em MongoDB nao oferece o mesmo nivel de isolamento transacional.

## Consequencias

- Maior consistencia e confiabilidade para operacoes financeiras.
- Alinhamento com o ecossistema .NET open-source (sem vendor lock-in).
- Necessidade de gerenciar migrations via EF Core (aceitavel, ja implementado).
- Operacoes como backup, replicacao e monitoramento seguem praticas bem documentadas da comunidade PostgreSQL.
