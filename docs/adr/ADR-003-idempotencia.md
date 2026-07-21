# ADR-003: Idempotencia via header + indice unico

**Status:** Aceito  
**Data:** 2026-07-13

## Decisao

Exigir header `Idempotency-Key` (UUID) em todo `POST /transactions` e garantir unicidade com indice UNIQUE no banco.

## Fluxo

1. Extrair `Idempotency-Key` do header (400 se ausente)
2. Buscar transacao existente com essa chave
3. Se encontrar → retorna a existente (sem criar nova)
4. Se nao encontrar → cria normalmente
5. Race condition → indice UNIQUE rejeita duplicata no banco

## Por que

- Sem dependencia extra (Redis/Memcached) — garantia vive no PostgreSQL
- Indice UNIQUE = atomicidade mesmo com multiplas instancias da API
- Sem TTL — chave persiste com a transacao, sem risco de reprocessamento
- Cliente controla a chave — facilita reconciliacao

## Alternativas descartadas

- **Redis com TTL** — componente extra, janela de vulnerabilidade apos expiracao
- **SELECT antes do INSERT sem indice** — vulneravel a race condition

## Consequencias

- Zero duplicatas garantidas no nivel do banco
- Retries do cliente sao transparentes
- Cliente precisa gerar UUID por requisicao (padrao de mercado para APIs financeiras)
