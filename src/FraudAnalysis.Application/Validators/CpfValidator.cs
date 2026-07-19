namespace FraudAnalysis.Application.Validators;

/// <summary>
/// Valida CPF usando o algoritmo oficial da Receita Federal.
/// Aceita CPF apenas com dígitos (11 caracteres).
/// </summary>
public static class CpfValidator
{
    public static bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove qualquer formatação residual
        cpf = cpf.Trim().Replace(".", "").Replace("-", "");

        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

        // CPFs com todos os dígitos iguais são inválidos (ex: 111.111.111-11)
        if (cpf.Distinct().Count() == 1)
            return false;

        return ValidateDigit(cpf, 9) && ValidateDigit(cpf, 10);
    }

    private static bool ValidateDigit(string cpf, int position)
    {
        var sum = 0;
        for (var i = 0; i < position; i++)
            sum += (cpf[i] - '0') * (position + 1 - i);

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (cpf[position] - '0') == digit;
    }
}
