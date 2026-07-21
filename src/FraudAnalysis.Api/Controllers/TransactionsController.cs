using FraudAnalysis.Application.DTOs;
using FraudAnalysis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FraudAnalysis.Api.Controllers;

// Endpoints para submissão e consulta de transações.
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
