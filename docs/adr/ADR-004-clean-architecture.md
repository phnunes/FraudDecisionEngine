# ADR-004: Clean Architecture

**Status:** Aceito  
**Data:** 2026-07-13

## Decisao

Organizar a solucao em 5 projetos com dependencias unidirecionais:

```
Domain         ← zero dependencias externas
Application    ← depende de Domain
Infrastructure ← implementa interfaces de Domain/Application
Api            ← composicao (Application + Infrastructure)
Worker         ← composicao (Domain + Application + Infrastructure)
```

## Por que

- **Domain isolado** — entidades e interfaces sem referencia a EF Core ou RabbitMQ
- **Testabilidade** — regras de risco testam com objetos em memoria, sem banco ou fila
- **Substituicao de infra** — trocar banco ou broker impacta apenas Infrastructure
- **Dois hosts, mesma base** — API e Worker compartilham Domain/Application/Infrastructure com composicoes distintas
- **Coesao** — nova regra vai em `Worker/Rules`, novo endpoint em `Api/Controllers`

## Camadas

| Camada | Conteudo |
|--------|----------|
| Domain | Transaction, enums, IRiskRule, ITransactionRepository, IMessagePublisher |
| Application | DTOs, validadores (CPF, IP, Geo), ITransactionService, eventos |
| Infrastructure | DbContext, repositorios, RabbitMqPublisher, migrations |
| Api | Controller, middleware de excecao, filtro Swagger |
| Worker | RabbitMqConsumer, RiskEngine, regras de risco |

## Alternativas descartadas

- **Monolito unico** — mistura responsabilidades, dificulta testes e refatoracao
- **Vertical Slices** — melhor para muitas features independentes; aqui o dominio e centrado em uma entidade com multiplas regras

## Consequencias

- 5 projetos com grafo de dependencias explicito
- Baixo acoplamento — mudanca em regra nao impacta API, mudanca em schema nao impacta dominio
- Adicionar regras/endpoints segue padroes ja estabelecidos
