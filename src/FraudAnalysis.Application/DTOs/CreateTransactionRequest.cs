using System.ComponentModel.DataAnnotations;
using FraudAnalysis.Application.Validators;

namespace FraudAnalysis.Application.DTOs;

// Payload de criação de transação para análise de fraude.
public class CreateTransactionRequest : IValidatableObject
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter exatamente 11 dígitos.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas dígitos numéricos.")]
    public string CustomerDocument { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor da transação deve ser maior que zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency deve ter exatamente 3 caracteres (ISO 4217).")]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public string Ip { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
    public decimal Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
    public decimal Longitude { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(CustomerDocument) &&
            !CpfValidator.IsValid(CustomerDocument))
        {
            yield return new ValidationResult(
                "CPF inválido. Verifique os dígitos verificadores.",
                [nameof(CustomerDocument)]);
        }

        if (!string.IsNullOrWhiteSpace(Ip) && !IpValidator.IsValid(Ip))
        {
            yield return new ValidationResult(
                "Endereço IP inválido. Informe um IPv4 ou IPv6 válido.",
                [nameof(Ip)]);
        }

        if (GeoLocationValidator.IsNullIsland(Latitude, Longitude))
        {
            yield return new ValidationResult(
                "Coordenadas geográficas inválidas. Latitude e longitude não podem ser ambas zero.",
                [nameof(Latitude), nameof(Longitude)]);
        }
    }
}
