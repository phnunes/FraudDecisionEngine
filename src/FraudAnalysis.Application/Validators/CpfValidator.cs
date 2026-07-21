namespace FraudAnalysis.Application.Validators;

// Valida CPF usando o algoritmo oficial da Receita Federal.
public static class CpfValidator
{
    public static bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = cpf.Trim().Replace(".", "").Replace("-", "");

        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

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
