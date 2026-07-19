# ADR-003: Estrategia de idempotencia via header e indice unico

**Status:** Aceito  
**Data:** 2026-07-13

## Contexto

Clientes que submetem transacoes podem sofrer falhas de rede (timeout, conexao interrompida) e reenviarem a mesma requisicao. Sem idempotencia, o sistema criaria transacoes duplicadas, gerando analises de risco redundantes e potenciais bloqueios indevidos.

Requisitos:
- Garantir que a mesma transacao nao seja processada mais de uma vez.
- Nao depender de cache externo (Redis) para simplificar a infraestrutura.
- Funcionar corretamente mesmo sob concorrencia (multiplas instancias da API).
- Retornar a resposta original ao cliente em caso de retry.

## Decisao

Exigir o header `Idempotency-Key` (UUID) em toda requisicao `POST /transactions` e criar um indice UNIQUE na coluna `idempotency_key` da tabela `transactions`.

## Implementacao

### Fluxo no TransactionService

```
1. Extrair Idempotency-Key do header.
2. Buscar transacao existente com essa chave no banco.
3. Se encontrar: retornar a transacao existente (sem criar nova).
4. Se nao encontrar: criar nova transacao com a chave.
5. Em caso de violacao de unicidade (race condition): capturar excecao, buscar novamente e retornar.
```

### Indice no banco

```sql
CREATE UNIQUE INDEX ix_transactions_idempotency_key
ON transactions (idempotency_key);
```

Configurado via EF Core em `TransactionConfiguration`:

```csharp
builder.HasIndex(t => t.IdempotencyKey)
    .IsUnique()
    .HasDatabaseName("ix_transactions_idempotency_key");
```

### Validacao na API

O header e obrigatorio. Requisicoes sem `Idempotency-Key` recebem `400 Bad Request` imediatamente, antes de qualquer processamento.

## Motivacao

1. **Simplicidade**: Nenhuma dependencia adicional (Redis, Memcached). A garantia vive no banco de dados, que ja faz parte da infraestrutura.

2. **Consistencia forte**: O indice UNIQUE no PostgreSQL garante atomicidade. Mesmo que duas requisicoes identicas cheguem simultaneamente em instancias diferentes da API, apenas uma tera sucesso no INSERT.

3. **Sem TTL para gerenciar**: Diferente de solucoes baseadas em cache, a chave persiste indefinidamente junto com a transacao. Nao ha risco de re-processamento apos expiracao de cache.

4. **Transparencia**: O cliente controla a chave. Pode usar o mesmo UUID para identificar a operacao do lado dele, facilitando reconciliacao.

## Alternativas consideradas

### Redis com TTL

- Chave armazenada em Redis com expiracao (ex: 24h).
- **Descartado** por adicionar mais um componente de infraestrutura, introduzir janela de vulnerabilidade apos TTL expirar, e requerer logica de sincronizacao entre Redis e banco.

### Verificacao apenas no banco sem indice unico

- SELECT antes do INSERT.
- **Descartado** por ser vulneravel a race conditions sem o constraint de unicidade no nivel do banco.

## Consequencias

- Zero duplicatas garantidas no nivel do banco, independente do numero de instancias da API.
- Retries do cliente sao transparentes e idemponentes.
- Custo de armazenamento adicional minimo (128 bytes por transacao para a chave).
- Necessidade do cliente gerar e gerenciar UUIDs (padrao de mercado para APIs financeiras).
