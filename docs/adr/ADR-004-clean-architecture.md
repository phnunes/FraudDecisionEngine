# ADR-004: Adocao de Clean Architecture

**Status:** Aceito  
**Data:** 2026-07-13

## Contexto

O FraudDecisionEngine e um sistema critico que tende a crescer em complexidade conforme novas regras de risco, integrações externas e requisitos regulatorios surjam. A estrutura do codigo precisa suportar:

- Testabilidade unitaria sem dependencias de infraestrutura.
- Substituicao de componentes de infraestrutura (banco, broker) sem impacto no dominio.
- Separacao clara de responsabilidades entre camadas.
- Facilidade para onboarding de novos desenvolvedores.

## Decisao

Organizar a solucao em 5 projetos seguindo Clean Architecture:

```
FraudAnalysis.Domain          (camada mais interna)
FraudAnalysis.Application     (casos de uso)
FraudAnalysis.Infrastructure  (implementacoes concretas)
FraudAnalysis.Api             (ponto de entrada HTTP)
FraudAnalysis.Worker          (ponto de entrada asssincrono)
```

## Estrutura de dependencias

```
Domain         <- nenhuma dependencia externa
Application    <- depende de Domain
Infrastructure <- depende de Domain e Application (implementa interfaces)
Api            <- depende de Application e Infrastructure (composicao)
Worker         <- depende de Domain, Application e Infrastructure (composicao)
```

### Responsabilidades por camada

| Camada | Conteudo |
|--------|----------|
| **Domain** | Entidade `Transaction`, enums (`TransactionStatus`, `FraudDecision`), interface `IRiskRule`, interface `ITransactionRepository`. |
| **Application** | DTOs (`CreateTransactionRequest`, `TransactionResponse`), validadores (CPF, IP, GeoLocation), `ITransactionService`, `TransactionService`, eventos. |
| **Infrastructure** | `FraudAnalysisDbContext`, `TransactionConfiguration`, `TransactionRepository`, `RabbitMqPublisher`, migrations, `DependencyInjection`. |
| **Api** | `TransactionsController`, middlewares (exception handling), filtros Swagger, `Program.cs` da API. |
| **Worker** | `RabbitMqConsumer`, `RiskEngine`, regras de risco, metricas Prometheus, `Program.cs` do Worker. |

## Motivacao

1. **Domain sem dependencias**: A entidade `Transaction` e as interfaces `IRiskRule`/`ITransactionRepository` nao referenciam EF Core, RabbitMQ ou qualquer framework. Isso permite testar regras de negocio isoladamente.

2. **Testabilidade**: Cada regra de risco recebe uma `Transaction` e retorna uma decisao. Pode ser testada unitariamente com objetos em memoria, sem banco ou fila.

3. **Flexibilidade de infraestrutura**: Se for necessario trocar PostgreSQL por outro banco, ou RabbitMQ por outro broker, apenas o projeto Infrastructure muda. Domain e Application permanecem intactos.

4. **Dois hosts, mesma Application**: API e Worker compartilham Domain, Application e Infrastructure, mas possuem composicoes diferentes (`Program.cs` distintos). A API registra o publisher; o Worker registra o consumer e as regras.

5. **Coesao**: Cada projeto tem uma razao clara para existir e um escopo bem definido. Novos desenvolvedores identificam rapidamente onde adicionar uma nova regra (Worker/Rules) ou um novo endpoint (Api/Controllers).

## Alternativas consideradas

### Monolito unico (todos os arquivos em um projeto)

- Simplicidade inicial, zero overhead de referencia entre projetos.
- **Descartado** porque a mistura de responsabilidades dificulta testes, aumenta acoplamento e torna refatoracoes arriscadas conforme o sistema cresce.

### Vertical Slices

- Organiza por feature (cada slice contem controller, handler, repositorio).
- **Descartado** para este projeto porque o dominio e centrado em uma unica entidade (`Transaction`) com multiplas regras. Vertical slices brilham em sistemas com muitas features independentes; aqui a coesao e melhor servida por camadas horizontais.

## Consequencias

- Mais projetos na solucao (5), com grafos de dependencia para gerenciar.
- Maior clareza sobre onde cada codigo vive e como as dependencias fluem.
- Baixo acoplamento: mudancas em regras de risco nao impactam a API, mudancas no schema do banco nao impactam o dominio.
- Facilita evolucao futura: adicionar novas regras, novos endpoints ou novos consumers segue padroes ja estabelecidos.
