using System.ComponentModel.DataAnnotations;
using FraudAnalysis.Application.Validators;

namespace FraudAnalysis.Application.DTOs;

/// <summary>
/// Payload recebido pelo cliente ao submeter uma transação para análise.
/// </summary>
public class CreateTransactionRequest : IValidatableObject
{
    /// <summary>Identificador único do cliente. Obrigatório.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// CPF do cliente — somente dígitos, sem pontos ou traços.
    /// Validado pelo algoritmo oficial da Receita Federal.
    /// </summary>
    /// <example>52998224725</example>
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter exatamente 11 dígitos.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas dígitos numéricos.")]
    public string CustomerDocument { get; set; } = string.Empty;

    /// <summary>Valor da transação. Deve ser maior que zero.</summary>
    /// <example>1500.00</example>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor da transação deve ser maior que zero.")]
    public decimal Amount { get; set; }

    /// <summary>Código da moeda no formato ISO 4217.</summary>
    /// <example>BRL</example>
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency deve ter exatamente 3 caracteres (ISO 4217).")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Endereço IP do dispositivo que originou a transação.
    /// Aceita IPv4 e IPv6.
    /// </summary>
    /// <example>177.45.120.33</example>
    [Required]
    public string Ip { get; set; } = string.Empty;

    /// <summary>Latitude geográfica do dispositivo.</summary>
    /// <example>-25.43</example>
    [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
    public decimal Latitude { get; set; }

    /// <summary>Longitude geográfica do dispositivo.</summary>
    /// <example>-49.27</example>
    [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
    public decimal Longitude { get; set; }

    /// <summary>
    /// Validações de domínio que não cabem em Data Annotations simples:
    /// CPF pelo algoritmo da Receita Federal, IP válido e coordenadas não-nulas.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validação do CPF
        if (!string.IsNullOrWhiteSpace(CustomerDocument) &&
            !CpfValidator.IsValid(CustomerDocument))
        {
            yield return new ValidationResult(
                "CPF inválido. Verifique os dígitos verificadores.",
                [nameof(CustomerDocument)]);
        }

        // Validação do IP
        if (!string.IsNullOrWhiteSpace(Ip) && !IpValidator.IsValid(Ip))
        {
            yield return new ValidationResult(
                "Endereço IP inválido. Informe um IPv4 ou IPv6 válido.",
                [nameof(Ip)]);
        }

        // Coordenada (0,0) — Null Island — indica ausência de dado real
        if (GeoLocationValidator.IsNullIsland(Latitude, Longitude))
        {
            yield return new ValidationResult(
                "Coordenadas geográficas inválidas. Latitude e longitude não podem ser ambas zero.",
                [nameof(Latitude), nameof(Longitude)]);
        }
    }
}
