# ADR-002: RabbitMQ como broker de mensageria

**Status:** Aceito  
**Data:** 2026-07-13

## Decisao

Usar RabbitMQ 3.13 para comunicacao assincrona entre API e Worker.

## Por que

- ACK/NACK manual — mensagem so e confirmada apos persistencia bem-sucedida
- Dead Letter Queue (DLQ) nativa — mensagens que falham N vezes vao para DLQ automaticamente
- Management UI (porta 15672) — monitoramento de filas, taxas e conexoes
- Otimizado para task queues com baixa latencia e poucos consumidores
- Protocolo AMQP — permite evoluir para fan-out/topic routing sem trocar broker
- Biblioteca `RabbitMQ.Client` com async/await nativo

## Alternativas descartadas

- **Kafka** — complexidade desnecessaria (particionamento, consumer groups) para task queue simples
- **AWS SQS** — vendor lock-in, sem portabilidade local, sem management UI

## Consequencias

- API e Worker completamente desacoplados
- Retry com requeue ate 3 tentativas, depois DLQ
- Observabilidade via Management UI
- Container Docker com healthcheck resolve operacao local
