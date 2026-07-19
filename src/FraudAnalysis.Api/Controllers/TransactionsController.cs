using FraudAnalysis.Application.DTOs;
using FraudAnalysis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FraudAnalysis.Api.Controllers;

/// <summary>
/// Gerencia o ciclo de vida das transações submetidas para análise de risco.
/// </summary>
[ApiController]
[Route("transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    /// <summary>Submete uma nova transação para análise de risco.</summary>
    /// <remarks>
    /// A transação é persistida imediatamente com status **Pending**.
    /// O processamento antifraude ocorre de forma assíncrona via Worker.
    ///
    /// **Idempotência:** inclua o header `Idempotency-Key` com um UUID único
    /// por tentativa. Reenvios com a mesma chave retornam a transação original
    /// sem criar duplicata.
    ///
    /// **Fluxo:**
    /// ```
    /// POST /transactions  →  202 Accepted (status: Pending)
    ///        ↓
    ///    RabbitMQ Queue
    ///        ↓
    ///    Worker (RiskEngine)
    ///        ↓
    ///    GET /transactions/{id}  →  200 OK (status: Finished, decision: Approved|Rejected|Review)
    /// ```
    /// </remarks>
    /// <param name="request">Dados da transação.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Transação aceita para processamento. Consulte o status via GET.</response>
    /// <response code="400">Payload inválido ou header Idempotency-Key ausente.</response>
    /// <response code="409">Conflito inesperado (violação de unicidade no banco).</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ProblemDetails
            {
                Title  = "Idempotency-Key obrigatório",
                Detail = "Inclua o header 'Idempotency-Key' com um UUID único por requisição.",
                Status = StatusCodes.Status400BadRequest
            });

        var result = await _service.CreateAsync(request, idempotencyKey, cancellationToken);
        return Accepted(result);
    }

    /// <summary>Consulta o estado atual de uma transação pelo seu identificador.</summary>
    /// <remarks>
    /// Use este endpoint para verificar o resultado da análise de risco após
    /// submeter a transação via POST. O campo `decision` será preenchido
    /// somente quando `status` for **Finished**.
    ///
    /// **Possíveis decisões:**
    /// | Valor    | Significado                                              |
    /// |----------|----------------------------------------------------------|
    /// | Approved | Nenhuma regra de risco acionada                          |
    /// | Rejected | Transação bloqueada por regra crítica (ex: valor suspeito)|
    /// | Review   | Risco moderado — requer revisão manual                   |
    /// </remarks>
    /// <param name="id">Identificador único da transação (UUID).</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Transação encontrada.</response>
    /// <response code="404">Transação não encontrada para o Id informado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new ProblemDetails
            {
                Title  = "Transação não encontrada",
                Detail = $"Nenhuma transação encontrada com o Id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });

        return Ok(result);
    }
}
