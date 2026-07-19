# ADR-002: Uso de RabbitMQ como broker de mensageria

**Status:** Aceito  
**Data:** 2026-07-13

## Contexto

O sistema requer processamento assincrono entre a API (que recebe transacoes) e o Worker (que executa a analise de risco). A API deve retornar imediatamente apos persistir a transacao, delegando o processamento pesado ao Worker via fila de mensagens.

Requisitos:
- Entrega garantida de mensagens (at-least-once).
- ACK/NACK manual para controle fino do ciclo de vida da mensagem.
- Dead Letter Queue (DLQ) para mensagens que falharam apos N tentativas.
- Baixa latencia para workloads de background (nao e streaming de alto volume).
- Facilidade operacional e de monitoramento.

## Decisao

Adotar **RabbitMQ 3.13** como broker de mensageria entre API e Worker.

## Motivacao

1. **Protocolo AMQP**: Padrao aberto com semantica rica de roteamento, exchanges e bindings. Permite evolucao futura (fan-out, topic routing) sem troca de broker.

2. **ACK/NACK manual**: O Worker so confirma (ACK) a mensagem apos persistir a decisao com sucesso no banco. Em caso de falha, NACK devolve a mensagem para a fila, garantindo zero perda.

3. **Dead Letter Queue nativa**: Mensagens que excedem o limite de retries sao automaticamente roteadas para uma DLQ, permitindo investigacao posterior sem bloquear o processamento normal.

4. **Management UI**: Interface web (porta 15672) para monitorar filas, taxas de consumo, mensagens pendentes e conexoes. Facilita troubleshooting em desenvolvimento e producao.

5. **Adequado para workloads de background**: RabbitMQ e otimizado para cenarios de task queue com poucos consumidores e latencia baixa, que e exatamente o padrao deste sistema.

6. **Baixa complexidade operacional**: Single-node e suficiente para o volume inicial. Clustering disponivel quando necessario, mas nao e pre-requisito.

7. **Ecossistema .NET**: Bibliotecas maduras (RabbitMQ.Client) com suporte a async/await e integracao direta com `IHostedService`.

## Alternativas consideradas

### Apache Kafka

- Superior para streaming de eventos em alta escala, log compaction e replay.
- **Descartado** por adicionar complexidade desnecessaria (Zookeeper/KRaft, particionamento, consumer groups) para um cenario de task queue simples com volume moderado. Kafka brilha em pipelines de dados; para comando-e-processamento, RabbitMQ e mais direto.

### AWS SQS

- Servico gerenciado com escala automatica e zero operacao.
- **Descartado** por criar vendor lock-in com AWS. O projeto prioriza portabilidade (roda em Docker Compose localmente) e independencia de cloud provider. Alem disso, SQS nao oferece management UI integrada nem routing avancado.

## Consequencias

- Desacoplamento completo entre API e Worker: a API nao precisa saber quantos Workers existem ou se estao disponiveis no momento da publicacao.
- Retry automatico com backoff exponencial configurado no consumer.
- Observabilidade da fila via Management UI e metricas Prometheus.
- Necessidade de provisionar e manter o RabbitMQ (mitigado pelo container Docker com healthcheck).
- Possibilidade de evolucao para multiplas filas/exchanges conforme novas regras ou fluxos surjam.
